using Microsoft.Extensions.DependencyInjection;

namespace KafkaWorker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<ITicketApiService, TicketApiService>();
            services.AddScoped<IKafkaProducerService, KafkaProducerService>();
            return services;
        }
    }
}