using Actiontime.Models.ResultModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface IQRReaderService
    {
        Task<ReaderResult> AddReader(string message);
        Task<ConfirmResult> ConfirmQR(string message);
        Task<StartResult> StartQR(string message);
        Task<CompleteResult> CompleteQR(string message);
        Task<WebSocketResult> CloudRideStartStop(string confirmNumber);
        Task<DrawerResult> AddDrawer(string message);
    }
}
