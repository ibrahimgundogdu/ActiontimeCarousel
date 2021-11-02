using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class BankController : BaseController
    {
        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

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
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCollections = Db.VDocumentPosCollection.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCollections = model.PosCollections.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosCollections = model.PosCollections.Where(x => x.BankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.BankAccountID = BankAccountID;
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

            return RedirectToAction("Index", "Bank");
        }

        [AllowAnonymous]
        public ActionResult AddCollection(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.Detail = Db.VDocumentPosCollection.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.Detail.ID.ToString()).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCollection(NewPosCollect posCollect)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);
                int? quantity = posCollect.Quantity;
                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }

                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0 && quantity > 0)
                {
                    PosCollection pos = new PosCollection();

                    pos.ActinTypeID = actType.ID;
                    pos.ActionTypeName = actType.Name;
                    pos.Amount = amount;
                    pos.BankAccountID = posCollect.BankAccountID;
                    pos.Currency = currency;
                    pos.DocumentDate = docDate;
                    pos.Description = posCollect.Description;
                    pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    pos.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    pos.LocationID = posCollect.LocationID;
                    pos.OurCompanyID = location.OurCompanyID;
                    pos.TimeZone = timezone;
                    pos.ReferanceID = posCollect.ReferanceID;
                    pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";
                    pos.Quantity = quantity;
                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddPosCollection(pos, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }



            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;

            return RedirectToAction("Index", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosCollection(NewPosCollect posCollect)
        {

            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (posCollect != null)
            {
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(posCollect.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(posCollect.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                int? quantity = posCollect.Quantity <= 0 ? 1 : posCollect.Quantity;
                if (amount > 0 && quantity > 0)
                {
                    PosCollection payment = new PosCollection();
                    payment.ActinTypeID = posCollect.ActinTypeID;
                    payment.Amount = amount;
                    payment.Currency = currency;
                    payment.Description = posCollect.Description;
                    payment.DocumentDate = docDate;
                    payment.EnvironmentID = 2;
                    payment.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    payment.BankAccountID = posCollect.BankAccountID;
                    payment.LocationID = posCollect.LocationID;
                    payment.UID = posCollect.UID;
                    payment.Quantity = quantity;
                    payment.ReferanceID = posCollect.ReferanceID;
                    payment.TimeZone = timezone;
                    if (newexchanges > 0)
                    {
                        payment.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        payment.ExchangeRate = exchanges;
                    }


                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditPosCollection(payment, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }


            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("Detail", new { id = posCollect.UID });
        }

        [AllowAnonymous]
        public ActionResult DeletePosCollection(string id)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeletePosCollection(Guid.Parse(id), model.Authentication);

            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;
            messageresult.IsSuccess = result.IsSuccess;

            TempData["result"] = messageresult;
            return RedirectToAction("Detail", new { id = id });
        }



        [AllowAnonymous]
        public ActionResult PosCancel(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

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
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCancel = Db.VDocumentPosCancel.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCancel = model.PosCancel.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosCancel = model.PosCancel.Where(x => x.FromBankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterCancel(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();
            model.BankAccountID = BankAccountID;
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

            return RedirectToAction("PosCancel", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCancel(NewPosCancel posCollect)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);
                int? quantity = posCollect.Quantity;
                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }

                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0 && quantity > 0)
                {
                    PosCancel pos = new PosCancel();

                    pos.ActinTypeID = actType.ID;
                    pos.ActionTypeName = actType.Name;
                    pos.Amount = amount;
                    pos.FromBankAccountID = posCollect.BankAccountID;
                    pos.Currency = currency;
                    pos.DocumentDate = docDate;
                    pos.Description = posCollect.Description;
                    pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    pos.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    pos.LocationID = posCollect.LocationID;
                    pos.OurCompanyID = location.OurCompanyID;
                    pos.TimeZone = timezone;
                    pos.ReferanceID = posCollect.ReferanceID;
                    pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";
                    pos.Quantity = quantity;
                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddPosCancel(pos, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }



            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;

            return RedirectToAction("PosCancel", "Bank");
        }

        [AllowAnonymous]
        public ActionResult PosCancelDetail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.PosCancelDetail = Db.VDocumentPosCancel.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "PosCancel" && x.Environment == "Office" && x.ProcessID == model.PosCancelDetail.ID.ToString()).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosCancel(NewPosCancel posCollect)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (posCollect != null)
            {
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(posCollect.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(posCollect.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                int? quantity = posCollect.Quantity;
                if (amount > 0 && quantity > 0)
                {
                    PosCancel payment = new PosCancel();
                    payment.ActinTypeID = posCollect.ActinTypeID;
                    payment.Amount = amount;
                    payment.Currency = currency;
                    payment.Description = posCollect.Description;
                    payment.DocumentDate = docDate;
                    payment.EnvironmentID = 2;
                    payment.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    payment.FromBankAccountID = posCollect.BankAccountID;
                    payment.LocationID = posCollect.LocationID;
                    payment.UID = posCollect.UID;
                    payment.ReferanceID = posCollect.ReferanceID;
                    payment.TimeZone = timezone;
                    payment.Quantity = quantity;
                    if (newexchanges > 0)
                    {
                        payment.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        payment.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditPosCancel(payment, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("PosCancelDetail", new { id = posCollect.UID });
        }

        [AllowAnonymous]
        public ActionResult DeletePosCancel(string id)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeletePosCancel(Guid.Parse(id), model.Authentication);
            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;
            messageresult.IsSuccess = result.IsSuccess;
            TempData["result"] = messageresult;

            return RedirectToAction("PosCancelDetail", new { id = id });
        }



        [AllowAnonymous]
        public ActionResult PosRefund(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

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
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosRefund = Db.VDocumentPosRefund.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosRefund = model.PosRefund.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosRefund = model.PosRefund.Where(x => x.FromBankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterRefund(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();
            model.BankAccountID = BankAccountID;
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

            return RedirectToAction("PosRefund", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosRefund(NewPosReturn posCollect)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }

                var exchange = OfficeHelper.GetExchange(docDate);

                int? quantity = posCollect.Quantity;
                if (amount > 0 && quantity > 0)
                {
                    PosRefund pos = new PosRefund();

                    pos.ActinTypeID = actType.ID;
                    pos.ActionTypeName = actType.Name;
                    pos.Amount = amount;
                    pos.FromBankAccountID = posCollect.BankAccountID;
                    pos.Currency = currency;
                    pos.DocumentDate = docDate;
                    pos.Description = posCollect.Description;
                    pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    pos.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    pos.LocationID = posCollect.LocationID;
                    pos.OurCompanyID = location.OurCompanyID;
                    pos.TimeZone = timezone;
                    pos.ReferanceID = posCollect.ReferanceID;
                    pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";
                    pos.Quantity = quantity;
                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddPosRefund(pos, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }




            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;

            return RedirectToAction("PosRefund", "Bank");
        }

        [AllowAnonymous]
        public ActionResult PosRefundDetail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.PosRefundDetail = Db.VDocumentPosRefund.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "PosRefund" && x.Environment == "Office" && x.ProcessID == model.PosRefundDetail.ID.ToString()).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosRefund(NewPosReturn posCollect)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (posCollect != null)
            {
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(posCollect.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(posCollect.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }

                int? quantity = posCollect.Quantity;
                if (amount > 0 && quantity > 0)
                {
                    PosRefund payment = new PosRefund();
                    payment.ActinTypeID = posCollect.ActinTypeID;
                    payment.Amount = amount;
                    payment.Currency = currency;
                    payment.Description = posCollect.Description;
                    payment.DocumentDate = docDate;
                    payment.EnvironmentID = 2;
                    payment.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    payment.FromBankAccountID = posCollect.BankAccountID;
                    payment.LocationID = posCollect.LocationID;
                    payment.UID = posCollect.UID;
                    payment.ReferanceID = posCollect.ReferanceID;
                    payment.TimeZone = timezone;
                    payment.Quantity = quantity;
                    if (newexchanges > 0)
                    {
                        payment.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        payment.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditPosRefund(payment, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }


            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("PosRefundDetail", new { id = posCollect.UID });
        }

        [AllowAnonymous]
        public ActionResult DeletePosRefund(string id)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeletePosRefund(Guid.Parse(id), model.Authentication);
            }

            Result<BankActions> messageresult = new Result<BankActions>();
            messageresult.Message = result.Message;
            messageresult.IsSuccess = result.IsSuccess;
            TempData["result"] = messageresult;

            return RedirectToAction("PosRefundDetail", new { id = id });
        }

        [AllowAnonymous]
        public ActionResult Current(int? locationId)
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

            model.bankAccount = Db.BankAccount.ToList();
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
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
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
                    Type = "Bank",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }


            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "TRL").Total
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
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "EUR").Total
            });

            model.FooterTotals = footerTotals;


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filters(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Current", "Bank");
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult BankTransferFilters(string SearchKey, int? LocationId, DateTime? Date, int? StatusID, int? BankAccountID)
        {
            FilterModel model = new FilterModel();

            model.LocationID = LocationId;
            model.Date = Date;
            model.StatusID = StatusID;
            model.BankAccountID = BankAccountID;
            model.SearchKey = SearchKey;

            //if (Date == null)
            //{
            //    model.Date = DateTime.Now.Date;
            //}

            TempData["filter"] = model;

            return RedirectToAction("BankTransfer", "Bank");
        }


        //BankTransfer
        [AllowAnonymous]
        public ActionResult BankTransfer(int? LocationId, string Date, int? StatusID, int? BankAccountID)
        {
            BankControlModel model = new BankControlModel();
            var _date = DateTime.Now.Date;

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                if (!string.IsNullOrEmpty(Date))
                    DateTime.TryParse(Date, out _date);

                filterModel.Date = _date.Date;
                filterModel.LocationID = LocationId ?? 0;
                model.Filters = filterModel;
            }

            model.BankAccounts = Db.VBankAccount.Where(x => x.AccountTypeID == 1 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.BankTransferStatus = Db.BankTransferStatus.OrderBy(x => x.LevelNumber).ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            if (!string.IsNullOrEmpty(model.Filters.SearchKey))
            {
                model.DocumentBankTransfers = Db.VDocumentBankTransfer.Where(x => x.ReferenceCode.Contains(model.Filters.SearchKey) && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }
            else
            {
                if (model.Filters.Date != null)
                {
                    model.DocumentBankTransfers = Db.VDocumentBankTransfer.Where(x => x.Date == model.Filters.Date && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
                }
                else
                {
                    model.DocumentBankTransfers = Db.VDocumentBankTransfer.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
                }

                if (model.Filters.LocationID > 0)
                {
                    model.DocumentBankTransfers = model.DocumentBankTransfers.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
                }

                if (model.Filters.BankAccountID > 0)
                {
                    model.DocumentBankTransfers = model.DocumentBankTransfers.Where(x => x.ToBankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
                }

                if (model.Filters.StatusID > 0)
                {
                    model.DocumentBankTransfers = model.DocumentBankTransfers.Where(x => x.StatusID == model.Filters.StatusID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
                }

            }
            List<BankTransferStatusCount> countlist = new List<BankTransferStatusCount>();

            foreach (var item in model.BankTransferStatus)
            {
                countlist.Add(new BankTransferStatusCount()
                {
                    StatusID = item.ID,
                    StatusName = item.StatusName,
                    Count = model.DocumentBankTransfers.Where(x => x.StatusID == item.ID).Count(),
                    Amount = model.DocumentBankTransfers.Where(x => x.StatusID == item.ID).Sum(y => y.Amount) ?? 0,
                    Commission = model.DocumentBankTransfers.Where(x => x.StatusID == item.ID).Sum(y => y.Commission) ?? 0,
                    Currency = model.DocumentBankTransfers.Where(x => x.StatusID == item.ID).FirstOrDefault()?.Currency
                });
            }

            model.StatusCounts = countlist;
            model.SelectedDate = model.Filters.Date ?? _date;
            model.PrevDate = model.SelectedDate.AddDays(-1).Date;
            model.NextDate = model.SelectedDate.AddDays(1).Date;

            return View(model);
        }


        //ChangeTransferStatus

        [AllowAnonymous]
        public ActionResult ChangeTransferStatus(Guid? UID, int? StatusID)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (UID == null || StatusID == null || StatusID == 0)
            {
                return RedirectToAction("BankTransfer");
            }

            BankControlModel model = new BankControlModel();

            var bankTransfer = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == UID);

            if (bankTransfer != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.EditBankTransferStatus(bankTransfer, model.Authentication, StatusID.Value);
            }

            TempData["result"] = result;
            return RedirectToAction("BankTransfer", new { Date = bankTransfer.Date?.ToString("yyyy-MM-dd") });

        }

    }
}