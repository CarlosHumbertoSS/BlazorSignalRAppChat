using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSignalRApp.Shared
{
    public interface IChatClient
    {
        Task ReceiveMessage(string User, string Message);

        Task UserConnected(string user);

        Task UserDesconnected(string user);
            
    }
}
