using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TCFiapConsumerUpdateContact.API;
using TechChallenge.SDK.Domain.Models;
using TechChallenge.SDK.Infrastructure.Message;
using TechChallenge.SDK.Infrastructure.Persistence;

namespace TCFiapConsumerUpdateContact.Tests.IntegrationTests
{
    [TestFixture]
    public class WorkerIntegrationTests
    {
        private IHost _host;
        private IServiceScope _scope;
        private Mock<IContactRepository> _consumerContactRepositoryMock;

        [SetUp]
        public async Task SetUp()
        {
            try
            {
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        _consumerContactRepositoryMock = new Mock<IContactRepository>();
                        services.AddSingleton(_consumerContactRepositoryMock.Object);

                        services.AddMassTransit(x =>
                        {
                            x.AddConsumer<UpdateContactConsumer>();
                            x.UsingInMemory((context, cfg) =>
                            {
                                cfg.ConfigureEndpoints(context);
                            });
                        });
                        services.AddLogging(builder =>
                        {
                            builder.AddConsole();
                            builder.SetMinimumLevel(LogLevel.Debug);
                        });
                    })
                    .Build();

                await _host.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);

                _scope = _host.Services.CreateScope();
                _scope.ServiceProvider.GetRequiredService<IBus>();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Error during SetUp: {ex.Message}");
            }
        }

        [Test]
        public async Task Worker_StartStop_WhenCalled_ShouldInvokeBusStartAndStop()
        {
            // Arrange
            var busControlMock = new Mock<IBusControl>();
            var loggerMock = new Mock<ILogger<Worker>>();

            busControlMock.Setup(b => b.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<BusHandle>());

            var worker = new Worker(busControlMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // Act
            await worker.StartAsync(cts.Token);
            await Task.Delay(500, cts.Token);
            await worker.StopAsync(cts.Token);

            // Assert
            busControlMock.Verify(b => b.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
            busControlMock.Verify(b => b.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateContactConsumer_Integration_WhenMessageReceived_ShouldProcessMessage()
        {
            // Arrange
            var harness = new InMemoryTestHarness();
            var fakeContactId = Guid.NewGuid();

            var consumerHarness = harness.Consumer(() =>
            {
                var loggerMock = new Mock<ILogger<UpdateContactConsumer>>();
                var contactRepositoryMock = new Mock<IContactRepository>();
                var fakeContact = new Contact { Id = fakeContactId };
                contactRepositoryMock.Setup(r => r.GetByIdAsync(fakeContactId))
                    .ReturnsAsync(fakeContact);

                _consumerContactRepositoryMock = contactRepositoryMock;

                return new UpdateContactConsumer(loggerMock.Object, contactRepositoryMock.Object);
            });

            await harness.Start();
            try
            {
                // Act
                var message = new UpdateContactMessage(fakeContactId, "","",11,987654321,"");
                await harness.InputQueueSendEndpoint.Send(message);

                Assert.IsTrue(await harness.Consumed.Any<UpdateContactMessage>(), "Message was not consumed.");
                Assert.IsTrue(await consumerHarness.Consumed.Any<UpdateContactMessage>(), "Consumer did not process the message.");

                // Assert
                _consumerContactRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contact>()), Times.Once);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await _host.StopAsync();
            _host.Dispose();

            _scope.Dispose();
        }
    }
}
