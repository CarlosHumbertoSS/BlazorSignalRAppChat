using BlazorSignalRApp.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSignalRApp.Server.Hubs
{
    public class ChatHub : Hub<BlazorSignalRApp.Shared.IChatClient>
    {
        const string IdentifiedUsers = "Identified Users";

        static ConcurrentDictionary<string, ChatUser> Users = new ConcurrentDictionary<string, ChatUser>();

        public async Task SendMessage(string message)
        {
            //Invocador de Clientes Hub IHubCallerClient
            if(GetCurrentChatUser() is ChatUser ChatUser)
            {
                await Clients.Groups(IdentifiedUsers).ReceiveMessage(ChatUser.NickName, message);
            }  
        }

        public List<string> GetUser()
        {
            return Users.Select(d => d.Value.NickName).ToList();
        }

        public async Task<bool> SingIn(string nickName)
        {
            bool Result = false;
            if (Users.TryAdd(nickName, new ChatUser(nickName, Context.ConnectionId))) 
            {

                //SignalR maneja grupos.
                await Groups.AddToGroupAsync(Context.ConnectionId, IdentifiedUsers);
                Result = true;
                await Clients.GroupExcept(IdentifiedUsers, Context.ConnectionId).UserConnected(nickName);
            };
            return Result;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {   
            if(GetCurrentChatUser() is ChatUser chatUser)
            {
                Users.TryRemove(chatUser.NickName, out var value);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, IdentifiedUsers);
                await Clients.Group(IdentifiedUsers).UserDesconnected(chatUser.NickName);
            }
            await base.OnDisconnectedAsync(exception);
        }

        ChatUser GetCurrentChatUser()
        {
            return Users.Where(u => u.Value.ConnectionId == Context.ConnectionId)
                .Select(u => u.Value).FirstOrDefault();
        }
    }
}
