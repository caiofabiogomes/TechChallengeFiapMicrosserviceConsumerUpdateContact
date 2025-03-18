using MassTransit;
using TCFiapConsumerUpdateContact.API;
using TechChallenge.SDK;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_DATABASE") ??
        hostContext.Configuration.GetConnectionString("DefaultConnection");

        var envHostRabbitMqServer = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        services.RegisterSdkModule(connectionString);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UpdateContactConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(envHostRabbitMqServer); 

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