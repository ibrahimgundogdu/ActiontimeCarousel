using ActionForce.Entity;
using ActionForce.Integration.UfeService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace ActionForce.Service
{
    public class DocumentManager
    {
        public readonly ProcessEmployee _employee;
        public readonly string _ip;
        public readonly ProcessCompany _company;
        public DocumentManager(ProcessEmployee Employee, string IP, ProcessCompany Company)
        {
            _employee = Employee;
            _ip = IP;
            _company = Company;
        }

        public Result<DocumentCashCollections> AddCashCollection(CashCollectionModel collection)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (collection != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {

                    var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                    var amount = collection.Amount;
                    var currency = collection.Currency;
                    var docDate = DateTime.UtcNow.AddHours(location.Timezone.Value).Date;
                    int timezone = location.Timezone != null ? location.Timezone.Value : _company.TimeZone;

                    if (collection.DocumentDate != null)
                    {
                        docDate = collection.DocumentDate.Value.Date;
                    }
                    var cash = ServiceHelper.GetCash(collection.LocationID, collection.Currency);
                    var refID = collection.ReferanceID ?? (long?)null;
                    try
                    {
                        var exchange = ServiceHelper.GetExchange(docDate);

                        DocumentCashCollections cashCollect = new DocumentCashCollections();

                        cashCollect.ActionTypeID = collection.ActionTypeID;
                        cashCollect.ActionTypeName = collection.ActionTypeName;
                        cashCollect.Amount = amount;
                        cashCollect.CashID = cash.ID;
                        cashCollect.Currency = currency;
                        cashCollect.Date = docDate;
                        cashCollect.Description = collection.Description;
                        cashCollect.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "CC");
                        cashCollect.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        cashCollect.FromBankAccountID = collection.FromBankAccountID ?? (int?)null;
                        cashCollect.FromEmployeeID = collection.FromEmployeeID ?? (int?)null;
                        cashCollect.FromCustomerID = collection.FromCustomerID ?? (int?)null;
                        cashCollect.IsActive = true;
                        cashCollect.LocationID = collection.LocationID;
                        cashCollect.OurCompanyID = _company.ID;
                        cashCollect.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        cashCollect.RecordEmployeeID = _employee.ID;
                        cashCollect.RecordIP = _ip;
                        cashCollect.SystemAmount = _company.Currency == currency ? amount : amount * cashCollect.ExchangeRate;
                        cashCollect.SystemCurrency = _company.Currency;
                        cashCollect.EnvironmentID = collection.EnvironmentID;
                        cashCollect.UID = Guid.NewGuid();
                        cashCollect.ResultID = collection.ResultID;
                        cashCollect.ReferenceID = refID;

                        Db.DocumentCashCollections.Add(cashCollect);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        ServiceHelper.AddCashAction(cashCollect.CashID, cashCollect.LocationID, null, cashCollect.ActionTypeID, cashCollect.Date, cashCollect.ActionTypeName, cashCollect.ID, cashCollect.Date, cashCollect.DocumentNumber, cashCollect.Description, 1, cashCollect.Amount, 0, cashCollect.Currency, null, null, cashCollect.RecordEmployeeID, cashCollect.RecordDate, cashCollect.UID.Value);

                        result.IsSuccess = true;
                        result.Message = $"{cashCollect.Date?.ToShortDateString()} tarihli { cashCollect.Amount } {cashCollect.Currency} tutarındaki kasa tahsilatı başarı ile eklendi";

                        // log atılır
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", cashCollect.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), _employee.FullName, _ip, string.Empty, cashCollect);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, location.LocalDateTime.Value, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashCollections> UpdateCashCollection(CashCollectionModel collection)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var cashCollect = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == collection.UID);

                if (cashCollect != null)
                {
                    try
                    {
                        var cash = ServiceHelper.GetCash(collection.LocationID, collection.Currency);
                        var exchange = ServiceHelper.GetExchange(collection.DocumentDate.Value);

                        DocumentCashCollections self = new DocumentCashCollections()
                        {
                            ActionTypeID = cashCollect.ActionTypeID,
                            ActionTypeName = cashCollect.ActionTypeName,
                            Amount = cashCollect.Amount,
                            CashID = cashCollect.CashID,
                            Currency = cashCollect.Currency,
                            Date = cashCollect.Date,
                            Description = cashCollect.Description,
                            DocumentNumber = cashCollect.DocumentNumber,
                            ExchangeRate = cashCollect.ExchangeRate,
                            ID = cashCollect.ID,
                            IsActive = cashCollect.IsActive,
                            LocationID = cashCollect.LocationID,
                            OurCompanyID = cashCollect.OurCompanyID,
                            RecordDate = cashCollect.RecordDate,
                            RecordEmployeeID = cashCollect.RecordEmployeeID,
                            RecordIP = cashCollect.RecordIP,
                            ReferenceID = cashCollect.ReferenceID,
                            SystemAmount = cashCollect.SystemAmount,
                            SystemCurrency = cashCollect.SystemCurrency,
                            UpdateDate = cashCollect.UpdateDate,
                            UpdateEmployee = cashCollect.UpdateEmployee,
                            UpdateIP = cashCollect.UpdateIP,
                            EnvironmentID = cashCollect.EnvironmentID,
                            FromBankAccountID = cashCollect.FromBankAccountID,
                            FromCustomerID = cashCollect.FromCustomerID,
                            FromEmployeeID = cashCollect.FromEmployeeID,
                            ResultID = cashCollect.ResultID,
                            UID = cashCollect.UID

                        };

                        cashCollect.CashID = cash.ID;
                        cashCollect.Date = collection.DocumentDate?.Date;
                        cashCollect.FromBankAccountID = collection.FromBankAccountID ?? (int?)null;
                        cashCollect.FromEmployeeID = collection.FromEmployeeID ?? (int?)null;
                        cashCollect.FromCustomerID = collection.FromCustomerID ?? (int?)null;
                        cashCollect.Amount = collection.Amount;
                        cashCollect.Currency = collection.Currency;
                        cashCollect.Description = collection.Description;
                        cashCollect.ExchangeRate = collection.Currency == "USD" ? exchange.USDA : collection.Currency == "EUR" ? exchange.EURA : 1;
                        cashCollect.UpdateDate = collection.ProcessDate;
                        cashCollect.UpdateEmployee = _employee.ID;
                        cashCollect.UpdateIP = _ip;
                        cashCollect.SystemAmount = _company.Currency == collection.Currency ? collection.Amount : collection.Amount * cashCollect.ExchangeRate;
                        cashCollect.SystemCurrency = _company.Currency;
                        cashCollect.IsActive = collection.IsActive;

                        Db.SaveChanges();

                        //Cari hesap hareketleri temizlenir.

                        Db.RemoveAllAccountActions(cashCollect.ID, cashCollect.UID);

                        // Aktif ise Cari hesap kaydı eklenir.

                        if (cashCollect.IsActive == true)
                        {
                            ServiceHelper.AddCashAction(cashCollect.CashID, cashCollect.LocationID, _employee.ID, cashCollect.ActionTypeID, cashCollect.Date, cashCollect.ActionTypeName, cashCollect.ID, cashCollect.Date, cashCollect.DocumentNumber, cashCollect.Description, 1, cashCollect.Amount, 0, cashCollect.Currency, null, null, cashCollect.UpdateEmployee, cashCollect.UpdateDate, cashCollect.UID.Value);
                        }

                        result.IsSuccess = true;
                        result.Message = $"{cashCollect.ID} ID li {cashCollect.Date?.ToShortDateString()} tarihli {cashCollect.Amount} {cashCollect.Currency} tutarındaki kasa tahsilatı başarı ile güncellendi";

                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, cashCollect, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", cashCollect.ID.ToString(), "Cash", "Index", isequal, true, $"{result.Message}", string.Empty, collection.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{cashCollect.Amount} {cashCollect.Currency} tutarındaki kasa tahsilatı güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, collection.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashCollections> DeleteCashCollection(Guid? id)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == id);

                    if (isCash != null)
                    {
                        DateTime processDate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime ?? DateTime.UtcNow;

                        try
                        {
                            isCash.IsActive = false;
                            isCash.UpdateDate = processDate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, isCash);
                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentCashPayments> AddCashPayment(CashPaymentModel payment)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (payment != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var cash = ServiceHelper.GetCash(payment.LocationID, payment.Currency);

                    try
                    {
                        var exchange = ServiceHelper.GetExchange(payment.DocumentDate);

                        DocumentCashPayments cashPayment = new DocumentCashPayments();

                        cashPayment.ActionTypeID = payment.ActionTypeID;
                        cashPayment.ActionTypeName = payment.ActionTypeName;
                        cashPayment.Amount = payment.Amount;
                        cashPayment.CashID = cash.ID;
                        cashPayment.Currency = payment.Currency;
                        cashPayment.Date = payment.DocumentDate;
                        cashPayment.Description = payment.Description;
                        cashPayment.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "CPY");
                        cashPayment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA : payment.Currency == "EUR" ? exchange.EURA : 1;
                        cashPayment.ToEmployeeID = payment.ToEmployeeID;
                        cashPayment.ToCustomerID = payment.ToCustomerID;
                        cashPayment.IsActive = true;
                        cashPayment.LocationID = payment.LocationID;
                        cashPayment.OurCompanyID = _company.ID;
                        cashPayment.RecordDate = payment.ProcessDate;
                        cashPayment.RecordEmployeeID = _employee.ID;
                        cashPayment.RecordIP = _ip;
                        cashPayment.SystemAmount = _company.Currency == payment.Currency ? payment.Amount : payment.Amount * cashPayment.ExchangeRate;
                        cashPayment.SystemCurrency = _company.Currency;
                        cashPayment.EnvironmentID = payment.EnvironmentID;
                        cashPayment.ReferenceID = payment.ReferanceID;
                        cashPayment.UID = Guid.NewGuid();

                        Db.DocumentCashPayments.Add(cashPayment);
                        Db.SaveChanges();

                        ServiceHelper.AddCashAction(cashPayment.CashID, cashPayment.LocationID, null, cashPayment.ActionTypeID, cashPayment.Date, cashPayment.ActionTypeName, cashPayment.ID, cashPayment.Date, cashPayment.DocumentNumber, cashPayment.Description, -1, 0, cashPayment.Amount, cashPayment.Currency, null, null, cashPayment.RecordEmployeeID, cashPayment.RecordDate, cashPayment.UID.Value);

                        result.IsSuccess = true;
                        result.Message = $"{cashPayment.Date?.ToShortDateString()} tarihli { cashPayment.Amount } {cashPayment.Currency} tutarındaki kasa ödemesi başarı ile eklendi";

                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", cashPayment.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, cashPayment);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{payment.Amount} {payment.Currency} tutarındaki kasa ödemesi eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashPayments> UpdateCashPayment(CashPaymentModel payment)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var cashPayment = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == payment.UID);

                if (cashPayment != null)
                {
                    try
                    {
                        var cash = ServiceHelper.GetCash(payment.LocationID, payment.Currency);
                        var exchange = ServiceHelper.GetExchange(payment.DocumentDate);

                        DocumentCashPayments self = new DocumentCashPayments()
                        {
                            ActionTypeID = cashPayment.ActionTypeID,
                            ActionTypeName = cashPayment.ActionTypeName,
                            Amount = cashPayment.Amount,
                            CashID = cashPayment.CashID,
                            Currency = cashPayment.Currency,
                            Date = cashPayment.Date,
                            Description = cashPayment.Description,
                            DocumentNumber = cashPayment.DocumentNumber,
                            ExchangeRate = cashPayment.ExchangeRate,
                            ID = cashPayment.ID,
                            IsActive = cashPayment.IsActive,
                            LocationID = cashPayment.LocationID,
                            OurCompanyID = cashPayment.OurCompanyID,
                            RecordDate = cashPayment.RecordDate,
                            RecordEmployeeID = cashPayment.RecordEmployeeID,
                            RecordIP = cashPayment.RecordIP,
                            ReferenceID = cashPayment.ReferenceID,
                            SystemAmount = cashPayment.SystemAmount,
                            SystemCurrency = cashPayment.SystemCurrency,
                            UpdateDate = cashPayment.UpdateDate,
                            UpdateEmployee = cashPayment.UpdateEmployee,
                            UpdateIP = cashPayment.UpdateIP,
                            EnvironmentID = cashPayment.EnvironmentID,
                            ResultID = cashPayment.ResultID,
                            ToCustomerID = cashPayment.ToCustomerID,
                            ToEmployeeID = cashPayment.ToEmployeeID,
                            UID = cashPayment.UID
                        };

                        cashPayment.LocationID = payment.LocationID;
                        cashPayment.CashID = cash.ID;
                        cashPayment.Date = payment.DocumentDate;
                        cashPayment.Amount = payment.Amount;
                        cashPayment.Currency = payment.Currency;
                        cashPayment.Description = payment.Description;
                        cashPayment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA : payment.Currency == "EUR" ? exchange.EURA : 1;
                        cashPayment.UpdateDate = payment.ProcessDate;
                        cashPayment.UpdateEmployee = _employee.ID;
                        cashPayment.UpdateIP = _ip;
                        cashPayment.SystemAmount = _company.Currency == payment.Currency ? payment.Amount : payment.Amount * cashPayment.ExchangeRate;
                        cashPayment.SystemCurrency = _company.Currency;
                        cashPayment.IsActive = payment.IsActive;

                        Db.SaveChanges();

                        Db.RemoveAllAccountActions(cashPayment.ID, cashPayment.UID);

                        if (cashPayment.IsActive == true)
                        {
                            ServiceHelper.AddCashAction(cashPayment.CashID, cashPayment.LocationID, _employee.ID, cashPayment.ActionTypeID, cashPayment.Date, cashPayment.ActionTypeName, cashPayment.ID, cashPayment.Date, cashPayment.DocumentNumber, cashPayment.Description, -1, 0, cashPayment.Amount, cashPayment.Currency, null, null, cashPayment.RecordEmployeeID, cashPayment.RecordDate, cashPayment.UID.Value);
                        }

                        result.IsSuccess = true;
                        result.Message = $"{cashPayment.ID} ID li {cashPayment.Date} tarihli {cashPayment.Amount} {cashPayment.Currency} tutarındaki kasa ödemesi başarı ile güncellendi";


                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentCashPayments>(self, cashPayment, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", cashPayment.ID.ToString(), "Cash", "CashPayment", isequal, true, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{payment.Amount} {payment.Currency} tutarındaki kasa ödemesi güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }


                }
            }

            return result;
        }

        public Result<DocumentCashPayments> DeleteCashPayment(Guid? id)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == id);

                    if (isCash != null)
                    {
                        DateTime processDate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime ?? DateTime.UtcNow;

                        try
                        {
                            var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));



                            isCash.IsActive = false;
                            isCash.UpdateDate = processDate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            //Cari hesap hareketleri temizlenir.

                            Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date?.ToShortDateString()} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentCashExpense> AddCashExpense(CashExpenseModel expense)
        {
            Result<DocumentCashExpense> result = new Result<DocumentCashExpense>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (expense != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var cash = ServiceHelper.GetCash(expense.LocationID, expense.Currency);
                    var exchange = ServiceHelper.GetExchange(expense.DocumentDate);

                    try
                    {

                        DocumentCashExpense cashExpense = new DocumentCashExpense();

                        cashExpense.ActionTypeID = expense.ActionTypeID;
                        cashExpense.ActionTypeName = expense.ActionTypeName;
                        cashExpense.Amount = expense.Amount;
                        cashExpense.CashID = cash.ID;
                        cashExpense.Currency = expense.Currency;
                        cashExpense.Date = expense.DocumentDate;
                        cashExpense.Description = expense.Description;
                        cashExpense.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "EXP");
                        cashExpense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA : expense.Currency == "EUR" ? exchange.EURA : 1;
                        cashExpense.ToBankAccountID = expense.ToBankAccountID ?? (int?)null;
                        cashExpense.ToEmployeeID = expense.ToEmployeeID ?? (int?)null;
                        cashExpense.ToCustomerID = expense.ToCustomerID ?? (int?)null;
                        cashExpense.IsActive = expense.IsActive;
                        cashExpense.LocationID = expense.LocationID;
                        cashExpense.OurCompanyID = _company.ID;
                        cashExpense.RecordDate = expense.ProcessDate;
                        cashExpense.RecordEmployeeID = _employee.ID;
                        cashExpense.RecordIP = _ip;
                        cashExpense.SystemAmount = _company.Currency == expense.Currency ? cashExpense.Amount : cashExpense.Amount * cashExpense.ExchangeRate;
                        cashExpense.SystemCurrency = _company.Currency;
                        cashExpense.SlipNumber = expense.SlipNumber;
                        cashExpense.SlipDate = expense.SlipDate;
                        cashExpense.ReferenceID = expense.ReferanceID;
                        cashExpense.EnvironmentID = expense.EnvironmentID;
                        cashExpense.UID = expense.UID;
                        cashExpense.ExpenseTypeID = expense.ExpenseTypeID;
                        cashExpense.SlipPath = expense.SlipPath;
                        cashExpense.SlipDocument = expense.SlipDocument;

                        Db.DocumentCashExpense.Add(cashExpense);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        ServiceHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate, cashExpense.UID.Value);


                        result.IsSuccess = true;
                        result.Message = "Masraf ödeme fişi başarı ile eklendi";

                        // log atılır
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", cashExpense.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, expense.ProcessDate, _employee.FullName, _ip, string.Empty, cashExpense);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, expense.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }
            }
            return result;
        }

        public Result<DocumentCashExpense> UpdateCashExpense(CashExpenseModel expense)
        {
            Result<DocumentCashExpense> result = new Result<DocumentCashExpense>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.UID == expense.UID);

                if (isExpense != null)
                {
                    try
                    {
                        var cash = ServiceHelper.GetCash(expense.LocationID, expense.Currency);
                        var exchange = ServiceHelper.GetExchange(expense.DocumentDate);

                        DocumentCashExpense self = new DocumentCashExpense()
                        {
                            ActionTypeID = isExpense.ActionTypeID,
                            ActionTypeName = isExpense.ActionTypeName,
                            Amount = isExpense.Amount,
                            CashID = isExpense.CashID,
                            Currency = isExpense.Currency,
                            Date = isExpense.Date,
                            Description = isExpense.Description,
                            DocumentNumber = isExpense.DocumentNumber,
                            ExchangeRate = isExpense.ExchangeRate,
                            ID = isExpense.ID,
                            IsActive = isExpense.IsActive,
                            LocationID = isExpense.LocationID,
                            OurCompanyID = isExpense.OurCompanyID,
                            RecordDate = isExpense.RecordDate,
                            RecordEmployeeID = isExpense.RecordEmployeeID,
                            RecordIP = isExpense.RecordIP,
                            ReferenceID = isExpense.ReferenceID,
                            SystemAmount = isExpense.SystemAmount,
                            SystemCurrency = isExpense.SystemCurrency,
                            UpdateDate = isExpense.UpdateDate,
                            UpdateEmployee = isExpense.UpdateEmployee,
                            UpdateIP = isExpense.UpdateIP,
                            SlipNumber = isExpense.SlipNumber,
                            SlipDocument = isExpense.SlipDocument,
                            EnvironmentID = isExpense.EnvironmentID,
                            ExpenseTypeID = isExpense.ExpenseTypeID,
                            ReferenceTableModel = isExpense.ReferenceTableModel,
                            ResultID = isExpense.ResultID,
                            SlipDate = isExpense.SlipDate,
                            SlipPath = isExpense.SlipPath,
                            ToBankAccountID = isExpense.ToBankAccountID,
                            ToCustomerID = isExpense.ToCustomerID,
                            ToEmployeeID = isExpense.ToEmployeeID,
                            UID = isExpense.UID
                        };

                        isExpense.SlipDate = expense.SlipDate;
                        isExpense.ExpenseTypeID = expense.ExpenseTypeID ?? (int?)null;
                        isExpense.ReferenceID = expense.ReferanceID;
                        isExpense.SlipDocument = !string.IsNullOrEmpty(expense.SlipDocument) ? expense.SlipDocument : self.SlipDocument;
                        isExpense.SlipPath = !string.IsNullOrEmpty(expense.SlipPath) ? expense.SlipPath : self.SlipPath;
                        isExpense.LocationID = expense.LocationID;
                        isExpense.CashID = cash.ID;
                        isExpense.Date = expense.DocumentDate;
                        isExpense.SlipNumber = expense.SlipNumber;
                        isExpense.ToBankAccountID = expense.ToBankAccountID ?? (int?)null;
                        isExpense.ToEmployeeID = expense.ToEmployeeID ?? (int?)null;
                        isExpense.ToCustomerID = expense.ToCustomerID ?? (int?)null;
                        isExpense.Amount = expense.Amount;
                        isExpense.Currency = expense.Currency;
                        isExpense.Description = expense.Description;
                        isExpense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA : expense.Currency == "EUR" ? exchange.EURA : 1;
                        isExpense.UpdateDate = expense.ProcessDate;
                        isExpense.UpdateEmployee = _employee.ID;
                        isExpense.UpdateIP = _ip;
                        isExpense.SystemAmount = _company.Currency == expense.Currency ? expense.Amount : expense.Amount * isExpense.ExchangeRate;
                        isExpense.SystemCurrency = _company.Currency;
                        isExpense.IsActive = expense.IsActive;

                        Db.SaveChanges();

                        Db.RemoveAllAccountActions(isExpense.ID, isExpense.UID);

                        if (isExpense.IsActive == true)
                        {
                            ServiceHelper.AddCashAction(isExpense.CashID, isExpense.LocationID, _employee.ID, isExpense.ActionTypeID, isExpense.Date, isExpense.ActionTypeName, isExpense.ID, isExpense.Date, isExpense.DocumentNumber, isExpense.Description, -1, 0, isExpense.Amount, isExpense.Currency, null, null, isExpense.UpdateEmployee, isExpense.UpdateDate, isExpense.UID.Value);
                        }

                        result.IsSuccess = true;
                        result.Message = $"{isExpense.ID} ID li {isExpense.Date?.ToShortDateString()} tarihli {isExpense.Amount} {isExpense.Currency} tutarındaki masraf ödeme fişi başarı ile güncellendi";

                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentCashExpense>(self, isExpense, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", isExpense.ID.ToString(), "Cash", "Expense", isequal, true, $"{result.Message}", string.Empty, expense.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Update", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, expense.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashExpense> DeleteCashExpense(Guid? id)
        {
            Result<DocumentCashExpense> result = new Result<DocumentCashExpense>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentCashExpense.FirstOrDefault(x => x.UID == id);
                    DateTime processDate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime ?? DateTime.UtcNow;

                    if (isCash != null)
                    {
                        try
                        {

                            isCash.IsActive = false;
                            isCash.UpdateDate = processDate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentSaleExchange> AddCashSellExchange(CashExchangeModel exchanged)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (exchanged != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var cash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.Currency);  // usd
                    var cashto = ServiceHelper.GetCash(exchanged.LocationID, exchanged.ToCurrency);  // trl

                    var exchange = ServiceHelper.GetExchange(exchanged.DocumentDate);

                    try
                    {
                        var balance = Db.GetCashBalance(exchanged.LocationID, cash.ID, exchanged.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= exchanged.Amount)
                        {
                            DocumentSaleExchange sale = new DocumentSaleExchange();

                            sale.ActionTypeID = exchanged.ActionTypeID;
                            sale.ActionTypeName = exchanged.ActionTypeName;
                            sale.Amount = exchanged.Amount;
                            sale.FromCashID = cash.ID;
                            sale.Currency = exchanged.Currency;

                            sale.ToCashID = cashto.ID;
                            sale.ToCurrency = exchanged.ToCurrency;
                            sale.ToAmount = exchanged.ToAmount;

                            sale.Date = exchanged.DocumentDate;
                            sale.Description = exchanged.Description;
                            sale.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "EXS");
                            sale.ExchangeRate = exchanged.Currency == "USD" ? exchange.USDA : exchanged.Currency == "EUR" ? exchange.EURA : 1;
                            sale.SaleExchangeRate = exchanged.SaleExchangeRate;
                            sale.IsActive = true;
                            sale.LocationID = exchanged.LocationID;
                            sale.OurCompanyID = _company.ID;
                            sale.RecordDate = exchanged.ProcessDate;
                            sale.RecordEmployeeID = _employee.ID;
                            sale.RecordIP = _ip;
                            sale.ReferenceID = exchanged.ReferanceID;
                            sale.EnvironmentID = exchanged.EnvironmentID;
                            sale.UID = exchanged.UID;
                            sale.SlipNumber = exchanged.SlipNumber;
                            sale.SlipDate = exchanged.SlipDate;
                            sale.ResultID = exchanged.ResultID;
                            sale.SlipDocument = exchanged.SlipDocument;
                            sale.SlipPath = exchanged.SlipPath;

                            Db.DocumentSaleExchange.Add(sale);
                            Db.SaveChanges();


                            // cari hesap işlemesi
                            ServiceHelper.AddCashAction(sale.FromCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, -1, 0, sale.Amount, sale.Currency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);
                            ServiceHelper.AddCashAction(sale.ToCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, 1, sale.ToAmount, 0, sale.ToCurrency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{sale.Date?.ToShortDateString()} tarihli { sale.Amount } {sale.Currency} kasa döviz satışı başarı ile eklendi";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", sale.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, sale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { exchanged.Amount } { exchanged.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { exchanged.Currency } tutardır.";
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                        }

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{exchanged.Amount} {exchanged.Currency} kasa döviz satışı eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }


                }

            }

            return result;
        }

        public Result<DocumentSaleExchange> UpdateCashSellExchange(CashExchangeModel exchanged)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            using (ActionTimeEntities Db = new ActionTimeEntities())
            {

                var isExchange = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == exchanged.UID);

                try
                {
                    var fromcash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.Currency);
                    var tocash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.ToCurrency);
                    var exchange = ServiceHelper.GetExchange(exchanged.DocumentDate);

                    DocumentSaleExchange self = new DocumentSaleExchange()
                    {
                        ActionTypeID = isExchange.ActionTypeID,
                        ActionTypeName = isExchange.ActionTypeName,
                        FromCashID = isExchange.FromCashID,
                        Amount = isExchange.Amount,
                        Currency = isExchange.Currency,
                        SaleExchangeRate = isExchange.SaleExchangeRate,
                        ToCashID = isExchange.ToCashID,
                        ToAmount = isExchange.ToAmount,
                        ToCurrency = isExchange.ToCurrency,
                        Date = isExchange.Date,
                        Description = isExchange.Description,
                        DocumentNumber = isExchange.DocumentNumber,
                        ExchangeRate = isExchange.ExchangeRate,
                        ID = isExchange.ID,
                        IsActive = isExchange.IsActive,
                        LocationID = isExchange.LocationID,
                        OurCompanyID = isExchange.OurCompanyID,
                        RecordDate = isExchange.RecordDate,
                        RecordEmployeeID = isExchange.RecordEmployeeID,
                        RecordIP = isExchange.RecordIP,
                        ReferenceID = isExchange.ReferenceID,
                        UpdateDate = isExchange.UpdateDate,
                        UpdateEmployee = isExchange.UpdateEmployee,
                        UpdateIP = isExchange.UpdateIP,
                        SlipPath = isExchange.SlipPath,
                        SlipDocument = isExchange.SlipDocument,
                        EnvironmentID = isExchange.EnvironmentID,
                        ResultID = isExchange.ResultID,
                        SlipDate = isExchange.SlipDate,
                        SlipNumber = isExchange.SlipNumber,
                        UID = isExchange.UID
                    };

                    isExchange.ReferenceID = exchanged.ReferanceID;
                    isExchange.LocationID = exchanged.LocationID;
                    isExchange.FromCashID = fromcash.ID;
                    isExchange.ToCashID = tocash.ID;
                    isExchange.Date = exchanged.DocumentDate;
                    isExchange.Amount = exchanged.Amount;
                    isExchange.ToCurrency = exchanged.ToCurrency;
                    isExchange.Currency = exchanged.Currency;
                    isExchange.ToAmount = (exchanged.Amount * exchanged.SaleExchangeRate);
                    isExchange.SaleExchangeRate = exchanged.SaleExchangeRate;
                    isExchange.Description = exchanged.Description;
                    isExchange.ExchangeRate = exchanged.Currency == "USD" ? exchange.USDA : exchanged.Currency == "EUR" ? exchange.EURA : 1;
                    isExchange.UpdateDate = exchanged.ProcessDate;
                    isExchange.UpdateEmployee = _employee.ID;
                    isExchange.UpdateIP = _ip;
                    isExchange.IsActive = exchanged.IsActive;
                    isExchange.SlipDocument = !string.IsNullOrEmpty(exchanged.SlipDocument) ? exchanged.SlipDocument : self.SlipDocument;
                    isExchange.SlipPath = !string.IsNullOrEmpty(exchanged.SlipPath) ? exchanged.SlipPath : self.SlipPath;
                    isExchange.SlipDate = exchanged.SlipDate;
                    isExchange.SlipNumber = exchanged.SlipNumber;

                    Db.SaveChanges();

                    Db.RemoveAllAccountActions(isExchange.ID, isExchange.UID);

                    if (isExchange.IsActive == true)
                    {
                        ServiceHelper.AddCashAction(isExchange.FromCashID, isExchange.LocationID, _employee.ID, isExchange.ActionTypeID, isExchange.Date, isExchange.ActionTypeName, isExchange.ID, isExchange.Date, isExchange.DocumentNumber, isExchange.Description, -1, 0, isExchange.Amount, isExchange.Currency, null, null, isExchange.UpdateEmployee, isExchange.UpdateDate, isExchange.UID.Value);
                        ServiceHelper.AddCashAction(isExchange.ToCashID, isExchange.LocationID, _employee.ID, isExchange.ActionTypeID, isExchange.Date, isExchange.ActionTypeName, isExchange.ID, isExchange.Date, isExchange.DocumentNumber, isExchange.Description, 1, isExchange.ToAmount, 0, isExchange.ToCurrency, null, null, isExchange.UpdateEmployee, isExchange.UpdateDate, isExchange.UID.Value);
                    }

                    result.IsSuccess = true;
                    result.Message = $"{isExchange.ID} ID li {isExchange.Date} tarihli {isExchange.Amount} {isExchange.Currency} kasa döviz satışı başarı ile güncellendi";

                    var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentSaleExchange>(self, isExchange, ServiceHelper.getIgnorelist());
                    ServiceHelper.AddApplicationLog("Location", "Cash", "Update", isExchange.ID.ToString(), "Cash", "ExchangeSale", isequal, true, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                }
                catch (Exception ex)
                {
                    result.Message = $"{exchanged.Amount} {exchanged.Currency} kasa döviz satışı Güncellenemedi : {ex.Message}";
                    ServiceHelper.AddApplicationLog("Location", "Cash", "Update", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                }


            }

            return result;
        }

        public Result<DocumentSaleExchange> DeleteSaleExchange(Guid? id)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == id);
                    DateTime processDate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime ?? DateTime.UtcNow;

                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = processDate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            ServiceHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            ServiceHelper.AddCashAction(isCash.ToCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.ToAmount, 0, isCash.ToCurrency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }

            }

            return result;
        }



        public Result<DocumentBuyExchange> AddCashBuyExchange(CashExchangeModel exchanged)
        {
            Result<DocumentBuyExchange> result = new Result<DocumentBuyExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (exchanged != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var cash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.Currency);  // usd
                    var cashto = ServiceHelper.GetCash(exchanged.LocationID, exchanged.ToCurrency);  // trl

                    var exchange = ServiceHelper.GetExchange(exchanged.DocumentDate);

                    try
                    {
                        var balance = Db.GetCashBalance(exchanged.LocationID, cashto.ID, exchanged.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= exchanged.ToAmount)
                        {
                            DocumentBuyExchange sale = new DocumentBuyExchange();

                            sale.ActionTypeID = exchanged.ActionTypeID;
                            sale.ActionTypeName = exchanged.ActionTypeName;
                            sale.Amount = exchanged.Amount;
                            sale.FromCashID = cash.ID;
                            sale.Currency = exchanged.Currency;

                            sale.ToCashID = cashto.ID;
                            sale.ToCurrency = exchanged.ToCurrency;
                            sale.ToAmount = exchanged.ToAmount;

                            sale.Date = exchanged.DocumentDate;
                            sale.Description = exchanged.Description;
                            sale.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "EXB");
                            sale.ExchangeRate = exchanged.Currency == "USD" ? exchange.USDA : exchanged.Currency == "EUR" ? exchange.EURA : 1;
                            sale.SaleExchangeRate = exchanged.SaleExchangeRate;
                            sale.IsActive = true;
                            sale.LocationID = exchanged.LocationID;
                            sale.OurCompanyID = _company.ID;
                            sale.RecordDate = exchanged.ProcessDate;
                            sale.RecordEmployeeID = _employee.ID;
                            sale.RecordIP = _ip;
                            sale.ReferenceID = exchanged.ReferanceID;
                            sale.EnvironmentID = exchanged.EnvironmentID;
                            sale.UID = exchanged.UID;
                            sale.SlipNumber = exchanged.SlipNumber;
                            sale.SlipDate = exchanged.SlipDate;
                            sale.ResultID = exchanged.ResultID;
                            sale.SlipDocument = exchanged.SlipDocument;
                            sale.SlipPath = exchanged.SlipPath;

                            Db.DocumentBuyExchange.Add(sale);
                            Db.SaveChanges();


                            // cari hesap işlemesi
                            ServiceHelper.AddCashAction(sale.FromCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, 1, sale.Amount, 0, sale.Currency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);
                            ServiceHelper.AddCashAction(sale.ToCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, -1, 0, sale.ToAmount, sale.ToCurrency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{sale.Date?.ToShortDateString()} tarihli { sale.Amount } {sale.Currency} kasa döviz alışı başarı ile eklendi";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", sale.ID.ToString(), "Cash", "ExchangeBuy", null, true, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, sale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { exchanged.ToAmount } { exchanged.ToCurrency } tutarlık alış için yeterli değildir. Kullanılabilir bakiye { balance } { exchanged.ToCurrency } tutardır.";
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "ExchangeBuy", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                        }

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{exchanged.Amount} {exchanged.Currency} kasa döviz alışı eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "ExchangeBuy", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }


                }

            }

            return result;
        }

        public Result<DocumentBuyExchange> UpdateCashBuyExchange(CashExchangeModel exchanged)
        {
            Result<DocumentBuyExchange> result = new Result<DocumentBuyExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            using (ActionTimeEntities Db = new ActionTimeEntities())
            {

                var isExchange = Db.DocumentBuyExchange.FirstOrDefault(x => x.UID == exchanged.UID);

                try
                {
                    var fromcash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.Currency);
                    var tocash = ServiceHelper.GetCash(exchanged.LocationID, exchanged.ToCurrency);
                    var exchange = ServiceHelper.GetExchange(exchanged.DocumentDate);

                    DocumentBuyExchange self = new DocumentBuyExchange()
                    {
                        ActionTypeID = isExchange.ActionTypeID,
                        ActionTypeName = isExchange.ActionTypeName,
                        FromCashID = isExchange.FromCashID,
                        Amount = isExchange.Amount,
                        Currency = isExchange.Currency,
                        SaleExchangeRate = isExchange.SaleExchangeRate,
                        ToCashID = isExchange.ToCashID,
                        ToAmount = isExchange.ToAmount,
                        ToCurrency = isExchange.ToCurrency,
                        Date = isExchange.Date,
                        Description = isExchange.Description,
                        DocumentNumber = isExchange.DocumentNumber,
                        ExchangeRate = isExchange.ExchangeRate,
                        ID = isExchange.ID,
                        IsActive = isExchange.IsActive,
                        LocationID = isExchange.LocationID,
                        OurCompanyID = isExchange.OurCompanyID,
                        RecordDate = isExchange.RecordDate,
                        RecordEmployeeID = isExchange.RecordEmployeeID,
                        RecordIP = isExchange.RecordIP,
                        ReferenceID = isExchange.ReferenceID,
                        UpdateDate = isExchange.UpdateDate,
                        UpdateEmployee = isExchange.UpdateEmployee,
                        UpdateIP = isExchange.UpdateIP,
                        SlipPath = isExchange.SlipPath,
                        SlipDocument = isExchange.SlipDocument,
                        EnvironmentID = isExchange.EnvironmentID,
                        ResultID = isExchange.ResultID,
                        SlipDate = isExchange.SlipDate,
                        SlipNumber = isExchange.SlipNumber,
                        UID = isExchange.UID
                    };

                    isExchange.ReferenceID = exchanged.ReferanceID;
                    isExchange.LocationID = exchanged.LocationID;
                    isExchange.FromCashID = fromcash.ID;
                    isExchange.ToCashID = tocash.ID;
                    isExchange.Date = exchanged.DocumentDate;
                    isExchange.Amount = exchanged.Amount;
                    isExchange.ToCurrency = exchanged.ToCurrency;
                    isExchange.Currency = exchanged.Currency;
                    isExchange.ToAmount = (exchanged.Amount * exchanged.SaleExchangeRate);
                    isExchange.SaleExchangeRate = exchanged.SaleExchangeRate;
                    isExchange.Description = exchanged.Description;
                    isExchange.ExchangeRate = exchanged.Currency == "USD" ? exchange.USDA : exchanged.Currency == "EUR" ? exchange.EURA : 1;
                    isExchange.UpdateDate = exchanged.ProcessDate;
                    isExchange.UpdateEmployee = _employee.ID;
                    isExchange.UpdateIP = _ip;
                    isExchange.IsActive = exchanged.IsActive;
                    isExchange.SlipDocument = !string.IsNullOrEmpty(exchanged.SlipDocument) ? exchanged.SlipDocument : self.SlipDocument;
                    isExchange.SlipPath = !string.IsNullOrEmpty(exchanged.SlipPath) ? exchanged.SlipPath : self.SlipPath;
                    isExchange.SlipDate = exchanged.SlipDate;
                    isExchange.SlipNumber = exchanged.SlipNumber;

                    Db.SaveChanges();

                    Db.RemoveAllAccountActions(isExchange.ID, isExchange.UID);

                    if (isExchange.IsActive == true)
                    {
                        ServiceHelper.AddCashAction(isExchange.FromCashID, isExchange.LocationID, _employee.ID, isExchange.ActionTypeID, isExchange.Date, isExchange.ActionTypeName, isExchange.ID, isExchange.Date, isExchange.DocumentNumber, isExchange.Description, 1, isExchange.Amount, 0, isExchange.Currency, null, null, isExchange.UpdateEmployee, isExchange.UpdateDate, isExchange.UID.Value);
                        ServiceHelper.AddCashAction(isExchange.ToCashID, isExchange.LocationID, _employee.ID, isExchange.ActionTypeID, isExchange.Date, isExchange.ActionTypeName, isExchange.ID, isExchange.Date, isExchange.DocumentNumber, isExchange.Description, -1, 0, isExchange.ToAmount, isExchange.ToCurrency, null, null, isExchange.UpdateEmployee, isExchange.UpdateDate, isExchange.UID.Value);
                    }

                    result.IsSuccess = true;
                    result.Message = $"{isExchange.ID} ID li {isExchange.Date} tarihli {isExchange.Amount} {isExchange.Currency} kasa döviz satışı başarı ile güncellendi";

                    var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentBuyExchange>(self, isExchange, ServiceHelper.getIgnorelist());
                    ServiceHelper.AddApplicationLog("Location", "Cash", "Update", isExchange.ID.ToString(), "Cash", "ExchangeSale", isequal, true, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                }
                catch (Exception ex)
                {
                    result.Message = $"{exchanged.Amount} {exchanged.Currency} kasa döviz satışı Güncellenemedi : {ex.Message}";
                    ServiceHelper.AddApplicationLog("Location", "Cash", "Update", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, exchanged.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                }


            }

            return result;
        }

        public Result<DocumentSaleExchange> DeleteBuyExchange(Guid? id)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == id);
                    DateTime processDate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime ?? DateTime.UtcNow;

                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = processDate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            ServiceHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            ServiceHelper.AddCashAction(isCash.ToCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.ToAmount, 0, isCash.ToCurrency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, processDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }

            }

            return result;
        }




        public Result<DocumentSalaryPayment> AddSalaryPayment(SalaryPaymentModel payment)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (payment != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var exchange = ServiceHelper.GetExchange(payment.DocumentDate);

                    try
                    {
                        var balance = Db.GetCashBalance(payment.LocationID, payment.FromCashID, payment.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= payment.Amount)
                        {
                            DocumentSalaryPayment salaryPayment = new DocumentSalaryPayment();

                            salaryPayment.ActionTypeID = payment.ActionTypeID;
                            salaryPayment.ActionTypeName = payment.ActionTypeName;
                            salaryPayment.ToEmployeeID = payment.EmployeeID;
                            salaryPayment.Amount = payment.Amount;
                            salaryPayment.FromCashID = payment.FromCashID;
                            salaryPayment.Currency = payment.Currency;
                            salaryPayment.Date = payment.DocumentDate;
                            salaryPayment.Description = payment.Description;
                            salaryPayment.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "SAP");
                            salaryPayment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA : payment.Currency == "EUR" ? exchange.EURA : 1;
                            salaryPayment.FromBankAccountID = payment.FromBankID;
                            salaryPayment.IsActive = payment.IsActive;
                            salaryPayment.LocationID = payment.LocationID;
                            salaryPayment.OurCompanyID = _company.ID;
                            salaryPayment.RecordDate = payment.ProcessDate;
                            salaryPayment.RecordEmployeeID = _employee.ID;
                            salaryPayment.RecordIP = _ip;
                            salaryPayment.SystemAmount = _company.Currency == payment.Currency ? payment.Amount : payment.Amount * salaryPayment.ExchangeRate;
                            salaryPayment.SystemCurrency = _company.Currency;
                            salaryPayment.SalaryTypeID = payment.SalaryTypeID;
                            salaryPayment.UID = payment.UID;
                            salaryPayment.EnvironmentID = payment.EnvironmentID;
                            salaryPayment.ReferenceID = payment.ReferanceID;
                            salaryPayment.ResultID = payment.ResultID;
                            salaryPayment.CategoryID = payment.CategoryID;

                            Db.DocumentSalaryPayment.Add(salaryPayment);
                            Db.SaveChanges();

                            // cari hesap işlemesi

                            if (salaryPayment.FromBankAccountID > 0)
                            {
                                ServiceHelper.AddBankAction(salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.FromBankAccountID, null, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value);
                            }
                            else if (salaryPayment.FromCashID > 0)
                            {
                                ServiceHelper.AddCashAction(salaryPayment.FromCashID, salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value);
                            }

                            //maaş hesap işlemi
                            ServiceHelper.AddEmployeeAction(salaryPayment.ToEmployeeID, salaryPayment.LocationID, salaryPayment.ActionTypeID, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.Description, 1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.SalaryType, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value, salaryPayment.DocumentNumber, salaryPayment.CategoryID.Value);

                            result.IsSuccess = true;
                            result.Message = "Maaş Avans ödemesi başarı ile eklendi";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", salaryPayment.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, salaryPayment);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { payment.Amount } { payment.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { payment.Currency } tutardır.";
                            ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result UpdateSalaryPayment(SalaryPaymentModel payment)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPayment = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == payment.UID);
                var exchange = ServiceHelper.GetExchange(payment.DocumentDate);


                if (isPayment != null)
                {
                    try
                    {

                        DocumentSalaryPayment self = new DocumentSalaryPayment()
                        {
                            ActionTypeID = isPayment.ActionTypeID,
                            ActionTypeName = isPayment.ActionTypeName,
                            Amount = isPayment.Amount,
                            ToEmployeeID = isPayment.ToEmployeeID,
                            FromCashID = isPayment.FromCashID,
                            Currency = isPayment.Currency,
                            Date = isPayment.Date,
                            Description = isPayment.Description,
                            DocumentNumber = isPayment.DocumentNumber,
                            ExchangeRate = isPayment.ExchangeRate,
                            ID = isPayment.ID,
                            FromBankAccountID = isPayment.FromBankAccountID,
                            IsActive = isPayment.IsActive,
                            LocationID = isPayment.LocationID,
                            OurCompanyID = isPayment.OurCompanyID,
                            RecordDate = isPayment.RecordDate,
                            RecordEmployeeID = isPayment.RecordEmployeeID,
                            RecordIP = isPayment.RecordIP,
                            ReferenceID = isPayment.ReferenceID,
                            SystemAmount = isPayment.SystemAmount,
                            SystemCurrency = isPayment.SystemCurrency,
                            UpdateDate = isPayment.UpdateDate,
                            UpdateEmployee = isPayment.UpdateEmployee,
                            UpdateIP = isPayment.UpdateIP,
                            CategoryID = isPayment.CategoryID,
                            EnvironmentID = isPayment.EnvironmentID,
                            ResultID = isPayment.ResultID,
                            SalaryTypeID = isPayment.SalaryTypeID,
                            UID = isPayment.UID,
                            SalaryType = isPayment.SalaryType
                        };

                        isPayment.CategoryID = payment.CategoryID;
                        isPayment.LocationID = payment.LocationID;
                        isPayment.Currency = payment.Currency;
                        isPayment.Date = payment.DocumentDate;
                        isPayment.ToEmployeeID = payment.EmployeeID;
                        isPayment.Amount = payment.Amount;
                        isPayment.FromCashID = payment.FromCashID;
                        isPayment.Description = payment.Description;
                        isPayment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA : payment.Currency == "EUR" ? exchange.EURA : 1;
                        isPayment.FromBankAccountID = payment.FromBankID;
                        isPayment.SalaryTypeID = payment.SalaryTypeID;
                        isPayment.UpdateDate = payment.ProcessDate;
                        isPayment.UpdateEmployee = _employee.ID;
                        isPayment.UpdateIP = _ip;
                        isPayment.SystemAmount = _company.Currency == payment.Currency ? payment.Amount : payment.Amount * isPayment.ExchangeRate;
                        isPayment.SystemCurrency = _company.Currency;
                        isPayment.IsActive = payment.IsActive;

                        Db.SaveChanges();

                        // cari hesap kayıtları silinir.
                        Db.RemoveAllAccountActions(isPayment.ID, isPayment.UID);

                        // hareketler eklenir

                        if (payment.IsActive == true)
                        {
                            if (payment.FromCashID > 0)
                            {
                                ServiceHelper.AddCashAction(isPayment.FromCashID, isPayment.LocationID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value);
                            }

                            if (payment.FromBankID > 0)
                            {
                                var bankactiontype = Db.BankActionType.FirstOrDefault(x => x.ID == 8);
                                ServiceHelper.AddBankAction(isPayment.LocationID, isPayment.ToEmployeeID, isPayment.FromBankAccountID, null, bankactiontype.ID, isPayment.Date, bankactiontype.Name, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value);
                            }

                            ServiceHelper.AddEmployeeAction(isPayment.ToEmployeeID, isPayment.LocationID, isPayment.ActionTypeID, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.Description, 1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.SalaryTypeID, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value, isPayment.DocumentNumber, isPayment.CategoryID.Value);
                        }

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi başarı ile güncellendi";


                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentSalaryPayment>(self, isPayment, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "DocumentSalaryPayment", "Update", isPayment.ID.ToString(), "UpdateCashSalaryPay", "SalaryPayment", isequal, true, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans güncellenemdi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "DocumentSalaryPayment", "Update", "-1", "UpdateCashSalaryPay", "SalaryPayment", null, false, $"{result.Message}", string.Empty, payment.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentSalaryPayment> DeleteSalaryPayment(Guid? id)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
                    DateTime? processdate = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID)?.LocalDateTime;
                    if (isCash != null)
                    {
                        try
                        {
                            isCash.IsActive = false;
                            isCash.UpdateDate = processdate;
                            isCash.UpdateEmployee = _employee.ID;
                            isCash.UpdateIP = _ip;

                            Db.SaveChanges();

                            Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

                            result.IsSuccess = true;
                            result.Message = "Maaş Avans ödemesi başarı ile iptal edildi";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Remove", isCash.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, isCash);
                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki maaş avans ödemesi iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Remove", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentBankTransfer> AddBankTransfer(BankTransferModel transfer)
        {
            Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (transfer != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var cash = ServiceHelper.GetCash(transfer.LocationID, transfer.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
                        var exchange = ServiceHelper.GetExchange(transfer.DocumentDate);

                        DocumentBankTransfer bankTransfer = new DocumentBankTransfer();

                        bankTransfer.ActionTypeID = transfer.ActionTypeID;
                        bankTransfer.ActionTypeName = transfer.ActionTypeName;
                        bankTransfer.Amount = transfer.Amount;
                        bankTransfer.Commission = transfer.Commission;
                        bankTransfer.FromCashID = cash.ID;
                        bankTransfer.Currency = transfer.Currency;
                        bankTransfer.Date = transfer.DocumentDate;
                        bankTransfer.Description = transfer.Description;
                        bankTransfer.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "BT");
                        bankTransfer.ExchangeRate = transfer.Currency == "USD" ? exchange.USDA : transfer.Currency == "EUR" ? exchange.EURA : 1;
                        bankTransfer.ToBankAccountID = transfer.ToBankID;
                        bankTransfer.IsActive = transfer.IsActive;
                        bankTransfer.LocationID = transfer.LocationID;
                        bankTransfer.OurCompanyID = _company.ID;
                        bankTransfer.RecordDate = transfer.ProcessDate;
                        bankTransfer.RecordEmployeeID = _employee.ID;
                        bankTransfer.RecordIP = _ip;
                        bankTransfer.SystemAmount = _company.Currency == transfer.Currency ? transfer.Amount : transfer.Amount * bankTransfer.ExchangeRate;
                        bankTransfer.SystemCurrency = _company.Currency;
                        bankTransfer.SlipNumber = transfer.SlipNumber;
                        bankTransfer.SlipDate = transfer.SlipDate;
                        bankTransfer.StatusID = transfer.StatusID ?? 1;
                        bankTransfer.EnvironmentID = transfer.EnvironmentID;
                        bankTransfer.ReferenceID = transfer.ReferanceID;
                        bankTransfer.UID = transfer.UID;
                        bankTransfer.SlipDocument = transfer.SlipDocument;
                        bankTransfer.SlipPath = transfer.SlipPath;
                        bankTransfer.ResultID = transfer.ResultID;

                        bankTransfer.ReferenceCode = ServiceHelper.BankReferenceCode(bankTransfer.OurCompanyID.Value);

                        Db.DocumentBankTransfer.Add(bankTransfer);
                        Db.SaveChanges();

                        if (new int?[] { 2, 3, 4, 5 }.Contains(bankTransfer.StatusID))
                        {
                            if (bankTransfer.Commission > 0)  // komisyonlu işlem ise
                            {
                                var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == bankTransfer.ID && x.Date == bankTransfer.Date && x.LocationID == bankTransfer.LocationID);
                                if (isExpense == null)
                                {
                                    CashExpenseModel expense = new CashExpenseModel();

                                    expense.ActionTypeID = 29;
                                    expense.ActionTypeName = "Masraf Ödeme Fişi";
                                    expense.Amount = bankTransfer.Commission.Value;
                                    expense.Currency = bankTransfer.Currency;
                                    expense.Description = bankTransfer.Description;
                                    expense.DocumentDate = bankTransfer.Date.Value;
                                    expense.EnvironmentID = bankTransfer.EnvironmentID;
                                    expense.CashID = cash.ID;
                                    expense.LocationID = location.LocationID;
                                    expense.SlipDate = bankTransfer.SlipDate;
                                    expense.SlipNumber = bankTransfer.SlipNumber;
                                    expense.SlipDocument = bankTransfer.SlipDocument;
                                    expense.UID = Guid.NewGuid();
                                    expense.ExpenseTypeID = 25;
                                    expense.ReferanceID = bankTransfer.ID;
                                    expense.ToBankAccountID = bankTransfer.ToBankAccountID;
                                    expense.SlipPath = bankTransfer.SlipPath;
                                    expense.ResultID = transfer.ResultID;

                                    var expenseresult = AddCashExpense(expense);
                                    result.Message += $" {expenseresult.Message}";
                                }

                            }

                            var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (cashaction == null)
                            {
                                ServiceHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                        }
                        else if (new int?[] { 6, 7 }.Contains(bankTransfer.StatusID))
                        {
                            var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (expaction != null)
                            {

                                ServiceHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, -1 * bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                            var expbank = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (expbank != null)
                            {
                                ServiceHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, -1 * bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                        }

                        if (new int?[] { 5 }.Contains(bankTransfer.StatusID))
                        {
                            var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (cashaction == null)
                            {
                                ServiceHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }

                        }

                        result.IsSuccess = true;
                        result.Message = "Havale / EFT bildirimi başarı ile eklendi";

                        // log atılır
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", bankTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, transfer.ProcessDate, _employee.FullName, _ip, string.Empty, bankTransfer);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, transfer.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentBankTransfer> UpdateBankTransfer(BankTransferModel transfer)
        {
            Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isTransfer = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == transfer.UID);

                if (isTransfer != null)
                {
                    try
                    {
                        var cash = ServiceHelper.GetCash(transfer.LocationID, transfer.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
                        var exchange = ServiceHelper.GetExchange(transfer.DocumentDate);

                        var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29); // masraf ödeme fişi

                        DocumentBankTransfer self = new DocumentBankTransfer()
                        {
                            ActionTypeID = isTransfer.ActionTypeID,
                            ActionTypeName = isTransfer.ActionTypeName,
                            Amount = isTransfer.Amount,
                            NetAmount = isTransfer.NetAmount,
                            FromCashID = isTransfer.FromCashID,
                            ToBankAccountID = isTransfer.ToBankAccountID,
                            Currency = isTransfer.Currency,
                            Date = isTransfer.Date,
                            Description = isTransfer.Description,
                            DocumentNumber = isTransfer.DocumentNumber,
                            ExchangeRate = isTransfer.ExchangeRate,
                            ID = isTransfer.ID,
                            IsActive = isTransfer.IsActive,
                            LocationID = isTransfer.LocationID,
                            OurCompanyID = isTransfer.OurCompanyID,
                            RecordDate = isTransfer.RecordDate,
                            RecordEmployeeID = isTransfer.RecordEmployeeID,
                            RecordIP = isTransfer.RecordIP,
                            ReferenceID = isTransfer.ReferenceID,
                            SystemAmount = isTransfer.SystemAmount,
                            SystemCurrency = isTransfer.SystemCurrency,
                            UpdateDate = isTransfer.UpdateDate,
                            UpdateEmployee = isTransfer.UpdateEmployee,
                            UpdateIP = isTransfer.UpdateIP,
                            SlipNumber = isTransfer.SlipNumber,
                            SlipDocument = isTransfer.SlipDocument,
                            SlipDate = isTransfer.SlipDate,
                            StatusID = isTransfer.StatusID,
                            TrackingNumber = isTransfer.TrackingNumber,
                            Commission = isTransfer.Commission,
                            UID = isTransfer.UID,
                            EnvironmentID = isTransfer.EnvironmentID,
                            ReferenceCode = isTransfer.ReferenceCode,
                            ResultID = isTransfer.ResultID,
                            SlipPath = isTransfer.SlipPath
                        };

                        var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.LocationID && x.Date == transfer.DocumentDate);


                        isTransfer.ReferenceID = transfer.ReferanceID;
                        isTransfer.Commission = transfer.Commission;
                        isTransfer.Date = transfer.DocumentDate;
                        isTransfer.FromCashID = cash.ID;
                        isTransfer.SlipDate = transfer.SlipDate;
                        isTransfer.SlipNumber = transfer.SlipNumber;
                        isTransfer.ToBankAccountID = transfer.ToBankID ?? (int?)null;
                        isTransfer.Amount = transfer.Amount;
                        isTransfer.Description = transfer.Description;
                        isTransfer.ExchangeRate = transfer.Currency == "USD" ? exchange.USDA : transfer.Currency == "EUR" ? exchange.EURA : 1;
                        isTransfer.UpdateDate = transfer.ProcessDate;
                        isTransfer.UpdateEmployee = _employee.ID;
                        isTransfer.UpdateIP = _ip;
                        isTransfer.SystemAmount = _company.Currency == transfer.Currency ? transfer.Amount : transfer.Amount * isTransfer.ExchangeRate;
                        isTransfer.SystemCurrency = _company.Currency;
                        isTransfer.StatusID = transfer.StatusID;
                        isTransfer.TrackingNumber = transfer.TrackingNumber;
                        isTransfer.ReferenceCode = transfer.ReferanceCode;
                        isTransfer.LocationID = transfer.LocationID;
                        isTransfer.Currency = transfer.Currency;
                        isTransfer.IsActive = transfer.IsActive;
                        isTransfer.SlipDocument = !string.IsNullOrEmpty(transfer.SlipDocument) ? transfer.SlipDocument : self.SlipDocument;
                        isTransfer.SlipPath = !string.IsNullOrEmpty(transfer.SlipPath) ? transfer.SlipPath : self.SlipPath;
                        isTransfer.ResultID = isTransfer.ResultID == null ? dayresult.ID : isTransfer.ResultID;


                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message += "Banka Havale / EFT işlemi Güncellendi";

                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, transfer.ProcessDate, _employee.FullName, _ip, string.Empty, null);


                        // 01. mevcut kasa çıkış hareketi, banka giriş hareketi ve kasa masraf hareketi varsa sil
                        Db.RemoveAllAccountActions(isTransfer.ID, isTransfer.UID);
                        // masraf hareketi.
                        var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == self.ID && x.Date == self.Date && x.LocationID == self.LocationID);

                        if (isexpenseexists != null)
                        {
                            Db.RemoveAllAccountActions(isexpenseexists.ID, isexpenseexists.UID);
                        }

                        // 02. yeni kasa çıkış hareketlerini ekle
                        if (isTransfer.IsActive == true)
                        {
                            if (new int?[] { 2, 3, 4, 5 }.Contains(isTransfer.StatusID))
                            {
                                // 01. yeni kasa çıkış hareketi komisyonsuz tutar miktarınca eklenir
                                var mainamount = (isTransfer.Amount - isTransfer.Commission);

                                ServiceHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, mainamount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                                result.Message += $" kasa çıkış işlemi yapıldı. ";

                                if (transfer.Commission > 0)  // komisyonlu işlem ise
                                {

                                    // 02. yeni kasa masraf evrağı komisyon tutarı miktarınca eklenir 

                                    CashExpenseModel expense = new CashExpenseModel();

                                    expense.ActionTypeID = 29;
                                    expense.ActionTypeName = "Masraf Ödeme Fişi";
                                    expense.Amount = isTransfer.Commission.Value;
                                    expense.Currency = isTransfer.Currency;
                                    expense.Description = isTransfer.Description;
                                    expense.DocumentDate = isTransfer.Date.Value;
                                    expense.EnvironmentID = isTransfer.EnvironmentID;
                                    expense.CashID = cash.ID;
                                    expense.LocationID = location.LocationID;
                                    expense.SlipDate = isTransfer.SlipDate;
                                    expense.SlipNumber = isTransfer.SlipNumber;
                                    expense.SlipDocument = isTransfer.SlipDocument;
                                    expense.UID = Guid.NewGuid();
                                    expense.ExpenseTypeID = 25;
                                    expense.ReferanceID = isTransfer.ID;
                                    expense.ResultID = isTransfer.ResultID;
                                    expense.ToBankAccountID = isTransfer.ToBankAccountID;
                                    expense.SlipPath = isTransfer.SlipPath;
                                    expense.IsActive = isTransfer.IsActive.Value;
                                    expense.ProcessDate = transfer.ProcessDate;

                                    var expenseresult = AddCashExpense(expense);
                                    result.Message += $" {expenseresult.Message}";
                                }
                            }
                            if (new int?[] { 7 }.Contains(isTransfer.StatusID))
                            {
                                isTransfer.IsActive = false;

                                Db.SaveChanges();
                            }
                            if (new int?[] { 5 }.Contains(isTransfer.StatusID))
                            {
                                ServiceHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                                result.Message += $" banka giriş işlemi yapıldı. ";
                            }
                        }
                        else
                        {
                            result.Message += $" Havale / EFT bilgisi pasife çekildi. ";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Havale / EFT güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, transfer.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }

            }

            return result;
        }

        public Result<DocumentBankTransfer> DeleteCashBankTransfer(Guid? id)
        {
            Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {

                    var bankTransfer = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == id);

                    if (bankTransfer != null)
                    {
                        DateTime? processdate = Db.Location.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID)?.LocalDateTime;

                        try
                        {

                            bankTransfer.IsActive = false;
                            bankTransfer.UpdateDate = processdate;
                            bankTransfer.UpdateEmployee = _employee.ID;
                            bankTransfer.UpdateIP = _ip;

                            Db.SaveChanges();

                            var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == bankTransfer.ID && x.Date == bankTransfer.Date && x.LocationID == bankTransfer.LocationID);

                            if (isexpenseexists != null)
                            {
                                var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
                                if (iscashexpenseexit != null)
                                {
                                    isexpenseexists.IsActive = false;
                                    isexpenseexists.UpdateDate = processdate;
                                    isexpenseexists.UpdateEmployee = _employee.ID;
                                    isexpenseexists.UpdateIP = _ip;
                                    Db.SaveChanges();
                                }

                                Db.DocumentCashExpense.Remove(isexpenseexists);
                                Db.SaveChanges();
                            }

                            Db.RemoveAllAccountActions(bankTransfer.ID, bankTransfer.UID);
                            Db.RemoveAllAccountActions(isexpenseexists.ID, isexpenseexists.UID);


                            result.IsSuccess = true;
                            result.Message = $"{bankTransfer.ID} ID li {bankTransfer.Date} tarihli {bankTransfer.Amount} {bankTransfer.Currency} tutarındaki havale eft başarı ile iptal edildi";

                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", bankTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{bankTransfer.Amount} {bankTransfer.Currency} tutarındaki havale eft iptal edilemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Location", "Cash", "Remove", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result AddSalaryEarn(SalaryEarnModel salary)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (salary != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var setcardparam = Db.SetcardParameter.Where(x => x.Year <= salary.DocumentDate.Year && x.OurCompanyID == _company.ID).OrderByDescending(x => x.Year).FirstOrDefault();
                        var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == salary.EmployeeID);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == salary.LocationID);
                        var exchange = ServiceHelper.GetExchange(salary.DocumentDate);
                        double salaryMultiplier = Db.GetSalaryMultiplier(salary.LocationID, salary.EmployeeID, salary.DocumentDate).FirstOrDefault() ?? 0;

                        var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == salary.EmployeeID && x.DateStart <= salary.DocumentDate && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();
                        double? unitprice = empunits?.Hourly ?? 0;

                        var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == salary.LocationID && x.StatsID == 2 && x.OptionID == 3);
                        if (location.OurCompanyID == 1 && locationstats != null)
                        {
                            unitprice = unitprice + 1;
                        }

                        unitprice = unitprice * salaryMultiplier;

                        var SalaryEarn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == salary.LocationID && x.EmployeeID == salary.EmployeeID && x.Date == salary.DocumentDate && x.ResultID == salary.ResultID);

                        if (SalaryEarn == null)
                        {


                            DocumentSalaryEarn salaryEarn = new DocumentSalaryEarn();

                            salaryEarn.ActionTypeID = salary.ActionTypeID;
                            salaryEarn.ActionTypeName = salary.ActionTypeName;
                            salaryEarn.EmployeeID = salary.EmployeeID;
                            salaryEarn.QuantityHour = salary.QuantityHour;
                            salaryEarn.UnitPrice = unitprice;
                            salaryEarn.TotalAmount = (salaryEarn.QuantityHour * salaryEarn.UnitPrice);
                            salaryEarn.UnitPriceMultiplierApplied = salaryMultiplier;

                            salaryEarn.Currency = salary.Currency;
                            salaryEarn.Date = salary.DocumentDate;
                            salaryEarn.Description = salary.Description;
                            salaryEarn.DocumentNumber = ServiceHelper.GetDocumentNumber(salary.OurCompanyID, "SE");
                            salaryEarn.IsActive = true;
                            salaryEarn.LocationID = salary.LocationID;
                            salaryEarn.OurCompanyID = salary.OurCompanyID;
                            salaryEarn.RecordDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
                            salaryEarn.RecordEmployeeID = _employee.ID;
                            salaryEarn.RecordIP = _ip;
                            salaryEarn.UID = salary.UID;
                            salaryEarn.EnvironmentID = salary.EnvironmentID;
                            salaryEarn.ReferenceID = salary.ReferanceID;
                            salaryEarn.ResultID = salary.ResultID;
                            salaryEarn.SystemQuantityHour = salary.QuantityHour;
                            salaryEarn.SystemTotalAmount = (salaryEarn.QuantityHour * salaryEarn.UnitPrice);
                            salaryEarn.SystemUnitPrice = unitprice;
                            salaryEarn.CategoryID = salary.CategoryID;

                            salaryEarn.UnitFoodPrice = 0;
                            salaryEarn.QuantityHourSalary = salaryEarn.QuantityHour;
                            salaryEarn.QuantityHourFood = salaryEarn.QuantityHour;

                            if (employee.OurCompanyID == 2 && employee.AreaCategoryID == 2 && (employee.PositionID == 5 || employee.PositionID == 6))
                            {
                                salaryEarn.UnitFoodPrice = setcardparam != null ? setcardparam.Amount ?? 0 : 0;
                                salaryEarn.QuantityHourSalary = (salaryEarn.QuantityHour * 0.9);
                                salaryEarn.QuantityHourFood = (salaryEarn.QuantityHour * 0.9);
                            }



                            Db.DocumentSalaryEarn.Add(salaryEarn);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            ServiceHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, salaryEarn.ActionTypeID, salaryEarn.ActionTypeName, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmountSalary, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber, 3);
                            if (salaryEarn.TotalAmountFood > 0)
                            {
                                var setcartacttype = Db.CashActionType.FirstOrDefault(x => x.ID == 39);
                                ServiceHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, setcartacttype.ID, setcartacttype.Name, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmountFood, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber, 17);
                            }

                            result.IsSuccess = true;
                            result.Message = "Ücret Hakediş başarı ile eklendi";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", salaryEarn.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, salary.ProcessDate, _employee.FullName, _ip, string.Empty, salaryEarn);
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = "Ücret Hakedişi daha önce zaten var";

                            // log atılır
                            ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", SalaryEarn.ID.ToString(), "Salary", "Index", null, false, $"{result.Message}", string.Empty, salary.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                        }
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Ücret Hakediş eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Salary", "Insert", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, salary.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }

                }
            }

            return result;
        }

        public Result UpdateSalaryEarn(SalaryEarnModel salary)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isEarn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.UID == salary.UID);
                if (isEarn != null)
                {
                    try
                    {
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == salary.LocationID);
                        var locId = salary.LocationID;
                        var exchange = ServiceHelper.GetExchange(salary.DocumentDate);
                        var setcardparam = Db.SetcardParameter.Where(x => x.Year <= salary.DocumentDate.Year && x.OurCompanyID == _company.ID).OrderByDescending(x => x.Year).FirstOrDefault();
                        var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == salary.EmployeeID);
                        double salaryMultiplier = Db.GetSalaryMultiplier(salary.LocationID, salary.EmployeeID, salary.DocumentDate).FirstOrDefault() ?? 0;

                        var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == salary.EmployeeID && x.DateStart <= salary.DocumentDate && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();
                        double? unitprice = empunits?.Hourly ?? 0;

                        var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == salary.LocationID && x.StatsID == 2 && x.OptionID == 3);
                        if (location.OurCompanyID == 1 && locationstats != null)
                        {
                            unitprice = unitprice + 1;
                        }
                        unitprice = unitprice * salaryMultiplier;

                        var isEmp = salary.EmployeeID;

                        DocumentSalaryEarn self = new DocumentSalaryEarn()
                        {
                            ActionTypeID = isEarn.ActionTypeID,
                            ActionTypeName = isEarn.ActionTypeName,
                            TotalAmount = isEarn.TotalAmount,
                            EmployeeID = isEarn.EmployeeID,
                            Currency = isEarn.Currency,
                            Date = isEarn.Date,
                            Description = isEarn.Description,
                            DocumentNumber = isEarn.DocumentNumber,
                            QuantityHour = isEarn.QuantityHour,
                            ID = isEarn.ID,
                            UnitPrice = isEarn.UnitPrice,
                            IsActive = isEarn.IsActive,
                            LocationID = isEarn.LocationID,
                            OurCompanyID = isEarn.OurCompanyID,
                            RecordDate = isEarn.RecordDate,
                            RecordEmployeeID = isEarn.RecordEmployeeID,
                            RecordIP = isEarn.RecordIP,
                            ReferenceID = isEarn.ReferenceID,
                            UpdateDate = isEarn.UpdateDate,
                            UpdateEmployee = isEarn.UpdateEmployee,
                            UpdateIP = isEarn.UpdateIP,
                            CategoryID = isEarn.CategoryID,
                            EnvironmentID = isEarn.EnvironmentID,
                            SystemQuantityHour = isEarn.SystemQuantityHour,
                            SystemTotalAmount = isEarn.SystemTotalAmount,
                            SystemUnitPrice = isEarn.SystemUnitPrice,
                            QuantityHourFood = isEarn.QuantityHourFood,
                            QuantityHourSalary = isEarn.QuantityHourSalary,
                            ResultID = isEarn.ResultID,
                            TotalAmountFood = isEarn.TotalAmountFood,
                            TotalAmountLabor = isEarn.TotalAmountLabor,
                            TotalAmountSalary = isEarn.TotalAmountSalary,
                            UID = isEarn.UID,
                            UnitFoodPrice = isEarn.UnitFoodPrice,
                            UnitPriceMultiplierApplied = isEarn.UnitPriceMultiplierApplied

                        };

                        isEarn.ReferenceID = salary.ReferanceID;
                        isEarn.CategoryID = salary.CategoryID;
                        isEarn.EmployeeID = salary.EmployeeID;
                        isEarn.UnitPrice = (double?)unitprice;
                        isEarn.QuantityHour = (double)salary.QuantityHour;
                        isEarn.TotalAmount = (double)((double?)isEarn.UnitPrice * (double)isEarn.QuantityHour);
                        isEarn.UnitPriceMultiplierApplied = salaryMultiplier;
                        isEarn.Description = salary.Description;

                        isEarn.UpdateDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
                        isEarn.UpdateEmployee = _employee.ID;
                        isEarn.UpdateIP = _ip;
                        isEarn.SystemQuantityHour = isEarn.QuantityHour;
                        isEarn.SystemTotalAmount = isEarn.TotalAmount;
                        isEarn.SystemUnitPrice = unitprice;

                        isEarn.UnitFoodPrice = 0;//setcardparam != null ? setcardparam.Amount ?? 0 : 0;
                        isEarn.QuantityHourSalary = isEarn.QuantityHour;
                        isEarn.QuantityHourFood = isEarn.QuantityHour;

                        if (employee.OurCompanyID == 2 && employee.AreaCategoryID == 2 && (employee.PositionID == 5 || employee.PositionID == 6))
                        {
                            isEarn.UnitFoodPrice = setcardparam != null ? setcardparam.Amount ?? 0 : 0;
                            isEarn.QuantityHourSalary = (isEarn.QuantityHour * 0.9);
                            isEarn.QuantityHourFood = (isEarn.QuantityHour * 0.9);
                        }

                        Db.SaveChanges();

                        var empaction = Db.EmployeeCashActions.Where(x => x.ProcessUID == isEarn.UID).ToList();
                        Db.EmployeeCashActions.RemoveRange(empaction);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        ServiceHelper.AddEmployeeAction(isEarn.EmployeeID, isEarn.LocationID, isEarn.ActionTypeID, isEarn.ActionTypeName, isEarn.ID, isEarn.Date, isEarn.Description, 1, isEarn.TotalAmountSalary, 0, isEarn.Currency, null, null, null, isEarn.RecordEmployeeID, isEarn.RecordDate, isEarn.UID.Value, isEarn.DocumentNumber, 3);
                        if (isEarn.TotalAmountFood > 0)
                        {
                            var setcartacttype = Db.CashActionType.FirstOrDefault(x => x.ID == 39);
                            ServiceHelper.AddEmployeeAction(isEarn.EmployeeID, isEarn.LocationID, setcartacttype.ID, setcartacttype.Name, isEarn.ID, isEarn.Date, isEarn.Description, 1, isEarn.TotalAmountFood, 0, isEarn.Currency, null, null, null, isEarn.RecordEmployeeID, isEarn.RecordDate, isEarn.UID.Value, isEarn.DocumentNumber, 17);
                        }

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş başarı ile güncellendi";

                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentSalaryEarn>(self, isEarn, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "Salary", "Update", isEarn.ID.ToString(), "Salary", "Index", isequal, true, $"{result.Message}", string.Empty, salary.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Ücret Hakediş güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Salary", "Update", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, salary.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }
                }

            }

            return result;
        }

        public Result CheckLocationTicketSale(long id, DateTime processDate)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (id > 0)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var i = Db.CheckLocationTicketSale(id, _employee.ID, processDate, _ip, _company.ID).FirstOrDefault();
                    result.IsSuccess = true;
                    result.Message = "Ok";
                }
            }

            return result;
        }



        public bool CheckSalaryEarn(DateTime? date, int? locationid)
        {
            bool issuccess = false;

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                var location = db.Location.FirstOrDefault(x => x.LocationID == locationid);
                var dayresult = db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
                var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

                if (locschedule != null)
                {
                    var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
                    List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();


                    var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
                    var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

                    foreach (var emp in empids)
                    {
                        var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID);
                    }

                    issuccess = true;
                }
            }

            return issuccess;
        }

        public bool CalculateSalaryEarn(long? resultid, int employeeid, DateTime? date, int? locationid)
        {
            bool issuccess = false;

            if (employeeid > 0 && (resultid > 0 || date != null || locationid > 0))
            {

                using (ActionTimeEntities db = new ActionTimeEntities())
                {
                    var dayresult = db.VDayResult.FirstOrDefault(x => x.ID == resultid.Value);

                    if (dayresult == null)
                    {
                        date = date.Value.Date;
                        dayresult = db.VDayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
                    }


                    if (dayresult != null)
                    {
                        List<DayResultItemList> itemlist = new List<DayResultItemList>();

                        var items = db.DayResultItems.ToList();

                        var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date);
                        var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
                        var processDate = DateTime.UtcNow.AddHours(location.Timezone.Value);

                        if (locschedule != null)
                        {
                            var employeeschedule = db.Schedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);
                            var employeeshift = db.EmployeeShift.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);

                            double? durationhour = 0;

                            TimeSpan? duration = null;

                            if (employeeschedule != null && employeeshift != null)
                            {
                                DateTime? starttime = employeeschedule.ShiftDateStart;
                                if (employeeshift.ShiftDateStart > starttime)
                                {
                                    starttime = employeeshift.ShiftDateStart;
                                }

                                DateTime? finishtime = employeeschedule.ShiftdateEnd;
                                if (employeeshift.ShiftDateEnd < finishtime)
                                {
                                    finishtime = employeeshift.ShiftDateEnd;
                                }

                                if (finishtime != null && starttime != null)
                                {
                                    duration = (finishtime - starttime).Value;
                                    double? durationminute = (finishtime - starttime).Value.TotalMinutes;
                                    durationhour = (durationminute / 60);
                                }

                                // varmı yokmu
                                var existsitem = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
                                if (existsitem != null)
                                {
                                    db.DayResultItemList.Remove(existsitem);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    SystemHourQuantity = durationhour,
                                    UnitHourPrice = 0,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = _employee.ID,
                                    RecordIP = _ip,
                                    Duration = duration,
                                    SystemDuration = duration,
                                    HourQuantity = durationhour

                                }).FirstOrDefault());

                                var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
                                if (existsitem9 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem9);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = _employee.ID,
                                    RecordIP = _ip

                                }).FirstOrDefault()); ;

                                // maaş hakedişi dosya ve hareket olarak ekle


                                var existssalaryearn = db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ActionTypeID == 32 && x.IsActive == true);

                                if (existssalaryearn != null)
                                {

                                    SalaryEarnModel earn = new SalaryEarnModel();

                                    earn.ActionTypeID = 32;
                                    earn.ActionTypeName = "Ücret Hakediş";
                                    earn.Currency = location.Currency;
                                    earn.DocumentDate = dayresult.Date;
                                    earn.EmployeeID = employeeid;
                                    earn.EnvironmentID = 2;
                                    earn.LocationID = location.LocationID;
                                    earn.OurCompanyID = location.OurCompanyID;
                                    earn.QuantityHour = durationhour.Value;
                                    earn.ResultID = dayresult.ID;
                                    earn.TimeZone = location.Timezone;
                                    earn.UID = existssalaryearn.UID.Value;
                                    earn.Description = existssalaryearn.Description;
                                    earn.ProcessDate = processDate;

                                    var res = UpdateSalaryEarn(earn);  // log zaten var.

                                }
                                else
                                {

                                    SalaryEarnModel earn = new SalaryEarnModel();

                                    earn.ActionTypeID = 32;
                                    earn.ActionTypeName = "Ücret Hakediş";
                                    earn.Currency = location.Currency;
                                    earn.DocumentDate = dayresult.Date;
                                    earn.EmployeeID = employeeid;
                                    earn.EnvironmentID = 2;
                                    earn.LocationID = location.LocationID;
                                    earn.OurCompanyID = location.OurCompanyID;
                                    earn.QuantityHour = durationhour.Value;
                                    earn.ResultID = dayresult.ID;
                                    earn.TimeZone = location.Timezone;
                                    earn.UID = Guid.NewGuid();
                                    earn.ProcessDate = processDate;


                                    var res = AddSalaryEarn(earn);  // log zaten var.
                                }

                            }
                            else
                            {

                                var existsitem8 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
                                if (existsitem8 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem8);
                                    db.SaveChanges();
                                }

                                var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
                                if (existsitem9 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem9);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    SystemHourQuantity = 0,
                                    UnitHourPrice = 0,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = _employee.ID,
                                    RecordIP = _ip

                                }).FirstOrDefault());

                                itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = _employee.ID,
                                    RecordIP = _ip

                                }).FirstOrDefault());


                            }
                        }

                        db.DayResultItemList.AddRange(itemlist);
                        db.SaveChanges();
                    }
                }
            }

            return issuccess;
        }


        public List<EmployeeShiftModel> GetEmployeeShifts(DateTime date, int locationid)
        {
            List<EmployeeShiftModel> list = new List<EmployeeShiftModel>();
            List<int> employeeids = new List<int>();

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var schedules = Db.Schedule.Where(x => x.LocationID == locationid && x.ShiftDate == date).ToList();
                var shifts = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.ShiftDate == date && x.IsWorkTime == true).ToList();

                employeeids.AddRange(schedules.Select(x => x.EmployeeID.Value).Distinct());
                employeeids.AddRange(shifts.Select(x => x.EmployeeID.Value).Distinct());

                employeeids = employeeids.Distinct().ToList();

                foreach (var empid in employeeids)
                {
                    var empschedule = schedules.FirstOrDefault(x => x.EmployeeID == empid);
                    var empshift = shifts.FirstOrDefault(x => x.EmployeeID == empid);

                    EmployeeShiftModel model = new EmployeeShiftModel();
                    model.EmployeeID = empid;
                    model.DocumentDate = date;
                    model.ScheduleDateStart = empschedule?.ShiftDateStart;
                    model.ScheduleDateEnd = empschedule?.ShiftdateEnd;
                    model.ScheduleDuration = new TimeSpan(0, empschedule.DurationMinute.Value, 0);
                    model.ShiftDateStart = empshift?.ShiftDateStart;
                    model.ShiftDateEnd = empshift?.ShiftDateEnd;
                    model.ShiftDuration = empshift?.ShiftDuration;

                    list.Add(model);
                }
            }

            return list;
        }


        public Result<DayResultDocuments> AddResultDocument(long? id, string filename, string path, int? typeid, string description, DateTime processdate)
        {
            Result<DayResultDocuments> result = new Result<DayResultDocuments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            int locationid = 0;

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

                        if (dayresult != null)
                        {
                            locationid = dayresult.LocationID;

                            DayResultDocuments resultDocuments = new DayResultDocuments();

                            resultDocuments.Date = dayresult.Date;
                            resultDocuments.Description = description;
                            resultDocuments.DocumentTypeID = typeid.Value;
                            resultDocuments.EnvironmentID = 2;

                            resultDocuments.IsActive = true;
                            resultDocuments.LocationID = dayresult.LocationID;
                            resultDocuments.RecordDate = processdate;
                            resultDocuments.RecordEmployeeID = _employee.ID;
                            resultDocuments.RecordIP = _ip;
                            resultDocuments.ResultID = dayresult.ID;
                            resultDocuments.FileName = filename;
                            resultDocuments.FilePath = path;

                            Db.DayResultDocuments.Add(resultDocuments);
                            Db.SaveChanges();


                            result.IsSuccess = true;
                            result.Message = $"{resultDocuments.ID} ID li {resultDocuments.Date} tarihli {resultDocuments.FileName} isimli dosya başarı ile eklendi.";

                            ServiceHelper.AddApplicationLog("Location", "Result Document", "Insert", resultDocuments.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, processdate, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{id} {filename} dosyası eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, processdate, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }


        public Result<DocumentCashRecorderSlip> AddCashRecorder(CashRecorderModel record)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (record != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentCashRecorderSlip cashRedord = new DocumentCashRecorderSlip();

                        cashRedord.ActionTypeID = record.ActionTypeID;
                        cashRedord.ActionTypeName = record.ActionTypeName;
                        cashRedord.NetAmount = record.NetAmount;
                        cashRedord.TotalAmount = record.TotalAmount;
                        cashRedord.Currency = record.Currency;
                        cashRedord.Date = record.DocumentDate;
                        cashRedord.DocumentNumber = ServiceHelper.GetDocumentNumber(_company.ID, "CR");
                        cashRedord.IsActive = record.IsActive;
                        cashRedord.LocationID = record.LocationID;
                        cashRedord.OurCompanyID = _company.ID;
                        cashRedord.RecordDate = record.ProcessDate;
                        cashRedord.RecordEmployeeID = _employee.ID;
                        cashRedord.RecordIP = _ip;
                        cashRedord.SlipDate = record.SlipDate;
                        cashRedord.SlipNumber = record.SlipNumber;
                        cashRedord.EnvironmentID = record.EnvironmentID;
                        cashRedord.UID = record.UID;
                        cashRedord.ResultID = record.ResultID;
                        cashRedord.CashAmount = record.CashAmount;
                        cashRedord.CreditAmount = record.CreditAmount;


                        cashRedord.SlipPath = record.SlipPath;
                        cashRedord.SlipFile = record.SlipFile;


                        Db.DocumentCashRecorderSlip.Add(cashRedord);
                        Db.SaveChanges();


                        result.IsSuccess = true;
                        result.Message = "Yazarkasa fişi başarı ile eklendi";

                        // log atılır
                        ServiceHelper.AddApplicationLog("Location", "Result Cash Recorder", "Insert", cashRedord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, record.ProcessDate, _employee.FullName, _ip, string.Empty, cashRedord);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Yazarkasa fişi eklenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Result Document", "Insert", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, record.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> UpdateCashRecorder(CashRecorderModel record)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isRecord = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.UID == record.UID);

                if (isRecord != null)
                {
                    try
                    {
                        DocumentCashRecorderSlip self = new DocumentCashRecorderSlip()
                        {
                            ActionTypeID = isRecord.ActionTypeID,
                            ActionTypeName = isRecord.ActionTypeName,
                            NetAmount = isRecord.NetAmount,
                            TotalAmount = isRecord.TotalAmount,
                            Currency = isRecord.Currency,
                            Date = isRecord.Date,
                            SlipDate = isRecord.SlipDate,
                            DocumentNumber = isRecord.DocumentNumber,
                            ID = isRecord.ID,
                            IsActive = isRecord.IsActive,
                            LocationID = isRecord.LocationID,
                            OurCompanyID = isRecord.OurCompanyID,
                            RecordDate = isRecord.RecordDate,
                            RecordEmployeeID = isRecord.RecordEmployeeID,
                            RecordIP = isRecord.RecordIP,
                            UpdateDate = isRecord.UpdateDate,
                            UpdateEmployee = isRecord.UpdateEmployee,
                            UpdateIP = isRecord.UpdateIP,
                            SlipNumber = isRecord.SlipNumber,
                            SlipFile = isRecord.SlipFile,
                            SlipPath = isRecord.SlipPath,
                            EnvironmentID = isRecord.EnvironmentID,
                            CashAmount = isRecord.CashAmount,
                            CreditAmount = isRecord.CreditAmount,
                            ResultID = isRecord.ResultID,
                            UID = isRecord.UID

                        };

                        isRecord.LocationID = record.LocationID;
                        isRecord.NetAmount = record.NetAmount;
                        isRecord.OurCompanyID = _company.ID;
                        isRecord.UpdateDate = record.ProcessDate;
                        isRecord.UpdateEmployee = _employee.ID;
                        isRecord.UpdateIP = _ip;
                        isRecord.SlipDate = record.SlipDate;
                        isRecord.SlipNumber = record.SlipNumber;
                        isRecord.TotalAmount = record.TotalAmount;
                        isRecord.CashAmount = record.CashAmount;
                        isRecord.CreditAmount = record.CreditAmount;
                        isRecord.ResultID = record.ResultID > 0 ? record.ResultID : self.ResultID;
                        isRecord.Date = record.DocumentDate;


                        if (isRecord.ResultID == null)
                        {
                            isRecord.ResultID = Db.DayResult.FirstOrDefault(x => x.LocationID == self.LocationID && x.Date == isRecord.Date)?.ID;
                        }

                        isRecord.SlipFile = !string.IsNullOrEmpty(record.SlipFile) ? record.SlipFile : self.SlipFile;
                        isRecord.SlipPath = !string.IsNullOrEmpty(record.SlipPath) ? record.SlipPath : self.SlipPath;

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{isRecord.ID} ID li {isRecord.SlipDate} tarihli {isRecord.SlipFile} isimli dosya ile beraber kayıt başarı ile güncellendi.";

                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentCashRecorderSlip>(self, isRecord, ServiceHelper.getIgnorelist());
                        ServiceHelper.AddApplicationLog("Location", "Result Cash Recorder", "Update", isRecord.ID.ToString(), "CashRecorder", "Index", isequal, true, $"{result.Message}", string.Empty, record.ProcessDate, _employee.FullName, _ip, string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isRecord.ID} {isRecord.SlipFile} dosyası güncellenemedi : {ex.Message}";
                        ServiceHelper.AddApplicationLog("Location", "Result Document", "Update", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, record.ProcessDate, _employee.FullName, _ip, string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> DeleteCashRecorder(Guid? id)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };


            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isRecord = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.UID == id);
                    DateTime? processdate = Db.Location.FirstOrDefault(x => x.LocationID == isRecord.LocationID)?.LocalDateTime;

                    if (isRecord != null)
                    {
                        try
                        {

                            isRecord.IsActive = false;
                            isRecord.UpdateDate = processdate;
                            isRecord.UpdateEmployee = _employee.ID;
                            isRecord.UpdateIP = _ip;

                            Db.SaveChanges();

                            result.IsSuccess = true;
                            result.Message = $"{isRecord.ID} ID li {isRecord.SlipDate} tarihli kayıt başarı ile silindi.";

                            ServiceHelper.AddApplicationLog("Location", "Result Cash Recorder", "Remove", isRecord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isRecord.TotalAmount} {isRecord.Currency} tarihli kayıt başarı ile silinemedi : {ex.Message}";
                            ServiceHelper.AddApplicationLog("Location", "Result Document", "Remove", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, processdate.Value, _employee.FullName, _ip, string.Empty, null);
                        }
                    }
                }
            }


            return result;
        }

        public Result<ActionRowResult> CheckResultBackward(long id, bool islocal)
        {
            Result<ActionRowResult> result = new Result<ActionRowResult>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isResult = Db.DayResult.FirstOrDefault(x => x.ID == id);

                if (isResult != null)
                {

                    var location = Db.Location.FirstOrDefault(x => x.LocationID == isResult.LocationID);
                    var datelist = Db.DateList.FirstOrDefault(x => x.DateKey == isResult.Date);

                    var action = Db.Action.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date);
                    var actionrow = Db.ActionRow.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date);
                    var actionrowresult = Db.ActionRowResult.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.ResultDate == isResult.Date);

                    List<int> cashsales = new int[] { 10, 21, 24, 28 }.ToList();
                    List<int> cashprocess = new int[] { 23, 27 }.ToList();
                    List<int> cardsales = new int[] { 1, 3, 5 }.ToList();
                    List<int> maas = new int[] { 3, 31 }.ToList();
                    List<int> hakedisid = new int[] { 32, 36, 39 }.ToList();
                    List<int> cashexpense = new int[] { 4, 29 }.ToList();
                    List<int> cashexchange = new int[] { 25, 40 }.ToList();
                    List<int> bankeft = new int[] { 11, 30 }.ToList();

                    var cashActions = Db.VCashActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == _company.Currency).ToList();
                    var bankActions = Db.VBankActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == _company.Currency).ToList();
                    var emplActions = Db.VEmployeeCashActions.Where(x => x.LocationID == isResult.LocationID && x.ProcessDate == isResult.Date && x.Currency == _company.Currency).ToList();

                    var cashtotal = cashActions.Where(x => cashsales.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var credittotal = bankActions.Where(x => cardsales.Contains(x.BankActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var maastotal = cashActions.Where(x => maas.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var hakedis = emplActions.Where(x => hakedisid.Contains(x.ActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var expensetotal = cashActions.Where(x => cashexpense.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;

                    var cashrecorder = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date);
                    var envelope = Db.DayResultDocuments.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date && x.ResultID == isResult.ID);


                    try
                    {
                        if (action == null)
                        {
                            var newaction = new Entity.Action()
                            {
                                ActionDate = isResult.Date,
                                ActionUID = Guid.NewGuid(),
                                LocationID = isResult.LocationID,
                                Metarials = 0,
                                RecordDate = location.LocalDateTime,
                                StateID = isResult.StateID,
                                Week = datelist.WeekNumber,
                                Year = datelist.WeekYear
                            };
                            Db.Action.Add(newaction);
                            Db.SaveChanges();

                            action = newaction;
                        }


                        if (actionrow == null)
                        {
                            var newactionrow = new Entity.ActionRow()
                            {
                                Date = isResult.Date,
                                ActionRowUID = Guid.NewGuid(),
                                LocationID = isResult.LocationID,
                                ActionID = action.ActionID,
                                StateID = isResult.StateID,
                                Week = datelist.WeekNumber,
                                Year = datelist.WeekYear,
                                DateWMonth = $"{datelist.Day} {datelist.MonthName.Substring(0, 3)} {datelist.Year}",
                                Day = datelist.Day
                            };
                            Db.ActionRow.Add(newactionrow);
                            Db.SaveChanges();

                            actionrow = newactionrow;
                        }

                        if (actionrowresult == null)
                        {
                            var newactionrowresult = new Entity.ActionRowResult()
                            {
                                ResultDate = isResult.Date,
                                ActionRowID = actionrow.ID,
                                LocationID = isResult.LocationID,
                                ActionID = action.ActionID,
                                StateID = isResult.StateID,
                                StatusID = 1,
                                EmployeeID = _employee.ID,
                                Cash = cashtotal,
                                Credit = credittotal,
                                LaborPayed = hakedis,
                                LaborPayedPayed = maastotal * -1,
                                Expense = expensetotal * -1,
                                CashIN = cashtotal - hakedis - (expensetotal * -1),
                                ZTime = cashrecorder != null ? cashrecorder.SlipDate : null,
                                ZNumber = cashrecorder != null ? cashrecorder.SlipNumber : null,
                                ZNetTotal = cashrecorder != null ? cashrecorder.NetAmount : null,
                                ZGeneralTotal = cashrecorder != null ? cashrecorder.TotalAmount : null,
                                IsSendPackage = false,
                                IsReceivePackage = false,
                                EnvelopeFile = envelope != null ? envelope.FileName : null,
                                IsMaster = true,
                                NotOpened = false,
                                AdminDescription = isResult.Description
                            };

                            Db.ActionRowResult.Add(newactionrowresult);
                            Db.SaveChanges();

                            actionrowresult = newactionrowresult;

                            ActionRowDocument ardocument = new ActionRowDocument()
                            {
                                ActionID = action.ActionID,
                                ActionRowID = actionrow.ID,
                                Date = isResult.Date,
                                DocumentTypeID = 1,
                                LocationID = isResult.LocationID,
                                EmployeeID = _employee.ID,
                                FileName = envelope != null ? envelope.FileName : null,
                                Description = "Envelope File",
                                RecordDate = location.LocalDateTime
                            };

                            Db.ActionRowDocument.Add(ardocument);
                            Db.SaveChanges();

                            // dosyayı kopyala




                            if (envelope != null && !string.IsNullOrEmpty(envelope.FileName) && !islocal)
                            {
                                try
                                {
                                    string fileName = envelope.FileName;
                                    string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
                                    string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
                                    string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                                    string destFile = System.IO.Path.Combine(targetPath, fileName);
                                    System.IO.File.Copy(sourceFile, destFile, true);
                                }
                                catch (Exception ex)
                                {
                                }
                            }





                            result.IsSuccess = true;
                            result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sisteme eklendi.";

                            ServiceHelper.AddApplicationLog("Office", "ActionRowResult", "Insert", newactionrowresult.ResultID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), _employee.FullName, _ip, string.Empty, newactionrowresult);

                            return result;
                        }
                        else
                        {
                            // 1 ocak 2020 den büyükse güncelle küçük ise güncelleme
                            if (isResult.Date >= new DateTime(2020, 1, 1))
                            {
                                ActionRowResult self = new ActionRowResult()
                                {
                                    RecordDate = actionrowresult.RecordDate,
                                    ActionID = actionrowresult.ActionID,
                                    NotOpened = actionrowresult.NotOpened,
                                    Longitude = actionrowresult.Longitude,
                                    ResultID = actionrowresult.ResultID,
                                    ActionRowID = actionrowresult.ActionRowID,
                                    AdminDescription = actionrowresult.AdminDescription,
                                    Cash = actionrowresult.Cash,
                                    CashIN = actionrowresult.CashIN,
                                    Credit = actionrowresult.Credit,
                                    Currency = actionrowresult.Currency,
                                    Description = actionrowresult.Description,
                                    EmployeeID = actionrowresult.EmployeeID,
                                    EnvelopeFile = actionrowresult.EnvelopeFile,
                                    Expense = actionrowresult.Expense,
                                    IsActive = actionrowresult.IsActive,
                                    IsMaster = actionrowresult.IsMaster,
                                    IsMobile = actionrowresult.IsMobile,
                                    IsReceivePackage = actionrowresult.IsReceivePackage,
                                    IsSendPackage = actionrowresult.IsSendPackage,
                                    LaborPayed = actionrowresult.LaborPayed,
                                    LaborPayedPayed = actionrowresult.LaborPayedPayed,
                                    Latitude = actionrowresult.Latitude,
                                    LocationID = actionrowresult.LocationID,
                                    RecordEmployeeID = actionrowresult.RecordEmployeeID,
                                    ResultDate = actionrowresult.ResultDate,
                                    ResultState = actionrowresult.ResultState,
                                    StateID = actionrowresult.StateID,
                                    StatusID = actionrowresult.StatusID,
                                    SubStatusID = actionrowresult.SubStatusID,
                                    Total = actionrowresult.Total,
                                    UpdateDate = actionrowresult.UpdateDate,
                                    UpdateEmployeeID = actionrowresult.UpdateEmployeeID,
                                    ZGeneralTotal = actionrowresult.ZGeneralTotal,
                                    ZNetTotal = actionrowresult.ZNetTotal,
                                    ZNumber = actionrowresult.ZNumber,
                                    ZTime = actionrowresult.ZTime

                                };


                                actionrowresult.ResultDate = isResult.Date;
                                actionrowresult.ActionRowID = actionrow.ID;
                                actionrowresult.LocationID = isResult.LocationID;
                                actionrowresult.ActionID = action.ActionID;
                                actionrowresult.StateID = isResult.StateID;
                                actionrowresult.StatusID = 1;
                                actionrowresult.EmployeeID = _employee.ID;
                                actionrowresult.Cash = cashtotal;
                                actionrowresult.Credit = credittotal;
                                actionrowresult.LaborPayed = hakedis;
                                actionrowresult.LaborPayedPayed = maastotal * -1;
                                actionrowresult.Expense = expensetotal * -1;
                                actionrowresult.CashIN = cashtotal - hakedis - (expensetotal * -1);

                                actionrowresult.ZTime = cashrecorder != null ? cashrecorder.SlipDate : null;
                                actionrowresult.ZNumber = cashrecorder != null ? cashrecorder.SlipNumber : null;
                                actionrowresult.ZNetTotal = cashrecorder != null ? cashrecorder.NetAmount : null;
                                actionrowresult.ZGeneralTotal = cashrecorder != null ? cashrecorder.TotalAmount : null;
                                actionrowresult.IsSendPackage = false;
                                actionrowresult.IsReceivePackage = false;
                                actionrowresult.EnvelopeFile = envelope != null ? envelope.FileName : null;
                                actionrowresult.IsMaster = true;
                                actionrowresult.NotOpened = false;
                                actionrowresult.AdminDescription = isResult.Description;

                                Db.SaveChanges();

                                result.IsSuccess = true;
                                result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sistemde güncellendi.";

                                var isequal = ServiceHelper.PublicInstancePropertiesEqual<ActionRowResult>(self, actionrowresult, ServiceHelper.getIgnorelist());
                                ServiceHelper.AddApplicationLog("Location", "ActionRowResult", "Update", actionrowresult.ResultID.ToString(), "Result", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), _employee.FullName, _ip, string.Empty, null);


                                if (envelope != null && !string.IsNullOrEmpty(envelope.FileName) && !islocal)
                                {
                                    try
                                    {
                                        string fileName = envelope.FileName;
                                        string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
                                        string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
                                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                                        System.IO.File.Copy(sourceFile, destFile, true);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }

                            }
                            else
                            {
                                result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sisteme 1 Ocak 2020 tarihinden önce eklendiği için güncellenmedi.";
                                ServiceHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), _employee.FullName, _ip, string.Empty, null);

                            }


                        }


                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarfta hata oluştu. {ex.Message}";
                        ServiceHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), _employee.FullName, _ip, string.Empty, null);

                    }
                }


            }

            return result;
        }

        public Result LocationShiftStart(string Token, DateTime processDate, int LocationID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.LocationShiftStart(LocationID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public Result LocationShiftEnd(string Token, DateTime processDate, int LocationID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.LocationShiftEnd(LocationID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public Result EmployeeShiftStart(string Token, DateTime processDate, int LocationID, int EmployeeID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.EmployeeShiftStart(LocationID, EmployeeID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public Result EmployeeShiftEnd(string Token, DateTime processDate, int LocationID, int EmployeeID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.EmployeeShiftEnd(LocationID, EmployeeID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public Result EmployeeBreakStart(string Token, DateTime processDate, int LocationID, int EmployeeID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.EmployeeBreakStart(LocationID, EmployeeID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public Result EmployeeBreakEnd(string Token, DateTime processDate, int LocationID, int EmployeeID)
        {
            Result result = new Result();

            UfeServiceClient service = new UfeServiceClient(Token);
            string date = processDate.ToString("yyyy-MM-dd");

            var serviceresult = service.EmployeeBreakEnd(LocationID, EmployeeID, 3, 0, 0, date);

            if (serviceresult != null)
            {
                result.IsSuccess = serviceresult.IsSuccess;
                result.Message = serviceresult?.Message;
            }

            return result;
        }

        public List<SaleDayTotalModal> GetDailySale(DateTime processDate)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            List<SaleDayTotalModal> salelist = new List<SaleDayTotalModal>();

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isResult = Db.GetSaleToday(processDate).ToList();


                salelist.Add(new SaleDayTotalModal() { PaymethodID = 1, Currency = "TRL", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "TRL")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "TRL")?.Amount ?? 0 });
                salelist.Add(new SaleDayTotalModal() { PaymethodID = 1, Currency = "USD", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "USD")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "USD")?.Amount ?? 0 });
                salelist.Add(new SaleDayTotalModal() { PaymethodID = 1, Currency = "EUR", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "EUR")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 1 && x.Currency == "EUR")?.Amount ?? 0 });

                salelist.Add(new SaleDayTotalModal() { PaymethodID = 2, Currency = "TRL", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "TRL")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "TRL")?.Amount ?? 0 });
                salelist.Add(new SaleDayTotalModal() { PaymethodID = 2, Currency = "USD", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "USD")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "USD")?.Amount ?? 0 });
                salelist.Add(new SaleDayTotalModal() { PaymethodID = 2, Currency = "EUR", Quantity = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "EUR")?.Quantity ?? 0, Amount = isResult.FirstOrDefault(x => x.PaymethodID == 2 && x.Currency == "EUR")?.Amount ?? 0 });

            }

            return salelist;
        }
    }
}