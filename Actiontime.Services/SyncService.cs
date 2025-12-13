using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class SyncService : ISyncService
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        private readonly ICloudService _cloudService;

        public SyncService(ApplicationDbContext db, ApplicationCloudDbContext cdb, ICloudService cloudService)
        {

            _db = db;
            _cdb = cdb;
            _cloudService = cloudService;
        }
       

        public void AddQuee(string EntityName, short Process, long Id, Guid? Uid)
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

                _db.SyncProcesses.Add(process);
                _db.SaveChanges();


                Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

            }
            catch (Exception ex)
            {
            }
        }

    }
}
