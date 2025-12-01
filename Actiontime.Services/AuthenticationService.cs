using Actiontime.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class AppAuthenticationService
    {
        private readonly ApplicationDbContext _db; 
        
        public AppAuthenticationService(ApplicationDbContext db)
        {
            _db = db;
        }



        public Data.Entities.Employee LoginAsync(string username, string password)
        {
            var passwordmd5 = ServiceHelper.MD5Hash(password);
            return _db.Employees.FirstOrDefault(x => x.Username == username && x.Password == passwordmd5);
        }
    }
}
