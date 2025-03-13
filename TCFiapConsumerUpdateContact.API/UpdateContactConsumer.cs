using MassTransit;
using TCFiapConsumerUpdateContact.API.Model;
using DotnetSDK;

namespace TCFiapConsumerUpdateContact.API
{
    public class UpdateContactConsumer : IConsumer<UpdateContactMessage>
    {
        private readonly ILogger<UpdateContactMessage> _logger;
        private readonly IContactRepository _contactRepository;

        public UpdateContactConsumer(ILogger<UpdateContactMessage> logger, IContactRepository contactRepository)
        {
            _logger = logger;
            _contactRepository = contactRepository;
        }

        public async Task Consume(ConsumeContext<UpdateContactMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Recebida solicitação para atualizar o contato com ID: {message.ContactId}");

            var contact = await _contactRepository.GetByIdAsync(message.ContactId);
            if(contact == null)
            {
                _logger.LogWarning($"Contato {message.ContactId} não encontrado!");
                return;
            }

            await _contactRepository.UpdateAsync(contact);

            await Task.Delay(500);
            _logger.LogInformation($"Contato {message.ContactId} atualizado com sucesso!");
        }
    }
}
