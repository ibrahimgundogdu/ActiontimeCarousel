using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class CashController : BaseController
    {
        private DocumentManager documentManager;

        public CashController()
        {

        }

        public ActionResult Index(string id)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            PosManager manager = new PosManager();

            DateTime selectedDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out selectedDate);
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);

            model.LocationDate = location.LocalDate ?? DateTime.UtcNow.AddHours(3).Date;
            model.SelectedDate = selectedDate;
            model.Summary = manager.GetLocationSummary(model.SelectedDate, model.Authentication.CurrentEmployee, location);

            return View(model);
        }

        public ActionResult CashCollect()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            PosManager manager = new PosManager();

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Currencies = Db.Currency.ToList();
            model.CurrentCash = PosManager.GetCash(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentLocation.Currency);
            model.CashCollections = Db.DocumentCashCollections.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        public ActionResult NewCashCollect()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            model.Exchange = ServiceHelper.GetExchange(documentDate);

            model.Currencies = Db.Currency.ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCashCollect(FormCashCollect collect)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var amount = PosManager.GetStringToAmount(collect.Amount);
            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());

            CashCollectionModel documentModel = new CashCollectionModel()
            {
                ActionTypeID = 23,
                ActionTypeName = "Kasa Tahsilatı",
                Amount = amount,
                Currency = collect.Currency,
                Description = collect.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                FromCustomerID = 2,
                IsActive = true,
                LocationID = model.Authentication.CurrentLocation.ID,
                ResultID = dayResultID
            };


            var result = documentManager.AddCashCollection(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("CashCollect");
        }

        public ActionResult CollectDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

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

            model.Exchange = ServiceHelper.GetExchange(documentDate);


            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateCashCollect(FormCashCollect collect)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var amount = PosManager.GetStringToAmount(collect.Amount);
            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            CashCollectionModel documentModel = new CashCollectionModel()
            {
                Amount = amount,
                Currency = collect.Currency,
                Description = collect.Description,
                DocumentDate = collect.DocumentDate,
                ProcessDate = processDate,
                IsActive = collect.IsActive == 1 ? true : false,
                LocationID = model.Authentication.CurrentLocation.ID,
                UID = collect.UID
            };


            var result = documentManager.UpdateCashCollection(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("CollectDetail", new { id = collect.UID });
        }

        [AllowAnonymous]
        public ActionResult CashPayment()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Currencies = Db.Currency.ToList();
            model.CurrentCash = PosManager.GetCash(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentLocation.Currency);
            model.CashPayments = Db.DocumentCashPayments.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        public ActionResult NewCashPayment()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            model.Exchange = ServiceHelper.GetExchange(documentDate);

            model.Currencies = Db.Currency.ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCashPayment(FormCashPayment payment)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
              new ProcessEmployee()
              {
                  ID = model.Authentication.CurrentEmployee.EmployeeID,
                  FullName = model.Authentication.CurrentEmployee.FullName
              },
              PosManager.GetIPAddress(),
              new ProcessCompany()
              {
                  ID = 2,
                  Name = "UFE GRUP",
                  Currency = "TRL",
                  TimeZone = 3
              }
          );

            var amount = PosManager.GetStringToAmount(payment.Amount);
            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                ActionTypeID = 27,
                ActionTypeName = "Kasa Ödemesi",
                Amount = amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                ToCustomerID = 2,
                IsActive = true,
                LocationID = model.Authentication.CurrentLocation.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate
            };

            var result = documentManager.AddCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("CashPayment");
        }


        public ActionResult PaymentDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));


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

                    return RedirectToAction("CashPayment");
                }
            }
            else
            {
                model.Result.Message = "Doküman bilgisi yok.";
                TempData["Result"] = model.Result;

                return RedirectToAction("CashPayment");
            }

            model.Currencies = Db.Currency.ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }


        [HttpPost]
        public ActionResult UpdateCashPayment(FormCashPayment payment)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
             new ProcessEmployee()
             {
                 ID = model.Authentication.CurrentEmployee.EmployeeID,
                 FullName = model.Authentication.CurrentEmployee.FullName
             },
             PosManager.GetIPAddress(),
             new ProcessCompany()
             {
                 ID = 2,
                 Name = "UFE GRUP",
                 Currency = "TRL",
                 TimeZone = 3
             }
         );

            var amount = PosManager.GetStringToAmount(payment.Amount);
            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            CashPaymentModel documentModel = new CashPaymentModel()
            {
                Amount = amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DocumentDate = payment.DocumentDate,
                ProcessDate = processDate,
                IsActive = payment.IsActive == 1 ? true : false,
                LocationID = model.Authentication.CurrentLocation.ID,
                UID = payment.UID
            };



            var result = documentManager.UpdateCashPayment(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("PaymentDetail", new { id = payment.UID });
        }

        //Expense


        public ActionResult Expense()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));


            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsLocation == true && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.Currencies = Db.Currency.ToList();
            model.CashExpenses = Db.DocumentCashExpense.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        public ActionResult NewExpense()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            model.Exchange = ServiceHelper.GetExchange(documentDate);
            model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsLocation == true && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.Currencies = Db.Currency.ToList();

            return View(model);
        }


        public ActionResult ExpenseDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

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
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCashExpense(FormCashExpense expense)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
            new ProcessEmployee()
            {
                ID = model.Authentication.CurrentEmployee.EmployeeID,
                FullName = model.Authentication.CurrentEmployee.FullName
            },
            PosManager.GetIPAddress(),
            new ProcessCompany()
            {
                ID = 2,
                Name = "UFE GRUP",
                Currency = "TRL",
                TimeZone = 3
            }
            );

            var amount = PosManager.GetStringToAmount(expense.Amount);
            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());

            DateTime slipdate = expense.ReceiptDate.Add(expense.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (expense.ReceiptFile != null && expense.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(expense.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

                try
                {
                    //Kaydetme
                    expense.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Expense";
                        string sourcePath = @"C:\inetpub\wwwroot\locationpos\Documents";
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
                ToCustomerID = 2,
                IsActive = true,
                LocationID = model.Authentication.CurrentLocation.ID,
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

        [HttpPost]
        public ActionResult UpdateCashExpense(FormCashExpense expense)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var amount = PosManager.GetStringToAmount(expense.Amount);
            var documentDate = expense.DocumentDate;
            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());
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
                ToCustomerID = 2,
                IsActive = expense.IsActive == 1 ? true : false,
                LocationID = model.Authentication.CurrentLocation.ID,
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

                string mappath = Server.MapPath("/Documents");

                try
                {
                    //Kaydetme
                    expense.ReceiptFile.SaveAs(Path.Combine(mappath, documentModel.SlipDocument));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Expense";
                        string sourcePath = @"C:\inetpub\wwwroot\locationpos\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, documentModel.SlipDocument);
                        string destFile = System.IO.Path.Combine(targetPath, documentModel.SlipDocument);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        //System.IO.File.Delete(sourceFile);
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

        //ExchangeSell

        [AllowAnonymous]
        public ActionResult ExchangeSell()
        {
            CashControlModel model = new CashControlModel();

            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.Currencies = Db.Currency.ToList();
            model.CashSaleExchanges = Db.DocumentSaleExchange.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        //NewExchangeSell
        public ActionResult NewExchangeSell()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            model.Exchange = ServiceHelper.GetExchange(documentDate);
            model.Currencies = Db.Currency.ToList();

            return View(model);
        }


        public ActionResult ExchangeSellDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

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

        [HttpPost]
        public ActionResult AddCashExchangeSell(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
            new ProcessEmployee()
            {
                ID = model.Authentication.CurrentEmployee.EmployeeID,
                FullName = model.Authentication.CurrentEmployee.FullName
            },
            PosManager.GetIPAddress(),
            new ProcessCompany()
            {
                ID = 2,
                Name = "UFE GRUP",
                Currency = "TRL",
                TimeZone = 3
            }
            );

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));


            var amount = PosManager.GetStringToAmount(exchange.Amount);
            var toAmount = PosManager.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = PosManager.GetStringToAmount(exchange.SaleExchangeRate.Replace(".",","));

            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());


            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

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
                        //System.IO.File.Delete(sourceFile);
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
                LocationID = model.Authentication.CurrentLocation.ID,
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

        [HttpPost]
        public ActionResult UpdateCashExchangeSell(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
            new ProcessEmployee()
            {
                ID = model.Authentication.CurrentEmployee.EmployeeID,
                FullName = model.Authentication.CurrentEmployee.FullName
            },
            PosManager.GetIPAddress(),
            new ProcessCompany()
            {
                ID = 2,
                Name = "UFE GRUP",
                Currency = "TRL",
                TimeZone = 3
            }
            );

            var amount = PosManager.GetStringToAmount(exchange.Amount);
            var toAmount = PosManager.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = PosManager.GetStringToAmount(exchange.SaleExchangeRate);

            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

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
                        //System.IO.File.Delete(sourceFile);
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
                LocationID = model.Authentication.CurrentLocation.ID,
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

        //ExchangeBuy
        //NewExchangeSell

        public ActionResult ExchangeBuy()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.ReceiptDate = documentDate;
            model.Currencies = Db.Currency.ToList();
            model.CashBuyExchanges = Db.DocumentBuyExchange.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.Exchange = ServiceHelper.GetExchange(documentDate);

            return View(model);
        }

        public ActionResult NewExchangeBuy()
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            model.Exchange = ServiceHelper.GetExchange(documentDate);
            model.Currencies = Db.Currency.ToList();

            return View(model);
        }

        public ActionResult ExchangeBuyDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

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

        [HttpPost]
        public ActionResult AddCashExchangeBuy(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
           new ProcessEmployee()
           {
               ID = model.Authentication.CurrentEmployee.EmployeeID,
               FullName = model.Authentication.CurrentEmployee.FullName
           },
           PosManager.GetIPAddress(),
           new ProcessCompany()
           {
               ID = 2,
               Name = "UFE GRUP",
               Currency = "TRL",
               TimeZone = 3
           }
           );

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone));

            var amount = PosManager.GetStringToAmount(exchange.Amount);
            var toAmount = PosManager.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = PosManager.GetStringToAmount(exchange.SaleExchangeRate.Replace(".", ","));

            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());


            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

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
                        //System.IO.File.Delete(sourceFile);
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
                LocationID = model.Authentication.CurrentLocation.ID,
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

        [HttpPost]
        public ActionResult UpdateCashExchangeBuy(FormExchangeSell exchange)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            documentManager = new DocumentManager(
           new ProcessEmployee()
           {
               ID = model.Authentication.CurrentEmployee.EmployeeID,
               FullName = model.Authentication.CurrentEmployee.FullName
           },
           PosManager.GetIPAddress(),
           new ProcessCompany()
           {
               ID = 2,
               Name = "UFE GRUP",
               Currency = "TRL",
               TimeZone = 3
           }
           );

            var amount = PosManager.GetStringToAmount(exchange.Amount);
            var toAmount = PosManager.GetStringToAmount(exchange.ToAmount);
            var SaleExchangeRate = PosManager.GetStringToAmount(exchange.SaleExchangeRate);

            var processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            DateTime slipdate = exchange.ReceiptDate.Add(exchange.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (exchange.ReceiptFile != null && exchange.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(exchange.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

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
                        // System.IO.File.Delete(sourceFile);
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
                LocationID = model.Authentication.CurrentLocation.ID,
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










    }
}