using IBM.XMS;
using Microsoft.Extensions.Configuration;
using MQKafkaService.Models;
using Serilog;

namespace MQKafkaService.Services
{
    public class MQReaderService
    {
        private readonly IConfiguration _configuration;

        public MQReaderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MyPocoModel? ReadMessage()
        {
            try
            {

                Log.Information("MQ Connection factory  starting up");
                var factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
                var connectionFactory = factoryFactory.CreateConnectionFactory();

                connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, _configuration["MQSettings:HostName"]);
                connectionFactory.SetIntProperty(XMSC.WMQ_PORT, int.Parse(_configuration["MQSettings:Port"]));
                connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, _configuration["MQSettings:Channel"]);
                connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, _configuration["MQSettings:QueueManager"]);
             //   connectionFactory.SetStringProperty(XMSC.WMQ_USER_ID, _configuration["MQSettings:UserId"]);
             //   connectionFactory.SetStringProperty(XMSC.WMQ_PASSWORD, _configuration["MQSettings:Password"]);

                using var connection = connectionFactory.CreateConnection();
                using var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
                var destination = session.CreateQueue(_configuration["MQSettings:QueueName"]);

                using var consumer = session.CreateConsumer(destination);
                connection.Start();

                var message = consumer.Receive();
                if (message is ITextMessage textMessage)
                {
                    Log.Information("Message received from IBM MQ");
                    return new MyPocoModel
                    {
                        JMSCorrelationID = textMessage.JMSCorrelationID,
                        Timestamp = new DateTime(1970, 1, 1).AddMilliseconds(textMessage.JMSTimestamp).ToString("o"), // Converts JMSTimestamp to ISO 8601 format
                        Payload = textMessage.Text
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading from IBM MQ.");
            }
            finally
            {

                Log.Information("end of MQ Service");
            }

            return null;
        }
    }
}
