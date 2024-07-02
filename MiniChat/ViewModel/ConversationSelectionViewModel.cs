using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using System.Collections.ObjectModel;

namespace MiniChat.ViewModel
{   
    public partial class ConversationSelectionViewModel : ObservableObject
    {   [ObservableProperty]
        ObservableCollection<Message> messages;

        public ConversationSelectionViewModel()
        {
            Messages =
            [
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
                new Message("Zdeněk", "V kolik zítra na pivo?"),
                new Message("Babička", "Nezapomeň koupit salám"),
                new Message("Bůh", "Hle, dal jsem vám všechny byliny vydávající semeno na celém povrchu země i každý strom, na němž je ovoce vydávající semeno. To vám bude za pokrm."),
                new Message("Stanislav z účetního", "1 000 000"),
            ];
        }

        [RelayCommand]
        async Task TapConversation()
        {
            await Shell.Current.GoToAsync(nameof(ConversationPage));
        }
    }
}
