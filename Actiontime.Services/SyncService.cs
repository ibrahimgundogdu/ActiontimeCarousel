using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class SyncService
    {
        public SyncService()
        {
        }

        public static void AddQuee(string EntityName, short Process, long Id, Guid? Uid)
        {
            try
            {
                SyncProcess process = new SyncProcess()
                {
                    DateCreate = DateTime.Now,
                    Entity = EntityName,
                    Process = Process,
                    EntityId = Id,
                    EntityUid = Uid
                };

                using (ApplicationDbContext _db = new ApplicationDbContext())
                {
                    _db.SyncProcesses.Add(process);
                    _db.SaveChanges();
                }

                CloudService _cloudService = new CloudService();

                Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

            }
            catch (Exception ex)
            {
            }
        }

    }
}
