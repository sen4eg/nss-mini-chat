using MiniProtoImpl;
using MiniServer.Core.Events;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 

public interface ISearchService
{
    // Add methods as needed
    Task Search(AuthorizedRequest<SearchRequest> authorizedRequest);
    Task FetchUsers(AuthorizedRequest<FetchUserInfoRequest> authorizedRequest);
    Task SetUserStatus(AuthorizedRequest<SetPersonStatus> authorizedRequest);
}

public class SearchService : ISearchService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SearchService(IServiceScopeFactory serviceScopeFactory) {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Search(AuthorizedRequest<SearchRequest> searchRequest) {
        
        using (var scope = _serviceScopeFactory.CreateScope()) {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var users = await userRepository.SearchUsersAsync(searchRequest.Request.Query);
            // Send users to the client
            await searchRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                SearchResponse = new SearchResponse {
                    Contacts = { users }
                }
            });
        }

    }

    public async Task FetchUsers(AuthorizedRequest<FetchUserInfoRequest> authorizedRequest) {
        using (var scope = _serviceScopeFactory.CreateScope()) {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var users = authorizedRequest.Request.Uids.Select(async id => {
                var user = await userRepository.FindById(id.ToString());
                return new UserInfo() {
                    Username = user.Username,
                    Email = user.Email,
                    Status = "Stranger" // TODO contacts
                };
            });
            await authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                UserInfo = new UserInfoResponse() {
                    Users = { await Task.WhenAll(users) }
                }
            });
        }
    }

    public Task SetUserStatus(AuthorizedRequest<SetPersonStatus> authorizedRequest) {
        
        return Task.CompletedTask;
    }
}