using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface ISyncService
    {
        void AddQuee(string EntityName, short Process, long Id, Guid? Uid);
    }
}
