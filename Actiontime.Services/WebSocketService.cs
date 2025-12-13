using Actiontime.Models.ResultModel;
using Actiontime.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.LowLevelClient;
using Actiontime.Services.Interfaces;

namespace Actiontime.Services
{
	public class WebSocketService : IWebSocketService
    {
		public async Task SendWebSocketMessage(WebSocketResult result)
		{

			var factory = new MqttClientFactory();

			var mqttClient = factory.CreateMqttClient();

			var options = new MqttClientOptionsBuilder()
					.WithClientId(result.ConfirmNumber)
					.WithWebSocketServer("ws://144.126.132.166:9001") // WebSockets URL
					.WithCredentials("hezarfen", "n4q4n6O0") // Opsiyonel
					.Build();

			await mqttClient.ConnectAsync(options, CancellationToken.None);

			var message = new MqttApplicationMessageBuilder()
					.WithTopic($"Cloud/{result.LocationId}")
					.WithPayload(JsonConvert.SerializeObject(result))
					.WithRetainFlag()
					.Build();

			await mqttClient.PublishAsync(message, CancellationToken.None);

			await mqttClient.DisconnectAsync();

		}
	}
}
