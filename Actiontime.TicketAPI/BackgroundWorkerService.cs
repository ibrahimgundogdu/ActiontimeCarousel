using Actiontime.Models;
using Actiontime.Services.Interfaces;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System.Buffers;
using System.Text;

namespace Actiontime.TicketAPI
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly ILogger<BackgroundWorkerService> _logger;
        private readonly IServiceProvider _sp;
        private readonly IConfiguration _configuration;

        private readonly MqttClientFactory _mqttFactory;
        private readonly IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions = default!;

        // İstersen config’e taşı
        private readonly string[] _topics = new[]
        {
            "DeviceInfo",
            "CashDrawerInfo",
            "QRRead",
            "Confirm",
            "Complete"
        };

        public BackgroundWorkerService(
            ILogger<BackgroundWorkerService> logger,
            IConfiguration configuration,
            IServiceProvider sp)
        {
            _logger = logger;
            _configuration = configuration;
            _sp = sp;

            _mqttFactory = new MqttClientFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            // Event handler’ları burada bağlamak OK (async iş yapma yok)
            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += ReceiveMessage;

            BuildClientOptions();
        }

        private void BuildClientOptions()
        {
            var host = _configuration["DefaultParameters:ServiceIp"] ?? "127.0.0.1";
            var portText = _configuration["DefaultParameters:ServicePort"];
            var port = 1883;

            if (!string.IsNullOrEmpty(portText) && int.TryParse(portText, out var p))
                port = p;

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(Environment.MachineName)
                .WithTcpServer(host, port)
                .WithCredentials("hezarfen", "n4q4n6O0")
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithCleanSession()
                .Build();
        }

        private async Task OnConnected(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("MQTT connected.");

            // Reconnect olunca otomatik tekrar subscribe
            var subscribeOptions = _mqttFactory
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("DeviceInfo"))
                .WithTopicFilter(f => f.WithTopic("CashDrawerInfo"))
                .WithTopicFilter(f => f.WithTopic("QRRead"))
                .WithTopicFilter(f => f.WithTopic("Confirm"))
                .WithTopicFilter(f => f.WithTopic("Complete"))
                .Build();

            try
            {
                await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
                _logger.LogInformation("Subscribed to MQTT topics.");

                // İlk bağlantıda “ben bağlandım” mesajı
                await SendMessageAsync(
                    "ConnectionInfo",
                    $"{Environment.MachineName} Bilgisayarı Bağlandı",
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscribe failed.");
            }
        }

        private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning("MQTT disconnected. Reason: {Reason}", args.Reason);
            // Reconnect’i ExecuteAsync döngüsü yönetecek.
            return Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundWorkerService starting.");
            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundWorkerService stopping.");

            if (_mqttClient.IsConnected)
            {
                try
                {
                    var disconnectOptions = _mqttFactory
                        .CreateClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                        .Build();

                    await _mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting MQTT client.");
                }
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundWorkerService ExecuteAsync starting.");

            // Tek reconnect döngüsü
            var retryDelay = TimeSpan.FromSeconds(2);
            var maxDelay = TimeSpan.FromSeconds(30);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        _logger.LogInformation("Connecting to MQTT...");
                        await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken);

                        // ConnectedAsync event’i subscribe + connection info işini halledecek.
                        retryDelay = TimeSpan.FromSeconds(2);
                    }

                    // Worker “idling”
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // normal shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MQTT connect/loop error. Retrying in {Delay}...", retryDelay);

                    try
                    {
                        await Task.Delay(retryDelay, stoppingToken);
                    }
                    catch (OperationCanceledException) { }

                    // exponential backoff
                    var next = TimeSpan.FromSeconds(Math.Min(maxDelay.TotalSeconds, retryDelay.TotalSeconds * 2));
                    retryDelay = next;
                }
            }
        }

        private async Task ReceiveMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var msg = args?.ApplicationMessage;
                if (msg is null) return;

                var topic = msg.Topic ?? string.Empty;

                // Use Payload property instead of PayloadSegment (PayloadSegment has no getter)
                var payload = msg.Payload.IsEmpty || msg.Payload.Length == 0
                    ? string.Empty
                    : Encoding.UTF8.GetString(msg.Payload.ToArray());

                _logger.LogInformation("MQTT RX Topic: {Topic} Payload: {Payload}", topic, payload);

                using var scope = _sp.CreateScope();
                var readerService = scope.ServiceProvider.GetRequiredService<IQRReaderService>();

                if (topic.Equals("DeviceInfo", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.AddReader(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
                else if (topic.Equals("QRRead", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.ConfirmQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
                else if (topic.Equals("Confirm", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.StartQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));

                    if (result.Success == 1)
                        await SendWebSocketMessage(WebSocketProcess.RideStart, result.ConfirmNumber);
                }
                else if (topic.Equals("Complete", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.CompleteQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));

                    if (result.Success == 1)
                        await SendWebSocketMessage(WebSocketProcess.RideStop, result.ConfirmNumber);
                }
                else if (topic.Equals("CashDrawerInfo", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.AddDrawer(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
                else
                {
                    _logger.LogDebug("Unhandled topic: {Topic}", topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MQTT message");
            }
        }

        private async Task SendMessageAsync(string topic, string payload, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                topic = "Unknown";

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload ?? string.Empty)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            // Publish sırasında bağlı değilse: connect dene (ama loop da bağlamaya çalışıyor)
            if (!_mqttClient.IsConnected && !cancellation.IsCancellationRequested)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, cancellation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Reconnect failed while publishing.");
                    return;
                }
            }

            try
            {
                await _mqttClient.PublishAsync(applicationMessage, cancellation);
                _logger.LogInformation("MQTT TX Topic: {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publishing MQTT message failed.");
            }
        }

        private async Task SendWebSocketMessage(WebSocketProcess process, string confirmNumber)
        {
            using var scope = _sp.CreateScope();
            var readerService = scope.ServiceProvider.GetRequiredService<IQRReaderService>();

            var result = await readerService.CloudRideStartStop(confirmNumber);
            if (result is null) return;

            result.Process = process.ToString();

            var factory = new MqttClientFactory();
            var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId($"cloud-{confirmNumber}")
                .WithWebSocketServer(ws => ws.WithUri("ws://144.126.132.166:9001"))
                .WithCredentials("hezarfen", "n4q4n6O0")
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithCleanSession()
                .Build();

            try
            {
                await client.ConnectAsync(options, CancellationToken.None);

                if (result.Success == 1)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic($"Cloud/{result.LocationId}")
                        .WithPayload(JsonConvert.SerializeObject(result))
                        .WithRetainFlag()
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(message, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendWebSocketMessage failed");
            }
            finally
            {
                if (client.IsConnected)
                {
                    try
                    {
                        var disconnectOptions = factory
                            .CreateClientDisconnectOptionsBuilder()
                            .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                            .Build();

                        await client.DisconnectAsync(disconnectOptions, CancellationToken.None);
                    }
                    catch { /* ignore */ }
                }
            }
        }
    }
}
