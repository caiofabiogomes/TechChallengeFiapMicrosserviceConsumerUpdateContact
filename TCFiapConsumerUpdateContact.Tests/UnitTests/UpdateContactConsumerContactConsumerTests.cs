using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TCFiapConsumerUpdateContact.API;
using TechChallenge.SDK.Domain.Models;
using TechChallenge.SDK.Domain.ValueObjects;
using TechChallenge.SDK.Infrastructure.Message;
using TechChallenge.SDK.Infrastructure.Persistence;

namespace TCFiapConsumerUpdateContact.Tests.UnitTests
{
    [TestFixture]
    public class UpdateContactConsumerContactConsumerTests
    {
        private UpdateContactConsumer _consumer;
        private Mock<ILogger<UpdateContactConsumer>> _loggerMock;
        private Mock<IContactRepository> _contactRepositoryMock;
        private Mock<ConsumeContext<UpdateContactMessage>> _consumeContextMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<UpdateContactConsumer>>();
            _contactRepositoryMock = new Mock<IContactRepository>();
            _consumer = new UpdateContactConsumer(_loggerMock.Object, _contactRepositoryMock.Object);
            _consumeContextMock = new Mock<ConsumeContext<UpdateContactMessage>>();
        }

        [Test]
        public async Task Consume_WhenContactExists_ShouldDeleteContactAndLogSuccess()
        { 
            var contact = new Contact(
                new Name("Contoso", "Kros"),
                new Email("contoso@outlook.com"),
                new Phone(11, 981888888)
            );
            var message = new UpdateContactMessage(
                contact.Id,
                "Contoso",
                "Kros",
                11,
                981888888,
                "contoso@outlook.com"                
                );

            _consumeContextMock.Setup(c => c.Message).Returns(message);
            var fakeContact = _consumer.MapContact(contact,message);
            _contactRepositoryMock.Setup(r => r.GetByIdAsync(contact.Id))
                .ReturnsAsync(fakeContact);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Recebida solicitação para atualizar o contato com ID: {contact.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _contactRepositoryMock.Verify(r => r.UpdateAsync(fakeContact), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Contato {contact.Id} atualizado com sucesso!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Consume_WhenContactDoesNotExist_ShouldLogWarningAndNotDeleteContact()
        {
            // Arrange
            var contactId = Guid.NewGuid();
            var message = new UpdateContactMessage(
                Guid.NewGuid(),
                "Contoso",
                "Kros",
                11,
                981888888,
                "contoso@outlook.com"
                );

            _consumeContextMock.Setup(c => c.Message).Returns(message);
            _contactRepositoryMock.Setup(r => r.GetByIdAsync(contactId))
                .ReturnsAsync((Contact)null);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Contato {message.Id} não encontrado!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _contactRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contact>()), Times.Never);
        }


        [Test]
        public async Task Consume_WhenCalled_ShouldLogReceivedMessage()
        {
            // Arrange
            var message = new UpdateContactMessage(
               Guid.NewGuid(),
               "Contoso",
               "Kros",
               11,
               981888888,
               "contoso@outlook.com"
               );

            _consumeContextMock.Setup(c => c.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Recebida solicitação para atualizar o contato com ID: {message.Id}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
