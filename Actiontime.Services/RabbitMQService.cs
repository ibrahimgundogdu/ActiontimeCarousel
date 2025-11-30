using Actiontime.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;
        public RabbitMQService()
        {
            _connectionFactory = new ConnectionFactory();
            _connectionFactory.Uri = new Uri("amqps://raybshpp:VV0TOB7LRUbNuWwzoNhutYa0lLj89Jxf@cow.rmq2.cloudamqp.com/raybshpp");
        }

        public bool SendMQOrderMessage(OrderMqModel message)
        {
            bool isSend = false;

            using var connection = _connectionFactory.CreateConnection();

            var channel = connection.CreateModel();

            channel.QueueDeclare("OrderProcess", true, false, false);

            var jsonMessage = JsonConvert.SerializeObject(message);

            var messageBody = Encoding.UTF8.GetBytes(jsonMessage);

            try
            {
                channel.BasicPublish(string.Empty, "OrderProcess", null, messageBody);
                isSend = true;
            }
            catch (Exception ex)
            {
            }



            return isSend;
        }
    }
}
