using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using MQKafkaService.Models;
using Serilog;
using MQKafkaService.Services;

public class KafkaProducerService
{
    private readonly IConfiguration _configuration;
    private readonly GlueSchemaRegistryService _schemaRegistryService;

    public KafkaProducerService(IConfiguration configuration, GlueSchemaRegistryService schemaRegistryService)
    {
        _configuration = configuration;
        _schemaRegistryService = schemaRegistryService;
    }

    public void ProduceMessage(MyPocoModel poco)
    {
        if (_schemaRegistryService.ValidateMessage(poco))
        {
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = _configuration["KafkaSettings:BootstrapServers"]
            };

            using var producer = new ProducerBuilder<string, string>(kafkaConfig).Build();
            var kafkaKey = $"{poco.JMSCorrelationID}_{poco.Timestamp}";

            producer.Produce(_configuration["KafkaSettings:Topic"], new Message<string, string>
            {
                Key = kafkaKey,
                Value = poco.Payload // Assuming payload is in XML format
            });

            Log.Information("Produced message to Kafka with key: {Key}", kafkaKey);
        }
        else
        {
            Log.Warning("Message validation failed. Not producing to Kafka.");
        }
    }
}
