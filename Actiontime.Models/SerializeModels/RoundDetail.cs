using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class RoundDetail
    {
        public TripRound? Round { get; set; }
        public List<TripConfirm>? ConfirmList { get; set; }
    }
}
