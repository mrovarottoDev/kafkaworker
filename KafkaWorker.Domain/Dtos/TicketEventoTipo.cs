using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaWorker.Models
{
    using System.Runtime.Serialization;

    public enum TicketEventoTipo
    {
        [EnumMember(Value = "criacao")]
        Criacao,

        [EnumMember(Value = "sla")]
        Sla,

        [EnumMember(Value = "comentarios")]
        Comentarios,

        [EnumMember(Value = "atualizacao")]
        Atualizacao,

        [EnumMember(Value = "criacao_assincrona")]
        CriacaoAssincrona
    }
}
