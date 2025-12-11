using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class SyncService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;
        private readonly CloudService _cloudService;

        public SyncService(IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextFactory<ApplicationCloudDbContext> cdbFactory, CloudService cloudService)
        {

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _cdbFactory = cdbFactory ?? throw new ArgumentNullException(nameof(cdbFactory));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
        }
       

        public void AddQuee(string EntityName, short Process, long Id, Guid? Uid)
        {
            using var _db = _dbFactory.CreateDbContext();

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
