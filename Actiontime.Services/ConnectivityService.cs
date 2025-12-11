using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class ConnectivityService
    {

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public ConnectivityService(IDbContextFactory<ApplicationDbContext> dbFactory) {

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public OurLocation GetOurLocation()
        {
            using var _db = _dbFactory.CreateDbContext();
            return _db.OurLocations.FirstOrDefault();
        }



    }
}
