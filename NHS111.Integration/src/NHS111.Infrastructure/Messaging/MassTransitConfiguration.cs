namespace NHS111.Infrastructure.Messaging;

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHS111.Contracts.Events;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddNhs111MassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? configuration["RabbitMQ:HostName"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";
                var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.Message<DispositionCreatedEvent>(m =>
                {
                    m.SetEntityName("disposition.exchange");
                });

                cfg.Publish<DispositionCreatedEvent>(p =>
                {
                    p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout;
                });

                EndpointConvention.Map<DispositionCreatedEvent>(new Uri("queue:disposition.ummanu"));
                EndpointConvention.Map<DispositionCreatedEvent>(new Uri("queue:disposition.adastra"));
            });
        });

        return services;
    }
}
