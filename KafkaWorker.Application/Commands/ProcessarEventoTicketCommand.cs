using Confluent.Kafka;
using KafkaWorker.Models;
using MediatR;
using System.Threading;

namespace KafkaWorker.Application.Commands.ProcessarEventoTicket
{
    public class ProcessarEventoTicketCommand : IRequest
    {
        public ConsumeResult<string, string> KafkaMessage { get; }
        public IConsumer<string, string> KafkaConsumer { get; }
        public SemaphoreSlim Semaphore { get; }
        public CancellationToken CancellationToken { get; }
        public TicketEventoTipo TipoEvento { get; }

        public ProcessarEventoTicketCommand(
            ConsumeResult<string, string> kafkaMessage,
            IConsumer<string, string> kafkaConsumer,
            SemaphoreSlim semaphore,
            CancellationToken cancellationToken,
            TicketEventoTipo tipoEvento)
        {
            KafkaMessage = kafkaMessage;
            KafkaConsumer = kafkaConsumer;
            Semaphore = semaphore;
            CancellationToken = cancellationToken;
            TipoEvento = tipoEvento;
        }
    }
}