﻿namespace MiniChat
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //--------- Register Routes ----------
            Routing.RegisterRoute(nameof(ConversationSelectionPage), typeof(ConversationSelectionPage));
            Routing.RegisterRoute(nameof(ConversationPage), typeof(ConversationPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(AddConversationPage), typeof(AddConversationPage));
        }
    }
}

