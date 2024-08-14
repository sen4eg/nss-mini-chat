using MiniProtoImpl;
using MiniServer.Core.Events;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 

public interface ISearchService
{
    // Add methods as needed
    Task Search(AuthorizedRequest<SearchRequest> authorizedRequest);
    Task FetchUsers(AuthorizedRequest<FetchUserInfoRequest> authorizedRequest);
}

public class SearchService : ISearchService
{
    private readonly IUserRepository _userRepository;
    public SearchService(IUserRepository userRepository) {
        this._userRepository = userRepository;
    }

    public async Task Search(AuthorizedRequest<SearchRequest> searchRequest) {
        // Search for users
        var users = await _userRepository.SearchUsersAsync(searchRequest.Request.Query);
        // Send users to the client
        await searchRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
            SearchResponse = new SearchResponse {
                Contacts = { users }
            }
        });
    }

    public async Task FetchUsers(AuthorizedRequest<FetchUserInfoRequest> authorizedRequest) {
        var users = authorizedRequest.Request.Uids.Select(async id => {
            var user = await _userRepository.FindById(id.ToString());
            return new UserInfo() {
                Username = user.Username,
                Email = user.Email,
                Status= "Stranger" // TODO contacts
            };
        });
        await authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
            UserInfo = new UserInfoResponse() {
                Users = { await Task.WhenAll(users) }
            }
        });
    }
}