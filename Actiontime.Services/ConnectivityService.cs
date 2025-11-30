using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
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
        private readonly ApplicationDbContext _db; 
        public ConnectivityService() {
        
            _db = new ApplicationDbContext();
        }

        public OurLocation GetOurLocation()
        {
            return _db.OurLocations.FirstOrDefault();
        }



    }
}
