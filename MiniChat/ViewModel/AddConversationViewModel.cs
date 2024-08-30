using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using MiniChat.Model;
using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.ViewModel
{
    public partial class AddConversationViewModel : ObservableObject
    {
        private ClientState state = ClientState.GetState();

        [ObservableProperty]
        ObservableCollection<User> searchResults = [];

        [ObservableProperty]
        string userQuery = string.Empty;

        public AddConversationViewModel()
        {
            searchResults = state.LastSearchResults;
        }

        [RelayCommand]
        public async Task TapUser(User user)
        {
            await Shell.Current.GoToAsync(nameof(ConversationPage), new Dictionary<String, Object> { { "ConversationObject", new Conversation(user.Id, null, 0) } });
        }

        [RelayCommand]
        public async Task SendQuery()
        {
            CommunicationRequest communicationRequest = new CommunicationRequest
            {
                Token = state.SessionToken,
                Search = new SearchRequest
                {
                    Query = UserQuery,
                }
            };

            await state.RequestStream.WriteAsync(communicationRequest);
            UserQuery = String.Empty;
        }
    }
}
