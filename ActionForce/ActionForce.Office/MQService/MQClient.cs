using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ActionForce.Office
{
    public class MQClient
    {

        private Uri MQUri = new Uri("amqps://afaxyhan:D6ZFCkpk2uxdyTpJ1WeQgEOLDVYedDWM@rat.rmq2.cloudamqp.com/afaxyhan");
        private ConnectionFactory factory;

        public MQClient()
        {
            factory = new ConnectionFactory();
            factory.Uri = MQUri;


            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.QueueDeclare("DocumentProcess", true, false, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume("DocumentProcess", false, consumer);

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {

                try
                {
                    // business kodlar buraya gelecek
                    var dataMessage = Encoding.UTF8.GetString(e.Body.ToArray());


                    channel.BasicAck(e.DeliveryTag, false);

                    //connection.Close();
                }
                catch (Exception ex)
                {
                    //connection.Close();
                }


            };
        }




    }
}