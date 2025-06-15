using KafkaWorker.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaWorker.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(typeof(DependencyInjection).Assembly);
            services.AddScoped<ITicketProcessorService, TicketProcessorService>();



            return services;
        }
    }
}