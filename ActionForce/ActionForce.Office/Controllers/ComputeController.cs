using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ComputeController : BaseController
    {
        // GET: Compute
        public ActionResult Index()
        {
            ComputeControlModel model = new ComputeControlModel();
            DateTime datenow = DateTime.UtcNow.AddHours(3).Date;
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.WeekLists = Db.DateList.Where(x => x.WeekYear >= 2017 && x.WeekYear <= DateTime.Now.Year && x.DateKey <= datenow).Select(x => new WeekModel()
            {
                WeekKey = x.WeekYear+"-"+x.WeekNumber,
                WeekYear = x.WeekYear.Value,
                WeekNumber = x.WeekNumber.Value
            }).Distinct().OrderByDescending(x=> x.WeekKey).ToList();

            return View(model);
        }
    }
}