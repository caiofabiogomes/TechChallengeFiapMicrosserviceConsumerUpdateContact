using MassTransit;
using TCFiapConsumerUpdateContact.API.Model;
using DotnetSDK.Persistence;

namespace TCFiapConsumerUpdateContact.API
{
    public class UpdateContactConsumer : IConsumer<UpdateContactMessage>
    {
        private readonly ILogger<UpdateContactConsumer> _logger;
        private readonly IContactRepository _contactRepository;

        public UpdateContactConsumer(ILogger<UpdateContactConsumer> logger, IContactRepository contactRepository)
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

            var contactUpdated = MapContact(message);

            await _contactRepository.UpdateAsync(contactUpdated);

            _logger.LogInformation($"Contato {message.ContactId} atualizado com sucesso!");
        }

        public Contact MapContact(UpdateContactMessage contactUpdated)
        {
            return new Contact(){                
                FirstName = contactUpdated.FirstName,
                LastName = contactUpdated.LastName,
                EmailAddress = contactUpdated.EmailAddress,
                PhoneDdd = contactUpdated.PhoneDdd,
                PhoneNumber = contactUpdated.PhoneNumber
            };            
        }
    }

}
