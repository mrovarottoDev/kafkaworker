using Confluent.Kafka;
using KafkaWorker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaWorker.Application.Services
{
    public interface ITicketProcessorService
    {
        Task ProcessAsync(
            ConsumeResult<string, string> result,
            IConsumer<string, string> consumer,
            SemaphoreSlim semaforo,
            CancellationToken stoppingToken,
            TicketEventoTipo tipoEvento);
    }
}
