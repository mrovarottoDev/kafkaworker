using Confluent.Kafka;
using KafkaWorker.Constants;
using KafkaWorker.Domain.Dtos;
using KafkaWorker.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaWorker.Application.Services
{
    public class TicketProcessorService : ITicketProcessorService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITicketApiService _api;
        private readonly ILogger<TicketProcessorService> _logger;

        public TicketProcessorService(IServiceScopeFactory scopeFactory, ITicketApiService api, ILogger<TicketProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _api = api;
            _logger = logger;
        }

        public async Task ProcessAsync(
            ConsumeResult<string, string> result,
            IConsumer<string, string> consumer,
            SemaphoreSlim semaforo,
            CancellationToken stoppingToken,
            TicketEventoTipo eventoEsperado)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var producer = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();

                TicketEvent message;
                try
                {
                    message = JsonSerializer.Deserialize<TicketEvent>(result.Message.Value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Falha ao desserializar mensagem Kafka: {Payload}", result.Message.Value);
                    return;
                }

                if (message == null || string.IsNullOrEmpty(message.Evento) || message.Dados == null)
                {
                    _logger.LogWarning("Mensagem Kafka inválida ou incompleta: {Payload}", result.Message.Value);
                    return;
                }

                var esperado = GetEnumMemberValue(TicketEventoTipo.CriacaoAssincrona);

                if (!string.Equals(message.Evento, esperado, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Evento ignorado: {Evento}", message.Evento);
                    return;
                }

                var tentativa = message.Tentativa ?? 0;

                if (tentativa >= 5)
                {
                    await producer.SendAsync(KafkaTopics.TicketsEventsDLQ, message);
                }
                else
                {
                    var response = await _api.CriarTicketAsync(message.Dados);

                    if (response.Sucesso)
                    {
                        await producer.SendAsync(KafkaTopics.TicketsEventsSucess, new
                        {
                            ticketId = response.TicketId,
                            status = "sucesso",
                            callbackUrl = message.CallbackUrl,
                            dados = message.Dados
                        });
                    }
                    else if (response.ErroDeNegocio)
                    {
                        await producer.SendAsync(KafkaTopics.TicketsEventsSucess, new
                        {
                            ticketId = (string)null,
                            status = "erro_negocio",
                            mensagem = response.Mensagem,
                            callbackUrl = message.CallbackUrl
                        });
                    }
                    else
                    {
                        message.Tentativa = tentativa + 1;
                        message.Erro = response.Mensagem;
                        await producer.SendAsync(KafkaTopics.TicketsEventsReprocess, message);
                    }
                }

                consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem Kafka");
            }
            finally
            {
                semaforo.Release();
            }
        }

        private static string GetEnumMemberValue(TicketEventoTipo value)
        {
            var member = typeof(TicketEventoTipo).GetMember(value.ToString()).FirstOrDefault();
            var attribute = member?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                                   .Cast<EnumMemberAttribute>()
                                   .FirstOrDefault();

            return attribute?.Value;
        }
    }
}
