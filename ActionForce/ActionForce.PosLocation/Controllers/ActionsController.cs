using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class ActionsController : BaseController
    {
        public ActionResult Index(string id)
        {
            ActionsControlModel model = new ActionsControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;
            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out DocumentDate);
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.DocumentDate = DocumentDate;

            model.ActionList = Db.VCashBankActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ActionDate == DocumentDate).OrderBy(x => x.ActionDate).ToList();


            //var balanceData = Db.VCashBankActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ActionDate < DocumentDate).ToList();
            //if (balanceData != null && balanceData.Count > 0)
            //{

            //}

            List<TotalModel> headerTotals = new List<TotalModel>();

            headerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Cash",
                Total = 0
            });

            headerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = 0
            });

            model.HeaderTotals = headerTotals;


            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 
            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "TRL").Total
            });

            model.FooterTotals = footerTotals;

            return View(model);
        }
    }
}