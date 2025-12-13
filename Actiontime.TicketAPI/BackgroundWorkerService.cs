using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Actiontime.TicketAPI
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly ILogger<BackgroundWorkerService> _logger;
        private readonly IServiceProvider _sp;
        private readonly IConfiguration _configuration;

        private readonly MqttFactory _mqttFactory;
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;

        public BackgroundWorkerService(
            ILogger<BackgroundWorkerService> logger,
            IConfiguration configuration,
            IServiceProvider sp)
        {
            _logger = logger;
            _configuration = configuration;
            _sp = sp;

            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            var host = _configuration["DefaultParameters:ServiceIp"] ?? "127.0.0.1";
            var portText = _configuration["DefaultParameters:ServicePort"];
            var port = 1883;

            if (!string.IsNullOrEmpty(portText) && int.TryParse(portText, out var p))
                port = p;

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(Environment.MachineName)
                .WithTcpServer(host, port)
                .WithCredentials("hezarfen", "n4q4n6O0")
                // Eğer broker sadece MQTT 3.1.1 destekliyorsa bunu ekle:
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .Build();

            _mqttClient.ConnectedAsync += args =>
            {
                _logger.LogInformation("MQTT client connected.");
                return Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += async args =>
            {
                _logger.LogWarning("MQTT client disconnected. Reconnecting in 5s...");

                // Çok agresif reconnect loop’larına girmemek için küçük bir delay
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
                    _logger.LogInformation("MQTT client reconnected.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reconnect failed");
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += ReceiveMessage;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundWorkerService starting.");
            await base.StartAsync(cancellationToken);
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

            try
            {
                if (!_mqttClient.IsConnected)
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken);
                }

                var subscribeOptions = _mqttFactory
                    .CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("DeviceInfo"))
                    .WithTopicFilter(f => f.WithTopic("CashDrawerInfo"))
                    .WithTopicFilter(f => f.WithTopic("QRRead"))
                    .WithTopicFilter(f => f.WithTopic("Confirm"))
                    .WithTopicFilter(f => f.WithTopic("Complete"))
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);

                _logger.LogInformation("Subscribed to MQTT topics.");

                await SendMessageAsync(
                    "ConnectionInfo",
                    $"{Environment.MachineName} Bilgisayarı Bağlandı",
                    stoppingToken
                );

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT background worker failed");
            }
        }

        private async Task ReceiveMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                if (args?.ApplicationMessage == null)
                    return;

                var topic = args.ApplicationMessage.Topic ?? string.Empty;
                var payload = args.ApplicationMessage.PayloadSegment.Count == 0
                    ? string.Empty
                    : Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

                _logger.LogInformation(
                    "MQTT Msg received. Topic: {topic} Payload: {payload}",
                    topic,
                    payload
                );

                using var scope = _sp.CreateScope();
                var readerService = scope.ServiceProvider.GetRequiredService<IQRReaderService>();

                if (string.Equals(topic, "DeviceInfo", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.AddReader(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
                else if (string.Equals(topic, "QRRead", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.ConfirmQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
                else if (string.Equals(topic, "Confirm", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.StartQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));

                    if (result.Success == 1)
                        await SendWebSocketMessage(WebSocketProcess.RideStart, result.ConfirmNumber);
                }
                else if (string.Equals(topic, "Complete", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.CompleteQR(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));

                    if (result.Success == 1)
                        await SendWebSocketMessage(WebSocketProcess.RideStop, result.ConfirmNumber);
                }
                else if (string.Equals(topic, "CashDrawerInfo", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await readerService.AddDrawer(payload);
                    await SendMessageAsync(result.SerialNumber, JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MQTT message");
            }
        }

        private async Task SendMessageAsync(string topic, string payload, CancellationToken cancellation = default)
        {
            if (string.IsNullOrEmpty(topic))
                topic = "Unknown";

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            if (!_mqttClient.IsConnected)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, cancellation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Reconnect failed while publishing.");
                }
            }

            try
            {
                await _mqttClient.PublishAsync(applicationMessage, cancellation);
                _logger.LogInformation("Published MQTT message to {topic}", topic);
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
            result.Process = process.ToString();

            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(confirmNumber)
                .WithWebSocketServer(ws =>
                {
                    // MQTTnet 5 tarzı WebSocket config
                    ws.WithUri("ws://144.126.132.166:9001");
                })
                .WithCredentials("hezarfen", "n4q4n6O0")
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .Build();

            try
            {
                await client.ConnectAsync(options, CancellationToken.None);

                if (result != null && result.Success == 1)
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
                    var disconnectOptions = factory
                        .CreateClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                        .Build();

                    await client.DisconnectAsync(disconnectOptions, CancellationToken.None);
                }
            }
        }
    }
}
