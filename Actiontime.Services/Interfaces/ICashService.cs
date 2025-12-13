using Actiontime.Data.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface ICashService
    {
        List<CashDocumentType> CashDocumentList();
        AppResult AddDocument(string? FileName, int EmployeeId, DateTime DocDate, double Amount, string Description, int DocType, string FilePath);
        List<DayResultState> GetDayResultState();
        DayResultModel GetDayResult();
        List<PosActionModel> GetActions();
        AppResult UpdateDayResult(string? FileName, int resultId, int stateId, string Description, int EmployeeId, DateTime DocDate, string FilePath);
        string GetCashDrawer();

    }
}
