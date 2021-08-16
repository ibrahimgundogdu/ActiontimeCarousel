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
        //private Uri MQUri = new Uri("amqps://afaxyhan:D6ZFCkpk2uxdyTpJ1WeQgEOLDVYedDWM@rat.rmq2.cloudamqp.com/afaxyhan");
        //private string MQPass = "D6ZFCkpk2uxdyTpJ1WeQgEOLDVYedDWM";
        private ConnectionFactory factory;

        public MQClient()
        {
            factory = new ConnectionFactory() { HostName = "localhost" };
            //factory.Uri = MQUri;
        }


        public ResultMQ SendPosResult(long OrderID, string SerialNumber, int StatusID)
        {
            ResultMQ result = new ResultMQ()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            using (var connection = factory.CreateConnection())
            {
                try
                {

                
                var channel = connection.CreateModel();

                channel.QueueDeclare("DocumentProcess", true, false, false);

                string datamesaj = $"{OrderID}|{SerialNumber}|{StatusID}";

                var messagebody = Encoding.UTF8.GetBytes(datamesaj);

                channel.BasicPublish(string.Empty, "DocumentProcess", null, messagebody);

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