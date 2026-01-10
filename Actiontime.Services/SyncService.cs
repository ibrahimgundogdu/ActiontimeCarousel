using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncService(ApplicationDbContext db, ApplicationCloudDbContext cdb, ICloudService cloudService, IServiceScopeFactory scopeFactory)
        {

            _db = db;
            _cdb = cdb;
            _cloudService = cloudService;
            _scopeFactory = scopeFactory;
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


                //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

                Task.Run(() =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                    // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                    var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                    if (persistedProcess != null)
                    {
                        scopedCloud.AddCloudProcess(persistedProcess);
                    }
                });

            }
            catch (Exception ex)
            {
            }
        }

    }
}
