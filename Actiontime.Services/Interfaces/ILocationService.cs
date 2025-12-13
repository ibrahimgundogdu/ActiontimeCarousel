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
    public interface ILocationService
    {
        OurLocation? GetOurLocation();
        List<ProductPriceModel>? GetProductPrice();
        LocationSchedule? GetLocationSchedule(DateTime date);
        List<LocationSchedule>? GetLocationSchedules(DateTime date);
        OurLocationInfo GetOurLocationInfo();
        string CheckLocationShift(int employeeId);
        List<LocationPartModel> GetLiveParts();
        InspectionModel GetInspection(int employeeId);
        InspectionPartModel GetPartInspection(int inspectionId, int partId, int pageId, int employeeId);
        AppResult SavePartInspection(int inspectionId, int partId, int pageId, int employeeId, string answer, string? description);
        AppResult CloseInspection(int inspectionId, int employeeId, string? description);
        bool CheckInspection();
        void GetSync();
        string CompletePartRide(int partId, int employeeId);
    }
}
