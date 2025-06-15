using Confluent.Kafka;
using KafkaWorker.Models;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Se quiser puxar do Parameter Store no futuro
        // var broker = configuration.GetValue<string>("Kafka:Broker");
        var broker = "localhost:9092";

        var config = new ConsumerConfig
        {
            BootstrapServers = broker,
            GroupId = "ticket-create-consumer-group-1",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = false,
            AllowAutoCreateTopics = true,
            SessionTimeoutMs = 6000
        };

        services.AddSingleton<ConsumerConfig>(config);


        //Configura o Producer

        var configProducer = new ProducerConfig
        {
            BootstrapServers = broker,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 100,
            CompressionType = CompressionType.Snappy
        };

        // Adiciona manualmente as propriedades que não têm setter direto
        config.Set("delivery.timeout.ms", "5000");
        config.Set("request.timeout.ms", "3000");

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            return new ProducerBuilder<string, string>(configProducer)
                .SetValueSerializer(Serializers.Utf8)
                .SetKeySerializer(Serializers.Utf8)
                .Build();
        });
        
        services.AddHostedService(sp =>
             new WorkerConsumerEvents(
                 sp.GetRequiredService<ConsumerConfig>(),
                 sp.GetRequiredService<ILogger<WorkerConsumerEvents>>(),
                 sp.GetRequiredService<IServiceScopeFactory>(),
                 TicketEventoTipo.CriacaoAssincrona));


        return services;
    }
}