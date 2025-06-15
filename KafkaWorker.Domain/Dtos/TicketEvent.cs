using KafkaWorker.Models;

namespace KafkaWorker.Domain.Dtos
{
    public class TicketEvent
    {
        public string EventId { get; set; }
        public string Evento { get; set; }
        public string Jornada { get; set; }
        public int? Tentativa { get; set; }
        public string CallbackUrl { get; set; }
        public string Erro { get; set; }
        public TicketDados Dados { get; set; }

        public bool TryGetEventoTipo(out TicketEventoTipo tipo)
            => Enum.TryParse(Evento?.Replace(".", "_"), true, out tipo);
    }
}

namespace KafkaWorker.Domain.Dtos
{
    public class TicketDados
    {
        public Cliente Cliente { get; set; }
        public string Assunto { get; set; }
        public string Canal { get; set; }
    }
}

namespace KafkaWorker.Domain.Dtos
{
    public class Cliente
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
    }
}