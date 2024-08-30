using Grpc.Core;
using Grpc.Net.Client;
using MiniChat.ViewModel;
using MiniProtoImpl;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace MiniChat.Model
{

    /// <summary>
    /// A singleton containing global values required by the client, such as the user ID, or the session token.
    /// </summary>
    public class ClientState
    {
        /// <summary>
        /// Default private constructor
        /// </summary>
        private ClientState() { }

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static ClientState? instance = null;

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        /// <returns> Singleton instance </returns>
        public static ClientState GetState() {
            instance ??= new ClientState(); // .NET magic - read as "if(instance == null){instance = new ClientState();}
            return instance;
        }

        public User? CurrentUser { get; set; } = null;

        public long UserID
        {
            get
            {
                foreach(var conversation in Conversations)
                {   
                    long otherID = conversation.ContactID;
                    foreach(var message in conversation.Messages)
                    {
                        if (message.Sender != otherID)
                        {
                            Trace.WriteLine(String.Format("I think my ID is {0}", message.Sender));
                            return message.Sender;
                        }
                    }
                }
                return 0;
            }
        }


        public string SessionToken = string.Empty;
        public string RefreshToken = string.Empty;

        public MiniProtoImpl.Device? UserDevice = null;
        public Chat.ChatClient? Client { get; set; } = null;  // used to make calls to the server
        private AsyncDuplexStreamingCall<CommunicationRequest, CommunicationResponse>? _connectionObject = null;
        public AsyncDuplexStreamingCall<CommunicationRequest, CommunicationResponse>? ConnectionObject {
            private get => _connectionObject;
            set {
                _connectionObject = value;
                _responseReader = _connectionObject.ResponseStream.ReadAllAsync();
                // Start listener thread
                // TODO move this to a separate function
                // TODO cancellation token
                Thread responseThread = new Thread(new ThreadStart(HandleResponses));
                responseThread.Start();
            }
        }

        public GrpcChannel? Channel = null; // represents connection ([protocol] + address + port)

        public IClientStreamWriter<CommunicationRequest>? RequestStream
        {
            get => ConnectionObject?.RequestStream; // tu pises rovne do toho
        }

        private IAsyncEnumerable<CommunicationResponse> _responseReader; // response stream iterator
        public IAsyncEnumerable<CommunicationResponse> ResponseStreamReader => _responseReader; // Message receiving thread pseudocode:
        //while (reciving messages) {
        //    await foreach (var response in ConnectionObject?.ResponseStream.ReadAllAsync()) {
        //       process(response);
        //    }
        //}

        /// <summary>
        /// Request a new Session Token from the Server.
        /// Used when the current token is expired
        /// </summary>
        public void RequestTokenRefresh()
        {
            if (Channel == null || CurrentUser == null || UserDevice == null || Client == null) return;
            Console.WriteLine("Refreshing token - " + DateTime.Now);
            RefreshTokenRequest request = new()
            {
                RefreshToken = this.RefreshToken,
                Device = this.UserDevice,
                Name = CurrentUser.Username
            };

            var response = Client.RefreshToken(request);
            if (response.IsSucceed)
            {
                SessionToken = response.Token;
                //tokenValid = true;
            }

        }

        public ObservableCollection<Conversation> Conversations { get; set; } = [];

        public ObservableCollection<User> Contacts { get; set; } = [];

        public ObservableCollection<User> LastSearchResults { get; set; } = [];

        /// <summary>
        /// Assign a user to the current session
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <param name="refreshToken"></param>
        public void LogUserIn(string sessionToken, string refreshToken)
        {
            // log out active user first
            LogUserOut();

            SessionToken = sessionToken;
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// Clear current session user
        /// </summary>
        public void LogUserOut()
        {
            SessionToken = string.Empty;
            RefreshToken = string.Empty;
            Conversations.Clear();
            Contacts.Clear();
            LastSearchResults.Clear();
        }

        async void HandleResponses()
        {
            Trace.WriteLine("Started listener thread");
            while (true)
            {
                await foreach(CommunicationResponse response in ResponseStreamReader)
                {
                    switch (response.ContentCase)
                    {
                        // response to requestUpdate
                        case CommunicationResponse.ContentOneofCase.ContactsMessage:
                            foreach(Dialog dialog in response.ContactsMessage.Dialogs)
                            {
                                Conversations.Add(new Conversation(dialog));
                            }
                        break;
                        // response to receiving a message
                        case CommunicationResponse.ContentOneofCase.Message:
                            var message = response.Message;
                            foreach(Conversation conversation in Conversations)
                            {
                                if(conversation.ContactID == message.ReceiverId)
                                {
                                    conversation.Messages.Add(new Message(message));
                                    break;
                                }
                            }
                            // TODO what should happen if no conversation is found
                        break;

                        case CommunicationResponse.ContentOneofCase.DialogUpdate:
                            foreach(Conversation conversation in Conversations)
                            {
                                if(conversation.ContactID == response.DialogUpdate.ContactId)
                                {
                                    foreach(var dialogUpdateMessage in response.DialogUpdate.Messages)
                                    {
                                        conversation.Messages.Add(new Message(dialogUpdateMessage));
                                    }
                                    break;
                                }
                            }
                            break;
                        case CommunicationResponse.ContentOneofCase.SearchResponse:
                            // D:
                            foreach(ContactMsg contactMsg in response.SearchResponse.Contacts)
                            {
                                LastSearchResults.Add(new User(contactMsg.Uid, contactMsg.Username, UserRelation.UNKNOWN));
                            }
                            break;
                        default:
                            Trace.WriteLine(String.Format("Received unimplemented response of type \"%s\"", response.ContentCase.ToString()));
                            break;
                    }
                    Trace.WriteLine(response.ToString());
                }
            }
        }
    }
}
