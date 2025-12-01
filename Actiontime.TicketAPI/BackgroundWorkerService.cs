using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Channels;

namespace Actiontime.TicketAPI
{
	public class BackgroundWorkerService : BackgroundService
	{
		private static ILogger<BackgroundWorkerService> _logger;
		private static MqttFactory mqttFactory = new MqttFactory();
		private static IMqttClient mqttClient = mqttFactory.CreateMqttClient();
		private static MqttClientOptions mqttClientOptions;
        private readonly IConfiguration _configuration;
		private readonly QRReaderService _readerService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;

        //private readonly SaleOrderService _orderService;


        public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbFactory,
    IDbContextFactory<ApplicationCloudDbContext> cdbFactory)
		{
			_logger = logger;
            _configuration = configuration;
			string ServiceIp = $"mqtt://{_configuration["DefaultParameters:ServiceIp"]}:1883";

            mqttClientOptions = new MqttClientOptionsBuilder().WithConnectionUri(new Uri(ServiceIp)).WithCredentials(username: "hezarfen", password: "n4q4n6O0").WithClientId(Environment.MachineName).Build();

            _dbFactory = dbFactory;
            _cdbFactory = cdbFactory;

            using var db = _dbFactory.CreateDbContext();
            using var cdb = _cdbFactory.CreateDbContext();
            
            _readerService = new QRReaderService(db, cdb);

            //_orderService = new SaleOrderService();

            //mqttClient.DisconnectedAsync += async e =>
            //{
            //	await ReconnectAsync();
            //};
        }



		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Servis Durdu.");
			await DisconnectClient();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
            using var db = _dbFactory.CreateDbContext();
            using var cdb = _cdbFactory.CreateDbContext();
            var reader = new QRReaderService(db, cdb);

            _logger.LogInformation("Servis Başladı.");

			await ConnectClient();

			_logger.LogInformation("MQTT Servere Bağlandı.");

			await Subscribe();

			_logger.LogInformation("MQTT Servere Subscribe Olundu.");

			await SendMessage("ConnectionInfo", $"{Environment.MachineName} Bilgisayarı Bağlandı");

		}

		public static async Task ConnectClient()
		{
			var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
		}


		public static async Task ReconnectAsync()
		{
			Console.WriteLine("Yeniden bağlanılıyor...");

			await Task.Delay(TimeSpan.FromSeconds(5)); // 5 saniye bekleme

			try
			{
				await mqttClient.ConnectAsync(mqttClientOptions);
				Console.WriteLine("Bağlantı Kuruldu.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Yeniden bağlanırken hata: {ex.Message}");
			}
		}

		public static async Task DisconnectClient()
		{
			var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
			await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
		}

		public async Task Subscribe()
		{
			var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("DeviceInfo");
					})
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("CashDrawerInfo");
					})
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("QRRead");
					})
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("Confirm");
					})
				.WithTopicFilter(
					f =>
					{
						f.WithTopic("Complete");
					})
				.Build();

			await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

			mqttClient.ApplicationMessageReceivedAsync += ReceiveMessage;
		}

		public static async Task SendMessage(string Topic, string Message)
		{
			var applicationMessage = new MqttApplicationMessageBuilder()
				.WithTopic(Topic)
				.WithPayload(Message)
				.Build();

			if (!mqttClient.IsConnected)
			{
				await ConnectClient();
			}
			
			await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

			_logger.LogInformation($"MQTT Mesaj Gitti: {Message}");
		}

		public async Task ReceiveMessage(MqttApplicationMessageReceivedEventArgs args)
		{

			if (args != null)
			{

				if (args.ApplicationMessage.Topic == "DeviceInfo")
				{
					var bytemessage = args.ApplicationMessage.Payload;

					var message = System.Text.Encoding.Default.GetString(bytemessage);

					var result = await _readerService.AddReader(message);

					_logger.LogInformation($"MQTT Mesaj Geldi: {message}");

					await SendMessage(result.SerialNumber, JsonConvert.SerializeObject(result));

				}

				if (args.ApplicationMessage.Topic == "QRRead")
				{
					var bytemessage = args.ApplicationMessage.Payload;

					var message = System.Text.Encoding.Default.GetString(bytemessage);


					var result = await _readerService.ConfirmQR(message);

					_logger.LogInformation($"MQTT Mesaj Geldi: {message}");

					await SendMessage(result.SerialNumber, JsonConvert.SerializeObject(result));

				}

				if (args.ApplicationMessage.Topic == "Confirm")
				{
					var bytemessage = args.ApplicationMessage.Payload;

					var message = System.Text.Encoding.Default.GetString(bytemessage);

					var result = await _readerService.StartQR(message);

					_logger.LogInformation($"MQTT Mesaj Geldi: {message}");

					await SendMessage(result.SerialNumber, JsonConvert.SerializeObject(result));

					if (result.Success == 1)
					{
						await SendWebSocketMessage(WebSocketProcess.RideStart, result.ConfirmNumber);
					}
				}

				if (args.ApplicationMessage.Topic == "Complete")
				{
					var bytemessage = args.ApplicationMessage.Payload;

					var message = System.Text.Encoding.Default.GetString(bytemessage);

					var result = await _readerService.CompleteQR(message);

					_logger.LogInformation($"MQTT Mesaj Geldi: {message}");

					await SendMessage(result.SerialNumber, JsonConvert.SerializeObject(result));

					if (result.Success == 1)
					{
						await SendWebSocketMessage(WebSocketProcess.RideStop, result.ConfirmNumber);
					}
				}

				if (args.ApplicationMessage.Topic == "CashDrawerInfo")
				{
					var bytemessage = args.ApplicationMessage.Payload;

					var message = System.Text.Encoding.Default.GetString(bytemessage);

					var result = await _readerService.AddDrawer(message);

					_logger.LogInformation($"MQTT Mesaj Geldi: {message}");

					await SendMessage(result.SerialNumber, JsonConvert.SerializeObject(result));

				}

			}
		}

		public async Task SendWebSocketMessage(WebSocketProcess Process, string ConfirmNumber)
		{


			WebSocketResult result = new WebSocketResult();

			result = await _readerService.CloudRideStartStop(ConfirmNumber);
			result.Process = Process.ToString();

			var factory = new MqttFactory();

			var mqttClient = factory.CreateMqttClient();

			var options = new MqttClientOptionsBuilder()
					.WithClientId(ConfirmNumber)
					.WithWebSocketServer("ws://144.126.132.166:9001") // WebSockets URL
					.WithCredentials("hezarfen", "n4q4n6O0") // Opsiyonel
					.Build();

			await mqttClient.ConnectAsync(options, CancellationToken.None);

			if (result != null && result.Success == 1)
			{		

				var message = new MqttApplicationMessageBuilder()
					.WithTopic($"Cloud/{result.LocationId}")
					.WithPayload(JsonConvert.SerializeObject(result))
					.WithRetainFlag()
					.Build();

				await mqttClient.PublishAsync(message, CancellationToken.None);

			}

			await mqttClient.DisconnectAsync();
		}

		



	}
}
