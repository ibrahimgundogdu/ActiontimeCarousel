using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ActionController : BaseController
    {
        // GET: Actions
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.LocationID = locationId != null ? locationId : Db.Location.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).LocationID;
                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            
            model.ActionList = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate >= model.Filters.DateBegin && x.ActionDate <= model.Filters.DateEnd).OrderBy(x => x.ActionDate).ToList();
            
            var balanceData = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate < model.Filters.DateBegin).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
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

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Cash",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Cash",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }


            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 
            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x=> x.Type == "Cash" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "USD").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "USD").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "EUR").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "EUR").Total
            });

            model.FooterTotals = footerTotals;


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Index", "Action");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterTransfer(int? locationId, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Transfers", "Action");
        }

        [AllowAnonymous]
        public ActionResult Transfers()
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Transfers = Db.VDocumentTransfer.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.DocumentDate >= model.Filters.DateBegin && x.DocumentDate <= model.Filters.DateEnd).OrderBy(x => x.DocumentDate).ToList();

            if (model.CurrentLocation != null)
            {
                model.Transfers = model.Transfers.Where(x => x.FromLocationID == model.CurrentLocation.LocationID).OrderBy(x => x.DocumentDate).ToList();
            }
            return View(model);
        }
    }
}