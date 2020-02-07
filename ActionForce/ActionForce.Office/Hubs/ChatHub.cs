using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ActionForce.Office
{
    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {

            return base.OnDisconnected(stopCalled);
        }

        public void SendMessage(string name, string message)
        {
            Clients.All.AddMessageToPage(name, message);
        }
    }
}