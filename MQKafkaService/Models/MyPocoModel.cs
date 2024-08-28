namespace MQKafkaService.Models
{
    public class MyPocoModel
    {
        public string JMSCorrelationID { get; set; }
        public string Timestamp { get; set; }
        public string Payload { get; set; }
    }
}
