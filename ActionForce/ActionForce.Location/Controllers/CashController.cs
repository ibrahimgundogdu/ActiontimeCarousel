using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class CashController : BaseController
    {
        private readonly DocumentManager documentManager;
        public CashController()
        {
            LayoutControlModel model = new LayoutControlModel();

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               LocationHelper.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentOurCompany.ID,
                   Name = model.Authentication.CurrentOurCompany.Name,
                   Currency = model.Authentication.CurrentOurCompany.Currency,
                   TimeZone = model.Authentication.CurrentOurCompany.TimeZone
               }
           );
        }


        [AllowAnonymous]
        public ActionResult Index(string id)
        {
            CashControlModel model = new CashControlModel();

            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);

            DateTime selectedDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out selectedDate);
            }

            model.SelectedDate = selectedDate;
            model.Summary = manager.GetLocationSummary(model.SelectedDate, model.Authentication.CurrentEmployee);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Collect()
        {
            CashControlModel model = new CashControlModel();
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Currencies = Db.Currency.ToList();
            model.CurrentCash = LocationHelper.GetCash(model.Location.ID, model.Location.Currency);
            model.CashCollections = Db.DocumentCashCollections.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult CollectDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.CashCollection = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == id);

                if (model.CashCollection != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashCollection.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashCollection.UpdateEmployee)?.FullName;

                    if (model.CashCollection.Date == documentDate)
                    {
                        model.IsUpdatible = true;
                    }
                    else
                    {
                        model.Result.Message = "Doküman bugüne ait değildir. Merkezden güncellenebilir.";
                        TempData["Result"] = model.Result;
                    }

                }
                else
                {
                    model.Result.Message = "Doküman bilgisine ulaşılamadı.";
                    TempData["Result"] = model.Result;

                    return RedirectToAction("Collect");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("Collect");
            }

            model.Currencies = Db.Currency.ToList();


            return View(model);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashCollection(FormCashCollect collect)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(collect.Amount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            CashCollectionModel documentModel = new CashCollectionModel()
            {
                ActionTypeID = 23,
                ActionTypeName = "Kasa Tahsilatı",
                Amount = amount,
                Currency = collect.Currency,
                Description = collect.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                FromCustomerID = model.Authentication.CurrentOurCompany.ID == 1 ? 1 : 2,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID
            };


            var result = documentManager.AddCashCollection(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Collect");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCollect(FormCashCollect collect)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(collect.Amount);
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            CashCollectionModel documentModel = new CashCollectionModel()
            {
                Amount = amount,
                Currency = collect.Currency,
                Description = collect.Description,
                DocumentDate = collect.DocumentDate,
                ProcessDate = processDate,
                IsActive = collect.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                UID = collect.UID
            };


            var result = documentManager.UpdateCashCollection(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("CollectDetail", new { id = collect.UID });
        }


        [AllowAnonymous]
        public ActionResult Payment()
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Currencies = Db.Currency.ToList();
            model.CurrentCash = LocationHelper.GetCash(model.Location.ID, model.Location.Currency);
            model.CashPayments = Db.DocumentCashPayments.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PaymentDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.CashPayment = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == id);

                if (model.CashPayment != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashPayment.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashPayment.UpdateEmployee)?.FullName;

                    if (model.CashPayment.Date == documentDate)
                    {
                        model.IsUpdatible = true;
                    }
                    else
                    {
                        model.Result.Message = "Doküman bugüne ait değildir. Merkezden güncellenebilir.";
                        TempData["Result"] = model.Result;
                    }

                }
                else
                {
                    model.Result.Message = "Doküman bilgisine ulaşılamadı.";
                    TempData["Result"] = model.Result;

                    return RedirectToAction("Payment");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("Payment");
            }

            model.Currencies = Db.Currency.ToList();


            return View(model);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashPayment(FormCashPayment payment)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(payment.Amount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                ActionTypeID = 27,
                ActionTypeName = "Kasa Ödemesi",
                Amount = amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                ToCustomerID = model.Authentication.CurrentOurCompany.ID == 1 ? 1 : 2,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate
            };


            var result = documentManager.AddCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Payment");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashPayment(FormCashPayment payment)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(payment.Amount);
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                Amount = amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DocumentDate = payment.DocumentDate,
                ProcessDate = processDate,
                IsActive = payment.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                UID = payment.UID
            };
            


            var result = documentManager.UpdateCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("PaymentDetail", new { id = payment.UID });
        }


        [AllowAnonymous]
        public ActionResult Expense()
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsLocation == true && x.IsActive == true).OrderBy(x=> x.SortBy).ToList();
            model.Currencies = Db.Currency.ToList();
            model.CashExpenses = Db.DocumentCashExpense.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ExpenseDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.CashPayment = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == id);

                if (model.CashPayment != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashPayment.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashPayment.UpdateEmployee)?.FullName;

                    if (model.CashPayment.Date == documentDate)
                    {
                        model.IsUpdatible = true;
                    }
                    else
                    {
                        model.Result.Message = "Doküman bugüne ait değildir. Merkezden güncellenebilir.";
                        TempData["Result"] = model.Result;
                    }

                }
                else
                {
                    model.Result.Message = "Doküman bilgisine ulaşılamadı.";
                    TempData["Result"] = model.Result;

                    return RedirectToAction("Payment");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("Payment");
            }

            model.Currencies = Db.Currency.ToList();


            return View(model);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashExpense(FormCashExpense expense)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(expense.Amount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                ActionTypeID = 27,
                ActionTypeName = "Kasa Ödemesi",
                Amount = amount,
                Currency = expense.Currency,
                Description = expense.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                ToCustomerID = model.Authentication.CurrentOurCompany.ID == 1 ? 1 : 2,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate
            };


            var result = documentManager.AddCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Payment");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashExpense(FormCashPayment payment)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(payment.Amount);
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                Amount = amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DocumentDate = payment.DocumentDate,
                ProcessDate = processDate,
                IsActive = payment.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                UID = payment.UID
            };



            var result = documentManager.UpdateCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("PaymentDetail", new { id = payment.UID });
        }

    }
}