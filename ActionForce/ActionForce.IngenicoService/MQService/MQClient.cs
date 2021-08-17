using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ActionForce.PosService
{
    public class MQClient
    {


        public IConnection GetRabbitMQConnection()
        {
            //ConnectionFactory connectionFactory = new ConnectionFactory()
            //{
            //    HostName = "37.1.145.98",
            //    UserName = "ufeservice",
            //    Password = "n4q4n6O0",
            //    VirtualHost = "/",
            //    Port = AmqpTcpEndpoint.UseDefaultPort
            //};

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "ufeservice",
                Password = "n4q4n6O0",
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort
            };

            //var connectionFactory = new ConnectionFactory { Uri = new Uri("amqp://ufeservice:n4q4n6O0@37.1.145.98:15672/") };


            return connectionFactory.CreateConnection();
        }

        public ResultMQ SendPosResult(string QueeName, long OrderID, string SerialNumber, int StatusID)
        {
            ResultMQ result = new ResultMQ()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            using (var connection = GetRabbitMQConnection())
            {
                try
                {

                    var channel = connection.CreateModel();

                    channel.QueueDeclare(QueeName, true, false, false); //"DocumentProcess"

                    string datamesaj = $"{OrderID}|{SerialNumber}|{StatusID}";

                    var messagebody = Encoding.UTF8.GetBytes(datamesaj);

                    channel.BasicPublish(string.Empty, QueeName, null, messagebody); //"DocumentProcess"

                    result.IsSuccess = true;
                    result.Message = "Kuyruğa Gönderildi";

                }
                catch (Exception ex)
                {
                    result.Message = "Kuyruğa Gönderililemedi : " + ex.Message;
                }

            }

            return result;

        }







    }
}