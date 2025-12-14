using Actiontime.Models.ResultModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface IWebSocketService
    {
        Task SendWebSocketMessage(
            WebSocketResult result,
            CancellationToken cancellationToken = default
        );
    }
}
