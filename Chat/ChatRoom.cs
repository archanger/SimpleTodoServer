using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TodoAPI.Config;
using TodoAPI.Core;
using TodoAPI.Core.Models;
using TodoAPI.Helpers;
using TodoAPI.Middleware;

namespace TodoAPI.Chat
{
    public class ChatRoom : Hub
    {
        private readonly List<ChatUser> AuthUsers = new List<ChatUser>();
        private readonly IUserRepository userRepository;
        private readonly JwtSettings settings;
        public ChatRoom(IUserRepository userRepository, IOptions<JwtSettings> settings)
        {
            this.userRepository = userRepository;
            this.settings = settings.Value;

        }
        public override async Task OnConnected(string clientId)
        {
            await Clients.Client(clientId).SendMessageAsync("auth", "Authorization Required");
        }

        public override async Task OnDisconnected(string clientId)
        {
            var user = RemoveUser(clientId);
            await Clients.All.SendMessageAsync("left", new UserStatusResponse { Username = user.User.Username });
        }

        public override async Task MessageReceived(string clientId, string message)
        {
            await Clients
                    .Client(clientId)
                    .SendMessageAsync("message", new { Error = "Wrong message fromat" });
        }

        public async void OnAuth(string clientId, TokenRequest tr)
        {
            if (!JwtHelper.IsValidToken(tr.Token, settings)) {
                await Clients.Client(clientId).SendMessageAsync("auth", new { Erorr = "Unauthorized"});
                throw new Exception();
            } else {
                var id = ExtractUserIdFromToken(tr.Token);
                if (!id.HasValue) {
                    await Clients.Client(clientId).SendMessageAsync("auth", new { Error = "User Not Found"});
                    return;
                } 

                var user = await userRepository.FindById(id.Value);
                AuthUsers.Add(new ChatUser { ClientId = clientId, User = user } );
                await Clients.Client(clientId).SendMessageAsync("auth", new { Ok = $"{user.Username} Authorized"});
                await Clients
                    .AllExecpt(new List<string>() { clientId })
                    .SendMessageAsync("join", new UserStatusResponse { Username = user.Username });
            }
        }

        public async void OnMessage(string clientId, MessageRequest message)
        {
            var user = AuthUsers.FirstOrDefault(u => u.ClientId == clientId);
            if (user != null) {

                await Clients
                    .AllExecpt( new List<string>() { clientId })
                    .SendMessageAsync("message", new MessageResponse { Message = message.Message, From = user.User.Username });
            }
        }

        public async void OnStatus(string clientId) 
        {
            var users = AuthUsers.Select(u => u.User.Username);
            await Clients.Client(clientId).SendMessageAsync("status", new UserStatusesResponse { Users = users });
        }

        private ChatUser RemoveUser(string clientId) 
        {
            var usr = AuthUsers.Find(u => u.ClientId == clientId);
            AuthUsers.Remove(usr);
            return usr;
        }

        private Guid? ExtractUserIdFromToken(string token)
        {
            var claim = JwtHelper.ExtractClaims(token, settings).FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return null;
            }

            return Guid.Parse(claim.Value);
        }
    }

    public class MessageRequest
    {
        public string Message { get; set; }
    }

    public class MessageResponse 
    {
        public string Message { get; set; }
        public string From { get; set; }
    }

    public class UserStatusesResponse
    {
        public IEnumerable<string> Users { get; set; }
    }

    public class UserStatusResponse 
    {
        public string Username { get; set; }
    }
    public class TokenRequest
    {
        public string Token { get; set; }
    }

    public class SimpleMessageResponse 
    {
        public string Message { get; set; }
    }

    public class ChatUser 
    {
        public string ClientId { get; set; }
        public User User { get; set; }
    }
}