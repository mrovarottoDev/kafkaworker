using KafkaWorker.Application.Services;
using MediatR;
using System.Threading.Tasks;

namespace KafkaWorker.Application.Commands.ProcessarEventoTicket
{
    public class ProcessarEventoTicketHandler : IRequestHandler<ProcessarEventoTicketCommand, Unit>
    {
        private readonly ITicketProcessorService _processor;
        

        public ProcessarEventoTicketHandler(
            ITicketProcessorService processor
            )
        {
            _processor = processor;
        }

        public async Task<Unit> Handle(ProcessarEventoTicketCommand request, CancellationToken cancellationToken)
        {
            await _processor.ProcessAsync(
                request.KafkaMessage,
                request.KafkaConsumer,
                request.Semaphore,
                request.CancellationToken,
                request.TipoEvento);

            return Unit.Value;
        }
    }
}