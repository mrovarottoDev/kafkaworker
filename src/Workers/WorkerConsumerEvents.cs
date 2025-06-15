using Confluent.Kafka;
using KafkaWorker.Application.Commands.ProcessarEventoTicket;
using KafkaWorker.Constants;
using KafkaWorker.Models;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

public class WorkerConsumerEvents : BackgroundService
{
    private readonly ConsumerConfig _config;
    private readonly ILogger<WorkerConsumerEvents> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TicketEventoTipo _tipoEvento;

    private const int MaxParallelism = 10;

    public WorkerConsumerEvents(
        ConsumerConfig config,
        ILogger<WorkerConsumerEvents> logger,
        IServiceScopeFactory scopeFactory,
        TicketEventoTipo tipoEvento)
    {
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _tipoEvento = tipoEvento;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateBounded<(ConsumeResult<string, string> Message, IConsumer<string, string> Consumer, SemaphoreSlim Semaphore)>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false
        });

        var semaforo = new SemaphoreSlim(MaxParallelism);

        var workers = Enumerable.Range(0, MaxParallelism)
            .Select(_ => Task.Run(() => ProcessChannelAsync(channel.Reader, stoppingToken)))
            .ToArray();

        _ = Task.Run(() =>
        {
            var consumer = new ConsumerBuilder<string, string>(_config).Build();
            consumer.Subscribe(new[] { KafkaTopics.TicketsEvents, KafkaTopics.TicketsEventsReprocess });

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message != null)
                    {
                        semaforo.Wait(stoppingToken);
                        channel.Writer.TryWrite((result, consumer, semaforo));
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                channel.Writer.Complete();
                consumer.Close();
            }
        }, stoppingToken);

        await Task.WhenAll(workers);
    }

    private async Task ProcessChannelAsync(ChannelReader<(ConsumeResult<string, string>, IConsumer<string, string>, SemaphoreSlim)> reader, CancellationToken token)
    {
        await foreach (var (message, consumer, semaforo) in reader.ReadAllAsync(token))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new ProcessarEventoTicketCommand(
                    message,
                    consumer,
                    semaforo,
                    token,
                    _tipoEvento);

                await mediator.Send(command, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem via MediatR");
            }
        }
    }
}