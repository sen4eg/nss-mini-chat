using Grpc.Net.Client;
using MiniChat.Model;
using MiniChat.ViewModel;
using MiniProtoImpl;
using System.Diagnostics;

namespace MiniChat
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel vm)
        { 
            InitializeComponent();
            this.BindingContext = vm;
        }
    }
}

