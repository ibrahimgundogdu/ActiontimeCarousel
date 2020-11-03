using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsLocation == true && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
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
                model.CashExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.UID == id);

                if (model.CashExpense != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashExpense.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashExpense.UpdateEmployee)?.FullName;

                    if (model.CashExpense.Date == documentDate)
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

                    return RedirectToAction("Expense");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("Expense");
            }

            model.Currencies = Db.Currency.ToList();
            model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsLocation == true && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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

            DateTime slipdate = expense.ReceiptDate.Add(expense.ReceiptTime.TimeOfDay);
            string SlipPath = "/Documents";
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (expense.ReceiptFile != null && expense.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(expense.ReceiptFile.FileName);

                string mappath = Server.MapPath(SlipPath);

                try
                {
                    //Kaydetme
                    expense.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Expense";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);

                    }
                }
                catch (Exception)
                {
                }
            }

            CashExpenseModel documentModel = new CashExpenseModel()
            {
                ActionTypeID = 4,
                ActionTypeName = "Expense",
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
                ProcessDate = processDate,
                ExpenseTypeID = expense.TypeID,
                SlipDate = slipdate,
                SlipNumber = expense.ReceiptNumber,
                SlipPath = @"\Document\Expense",
                SlipDocument = fileName
            };


            var result = documentManager.AddCashExpense(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Expense");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashExpense(FormCashExpense expense)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(expense.Amount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, expense.DocumentDate);
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());
            DateTime slipdate = expense.ReceiptDate.Add(expense.ReceiptTime.TimeOfDay);



            CashExpenseModel documentModel = new CashExpenseModel()
            {
                ActionTypeID = 4,
                ActionTypeName = "Expense",
                Amount = amount,
                Currency = expense.Currency,
                Description = expense.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                ToCustomerID = model.Authentication.CurrentOurCompany.ID == 1 ? 1 : 2,
                IsActive = expense.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = expense.UID,
                ProcessDate = processDate,
                ExpenseTypeID = expense.TypeID,
                SlipDate = slipdate,
                SlipNumber = expense.ReceiptNumber,
                SlipPath = @"\Document\Expense",
                SlipDocument = string.Empty
            };

            // dosya işlemleri yapılır
            if (expense.ReceiptFile != null && expense.ReceiptFile.ContentLength > 0)
            {
                documentModel.SlipDocument = Guid.NewGuid().ToString() + Path.GetExtension(expense.ReceiptFile.FileName);

                string mappath = Server.MapPath(documentModel.SlipPath);

                try
                {
                    //Kaydetme
                    expense.ReceiptFile.SaveAs(Path.Combine(mappath, documentModel.SlipDocument));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Expense";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, documentModel.SlipDocument);
                        string destFile = System.IO.Path.Combine(targetPath, documentModel.SlipDocument);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            var result = documentManager.UpdateCashExpense(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("ExpenseDetail", new { id = expense.UID });
        }




        [AllowAnonymous]
        public ActionResult ExchangeSell()
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.Currencies = Db.Currency.ToList();
            model.CashSaleExchanges = Db.DocumentSaleExchange.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ExchangeSellDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.CashSaleExchange = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == id);

                if (model.CashSaleExchange != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashSaleExchange.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashSaleExchange.UpdateEmployee)?.FullName;

                    if (model.CashSaleExchange.Date == documentDate)
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

                    return RedirectToAction("ExchangeSell");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("ExchangeSell");
            }

            model.Currencies = Db.Currency.ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashExchangeSell(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(exchange.Amount);
            var toAmount = LocationHelper.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = LocationHelper.GetStringToAmount(exchange.SaleExchangeRate);

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());


            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string SlipPath = "/Documents";
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath(SlipPath);

                try
                {
                    //Kaydetme
                    exchange.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Exchange";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashExchangeModel documentModel = new CashExchangeModel()
            {
                ActionTypeID = 25,
                ActionTypeName = "Döviz Satışı",
                Amount = amount,
                Currency = exchange.Currency,
                ToAmount = toAmount,
                ToCurrency = exchange.ToCurrency,
                SaleExchangeRate = SaleExchangeRate,
                Description = exchange.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = exchange.ReceiptNumber,
                SlipPath = @"\Document\Exchange",
                SlipDocument = fileName
            };


            var result = documentManager.AddCashSellExchange(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("ExchangeSell");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashExchangeSell(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(exchange.Amount);
            var toAmount = LocationHelper.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = LocationHelper.GetStringToAmount(exchange.SaleExchangeRate);

            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string SlipPath = "/Documents";
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath(SlipPath);

                try
                {
                    //Kaydetme
                    exchange.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Exchange";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashExchangeModel documentModel = new CashExchangeModel()
            {
                ActionTypeID = 25,
                ActionTypeName = "Döviz Satışı",
                Amount = amount,
                Currency = exchange.Currency,
                ToAmount = toAmount,
                ToCurrency = exchange.ToCurrency,
                SaleExchangeRate = SaleExchangeRate,
                Description = exchange.Description,
                DocumentDate = exchange.DocumentDate,
                IsActive = exchange.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                UID = exchange.UID,
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = exchange.ReceiptNumber,
                SlipPath = @"\Document\Exchange",
                SlipDocument = fileName

            };


            var result = documentManager.UpdateCashSellExchange(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("ExchangeSellDetail", new { id = exchange.UID });
        }




        [AllowAnonymous]
        public ActionResult ExchangeBuy()
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.Currencies = Db.Currency.ToList();
            model.CashBuyExchanges = Db.DocumentBuyExchange.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ExchangeBuyDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.CashBuyExchange = Db.DocumentBuyExchange.FirstOrDefault(x => x.UID == id);

                if (model.CashBuyExchange != null)
                {
                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashBuyExchange.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashBuyExchange.UpdateEmployee)?.FullName;

                    if (model.CashBuyExchange.Date == documentDate)
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

                    return RedirectToAction("ExchangeBuy");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("ExchangeBuy");
            }

            model.Currencies = Db.Currency.ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashExchangeBuy(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(exchange.Amount);
            var toAmount = LocationHelper.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = LocationHelper.GetStringToAmount(exchange.SaleExchangeRate);

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());


            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string SlipPath = "/Documents";
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath(SlipPath);

                try
                {
                    //Kaydetme
                    exchange.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Exchange";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashExchangeModel documentModel = new CashExchangeModel()
            {
                ActionTypeID = 40,
                ActionTypeName = "Döviz Alışı",
                Amount = amount,
                Currency = exchange.Currency,
                ToAmount = toAmount,
                ToCurrency = exchange.ToCurrency,
                SaleExchangeRate = SaleExchangeRate,
                Description = exchange.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = exchange.ReceiptNumber,
                SlipPath = @"\Document\Exchange",
                SlipDocument = fileName
            };


            var result = documentManager.AddCashBuyExchange(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("ExchangeBuy");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashExchangeBuy(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(exchange.Amount);
            var toAmount = LocationHelper.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = LocationHelper.GetStringToAmount(exchange.SaleExchangeRate);

            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string SlipPath = "/Documents";
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath(SlipPath);

                try
                {
                    //Kaydetme
                    exchange.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Exchange";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashExchangeModel documentModel = new CashExchangeModel()
            {
                ActionTypeID = 40,
                ActionTypeName = "Döviz Alışı",
                Amount = amount,
                Currency = exchange.Currency,
                ToAmount = toAmount,
                ToCurrency = exchange.ToCurrency,
                SaleExchangeRate = SaleExchangeRate,
                Description = exchange.Description,
                DocumentDate = exchange.DocumentDate,
                IsActive = exchange.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                UID = exchange.UID,
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = exchange.ReceiptNumber,
                SlipPath = @"\Document\Exchange",
                SlipDocument = fileName

            };


            var result = documentManager.UpdateCashBuyExchange(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("ExchangeBuyDetail", new { id = exchange.UID });
        }




        [AllowAnonymous]
        public ActionResult SalaryPay()
        {
            CashControlModel model = new CashControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            List<int> scheduledempids = Db.Schedule.Where(x => x.LocationID == model.Location.ID && x.ShiftDate == documentDate).Select(x => x.EmployeeID.Value).ToList();
            model.Employees = Db.Employee.Where(x => scheduledempids.Contains(x.EmployeeID)).ToList();

            model.Currencies = Db.Currency.ToList();
            model.SalaryPayments = Db.DocumentSalaryPayment.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).Select(x => new SalaryPayment()
            {
                Amount = x.Amount.Value,
                Currency = x.Currency,
                Date = x.Date.Value,
                EmployeeID = x.ToEmployeeID.Value,
                ID = x.ID,
                IsActive = x.IsActive.Value,
                UID = x.UID.Value,
                DocumentNumber = x.DocumentNumber
            }).ToList();

            foreach (var salary in model.SalaryPayments)
            {
                salary.EmployeeName = model.Employees.FirstOrDefault(y => y.EmployeeID == salary.EmployeeID).FullName ?? string.Empty;
            }

            return View(model);
        }

        //[AllowAnonymous]
        //public ActionResult SalaryPayDetail(Guid? id)
        //{
        //    CashControlModel model = new CashControlModel();

        //    var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

        //    if (TempData["Result"] != null)
        //    {
        //        model.Result = TempData["Result"] as Result;
        //    }

        //    if (id != null)
        //    {
        //        model.CashSaleExchange = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == id);

        //        if (model.CashSaleExchange != null)
        //        {
        //            model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashSaleExchange.RecordEmployeeID)?.FullName;
        //            model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.CashSaleExchange.UpdateEmployee)?.FullName;

        //            if (model.CashSaleExchange.Date == documentDate)
        //            {
        //                model.IsUpdatible = true;
        //            }
        //            else
        //            {
        //                model.Result.Message = "Doküman bugüne ait değildir. Merkezden güncellenebilir.";
        //                TempData["Result"] = model.Result;
        //            }
        //        }
        //        else
        //        {
        //            model.Result.Message = "Doküman bilgisine ulaşılamadı.";
        //            TempData["Result"] = model.Result;

        //            return RedirectToAction("ExchangeSell");
        //        }
        //    }
        //    else
        //    {
        //        model.Result.Message = "Doküman bilgisi yok.";
        //        TempData["Result"] = model.Result;

        //        return RedirectToAction("ExchangeSell");
        //    }

        //    model.Currencies = Db.Currency.ToList();
        //    model.Exchange = ServiceHelper.GetExchange(documentDate);

        //    return View(model);
        //}

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashSalaryPay(FormSalaryPay salary)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(salary.Amount);

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());
            var cash = ServiceHelper.GetCash(model.Location.ID, salary.Currency);

            SalaryPaymentModel documentModel = new SalaryPaymentModel()
            {
                ActionTypeID = 31,
                ActionTypeName = "Maaş Avans Ödemesi",
                Amount = amount,
                Currency = salary.Currency,
                Description = salary.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate,
                CategoryID = 8,
                EmployeeID = salary.EmployeeID,
                SalaryTypeID = 2,
                FromCashID = cash.ID
            };


            var result = documentManager.AddSalaryPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("SalaryPay");
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult UpdateCashSalaryPay(FormSalaryPay salary)
        //{
        //    CashControlModel model = new CashControlModel();

        //    var amount = LocationHelper.GetStringToAmount(exchange.Amount);
        //    var toAmount = LocationHelper.GetStringToAmount(exchange.ToAmount);
        //    var SaleExchangeRate = LocationHelper.GetStringToAmount(exchange.SaleExchangeRate);

        //    var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

        //    DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
        //    string SlipPath = "/Documents";
        //    string fileName = string.Empty;

        //    // dosya işlemleri yapılır
        //    if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
        //    {
        //        fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

        //        string mappath = Server.MapPath(SlipPath);

        //        try
        //        {
        //            //Kaydetme
        //            exchange.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

        //            //Kopyalama
        //            if (!Request.IsLocal)
        //            {
        //                string targetPath = @"C:\inetpub\wwwroot\Action\Document\Exchange";
        //                string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
        //                string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
        //                string destFile = System.IO.Path.Combine(targetPath, fileName);
        //                System.IO.File.Copy(sourceFile, destFile, true);
        //                System.IO.File.Delete(sourceFile);
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //    CashExchangeModel documentModel = new CashExchangeModel()
        //    {
        //        ActionTypeID = 25,
        //        ActionTypeName = "Döviz Satışı",
        //        Amount = amount,
        //        Currency = exchange.Currency,
        //        ToAmount = toAmount,
        //        ToCurrency = exchange.ToCurrency,
        //        SaleExchangeRate = SaleExchangeRate,
        //        Description = exchange.Description,
        //        DocumentDate = exchange.DocumentDate,
        //        IsActive = exchange.IsActive == 1 ? true : false,
        //        LocationID = model.Location.ID,
        //        UID = exchange.UID,
        //        ProcessDate = processDate,
        //        SlipDate = slipdate,
        //        SlipNumber = exchange.ReceiptNumber,
        //        SlipPath = @"\Document\Exchange",
        //        SlipDocument = fileName

        //    };


        //    var result = documentManager.UpdateCashSellExchange(documentModel);

        //    TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

        //    return RedirectToAction("ExchangeSellDetail", new { id = exchange.UID });
        //}

    }
}