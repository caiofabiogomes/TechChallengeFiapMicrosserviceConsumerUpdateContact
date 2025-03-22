using MassTransit;
using TechChallenge.SDK.Domain.Models;
using TechChallenge.SDK.Domain.ValueObjects;
using TechChallenge.SDK.Infrastructure.Message;
using TechChallenge.SDK.Infrastructure.Persistence;

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
            _logger.LogInformation($"Recebida solicitação para atualizar o contato com ID: {message.Id}");

            var contact = await _contactRepository.GetByIdAsync(message.Id);
            if (contact == null)
            {
                _logger.LogWarning($"Contato {message.Id} não encontrado!");
                return;
            }

            var contactUpdated = MapContact(contact, message);

            var existingContact = _contactRepository.Query().Where(x => x.Phone.DDD == contactUpdated.Phone.DDD && x.Phone.Number == contactUpdated.Phone.Number).Any();

            if (existingContact)
            {
                _logger.LogInformation($"Contato com o numero {contactUpdated.Phone.DDD} {contactUpdated.Phone.Number} já existe!");
                return;
            }

            await _contactRepository.UpdateAsync(contactUpdated);

            _logger.LogInformation($"Contato {message.Id} atualizado com sucesso!");
        }

        public Contact MapContact(Contact contact, UpdateContactMessage message)
        {
            var name = new Name(message.FirstName, message.LastName);
            contact.UpdateName(name);

            var phone = new Phone(message.DDD, message.Phone);
            contact.UpdatePhone(phone);

            var email = new Email(message.Email);
            contact.UpdateEmail(email);

            return contact;
        }
    }

}
