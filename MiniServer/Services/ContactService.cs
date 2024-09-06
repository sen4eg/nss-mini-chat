using MiniProtoImpl;
using MiniServer.Data;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;
using MiniServer.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace MiniServer.Services
{
    public interface IContactService
    {
        List<Dialog> GetDialogsForUser(long authorizedRequestUserId);
        void UpdateDialog(long msgUserId, long msgReceiverId);
    }

    public class ContactService : IContactService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ContactService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public List<Dialog> GetDialogsForUser(long authorizedRequestUserId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var contactRepository = scope.ServiceProvider.GetRequiredService<IContactRepository>();
                var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();

                var contacts = contactRepository.GetDialogsForUser(authorizedRequestUserId);
                var groups = groupRepository.GetGroupsForUser(authorizedRequestUserId);
                contacts.AddRange(groups);
                return contacts;
            }
        }

        public void UpdateDialog(long msgUserId, long msgReceiverId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var contactRepository = scope.ServiceProvider.GetRequiredService<IContactRepository>();
                contactRepository.AddOrUpdate(new Contact
                {
                    UserId = msgUserId,
                    ContactId = msgReceiverId,
                    ContactTypeId = 0
                });
            }
        }
    }
}