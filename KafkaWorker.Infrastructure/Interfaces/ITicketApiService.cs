using KafkaWorker.Domain.Dtos;

public interface ITicketApiService
{
    Task<TicketApiResultado> CriarTicketAsync(TicketDados dados);
}