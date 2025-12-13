using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface ICloudService
    {
        void AddCloudProcess(SyncProcess process);
        //void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID);
        //void AddEmployeeAction(int? EmployeeID, int? LocationID, int? ActionTypeID, string ProcessName, long? ProcessID, DateTime? ProcessDate, string ProcessDetail, short? Direction, double? Collection, double? Payment, string Currency, int? SalaryTypeID, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID, string DocumentNumber, int SalaryCategoryID);
    }
}
