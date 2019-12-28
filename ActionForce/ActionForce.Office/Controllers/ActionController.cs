using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

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
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "TRL").Total
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
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Transfers = Db.VDocumentTransfer.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.DocumentDate >= model.Filters.DateBegin && x.DocumentDate <= model.Filters.DateEnd).OrderBy(x => x.DocumentDate).ToList();

            if (model.CurrentLocation != null)
            {
                model.Transfers = model.Transfers.Where(x => x.FromLocationID == model.CurrentLocation.LocationID).OrderBy(x => x.DocumentDate).ToList();
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddTransfer()
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["result"] != null)
            {
                if (TempData["result"] != null)
                {
                    model.Result = TempData["result"] as Result<DocumentTransfer> ?? null;
                }
            }


            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            List<int> locationids = model.LocationList.Select(z => z.LocationID).ToList();
            model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrencyList = Db.Currency.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewTransfer(newTransfer transfer)
        {
            Result<DocumentTransfer> result = new Result<DocumentTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ActionControlModel model = new ActionControlModel();
            var amount = Convert.ToDouble(transfer.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var date = Convert.ToDateTime(transfer.DocumentDate);
            string uid = string.Empty;

            if (transfer != null && amount >0)
            {
                DocumentManager manager = new DocumentManager();

                TransferModel transfermodel = new TransferModel();

                transfermodel.Amount = amount;
                transfermodel.CarrierEmployeeID = transfer.CarrierEmployeeID;
                transfermodel.Currency = transfer.Currency;
                transfermodel.Description = transfer.Description;
                transfermodel.DocumentDate = date;
                transfermodel.FromBankID = transfer.FromBankID;
                transfermodel.FromCashID = transfer.FromCashID;
                transfermodel.FromCustID = transfer.FromCustID;
                transfermodel.FromEmplID = transfer.FromEmplID;
                transfermodel.FromLocationID = transfer.FromLocationID;
                transfermodel.ToBankID = transfer.ToBankID;
                transfermodel.ToCashID = transfer.ToCashID;
                transfermodel.ToCustID = transfer.ToCustID;
                transfermodel.ToEmplID = transfer.ToEmplID;
                transfermodel.ToLocationID = transfer.ToLocationID;
                transfermodel.UID = Guid.NewGuid();

                uid = transfermodel.UID.ToString();

                result = manager.AddTransfer(transfermodel, model.Authentication);
            }
            else
            {
                result.Message = "Tutar 0 dan büyük olduğuna emin olun";
            }

            TempData["result"] = result;

            if (result.IsSuccess == true)
            {
                return RedirectToAction("Transfer", new { id = uid });
            }
            else
            {
                return RedirectToAction("AddTransfer");
            }
            
        }









        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationCashSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CashList = Db.VCash.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialSelectLocationCash", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationBankSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationBank", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationEmployeeSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationEmployee", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationCustomerSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CustomerList = Db.Customer.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationCustomer", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationCashSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CashList = Db.VCash.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialSelectTLocationCash", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationBankSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationBank", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationEmployeeSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationEmployee", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationCustomerSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CustomerList = Db.Customer.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationCustomer", model);
        }









    }
}