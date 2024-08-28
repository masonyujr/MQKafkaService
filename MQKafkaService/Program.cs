using Microsoft.Extensions.Configuration;
using Serilog;
using MQKafkaService.Services;
using MQKafkaService.Utils;
using MQKafkaService.Models;






var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
    .Build();

Log.Logger = LoggingSetup.ConfigureLogging();

try
{
    Log.Information("Application starting up");

    var mqService = new MQReaderService(configuration);
    var schemaRegistryService = new GlueSchemaRegistryService(configuration);
    var kafkaService = new KafkaProducerService(configuration, schemaRegistryService);

    var mqMessage = mqService.ReadMessage();
    if (mqMessage != null)
    {
        kafkaService.ProduceMessage(mqMessage);
        Log.Information("Message successfully sent to Kafka.");
    }
    else
    {
        Log.Warning("No message read from IBM MQ.");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{

    Log.Information("Application shutting down up");
    Log.CloseAndFlush();
}

