using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;


[assembly: OwinStartup(typeof(ActionForce.WebSocket.Startup))]
namespace ActionForce.WebSocket
{
    
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }

       
    }
}
