using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TodoAPI.Config;
using TodoAPI.Core;
using TodoAPI.Helpers;
using TodoAPI.Middleware;

namespace TodoAPI.Chat
{
    public class ChatRoom : Hub
    {
        private readonly List<User> AuthUsers = new List<User>();
        private readonly IUserRepository userRepository;
        private readonly JwtSettings settings;
        public ChatRoom(IUserRepository userRepository, JwtSettings settings)
        {
            this.userRepository = userRepository;
            this.settings = settings;

        }
        public override async Task OnConnected(string clientId)
        {
            // var list = new List<string>() { clientId };
            // await Clients.AllExecpt(list).SendMessageAsync("new", new MessageResponse{ Message = $"{clientId} joined!"});
            // await Clients.Client(clientId).SendMessageAsync("greeting", $"Hello {clientId}");

            await Clients.Client(clientId).SendMessageAsync("auth", "Authorization Required");
        }

        public override async Task OnDisconnected(string clientId)
        {
            RemoveUser(clientId);
            await Clients.All.SendMessageAsync("bye", new MessageResponse{ Message = $"{clientId} left us" });
        }

        public override async Task MessageReceived(string clientId, string message)
        {
            await Clients
                    .AllExecpt(
                        new List<string>() { clientId }
                    )
                    .SendMessageAsync("message", new MessageResponse{ Message = $"{clientId}: {message}" });
        }

        public void OnAuth(string clientId, TokenRequest tr)
        {
            if (!JwtHelper.IsValidToken(tr.Token, settings)) {
                Clients.Client(clientId).SendMessageAsync("auth", new { Erorr = "Unauthorized"});
                throw new Exception();
            } else {
                // Authusers.Add(clientId);
                Clients.Client(clientId).SendMessageAsync("auth", new { Ok = "Authorized"});
            }
            
        }

        public void OnMessage(string clientId, Message message)
        {
            // if (Authusers.Contains(clientId)) {

            //     Clients
            //         .AllExecpt( new List<string>() { clientId })
            //         .SendMessageAsync("message", new Message { Text = message.Text, From = message.From});

            // }
        }

        private void RemoveUser(string clientId) 
        {
            var usr = AuthUsers.Find(u => u.ClientId == clientId);
            AuthUsers.Remove(usr);
        }
    }

    public class Message
    {
        public string Text { get; set; }
        public string From { get; set; }
    }

    public class TokenRequest
    {
        public string Token { get; set; }
    }

    public class MessageResponse 
    {
        public string Message { get; set; }
    }

    public class User 
    {
        public string ClientId { get; set; }
        public string Nickname { get; set; }
    }
}