using MassTransit;
using TCFiapConsumerUpdateContact.API;
using TechChallenge.SDK;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.RegisterSdkModule(hostContext.Configuration);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UpdateContactConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("update-contact-queue", e =>
                {
                    e.ConfigureConsumer<UpdateContactConsumer>(context);
                });
            });
        });

        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();