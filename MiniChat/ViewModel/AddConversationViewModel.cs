using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        ObservableCollection<MiniProtoImpl.ContactMsg> contacts = [];

        [ObservableProperty]
        string userQuery = string.Empty;

        public AddConversationViewModel()
        {
           
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
