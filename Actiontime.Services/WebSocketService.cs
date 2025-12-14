using Actiontime.Models.ResultModel;
using Actiontime.Services.Interfaces;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace Actiontime.Services
{
    public class WebSocketService : IWebSocketService
    {
        public async Task SendWebSocketMessage(WebSocketResult result, CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var clientId = string.IsNullOrWhiteSpace(result.ConfirmNumber)
                ? $"cloud-{Guid.NewGuid():N}"
                : $"cloud-{result.ConfirmNumber}";

            var factory = new MqttClientFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithWebSocketServer(ws =>
                {
                    ws.WithUri("ws://144.126.132.166:9001");
                })
                .WithCredentials("hezarfen", "n4q4n6O0")
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithCleanSession()
                .Build();

            try
            {
                await mqttClient.ConnectAsync(options, cancellationToken);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"Cloud/{result.LocationId}")
                    .WithPayload(JsonConvert.SerializeObject(result))
                    .WithRetainFlag()
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message, cancellationToken);
            }
            finally
            {
                if (mqttClient.IsConnected)
                {
                    try
                    {
                        var disconnectOptions = factory
                            .CreateClientDisconnectOptionsBuilder()
                            .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                            .Build();

                        await mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
                    }
                    catch
                    {
                        // disconnect hata verirse swallow (loglamak istersen ILogger ekleyebilirsin)
                    }
                }
            }
        }
    }
}
