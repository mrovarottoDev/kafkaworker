using Confluent.Kafka;
using System.Text.Json;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task SendAsync(string topic, object payload)
    {
        var key = Guid.NewGuid().ToString();
        var value = JsonSerializer.Serialize(payload);
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value });
    }
}