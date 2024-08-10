using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.ViewModel
{
    public partial class ConversationViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<String> messages;

        [ObservableProperty]
        String messageText = string.Empty;
        public ConversationViewModel()
        {

        }

        [RelayCommand]
        void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                Messages.Add(MessageText);
                MessageText = string.Empty;
            }
        }
    }
}
