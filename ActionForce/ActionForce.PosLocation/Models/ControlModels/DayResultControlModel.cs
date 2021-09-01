using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class DayResultControlModel : LayoutControlModel
    {
        public DateTime DocumentDate { get; set; }
        public DateTime ProcessDate { get; set; }
        public IEnumerable<VEmployeeCashActions> EmployeeActions { get; set; }
        public IEnumerable<Service.EmployeeShiftModel> EmployeeShifts { get; set; }
        public IEnumerable<LocationTicketSaleInfo> TicketList { get; set; }
        public IEnumerable<VPriceLastList> PriceList { get; set; }
        public IEnumerable<DocumentCashRecorderSlip> CashRecordSlip { get; set; }
        public IEnumerable<DayResultDocuments> ResultDocuments { get; set; }
        public IEnumerable<ResultState> ResultStates { get; set; }
        public LocationBalance LocationBalance { get; set; }
        public SummaryControlModel Summary { get; set; }
        public DocumentCashRecorderSlip CashRecorderSlip { get; set; }
        public DayResult CurrentDayResult { get; set; }
        public DayResultDocuments ResultDocument { get; set; }

        public LocationScheduleInfo Schedule { get; set; }
        public LocationShiftInfo Shift { get; set; }
    }
}