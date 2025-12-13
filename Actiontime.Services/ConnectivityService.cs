using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services.Interfaces;
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
    public class ConnectivityService : IConnectivityService
    {

        private readonly ApplicationDbContext _db;
        public ConnectivityService(ApplicationDbContext db) {

            _db = db;
        }

        public OurLocation GetOurLocation()
        {
            return _db.OurLocations.FirstOrDefault();
        }



    }
}
