public interface IKafkaProducerService
{
    Task SendAsync(string topic, object payload);
}