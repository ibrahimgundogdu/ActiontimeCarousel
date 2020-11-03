using ActionForce.Entity;
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
                            ServiceHelper.AddCashAction(sale.FromCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, 1,  sale.Amount, 0, sale.Currency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);
                            ServiceHelper.AddCashAction(sale.ToCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, -1, 0, sale.ToAmount,  sale.ToCurrency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);

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
                        ServiceHelper.AddCashAction(isExchange.ToCashID, isExchange.LocationID, _employee.ID, isExchange.ActionTypeID, isExchange.Date, isExchange.ActionTypeName, isExchange.ID, isExchange.Date, isExchange.DocumentNumber, isExchange.Description, -1, 0, isExchange.ToAmount,  isExchange.ToCurrency, null, null, isExchange.UpdateEmployee, isExchange.UpdateDate, isExchange.UID.Value);
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

        //public Result UpdateSalaryPayment(SalaryPaymentModel payment)
        //{
        //    Result result = new Result()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPayment = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == payment.UID && x.ID == payment.ID);

        //        if (isPayment != null)
        //        {
        //            try
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == payment.LocationID);

        //                DocumentSalaryPayment self = new DocumentSalaryPayment()
        //                {
        //                    ActionTypeID = isPayment.ActionTypeID,
        //                    ActionTypeName = isPayment.ActionTypeName,
        //                    Amount = isPayment.Amount,
        //                    ToEmployeeID = isPayment.ToEmployeeID,
        //                    FromCashID = isPayment.FromCashID,
        //                    Currency = isPayment.Currency,
        //                    Date = isPayment.Date,
        //                    Description = isPayment.Description,
        //                    DocumentNumber = isPayment.DocumentNumber,
        //                    ExchangeRate = isPayment.ExchangeRate,
        //                    ID = isPayment.ID,
        //                    FromBankAccountID = isPayment.FromBankAccountID,
        //                    IsActive = isPayment.IsActive,
        //                    LocationID = isPayment.LocationID,
        //                    OurCompanyID = isPayment.OurCompanyID,
        //                    RecordDate = isPayment.RecordDate,
        //                    RecordEmployeeID = isPayment.RecordEmployeeID,
        //                    RecordIP = isPayment.RecordIP,
        //                    ReferenceID = isPayment.ReferenceID,
        //                    SystemAmount = isPayment.SystemAmount,
        //                    SystemCurrency = isPayment.SystemCurrency,
        //                    UpdateDate = isPayment.UpdateDate,
        //                    UpdateEmployee = isPayment.UpdateEmployee,
        //                    UpdateIP = isPayment.UpdateIP,
        //                    CategoryID = isPayment.CategoryID,
        //                    EnvironmentID = isPayment.EnvironmentID,
        //                    ResultID = isPayment.ResultID,
        //                    SalaryTypeID = isPayment.SalaryTypeID,
        //                    UID = isPayment.UID
        //                };

        //                isPayment.CategoryID = payment.CategoryID;
        //                isPayment.LocationID = payment.LocationID;
        //                isPayment.Currency = payment.Currency;
        //                isPayment.Date = payment.DocumentDate;
        //                isPayment.ToEmployeeID = payment.EmployeeID;
        //                isPayment.Amount = payment.Amount;
        //                isPayment.FromCashID = payment.FromCashID;
        //                isPayment.Description = payment.Description;
        //                isPayment.ExchangeRate = payment.ExchangeRate != null ? payment.ExchangeRate : self.ExchangeRate;
        //                isPayment.FromBankAccountID = payment.FromBankID;
        //                isPayment.SalaryTypeID = payment.SalaryTypeID;
        //                isPayment.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isPayment.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isPayment.UpdateIP = ServiceHelper.GetIPAddress();
        //                isPayment.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == payment.Currency ? payment.Amount : payment.Amount * isPayment.ExchangeRate;
        //                isPayment.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                isPayment.IsActive = payment.IsActive;

        //                Db.SaveChanges();

        //                // cari hesap kayıtları silinir.
        //                Db.RemoveAllAccountActions(payment.ID, payment.UID);

        //                // hareketler eklenir

        //                if (payment.IsActive == true)
        //                {
        //                    if (payment.FromCashID > 0)
        //                    {
        //                        ServiceHelper.AddCashAction(isPayment.FromCashID, isPayment.LocationID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value);
        //                    }

        //                    if (payment.FromBankID > 0)
        //                    {
        //                        var bankactiontype = Db.BankActionType.FirstOrDefault(x => x.ID == 8);
        //                        ServiceHelper.AddBankAction(isPayment.LocationID, isPayment.ToEmployeeID, isPayment.FromBankAccountID, null, bankactiontype.ID, isPayment.Date, bankactiontype.Name, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value);
        //                    }

        //                    ServiceHelper.AddEmployeeAction(isPayment.ToEmployeeID, isPayment.LocationID, isPayment.ActionTypeID, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.Description, 1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.SalaryTypeID, isPayment.UpdateEmployee, isPayment.UpdateDate, isPayment.UID.Value, isPayment.DocumentNumber, isPayment.CategoryID.Value);
        //                }

        //                result.IsSuccess = true;
        //                result.Message = "Maaş Avans ödemesi başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentSalaryPayment>(self, isPayment, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Update", isPayment.ID.ToString(), payment.Controller, "SalaryPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Maaş Avans güncellenemdi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Update", "-1", payment.Controller, "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentSalaryPayment> DeleteSalaryPayment(Guid? id)
        //{
        //    Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isCash.LocationID);
        //            if (isCash != null)
        //            {
        //                try
        //                {
        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                    isCash.UpdateEmployee = _employee.ID;
        //                    isCash.UpdateIP = _ip;

        //                    Db.SaveChanges();

        //                    Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

        //                    result.IsSuccess = true;
        //                    result.Message = "Maaş Avans ödemesi başarı ile iptal edildi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Remove", isCash.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isCash);
        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki maaş avans ödemesi iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "DocumentSalaryPayment", "Remove", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}




        //public Result<DocumentTicketSales> AddCashSale(CashSale sale, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (sale != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                DocumentTicketSales ticketSale = new DocumentTicketSales();

        //                ticketSale.ActionTypeID = sale.ActinTypeID;
        //                ticketSale.ActionTypeName = sale.ActionTypeName;
        //                ticketSale.Amount = sale.Amount;
        //                ticketSale.CashID = sale.CashID;
        //                ticketSale.Currency = sale.Currency;
        //                ticketSale.Date = sale.DocumentDate;
        //                ticketSale.Description = sale.Description;
        //                ticketSale.DocumentNumber = ServiceHelper.GetDocumentNumber(sale.OurCompanyID, "TS");
        //                ticketSale.ExchangeRate = sale.ExchangeRate;
        //                ticketSale.FromCustomerID = sale.FromCustomerID;
        //                ticketSale.IsActive = true;
        //                ticketSale.LocationID = sale.LocationID;
        //                ticketSale.OurCompanyID = sale.OurCompanyID;
        //                ticketSale.RecordDate = DateTime.UtcNow.AddHours(sale.TimeZone.Value);
        //                ticketSale.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                ticketSale.RecordIP = ServiceHelper.GetIPAddress();
        //                ticketSale.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * sale.ExchangeRate;
        //                ticketSale.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                ticketSale.ReferenceID = sale.ReferanceID;
        //                ticketSale.Quantity = sale.Quantity;
        //                ticketSale.PayMethodID = sale.PayMethodID;
        //                ticketSale.EnvironmentID = sale.EnvironmentID;
        //                ticketSale.UID = Guid.NewGuid();
        //                ticketSale.ResultID = sale.ResultID;

        //                Db.DocumentTicketSales.Add(ticketSale);
        //                Db.SaveChanges();

        //                // cari hesap işlemesi
        //                ServiceHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, ticketSale.Amount, 0, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate, ticketSale.UID.Value);

        //                result.IsSuccess = true;
        //                result.Message = "Bilet satış başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, ticketSale);

        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Bilet satış eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(sale.LocationID)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentTicketSales> EditCashSale(CashSale sale, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == sale.UID);

        //        if (isCash != null)
        //        {
        //            try
        //            {
        //                var cash = ServiceHelper.GetCash(sale.LocationID, sale.Currency);
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == sale.LocationID);
        //                var locId = isCash.LocationID;
        //                var exchange = ServiceHelper.GetExchange(sale.DocumentDate.Value);

        //                DocumentTicketSales self = new DocumentTicketSales()
        //                {
        //                    ActionTypeID = isCash.ActionTypeID,
        //                    ActionTypeName = isCash.ActionTypeName,
        //                    Amount = isCash.Amount,
        //                    CashID = isCash.CashID,
        //                    Currency = isCash.Currency,
        //                    Date = isCash.Date,
        //                    Description = isCash.Description,
        //                    DocumentNumber = isCash.DocumentNumber,
        //                    ExchangeRate = isCash.ExchangeRate,
        //                    ID = isCash.ID,
        //                    IsActive = isCash.IsActive,
        //                    LocationID = isCash.LocationID,
        //                    OurCompanyID = isCash.OurCompanyID,
        //                    RecordDate = isCash.RecordDate,
        //                    RecordEmployeeID = isCash.RecordEmployeeID,
        //                    RecordIP = isCash.RecordIP,
        //                    ReferenceID = isCash.ReferenceID,
        //                    SystemAmount = isCash.SystemAmount,
        //                    SystemCurrency = isCash.SystemCurrency,
        //                    UpdateDate = isCash.UpdateDate,
        //                    UpdateEmployee = isCash.UpdateEmployee,
        //                    UpdateIP = isCash.UpdateIP,
        //                    PayMethodID = isCash.PayMethodID,
        //                    EnvironmentID = isCash.EnvironmentID
        //                };
        //                isCash.ReferenceID = sale.ReferanceID;
        //                isCash.LocationID = sale.LocationID;
        //                isCash.CashID = cash.ID;
        //                isCash.Date = sale.DocumentDate;
        //                isCash.Quantity = sale.Quantity;
        //                isCash.PayMethodID = sale.PayMethodID;
        //                isCash.FromCustomerID = sale.FromCustomerID ?? (int?)null;
        //                isCash.Amount = sale.Amount;
        //                isCash.Description = sale.Description;
        //                isCash.ExchangeRate = sale.ExchangeRate != null ? sale.ExchangeRate : self.ExchangeRate;
        //                isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isCash.UpdateIP = ServiceHelper.GetIPAddress();
        //                isCash.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * isCash.ExchangeRate;
        //                isCash.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                isCash.IsActive = sale.IsActive;
        //                isCash.Currency = sale.Currency;

        //                Db.SaveChanges();

        //                //Cari hesap hareketleri temizlenir.

        //                Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

        //                // Aktif ise Cari hesap kaydı eklenir.

        //                if (isCash.IsActive == true)
        //                {
        //                    ServiceHelper.AddCashAction(isCash.CashID, isCash.LocationID, authentication.ActionEmployee.EmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
        //                }

        //                result.IsSuccess = true;
        //                result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satışı başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Sale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satışı güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentTicketSales> DeleteCashSale(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    //Cari hesap hareketleri temizlenir.

        //                    Db.RemoveAllAccountActions(isCash.ID, isCash.UID);


        //                    //ServiceHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


        //                    result.IsSuccess = true;
        //                    result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış başarı ile iptal edildi";

        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}









        //public Result<DocumentTicketSaleReturns> AddCashSaleReturn(SaleReturn sale, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (sale != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var balance = Db.GetCashBalance(sale.LocationID, sale.CashID, sale.DocumentDate).FirstOrDefault() ?? 0;

        //                if (balance >= sale.Amount)
        //                {
        //                    DocumentTicketSaleReturns ticketSale = new DocumentTicketSaleReturns();

        //                    ticketSale.ActionTypeID = sale.ActinTypeID;
        //                    ticketSale.ActionTypeName = sale.ActionTypeName;
        //                    ticketSale.Amount = sale.Amount;
        //                    ticketSale.CashID = sale.CashID;
        //                    ticketSale.Currency = sale.Currency;
        //                    ticketSale.Date = sale.DocumentDate;
        //                    ticketSale.Description = sale.Description;
        //                    ticketSale.DocumentNumber = ServiceHelper.GetDocumentNumber(sale.OurCompanyID, "TSR");
        //                    ticketSale.ExchangeRate = sale.ExchangeRate;
        //                    ticketSale.ToCustomerID = sale.ToCustomerID;
        //                    ticketSale.IsActive = true;
        //                    ticketSale.LocationID = sale.LocationID;
        //                    ticketSale.OurCompanyID = sale.OurCompanyID;
        //                    ticketSale.RecordDate = DateTime.UtcNow.AddHours(sale.TimeZone.Value);
        //                    ticketSale.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    ticketSale.RecordIP = ServiceHelper.GetIPAddress();
        //                    ticketSale.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * sale.ExchangeRate;
        //                    ticketSale.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                    ticketSale.ReferenceID = sale.ReferanceID;
        //                    ticketSale.Quantity = sale.Quantity;
        //                    ticketSale.PayMethodID = sale.PayMethodID;
        //                    ticketSale.EnvironmentID = 2;
        //                    ticketSale.UID = Guid.NewGuid();




        //                    Db.DocumentTicketSaleReturns.Add(ticketSale);
        //                    Db.SaveChanges();

        //                    // cari hesap işlemesi
        //                    ServiceHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, 0, ticketSale.Amount, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate, ticketSale.UID.Value);




        //                    result.IsSuccess = true;
        //                    result.Message = "Bilet satış iadesi başarı ile eklendi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, ticketSale);
        //                }
        //                else
        //                {
        //                    result.Message = $"Kasa bakiyesi { sale.Amount } { sale.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { sale.Currency } tutardır.";
        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Bilet satış iadesi eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentTicketSaleReturns> EditCashSaleReturn(SaleReturn sale, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.UID == sale.UID);

        //        if (isCash != null)
        //        {
        //            try
        //            {
        //                var cash = ServiceHelper.GetCash(sale.LocationID, sale.Currency);
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == sale.LocationID);
        //                var locId = isCash.LocationID;
        //                var exchange = ServiceHelper.GetExchange(sale.DocumentDate.Value);

        //                DocumentTicketSaleReturns self = new DocumentTicketSaleReturns()
        //                {
        //                    ActionTypeID = isCash.ActionTypeID,
        //                    ActionTypeName = isCash.ActionTypeName,
        //                    Amount = isCash.Amount,
        //                    CashID = isCash.CashID,
        //                    Currency = isCash.Currency,
        //                    Date = isCash.Date,
        //                    Description = isCash.Description,
        //                    DocumentNumber = isCash.DocumentNumber,
        //                    ExchangeRate = isCash.ExchangeRate,
        //                    ID = isCash.ID,
        //                    IsActive = isCash.IsActive,
        //                    LocationID = isCash.LocationID,
        //                    OurCompanyID = isCash.OurCompanyID,
        //                    RecordDate = isCash.RecordDate,
        //                    RecordEmployeeID = isCash.RecordEmployeeID,
        //                    RecordIP = isCash.RecordIP,
        //                    ReferenceID = isCash.ReferenceID,
        //                    SystemAmount = isCash.SystemAmount,
        //                    SystemCurrency = isCash.SystemCurrency,
        //                    UpdateDate = isCash.UpdateDate,
        //                    UpdateEmployee = isCash.UpdateEmployee,
        //                    UpdateIP = isCash.UpdateIP,
        //                    EnvironmentID = isCash.EnvironmentID
        //                };
        //                isCash.ReferenceID = sale.ReferanceID;
        //                isCash.Currency = sale.Currency;
        //                isCash.LocationID = sale.LocationID;
        //                isCash.CashID = cash.ID;
        //                isCash.Date = sale.DocumentDate;
        //                isCash.Quantity = sale.Quantity;
        //                isCash.PayMethodID = sale.PayMethodID;
        //                isCash.ToCustomerID = sale.ToCustomerID ?? (int?)null;
        //                isCash.Amount = sale.Amount;
        //                isCash.Description = sale.Description;
        //                isCash.ExchangeRate = sale.ExchangeRate != null ? sale.ExchangeRate : self.ExchangeRate;
        //                isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isCash.UpdateIP = ServiceHelper.GetIPAddress();
        //                isCash.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * isCash.ExchangeRate;
        //                isCash.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                isCash.IsActive = sale.IsActive;

        //                Db.SaveChanges();
        //                //Cari hesap hareketleri temizlenir.

        //                Db.RemoveAllAccountActions(isCash.ID, isCash.UID);

        //                // Aktif ise Cari hesap kaydı eklenir.

        //                if (isCash.IsActive == true)
        //                {
        //                    ServiceHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
        //                }

        //                result.IsSuccess = true;
        //                result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentTicketSaleReturns>(self, isCash, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "SaleReturn", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentTicketSaleReturns> DeleteCashSaleReturn(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {
        //                    var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    //Cari hesap hareketleri temizlenir.

        //                    Db.RemoveAllAccountActions(isCash.ID, isCash.UID);


        //                    //ServiceHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


        //                    result.IsSuccess = true;
        //                    result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile iptal edildi";

        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iade iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}









        //public Result<DocumentBankTransfer> AddBankTransfer(BankTransfer transfer, AuthenticationModel authentication)
        //{
        //    Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (transfer != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var cash = ServiceHelper.GetCash(transfer.LocationID, transfer.Currency);
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
        //                var exchange = ServiceHelper.GetExchange(transfer.DocumentDate.Value);

        //                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29);

        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.LocationID && x.Date == transfer.DocumentDate);

        //                DocumentBankTransfer bankTransfer = new DocumentBankTransfer();

        //                bankTransfer.ActionTypeID = transfer.ActinTypeID;
        //                bankTransfer.ActionTypeName = transfer.ActionTypeName;
        //                bankTransfer.Amount = transfer.Amount;
        //                bankTransfer.Commission = transfer.Commission;
        //                bankTransfer.FromCashID = transfer.FromCashID;
        //                bankTransfer.Currency = transfer.Currency;
        //                bankTransfer.Date = transfer.DocumentDate;
        //                bankTransfer.Description = transfer.Description;
        //                bankTransfer.DocumentNumber = ServiceHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompanyID.Value, "BT");
        //                bankTransfer.ExchangeRate = transfer.ExchangeRate;
        //                bankTransfer.ToBankAccountID = transfer.ToBankID;
        //                bankTransfer.IsActive = true;
        //                bankTransfer.LocationID = transfer.LocationID;
        //                bankTransfer.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
        //                bankTransfer.RecordDate = DateTime.UtcNow.AddHours(transfer.TimeZone.Value);
        //                bankTransfer.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                bankTransfer.RecordIP = ServiceHelper.GetIPAddress();
        //                bankTransfer.SystemAmount = bankTransfer.Amount * bankTransfer.ExchangeRate;
        //                bankTransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                bankTransfer.SlipNumber = transfer.SlipNumber;
        //                bankTransfer.SlipDate = transfer.SlipDate;
        //                bankTransfer.StatusID = transfer.StatusID ?? 1;
        //                bankTransfer.EnvironmentID = 2;
        //                bankTransfer.ReferenceID = transfer.ReferanceID;
        //                bankTransfer.UID = transfer.UID;
        //                bankTransfer.SlipDocument = transfer.SlipDocument;
        //                bankTransfer.SlipPath = transfer.SlipPath;

        //                bankTransfer.ReferenceCode = ServiceHelper.BankReferenceCode(bankTransfer.OurCompanyID.Value);

        //                Db.DocumentBankTransfer.Add(bankTransfer);
        //                Db.SaveChanges();

        //                if (new int?[] { 2, 3, 4, 5 }.Contains(bankTransfer.StatusID))
        //                {
        //                    if (bankTransfer.Commission > 0)  // komisyonlu işlem ise
        //                    {
        //                        var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == bankTransfer.ID && x.Date == bankTransfer.Date && x.LocationID == bankTransfer.LocationID);
        //                        if (isExpense == null)
        //                        {
        //                            CashExpense expense = new CashExpense();

        //                            expense.ActinTypeID = actType.ID;
        //                            expense.ActionTypeName = actType.Name;
        //                            expense.Amount = bankTransfer.Commission.Value;
        //                            expense.Currency = bankTransfer.Currency;
        //                            expense.Description = bankTransfer.Description;
        //                            expense.DocumentDate = bankTransfer.Date;
        //                            expense.EnvironmentID = bankTransfer.EnvironmentID;
        //                            expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
        //                            expense.CashID = cash.ID;
        //                            expense.LocationID = location.LocationID;
        //                            expense.OurCompanyID = location.OurCompanyID;
        //                            expense.SlipDate = bankTransfer.SlipDate;
        //                            expense.SlipNumber = bankTransfer.SlipNumber;
        //                            expense.SlipDocument = bankTransfer.SlipDocument;
        //                            expense.TimeZone = location.Timezone.Value;
        //                            expense.UID = Guid.NewGuid();
        //                            expense.ExpenseTypeID = 25;
        //                            expense.ReferanceID = bankTransfer.ID;
        //                            expense.ResultID = dayresult?.ID;
        //                            expense.ToBankAccountID = bankTransfer.ToBankAccountID;
        //                            expense.SlipPath = bankTransfer.SlipPath;
        //                            expense.ReferanceModel = transfer.ReferanceModel;

        //                            var expenseresult = AddCashExpense(expense, authentication);
        //                            result.Message += $" {expenseresult.Message}";
        //                        }

        //                    }

        //                    var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
        //                    if (cashaction == null)
        //                    {
        //                        ServiceHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
        //                    }
        //                }
        //                else if (new int?[] { 6, 7 }.Contains(bankTransfer.StatusID))
        //                {
        //                    var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
        //                    if (expaction != null)
        //                    {

        //                        ServiceHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, -1 * bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
        //                    }
        //                    var expbank = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
        //                    if (expbank != null)
        //                    {
        //                        ServiceHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, -1 * bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
        //                    }
        //                }

        //                if (new int?[] { 5 }.Contains(bankTransfer.StatusID))
        //                {
        //                    var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
        //                    if (cashaction == null)
        //                    {
        //                        ServiceHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
        //                    }

        //                }

        //                result.IsSuccess = true;
        //                result.Message = "Havale / EFT bildirimi başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", bankTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, bankTransfer);
        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentBankTransfer> EditBankTransfer(BankTransfer transfer, AuthenticationModel authentication)
        //{
        //    Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isTransfer = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == transfer.UID);

        //        if (isTransfer != null)
        //        {
        //            try
        //            {
        //                var cash = ServiceHelper.GetCash(transfer.LocationID, transfer.Currency);
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
        //                var exchange = ServiceHelper.GetExchange(transfer.DocumentDate.Value);

        //                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29); // masraf ödeme fişi

        //                DocumentBankTransfer self = new DocumentBankTransfer()
        //                {
        //                    ActionTypeID = isTransfer.ActionTypeID,
        //                    ActionTypeName = isTransfer.ActionTypeName,
        //                    Amount = isTransfer.Amount,
        //                    NetAmount = isTransfer.NetAmount,
        //                    FromCashID = isTransfer.FromCashID,
        //                    ToBankAccountID = isTransfer.ToBankAccountID,
        //                    Currency = isTransfer.Currency,
        //                    Date = isTransfer.Date,
        //                    Description = isTransfer.Description,
        //                    DocumentNumber = isTransfer.DocumentNumber,
        //                    ExchangeRate = isTransfer.ExchangeRate,
        //                    ID = isTransfer.ID,
        //                    IsActive = isTransfer.IsActive,
        //                    LocationID = isTransfer.LocationID,
        //                    OurCompanyID = isTransfer.OurCompanyID,
        //                    RecordDate = isTransfer.RecordDate,
        //                    RecordEmployeeID = isTransfer.RecordEmployeeID,
        //                    RecordIP = isTransfer.RecordIP,
        //                    ReferenceID = isTransfer.ReferenceID,
        //                    SystemAmount = isTransfer.SystemAmount,
        //                    SystemCurrency = isTransfer.SystemCurrency,
        //                    UpdateDate = isTransfer.UpdateDate,
        //                    UpdateEmployee = isTransfer.UpdateEmployee,
        //                    UpdateIP = isTransfer.UpdateIP,
        //                    SlipNumber = isTransfer.SlipNumber,
        //                    SlipDocument = isTransfer.SlipDocument,
        //                    SlipDate = isTransfer.SlipDate,
        //                    StatusID = isTransfer.StatusID,
        //                    TrackingNumber = isTransfer.TrackingNumber,
        //                    Commission = isTransfer.Commission,
        //                    UID = isTransfer.UID,
        //                    EnvironmentID = isTransfer.EnvironmentID,
        //                    ReferenceCode = isTransfer.ReferenceCode,
        //                    ResultID = isTransfer.ResultID,
        //                    SlipPath = isTransfer.SlipPath

        //                };

        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.LocationID && x.Date == transfer.DocumentDate);


        //                isTransfer.ReferenceID = transfer.ReferanceID;
        //                isTransfer.Commission = transfer.Commission;
        //                isTransfer.Date = transfer.DocumentDate;
        //                isTransfer.FromCashID = cash.ID;
        //                isTransfer.SlipDate = transfer.SlipDate;
        //                isTransfer.SlipNumber = transfer.SlipNumber;
        //                isTransfer.ToBankAccountID = transfer.ToBankID ?? (int?)null;
        //                isTransfer.Amount = transfer.Amount;
        //                isTransfer.Description = transfer.Description;
        //                isTransfer.ExchangeRate = transfer.ExchangeRate != null ? transfer.ExchangeRate : self.ExchangeRate;
        //                isTransfer.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isTransfer.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isTransfer.UpdateIP = ServiceHelper.GetIPAddress();
        //                isTransfer.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == transfer.Currency ? transfer.Amount : transfer.Amount * isTransfer.ExchangeRate;
        //                isTransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                isTransfer.StatusID = transfer.StatusID;
        //                isTransfer.TrackingNumber = transfer.TrackingNumber;
        //                isTransfer.ReferenceCode = transfer.ReferanceCode;
        //                isTransfer.LocationID = transfer.LocationID;
        //                isTransfer.Currency = transfer.Currency;
        //                isTransfer.IsActive = transfer.IsActive;
        //                isTransfer.SlipDocument = !string.IsNullOrEmpty(transfer.SlipDocument) ? transfer.SlipDocument : self.SlipDocument;
        //                isTransfer.SlipPath = !string.IsNullOrEmpty(transfer.SlipPath) ? transfer.SlipPath : self.SlipPath;



        //                Db.SaveChanges();

        //                result.IsSuccess = true;
        //                result.Message += "Banka Havale / EFT işlemi Güncellendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);



        //                // 01. mevcut kasa çıkış hareketi, banka giriş hareketi ve kasa masraf hareketi varsa sil 

        //                var iscashexit = Db.CashActions.FirstOrDefault(x => x.LocationID == self.LocationID && x.CashActionTypeID == self.ActionTypeID && x.ProcessID == self.ID && x.ProcessUID == self.UID);
        //                if (iscashexit != null)
        //                {
        //                    Db.CashActions.Remove(iscashexit);
        //                    Db.SaveChanges();
        //                }

        //                var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == self.ID && x.Date == self.Date && x.LocationID == self.LocationID);

        //                if (isexpenseexists != null)
        //                {
        //                    var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
        //                    if (iscashexpenseexit != null)
        //                    {
        //                        Db.CashActions.Remove(iscashexpenseexit);
        //                        Db.SaveChanges();
        //                    }

        //                    Db.DocumentCashExpense.Remove(isexpenseexists);
        //                    Db.SaveChanges();
        //                }

        //                var isbankexists = Db.BankActions.FirstOrDefault(x => x.LocationID == isTransfer.LocationID && x.BankActionTypeID == isTransfer.ActionTypeID && x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID);
        //                if (isbankexists != null)
        //                {
        //                    Db.BankActions.Remove(isbankexists);
        //                    Db.SaveChanges();
        //                }

        //                // 02. yeni kasa çıkış hareketlerini ekle

        //                if (new int?[] { 2, 3, 4, 5 }.Contains(isTransfer.StatusID))
        //                {
        //                    // 01. yeni kasa çıkış hareketi komisyonsuz tutar miktarınca eklenir
        //                    var mainamount = (isTransfer.Amount - isTransfer.Commission);

        //                    ServiceHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, mainamount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
        //                    result.Message += $" kasa çıkış işlemi yapıldı. ";

        //                    if (transfer.Commission > 0)  // komisyonlu işlem ise
        //                    {

        //                        // 02. yeni kasa masraf evrağı komisyon tutarı miktarınca eklenir 

        //                        CashExpense expense = new CashExpense();

        //                        expense.ActinTypeID = actType.ID;
        //                        expense.ActionTypeName = actType.Name;
        //                        expense.Amount = isTransfer.Commission.Value;
        //                        expense.Currency = isTransfer.Currency;
        //                        expense.Description = isTransfer.Description;
        //                        expense.DocumentDate = isTransfer.Date;
        //                        expense.EnvironmentID = isTransfer.EnvironmentID;
        //                        expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
        //                        expense.CashID = cash.ID;
        //                        expense.LocationID = location.LocationID;
        //                        expense.OurCompanyID = location.OurCompanyID;
        //                        expense.SlipDate = isTransfer.SlipDate;
        //                        expense.SlipNumber = isTransfer.SlipNumber;
        //                        expense.SlipDocument = isTransfer.SlipDocument;
        //                        expense.TimeZone = location.Timezone.Value;
        //                        expense.UID = Guid.NewGuid();
        //                        expense.ExpenseTypeID = 25;
        //                        expense.ReferanceID = isTransfer.ID;
        //                        expense.ResultID = dayresult?.ID;
        //                        expense.ToBankAccountID = isTransfer.ToBankAccountID;
        //                        expense.SlipPath = isTransfer.SlipPath;
        //                        expense.ReferanceModel = transfer.ReferanceModel;

        //                        var expenseresult = AddCashExpense(expense, authentication);
        //                        result.Message += $" {expenseresult.Message}";
        //                    }
        //                }
        //                if (new int?[] { 7 }.Contains(isTransfer.StatusID))
        //                {
        //                    isTransfer.IsActive = false;

        //                    Db.SaveChanges();
        //                }

        //                if (new int?[] { 5 }.Contains(isTransfer.StatusID))
        //                {
        //                    ServiceHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
        //                    result.Message += $" banka giriş işlemi yapıldı. ";
        //                }


        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"Havale / EFT güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }


        //    }

        //    return result;
        //}

        //public Result<DocumentBankTransfer> DeleteCashBankTransfer(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isCash.ID && x.Date == isCash.Date && x.LocationID == isCash.LocationID);

        //                    if (isexpenseexists != null)
        //                    {
        //                        var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
        //                        if (iscashexpenseexit != null)
        //                        {
        //                            isexpenseexists.IsActive = false;
        //                            isexpenseexists.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                            isexpenseexists.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                            isexpenseexists.UpdateIP = ServiceHelper.GetIPAddress();
        //                            Db.SaveChanges();
        //                        }

        //                        Db.DocumentCashExpense.Remove(isexpenseexists);
        //                        Db.SaveChanges();
        //                    }

        //                    var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessUID == isCash.UID);
        //                    if (expaction != null)
        //                    {

        //                        ServiceHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.NetAmount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
        //                    }
        //                    var expbank = Db.BankActions.FirstOrDefault(x => x.LocationID == isCash.LocationID && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessUID == isCash.UID);
        //                    if (expbank != null)
        //                    {
        //                        ServiceHelper.AddBankAction(isCash.LocationID, null, isCash.ToBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.NetAmount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
        //                    }

        //                    result.IsSuccess = true;
        //                    result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki havale eft başarı ile iptal edildi";

        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                }
        //                catch (Exception ex)
        //                {
        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki havale eft iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}



        //public Result<DocumentSalaryEarn> AddSalaryEarn(SalaryEarn salary, AuthenticationModel authentication)
        //{
        //    Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (salary != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var setcardparam = Db.SetcardParameter.Where(x => x.Year <= salary.DocumentDate.Value.Year && x.OurCompanyID == authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Year).FirstOrDefault();
        //                var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == salary.EmployeeID);
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == salary.LocationID);
        //                var exchange = ServiceHelper.GetExchange(DateTime.UtcNow);
        //                double salaryMultiplier = Db.GetSalaryMultiplier(salary.LocationID, salary.EmployeeID, salary.DocumentDate).FirstOrDefault() ?? 0;

        //                var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == salary.EmployeeID && x.DateStart <= salary.DocumentDate && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();
        //                double? unitprice = empunits?.Hourly ?? 0;

        //                var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == salary.LocationID && x.StatsID == 2 && x.OptionID == 3);
        //                if (location.OurCompanyID == 1 && locationstats != null)
        //                {
        //                    unitprice = unitprice + 1;
        //                }

        //                unitprice = unitprice * salaryMultiplier;

        //                var SalaryEarn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == salary.LocationID && x.EmployeeID == salary.EmployeeID && x.Date == salary.DocumentDate && x.ResultID == salary.ResultID);

        //                if (SalaryEarn == null)
        //                {


        //                    DocumentSalaryEarn salaryEarn = new DocumentSalaryEarn();

        //                    salaryEarn.ActionTypeID = salary.ActionTypeID;
        //                    salaryEarn.ActionTypeName = salary.ActionTypeName;
        //                    salaryEarn.EmployeeID = salary.EmployeeID;
        //                    salaryEarn.QuantityHour = salary.QuantityHour;
        //                    salaryEarn.UnitPrice = unitprice;
        //                    salaryEarn.TotalAmount = (salaryEarn.QuantityHour * salaryEarn.UnitPrice);
        //                    salaryEarn.UnitPriceMultiplierApplied = salaryMultiplier;

        //                    salaryEarn.Currency = salary.Currency;
        //                    salaryEarn.Date = salary.DocumentDate;
        //                    salaryEarn.Description = salary.Description;
        //                    salaryEarn.DocumentNumber = ServiceHelper.GetDocumentNumber(salary.OurCompanyID, "SE");
        //                    salaryEarn.IsActive = true;
        //                    salaryEarn.LocationID = salary.LocationID;
        //                    salaryEarn.OurCompanyID = salary.OurCompanyID;
        //                    salaryEarn.RecordDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
        //                    salaryEarn.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    salaryEarn.RecordIP = ServiceHelper.GetIPAddress();
        //                    salaryEarn.UID = salary.UID;
        //                    salaryEarn.EnvironmentID = salary.EnvironmentID;
        //                    salaryEarn.ReferenceID = salary.ReferanceID;
        //                    salaryEarn.ResultID = salary.ResultID;
        //                    salaryEarn.SystemQuantityHour = salary.QuantityHour;
        //                    salaryEarn.SystemTotalAmount = (salaryEarn.QuantityHour * salaryEarn.UnitPrice);
        //                    salaryEarn.SystemUnitPrice = unitprice;
        //                    salaryEarn.CategoryID = salary.CategoryID;

        //                    salaryEarn.UnitFoodPrice = 0;
        //                    salaryEarn.QuantityHourSalary = salaryEarn.QuantityHour;
        //                    salaryEarn.QuantityHourFood = salaryEarn.QuantityHour;

        //                    if (employee.OurCompanyID == 2 && employee.AreaCategoryID == 2 && (employee.PositionID == 5 || employee.PositionID == 6))
        //                    {
        //                        salaryEarn.UnitFoodPrice = setcardparam != null ? setcardparam.Amount ?? 0 : 0;
        //                        salaryEarn.QuantityHourSalary = (salaryEarn.QuantityHour * 0.9);
        //                        salaryEarn.QuantityHourFood = (salaryEarn.QuantityHour * 0.9);
        //                    }



        //                    Db.DocumentSalaryEarn.Add(salaryEarn);
        //                    Db.SaveChanges();

        //                    // cari hesap işlemesi
        //                    ServiceHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, salaryEarn.ActionTypeID, salaryEarn.ActionTypeName, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmountSalary, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber, 3);
        //                    if (salaryEarn.TotalAmountFood > 0)
        //                    {
        //                        var setcartacttype = Db.CashActionType.FirstOrDefault(x => x.ID == 39);
        //                        ServiceHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, setcartacttype.ID, setcartacttype.Name, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmountFood, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber, 17);
        //                    }

        //                    result.IsSuccess = true;
        //                    result.Message = "Ücret Hakediş başarı ile eklendi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Salary", "Insert", salaryEarn.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, salaryEarn);
        //                }
        //                else
        //                {
        //                    result.IsSuccess = false;
        //                    result.Message = "Ücret Hakedişi daha önce zaten var";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Salary", "Insert", SalaryEarn.ID.ToString(), "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"Ücret Hakediş eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentSalaryEarn> EditSalaryEarn(SalaryEarn salary, AuthenticationModel authentication)
        //{
        //    Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isEarn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.UID == salary.UID);
        //        if (isEarn != null)
        //        {
        //            try
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == salary.LocationID);
        //                var locId = salary.LocationID;
        //                var exchange = ServiceHelper.GetExchange(salary.DocumentDate.Value);
        //                var setcardparam = Db.SetcardParameter.Where(x => x.Year <= salary.DocumentDate.Value.Year && x.OurCompanyID == authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Year).FirstOrDefault();
        //                var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == salary.EmployeeID);
        //                double salaryMultiplier = Db.GetSalaryMultiplier(salary.LocationID, salary.EmployeeID, salary.DocumentDate).FirstOrDefault() ?? 0;

        //                var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == salary.EmployeeID && x.DateStart <= salary.DocumentDate && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();
        //                double? unitprice = empunits?.Hourly ?? 0;

        //                var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == salary.LocationID && x.StatsID == 2 && x.OptionID == 3);
        //                if (location.OurCompanyID == 1 && locationstats != null)
        //                {
        //                    unitprice = unitprice + 1;
        //                }
        //                unitprice = unitprice * salaryMultiplier;

        //                var isEmp = salary.EmployeeID;

        //                DocumentSalaryEarn self = new DocumentSalaryEarn()
        //                {
        //                    ActionTypeID = isEarn.ActionTypeID,
        //                    ActionTypeName = isEarn.ActionTypeName,
        //                    TotalAmount = isEarn.TotalAmount,
        //                    EmployeeID = isEarn.EmployeeID,
        //                    Currency = isEarn.Currency,
        //                    Date = isEarn.Date,
        //                    Description = isEarn.Description,
        //                    DocumentNumber = isEarn.DocumentNumber,
        //                    QuantityHour = isEarn.QuantityHour,
        //                    ID = isEarn.ID,
        //                    UnitPrice = isEarn.UnitPrice,
        //                    IsActive = isEarn.IsActive,
        //                    LocationID = isEarn.LocationID,
        //                    OurCompanyID = isEarn.OurCompanyID,
        //                    RecordDate = isEarn.RecordDate,
        //                    RecordEmployeeID = isEarn.RecordEmployeeID,
        //                    RecordIP = isEarn.RecordIP,
        //                    ReferenceID = isEarn.ReferenceID,
        //                    UpdateDate = isEarn.UpdateDate,
        //                    UpdateEmployee = isEarn.UpdateEmployee,
        //                    UpdateIP = isEarn.UpdateIP,
        //                    CategoryID = isEarn.CategoryID,
        //                    EnvironmentID = isEarn.EnvironmentID,
        //                    SystemQuantityHour = isEarn.SystemQuantityHour,
        //                    SystemTotalAmount = isEarn.SystemTotalAmount,
        //                    SystemUnitPrice = isEarn.SystemUnitPrice,
        //                    QuantityHourFood = isEarn.QuantityHourFood,
        //                    QuantityHourSalary = isEarn.QuantityHourSalary,
        //                    ResultID = isEarn.ResultID,
        //                    TotalAmountFood = isEarn.TotalAmountFood,
        //                    TotalAmountLabor = isEarn.TotalAmountLabor,
        //                    TotalAmountSalary = isEarn.TotalAmountSalary,
        //                    UID = isEarn.UID,
        //                    UnitFoodPrice = isEarn.UnitFoodPrice,
        //                    UnitPriceMultiplierApplied = isEarn.UnitPriceMultiplierApplied

        //                };

        //                isEarn.ReferenceID = salary.ReferanceID;
        //                isEarn.CategoryID = salary.CategoryID;
        //                isEarn.EmployeeID = salary.EmployeeID;
        //                isEarn.UnitPrice = (double?)unitprice;
        //                isEarn.QuantityHour = (double)salary.QuantityHour;
        //                isEarn.TotalAmount = (double)((double?)isEarn.UnitPrice * (double)isEarn.QuantityHour);
        //                isEarn.UnitPriceMultiplierApplied = salaryMultiplier;
        //                isEarn.Description = salary.Description;

        //                isEarn.UpdateDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
        //                isEarn.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isEarn.UpdateIP = ServiceHelper.GetIPAddress();
        //                isEarn.SystemQuantityHour = isEarn.QuantityHour;
        //                isEarn.SystemTotalAmount = isEarn.TotalAmount;
        //                isEarn.SystemUnitPrice = unitprice;

        //                isEarn.UnitFoodPrice = 0;//setcardparam != null ? setcardparam.Amount ?? 0 : 0;
        //                isEarn.QuantityHourSalary = isEarn.QuantityHour;
        //                isEarn.QuantityHourFood = isEarn.QuantityHour;

        //                if (employee.OurCompanyID == 2 && employee.AreaCategoryID == 2 && (employee.PositionID == 5 || employee.PositionID == 6))
        //                {
        //                    isEarn.UnitFoodPrice = setcardparam != null ? setcardparam.Amount ?? 0 : 0;
        //                    isEarn.QuantityHourSalary = (isEarn.QuantityHour * 0.9);
        //                    isEarn.QuantityHourFood = (isEarn.QuantityHour * 0.9);
        //                }

        //                Db.SaveChanges();

        //                var empaction = Db.EmployeeCashActions.Where(x => x.ProcessUID == isEarn.UID).ToList();
        //                Db.EmployeeCashActions.RemoveRange(empaction);
        //                Db.SaveChanges();

        //                // cari hesap işlemesi
        //                ServiceHelper.AddEmployeeAction(isEarn.EmployeeID, isEarn.LocationID, isEarn.ActionTypeID, isEarn.ActionTypeName, isEarn.ID, isEarn.Date, isEarn.Description, 1, isEarn.TotalAmountSalary, 0, isEarn.Currency, null, null, null, isEarn.RecordEmployeeID, isEarn.RecordDate, isEarn.UID.Value, isEarn.DocumentNumber, 3);
        //                if (isEarn.TotalAmountFood > 0)
        //                {
        //                    var setcartacttype = Db.CashActionType.FirstOrDefault(x => x.ID == 39);
        //                    ServiceHelper.AddEmployeeAction(isEarn.EmployeeID, isEarn.LocationID, setcartacttype.ID, setcartacttype.Name, isEarn.ID, isEarn.Date, isEarn.Description, 1, isEarn.TotalAmountFood, 0, isEarn.Currency, null, null, null, isEarn.RecordEmployeeID, isEarn.RecordDate, isEarn.UID.Value, isEarn.DocumentNumber, 17);
        //                }

        //                result.IsSuccess = true;
        //                result.Message = "Ücret Hakediş başarı ile güncellendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentSalaryEarn>(self, isEarn, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Salary", "Update", isEarn.ID.ToString(), "Salary", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"Ücret Hakediş güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Salary", "Update", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //        }

        //    }

        //    return result;
        //}

        //public Result<DocumentSalaryEarn> DeleteSalaryEarn(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var SalaryEarn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.UID == id);
        //            if (SalaryEarn != null)
        //            {
        //                try
        //                {

        //                    SalaryEarn.IsActive = false;
        //                    SalaryEarn.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(SalaryEarn.LocationID ?? 3));
        //                    SalaryEarn.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    SalaryEarn.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    //maaş hesap işlemi
        //                    var actions = Db.EmployeeCashActions.Where(x => x.ProcessUID == SalaryEarn.UID && x.ProcessID == SalaryEarn.ID).ToList();
        //                    Db.EmployeeCashActions.RemoveRange(actions);
        //                    Db.SaveChanges();

        //                    result.IsSuccess = true;
        //                    result.Message = "Ücret Hakediş iptal edildi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "DocumentSalaryEarn", "Delete", SalaryEarn.ID.ToString(), "Salary", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(SalaryEarn.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, SalaryEarn);

        //                }
        //                catch (Exception ex)
        //                {
        //                    result.Message = $"{SalaryEarn.TotalAmount} {SalaryEarn.Currency} tutarındaki ÜCRET HAKEDİŞ iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(SalaryEarn.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }

        //        }
        //    }

        //    return result;
        //}





        //public Result<DayResultDocuments> AddResultDocument(long? id, string filename, string path, int? typeid, string description, AuthenticationModel authentication)
        //{
        //    Result<DayResultDocuments> result = new Result<DayResultDocuments>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    int locationid = 0;

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

        //                if (dayresult != null)
        //                {
        //                    locationid = dayresult.LocationID;

        //                    DayResultDocuments resultDocuments = new DayResultDocuments();

        //                    resultDocuments.Date = dayresult.Date;
        //                    resultDocuments.Description = description;
        //                    resultDocuments.DocumentTypeID = typeid.Value;
        //                    resultDocuments.EnvironmentID = 2;

        //                    resultDocuments.IsActive = true;
        //                    resultDocuments.LocationID = dayresult.LocationID;
        //                    resultDocuments.RecordDate = DateTime.Now;
        //                    resultDocuments.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    resultDocuments.RecordIP = ServiceHelper.GetIPAddress();
        //                    resultDocuments.ResultID = dayresult.ID;
        //                    resultDocuments.FileName = filename;
        //                    resultDocuments.FilePath = path;

        //                    Db.DayResultDocuments.Add(resultDocuments);
        //                    Db.SaveChanges();


        //                    result.IsSuccess = true;
        //                    result.Message = $"{resultDocuments.ID} ID li {resultDocuments.Date} tarihli {resultDocuments.FileName} isimli dosya başarı ile eklendi.";

        //                    ServiceHelper.AddApplicationLog("Office", "Result Document", "Insert", resultDocuments.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{id} {filename} dosyası eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentCashRecorderSlip> AddCashRecorder(long? id, string filename, string path, int? typeid, string description, string slipnumber, string slipdate, string sliptime, string slipamount, string cashamount, string cardamount, string sliptotalmount, AuthenticationModel authentication)
        //{
        //    Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    int locationid = 0;

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

        //                if (dayresult != null)
        //                {
        //                    locationid = dayresult.LocationID;
        //                    double? scardamount = Convert.ToDouble(cardamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
        //                    double? scashamount = Convert.ToDouble(cashamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
        //                    double? netamount = scardamount + scashamount; //Convert.ToDouble(slipamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
        //                    double? totalamount = Convert.ToDouble(sliptotalmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
        //                    DateTime? date = Convert.ToDateTime(slipdate);
        //                    TimeSpan? time = Convert.ToDateTime(sliptime).TimeOfDay;
        //                    DateTime? slipdatetime = date.Value.Add(time.Value);

        //                    DocumentCashRecorderSlip resultCashSlip = new DocumentCashRecorderSlip();

        //                    resultCashSlip.ActionTypeID = typeid;
        //                    resultCashSlip.ActionTypeName = "Yazar Kasa Z Raporu";
        //                    resultCashSlip.Currency = authentication.ActionEmployee.OurCompany.Currency;
        //                    resultCashSlip.DocumentNumber = ServiceHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompany.CompanyID, "CR");
        //                    resultCashSlip.EnvironmentID = 2;
        //                    resultCashSlip.IsActive = true;
        //                    resultCashSlip.LocationID = dayresult.LocationID;
        //                    resultCashSlip.NetAmount = netamount;
        //                    resultCashSlip.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
        //                    resultCashSlip.RecordDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(locationid));
        //                    resultCashSlip.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    resultCashSlip.RecordIP = ServiceHelper.GetIPAddress();
        //                    resultCashSlip.ResultID = id;
        //                    resultCashSlip.SlipDate = slipdatetime;
        //                    resultCashSlip.SlipNumber = slipnumber;
        //                    resultCashSlip.TotalAmount = totalamount;
        //                    resultCashSlip.UID = Guid.NewGuid();
        //                    resultCashSlip.SlipFile = filename;
        //                    resultCashSlip.SlipPath = path;
        //                    resultCashSlip.CashAmount = scashamount;
        //                    resultCashSlip.CreditAmount = scardamount;
        //                    resultCashSlip.Date = slipdatetime?.Date;

        //                    Db.DocumentCashRecorderSlip.Add(resultCashSlip);
        //                    Db.SaveChanges();

        //                    result.IsSuccess = true;
        //                    result.Message = $"{resultCashSlip.ID} ID li {resultCashSlip.SlipDate} tarihli {resultCashSlip.SlipFile} isimli dosya ile beraber kayıt başarı ile eklendi.";

        //                    ServiceHelper.AddApplicationLog("Office", "Result Cash Recorder", "Insert", resultCashSlip.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{id} {filename} dosyası eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}




        //public Result<DocumentPosCollections> AddPosCollection(PosCollection collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (collection != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                DocumentPosCollections posCollection = new DocumentPosCollections();

        //                posCollection.ActionTypeID = collection.ActinTypeID;
        //                posCollection.ActionTypeName = collection.ActionTypeName;
        //                posCollection.Amount = collection.Amount;
        //                posCollection.BankAccountID = collection.BankAccountID;
        //                posCollection.Currency = collection.Currency;
        //                posCollection.Date = collection.DocumentDate;
        //                posCollection.Description = collection.Description;
        //                posCollection.DocumentNumber = ServiceHelper.GetDocumentNumber(collection.OurCompanyID, "PC");
        //                posCollection.ExchangeRate = collection.ExchangeRate;
        //                posCollection.FromCustomerID = collection.FromCustomerID;
        //                posCollection.IsActive = true;
        //                posCollection.LocationID = collection.LocationID;
        //                posCollection.OurCompanyID = collection.OurCompanyID;
        //                posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
        //                posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                posCollection.RecordIP = ServiceHelper.GetIPAddress();
        //                posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
        //                posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                posCollection.ReferenceID = collection.ReferanceID;
        //                posCollection.TerminalID = collection.TerminalID;
        //                posCollection.EnvironmentID = 2;
        //                posCollection.UID = Guid.NewGuid();
        //                posCollection.Quantity = collection.Quantity;
        //                posCollection.ResultID = collection.ResultID;


        //                Db.DocumentPosCollections.Add(posCollection);
        //                Db.SaveChanges();

        //                // cari hesap işlemesi
        //                ServiceHelper.AddBankAction(posCollection.LocationID, null, posCollection.BankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, posCollection.Amount, 0, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

        //                result.IsSuccess = true;
        //                result.Message = "Pos Tahsilatı başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, posCollection);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentPosCollections> EditPosCollection(PosCollection collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPos = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == collection.UID);
        //        if (isPos != null)
        //        {
        //            try
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
        //                var locId = collection.LocationID;
        //                var exchange = ServiceHelper.GetExchange(collection.DocumentDate.Value);

        //                DocumentPosCollections self = new DocumentPosCollections()
        //                {
        //                    ActionTypeID = isPos.ActionTypeID,
        //                    ActionTypeName = isPos.ActionTypeName,
        //                    Amount = isPos.Amount,
        //                    Currency = isPos.Currency,
        //                    Date = isPos.Date,
        //                    Description = isPos.Description,
        //                    DocumentNumber = isPos.DocumentNumber,
        //                    ExchangeRate = isPos.ExchangeRate,
        //                    ID = isPos.ID,
        //                    FromCustomerID = isPos.FromCustomerID,
        //                    IsActive = isPos.IsActive,
        //                    LocationID = isPos.LocationID,
        //                    OurCompanyID = isPos.OurCompanyID,
        //                    RecordDate = isPos.RecordDate,
        //                    RecordEmployeeID = isPos.RecordEmployeeID,
        //                    RecordIP = isPos.RecordIP,
        //                    ReferenceID = isPos.ReferenceID,
        //                    SystemAmount = isPos.SystemAmount,
        //                    SystemCurrency = isPos.SystemCurrency,
        //                    UpdateDate = isPos.UpdateDate,
        //                    UpdateEmployee = isPos.UpdateEmployee,
        //                    UpdateIP = isPos.UpdateIP,
        //                    BankAccountID = isPos.BankAccountID,
        //                    TerminalID = isPos.TerminalID,
        //                    EnvironmentID = isPos.EnvironmentID,
        //                    Quantity = isPos.Quantity
        //                };
        //                isPos.ReferenceID = collection.ReferanceID;
        //                isPos.LocationID = collection.LocationID;
        //                isPos.Date = collection.DocumentDate;
        //                isPos.BankAccountID = collection.BankAccountID;
        //                isPos.FromCustomerID = collection.FromCustomerID ?? (int?)null;
        //                isPos.Amount = collection.Amount;
        //                isPos.Currency = collection.Currency;
        //                isPos.Description = collection.Description;
        //                isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
        //                isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isPos.UpdateIP = ServiceHelper.GetIPAddress();
        //                isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
        //                isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                isPos.Quantity = collection.Quantity;
        //                Db.SaveChanges();

        //                var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

        //                if (cashaction != null)
        //                {
        //                    cashaction.LocationID = isPos.LocationID;
        //                    cashaction.Collection = isPos.Amount;
        //                    cashaction.Currency = isPos.Currency;
        //                    cashaction.BankAccountID = isPos.BankAccountID;
        //                    cashaction.ActionDate = isPos.Date;
        //                    cashaction.ProcessDate = isPos.Date;
        //                    cashaction.UpdateDate = isPos.UpdateDate;
        //                    cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

        //                    Db.SaveChanges();

        //                }

        //                result.IsSuccess = true;
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos tahsilatı başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isPos, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos tahsilatı güncellenemdi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }


        //    }

        //    return result;
        //}

        //public Result<DocumentPosCollections> DeletePosCollection(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {
        //                    var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    ServiceHelper.AddBankAction(isCash.LocationID, null, isCash.BankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

        //                    result.IsSuccess = true;
        //                    result.Message = "Pos tahsilatı başarı ile iptal edildi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isCash);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos tahsilatı iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }


        //    return result;
        //}




        //public Result<DocumentPosCancel> AddPosCancel(PosCancel collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (collection != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                DocumentPosCancel posCollection = new DocumentPosCancel();

        //                posCollection.ActionTypeID = collection.ActinTypeID;
        //                posCollection.ActionTypeName = collection.ActionTypeName;
        //                posCollection.Amount = collection.Amount;
        //                posCollection.FromBankAccountID = collection.FromBankAccountID;
        //                posCollection.Currency = collection.Currency;
        //                posCollection.Date = collection.DocumentDate;
        //                posCollection.Description = collection.Description;
        //                posCollection.DocumentNumber = ServiceHelper.GetDocumentNumber(collection.OurCompanyID, "PCN");
        //                posCollection.ExchangeRate = collection.ExchangeRate;
        //                posCollection.ToCustomerID = collection.ToCustomerID;
        //                posCollection.IsActive = true;
        //                posCollection.LocationID = collection.LocationID;
        //                posCollection.OurCompanyID = collection.OurCompanyID;
        //                posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
        //                posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                posCollection.RecordIP = ServiceHelper.GetIPAddress();
        //                posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
        //                posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                posCollection.ReferenceID = collection.ReferanceID;
        //                posCollection.TerminalID = collection.TerminalID;
        //                posCollection.EnvironmentID = 2;
        //                posCollection.UID = Guid.NewGuid();
        //                posCollection.Quantity = collection.Quantity;



        //                Db.DocumentPosCancel.Add(posCollection);
        //                Db.SaveChanges();

        //                // cari hesap işlemesi
        //                ServiceHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

        //                result.IsSuccess = true;
        //                result.Message = "Pos iptali başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, posCollection);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Pos iptali eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentPosCancel> EditPosCancel(PosCancel collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPos = Db.DocumentPosCancel.FirstOrDefault(x => x.UID == collection.UID);
        //        if (isPos != null)
        //        {
        //            try
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
        //                var locId = collection.LocationID;
        //                var exchange = ServiceHelper.GetExchange(collection.DocumentDate.Value);

        //                DocumentPosCancel self = new DocumentPosCancel()
        //                {
        //                    ActionTypeID = isPos.ActionTypeID,
        //                    ActionTypeName = isPos.ActionTypeName,
        //                    Amount = isPos.Amount,
        //                    Currency = isPos.Currency,
        //                    Date = isPos.Date,
        //                    Description = isPos.Description,
        //                    DocumentNumber = isPos.DocumentNumber,
        //                    ExchangeRate = isPos.ExchangeRate,
        //                    ID = isPos.ID,
        //                    ToCustomerID = isPos.ToCustomerID,
        //                    IsActive = isPos.IsActive,
        //                    LocationID = isPos.LocationID,
        //                    OurCompanyID = isPos.OurCompanyID,
        //                    RecordDate = isPos.RecordDate,
        //                    RecordEmployeeID = isPos.RecordEmployeeID,
        //                    RecordIP = isPos.RecordIP,
        //                    ReferenceID = isPos.ReferenceID,
        //                    SystemAmount = isPos.SystemAmount,
        //                    SystemCurrency = isPos.SystemCurrency,
        //                    UpdateDate = isPos.UpdateDate,
        //                    UpdateEmployee = isPos.UpdateEmployee,
        //                    UpdateIP = isPos.UpdateIP,
        //                    FromBankAccountID = isPos.FromBankAccountID,
        //                    TerminalID = isPos.TerminalID,
        //                    EnvironmentID = isPos.EnvironmentID,
        //                    Quantity = isPos.Quantity
        //                };
        //                isPos.Quantity = collection.Quantity;
        //                isPos.ReferenceID = collection.ReferanceID;
        //                isPos.LocationID = collection.LocationID;
        //                isPos.Date = collection.DocumentDate;
        //                isPos.FromBankAccountID = collection.FromBankAccountID;
        //                isPos.ToCustomerID = collection.ToCustomerID ?? (int?)null;
        //                isPos.Amount = collection.Amount;
        //                isPos.Currency = collection.Currency;
        //                isPos.Description = collection.Description;
        //                isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
        //                isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isPos.UpdateIP = ServiceHelper.GetIPAddress();
        //                isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * isPos.ExchangeRate;
        //                isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

        //                Db.SaveChanges();

        //                var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

        //                if (cashaction != null)
        //                {
        //                    cashaction.LocationID = isPos.LocationID;
        //                    cashaction.Collection = isPos.Amount;
        //                    cashaction.Currency = isPos.Currency;
        //                    cashaction.BankAccountID = isPos.FromBankAccountID;
        //                    cashaction.ActionDate = isPos.Date;
        //                    cashaction.ProcessDate = isPos.Date;
        //                    cashaction.UpdateDate = isPos.UpdateDate;
        //                    cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

        //                    Db.SaveChanges();

        //                }

        //                result.IsSuccess = true;
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iptali başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentPosCancel>(self, isPos, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "PosCancel", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iptali güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }


        //    }

        //    return result;
        //}

        //public Result<DocumentPosCancel> DeletePosCancel(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentPosCancel.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {
        //                    var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    ServiceHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

        //                    result.IsSuccess = true;
        //                    result.Message = "Pos iptali başarı ile iptal edildi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isCash);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iptali iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }


        //    return result;
        //}




        //public Result<DocumentPosRefund> AddPosRefund(PosRefund collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (collection != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                DocumentPosRefund posCollection = new DocumentPosRefund();

        //                posCollection.ActionTypeID = collection.ActinTypeID;
        //                posCollection.ActionTypeName = collection.ActionTypeName;
        //                posCollection.Amount = collection.Amount;
        //                posCollection.FromBankAccountID = collection.FromBankAccountID;
        //                posCollection.Currency = collection.Currency;
        //                posCollection.Date = collection.DocumentDate;
        //                posCollection.Description = collection.Description;
        //                posCollection.DocumentNumber = ServiceHelper.GetDocumentNumber(collection.OurCompanyID, "PRF");
        //                posCollection.ExchangeRate = collection.ExchangeRate;
        //                posCollection.ToCustomerID = collection.ToCustomerID;
        //                posCollection.IsActive = true;
        //                posCollection.LocationID = collection.LocationID;
        //                posCollection.OurCompanyID = collection.OurCompanyID;
        //                posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
        //                posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                posCollection.RecordIP = ServiceHelper.GetIPAddress();
        //                posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
        //                posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                posCollection.ReferenceID = collection.ReferanceID;
        //                posCollection.TerminalID = collection.TerminalID;
        //                posCollection.EnvironmentID = 2;
        //                posCollection.UID = Guid.NewGuid();
        //                posCollection.Quantity = collection.Quantity;


        //                Db.DocumentPosRefund.Add(posCollection);
        //                Db.SaveChanges();

        //                // cari hesap işlemesi
        //                ServiceHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

        //                result.IsSuccess = true;
        //                result.Message = "Pos iadesi başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, posCollection);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Pos iadesi eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentPosRefund> EditPosRefund(PosRefund collection, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPos = Db.DocumentPosRefund.FirstOrDefault(x => x.UID == collection.UID);
        //        if (isPos != null)
        //        {
        //            try
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
        //                var locId = collection.LocationID;
        //                var exchange = ServiceHelper.GetExchange(collection.DocumentDate.Value);

        //                DocumentPosRefund self = new DocumentPosRefund()
        //                {
        //                    ActionTypeID = isPos.ActionTypeID,
        //                    ActionTypeName = isPos.ActionTypeName,
        //                    Amount = isPos.Amount,
        //                    Currency = isPos.Currency,
        //                    Date = isPos.Date,
        //                    Description = isPos.Description,
        //                    DocumentNumber = isPos.DocumentNumber,
        //                    ExchangeRate = isPos.ExchangeRate,
        //                    ID = isPos.ID,
        //                    ToCustomerID = isPos.ToCustomerID,
        //                    IsActive = isPos.IsActive,
        //                    LocationID = isPos.LocationID,
        //                    OurCompanyID = isPos.OurCompanyID,
        //                    RecordDate = isPos.RecordDate,
        //                    RecordEmployeeID = isPos.RecordEmployeeID,
        //                    RecordIP = isPos.RecordIP,
        //                    ReferenceID = isPos.ReferenceID,
        //                    SystemAmount = isPos.SystemAmount,
        //                    SystemCurrency = isPos.SystemCurrency,
        //                    UpdateDate = isPos.UpdateDate,
        //                    UpdateEmployee = isPos.UpdateEmployee,
        //                    UpdateIP = isPos.UpdateIP,
        //                    FromBankAccountID = isPos.FromBankAccountID,
        //                    TerminalID = isPos.TerminalID,
        //                    EnvironmentID = isPos.EnvironmentID,
        //                    Quantity = isPos.Quantity

        //                };
        //                isPos.Quantity = collection.Quantity;
        //                isPos.ReferenceID = collection.ReferanceID;
        //                isPos.LocationID = collection.LocationID;
        //                isPos.Date = collection.DocumentDate;
        //                isPos.FromBankAccountID = collection.FromBankAccountID;
        //                isPos.ToCustomerID = collection.ToCustomerID ?? (int?)null;
        //                isPos.Amount = collection.Amount;
        //                isPos.Currency = collection.Currency;
        //                isPos.Description = collection.Description;
        //                isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
        //                isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isPos.UpdateIP = ServiceHelper.GetIPAddress();
        //                isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * isPos.ExchangeRate;
        //                isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

        //                Db.SaveChanges();

        //                var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

        //                if (cashaction != null)
        //                {
        //                    cashaction.LocationID = isPos.LocationID;
        //                    cashaction.Collection = isPos.Amount;
        //                    cashaction.Currency = isPos.Currency;
        //                    cashaction.BankAccountID = isPos.FromBankAccountID;
        //                    cashaction.ActionDate = isPos.Date;
        //                    cashaction.ProcessDate = isPos.Date;
        //                    cashaction.UpdateDate = isPos.UpdateDate;
        //                    cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

        //                    Db.SaveChanges();

        //                }

        //                result.IsSuccess = true;
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iadesi başarı ile güncellendi";


        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentPosRefund>(self, isPos, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "PosRefund", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iadesi güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }


        //    }

        //    return result;
        //}

        //public Result<DocumentPosRefund> DeletePosRefund(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isCash = Db.DocumentPosRefund.FirstOrDefault(x => x.UID == id);
        //            if (isCash != null)
        //            {
        //                try
        //                {
        //                    var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isCash.Date));

        //                    isCash.IsActive = false;
        //                    isCash.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3));
        //                    isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isCash.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    ServiceHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

        //                    result.IsSuccess = true;
        //                    result.Message = "Pos iadesi başarı ile iptal edildi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isCash);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iptali iptal edilemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }


        //    return result;
        //}




        //public Result<DocumentCashRecorderSlip> AddCashRecorder(CashRecorder record, AuthenticationModel authentication)
        //{
        //    Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (record != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                DocumentCashRecorderSlip cashRedord = new DocumentCashRecorderSlip();

        //                cashRedord.ActionTypeID = record.ActinTypeID;
        //                cashRedord.ActionTypeName = record.ActionTypeName;
        //                cashRedord.NetAmount = record.NetAmount;
        //                cashRedord.TotalAmount = record.TotalAmount;
        //                cashRedord.Currency = record.Currency;
        //                cashRedord.Date = record.DocumentDate;
        //                cashRedord.DocumentNumber = ServiceHelper.GetDocumentNumber(record.OurCompanyID, "CR");
        //                cashRedord.IsActive = true;
        //                cashRedord.LocationID = record.LocationID;
        //                cashRedord.OurCompanyID = record.OurCompanyID;
        //                cashRedord.RecordDate = DateTime.UtcNow.AddHours(record.TimeZone.Value);
        //                cashRedord.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                cashRedord.RecordIP = ServiceHelper.GetIPAddress();
        //                cashRedord.SlipDate = record.SlipDate;
        //                cashRedord.SlipNumber = record.SlipNumber;
        //                cashRedord.EnvironmentID = record.EnvironmentID;
        //                cashRedord.UID = Guid.NewGuid();
        //                cashRedord.ResultID = record.ResultID;
        //                cashRedord.CashAmount = record.CashAmount;
        //                cashRedord.CreditAmount = record.CreditAmount;


        //                cashRedord.SlipPath = record.SlipPath;
        //                cashRedord.SlipFile = record.SlipFile;


        //                Db.DocumentCashRecorderSlip.Add(cashRedord);
        //                Db.SaveChanges();


        //                result.IsSuccess = true;
        //                result.Message = "Yazarkasa fişi başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Result Cash Recorder", "Insert", cashRedord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(record.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, cashRedord);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Yazarkasa fişi eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(record.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentCashRecorderSlip> EditCashRecorder(CashRecorder record, AuthenticationModel authentication)
        //{
        //    Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var location = Db.Location.FirstOrDefault(x => x.LocationID == record.LocationID);
        //        var isRecord = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.UID == record.UID);

        //        if (isRecord != null)
        //        {
        //            try
        //            {
        //                DocumentCashRecorderSlip self = new DocumentCashRecorderSlip()
        //                {
        //                    ActionTypeID = isRecord.ActionTypeID,
        //                    ActionTypeName = isRecord.ActionTypeName,
        //                    NetAmount = isRecord.NetAmount,
        //                    TotalAmount = isRecord.TotalAmount,
        //                    Currency = isRecord.Currency,
        //                    Date = isRecord.Date,
        //                    SlipDate = isRecord.SlipDate,
        //                    DocumentNumber = isRecord.DocumentNumber,
        //                    ID = isRecord.ID,
        //                    IsActive = isRecord.IsActive,
        //                    LocationID = isRecord.LocationID,
        //                    OurCompanyID = isRecord.OurCompanyID,
        //                    RecordDate = isRecord.RecordDate,
        //                    RecordEmployeeID = isRecord.RecordEmployeeID,
        //                    RecordIP = isRecord.RecordIP,
        //                    UpdateDate = isRecord.UpdateDate,
        //                    UpdateEmployee = isRecord.UpdateEmployee,
        //                    UpdateIP = isRecord.UpdateIP,
        //                    SlipNumber = isRecord.SlipNumber,
        //                    SlipFile = isRecord.SlipFile,
        //                    SlipPath = isRecord.SlipPath,
        //                    EnvironmentID = isRecord.EnvironmentID,
        //                    CashAmount = isRecord.CashAmount,
        //                    CreditAmount = isRecord.CreditAmount,
        //                    ResultID = isRecord.ResultID,
        //                    UID = isRecord.UID
        //                };

        //                isRecord.LocationID = isRecord.LocationID;
        //                isRecord.NetAmount = record.NetAmount;
        //                isRecord.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
        //                isRecord.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isRecord.UpdateIP = ServiceHelper.GetIPAddress();
        //                isRecord.SlipDate = record.SlipDate;
        //                isRecord.SlipNumber = record.SlipNumber;
        //                isRecord.TotalAmount = record.TotalAmount;
        //                isRecord.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
        //                isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                isRecord.UpdateIP = ServiceHelper.GetIPAddress();
        //                isRecord.CashAmount = record.CashAmount;
        //                isRecord.CreditAmount = record.CreditAmount;
        //                isRecord.ResultID = record.ResultID > 0 ? record.ResultID : self.ResultID;
        //                isRecord.Date = record.DocumentDate;


        //                if (isRecord.ResultID == null)
        //                {
        //                    isRecord.ResultID = Db.DayResult.FirstOrDefault(x => x.LocationID == self.LocationID && x.Date == isRecord.Date)?.ID;
        //                }

        //                isRecord.SlipFile = !string.IsNullOrEmpty(record.SlipFile) ? record.SlipFile : self.SlipFile;
        //                isRecord.SlipPath = !string.IsNullOrEmpty(record.SlipPath) ? record.SlipPath : self.SlipPath;

        //                Db.SaveChanges();

        //                result.IsSuccess = true;
        //                result.Message = $"{isRecord.ID} ID li {isRecord.SlipDate} tarihli {isRecord.SlipFile} isimli dosya ile beraber kayıt başarı ile güncellendi.";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentCashRecorderSlip>(self, isRecord, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Result Cash Recorder", "Update", isRecord.ID.ToString(), "CashRecorder", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isRecord.ID} {isRecord.SlipFile} dosyası güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Result Document", "Update", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentCashRecorderSlip> DeleteCashRecorder(Guid? id, AuthenticationModel authentication)
        //{
        //    Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };


        //    if (id != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            var isRecord = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.UID == id);
        //            if (isRecord != null)
        //            {
        //                try
        //                {
        //                    var exchange = ServiceHelper.GetExchange(Convert.ToDateTime(isRecord.Date));

        //                    isRecord.IsActive = false;
        //                    isRecord.UpdateDate = DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isRecord.LocationID ?? 3));
        //                    isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
        //                    isRecord.UpdateIP = ServiceHelper.GetIPAddress();

        //                    Db.SaveChanges();

        //                    result.IsSuccess = true;
        //                    result.Message = $"{isRecord.ID} ID li {isRecord.SlipDate} tarihli kayıt başarı ile silindi.";

        //                    ServiceHelper.AddApplicationLog("Office", "Result Cash Recorder", "Remove", isRecord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isRecord.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                }
        //                catch (Exception ex)
        //                {

        //                    result.Message = $"{isRecord.TotalAmount} {isRecord.Currency} tarihli kayıt başarı ile silinemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "Result Document", "Remove", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(ServiceHelper.GetTimeZone(isRecord.LocationID ?? 3)), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //        }
        //    }


        //    return result;
        //}


        //public Result<DocumentTransfer> AddTransfer(TransferModel transfer, AuthenticationModel authentication)
        //{
        //    Result<DocumentTransfer> result = new Result<DocumentTransfer>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (transfer != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                var flocation = Db.Location.FirstOrDefault(x => x.LocationID == transfer.FromLocationID);

        //                var actiontype = Db.CashActionType.FirstOrDefault(x => x.ID == 34);

        //                var exchange = ServiceHelper.GetExchange(transfer.DocumentDate);

        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.FromLocationID && x.Date == transfer.DocumentDate);

        //                DocumentTransfer doctransfer = new DocumentTransfer();

        //                doctransfer.ActionTypeID = actiontype.ID;
        //                doctransfer.ActionTypeName = actiontype.Name;
        //                doctransfer.Amount = transfer.Amount;
        //                doctransfer.CarrierEmployeeID = transfer.CarrierEmployeeID;
        //                doctransfer.FromCashID = transfer.FromCashID;
        //                doctransfer.FromBankAccountID = transfer.FromBankID;
        //                doctransfer.FromCustomerID = transfer.FromCustID;
        //                doctransfer.FromDate = transfer.DocumentDate;
        //                doctransfer.FromEmployeeID = transfer.FromEmplID;
        //                doctransfer.FromLocationID = transfer.FromLocationID;
        //                doctransfer.FromRecordEmployeeID = authentication.ActionEmployee.EmployeeID;

        //                doctransfer.ToBankAccountID = transfer.ToBankID;
        //                doctransfer.ToCashID = transfer.ToCashID;
        //                doctransfer.ToCustomerID = transfer.ToCustID;
        //                doctransfer.ToEmployeeID = transfer.ToEmplID;
        //                doctransfer.ToLocationID = transfer.ToLocationID;

        //                doctransfer.Currency = transfer.Currency;
        //                doctransfer.DocumentDate = transfer.DocumentDate;
        //                doctransfer.Description = transfer.Description;
        //                doctransfer.DocumentNumber = ServiceHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompanyID.Value, "TR");
        //                doctransfer.ExchangeRate = transfer.Currency == "USD" ? exchange.USDA : transfer.Currency == "EUR" ? exchange.EURA : 1;
        //                doctransfer.ToBankAccountID = transfer.ToBankID;
        //                doctransfer.IsActive = true;
        //                doctransfer.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
        //                doctransfer.RecordDate = DateTime.UtcNow.AddHours(flocation.Timezone.Value);
        //                doctransfer.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                doctransfer.RecordIP = ServiceHelper.GetIPAddress();
        //                doctransfer.SystemAmount = doctransfer.Amount * doctransfer.ExchangeRate;
        //                doctransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
        //                doctransfer.StatusID = 1;
        //                doctransfer.EnvironmentID = 2;
        //                doctransfer.UID = transfer.UID;

        //                doctransfer.ResultID = dayresult?.ID;

        //                Db.DocumentTransfer.Add(doctransfer);
        //                Db.SaveChanges();

        //                result.IsSuccess = true;
        //                result.Message = "Cari Virman kaydı başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "DocumentTransfer", "Insert", doctransfer.ID.ToString(), "Action", "AddTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(flocation.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, doctransfer);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Cari Virman kaydı eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "DocumentTransfer", "Insert", "-1", "Action", "AddTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(authentication.ActionEmployee.OurCompany.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentTransfer> EditTransfer(TransferModel transfer, AuthenticationModel authentication)
        //{
        //    Result<DocumentTransfer> result = new Result<DocumentTransfer>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isTransfer = Db.DocumentTransfer.FirstOrDefault(x => x.UID == transfer.UID && x.ID == transfer.ID);

        //        if (isTransfer != null)
        //        {

        //            var cash = ServiceHelper.GetCash(transfer.FromLocationID.Value, transfer.Currency);
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.FromLocationID);
        //            var exchange = ServiceHelper.GetExchange(transfer.DocumentDate);

        //            try
        //            {

        //                DocumentTransfer self = new DocumentTransfer()
        //                {
        //                    ActionTypeID = isTransfer.ActionTypeID,
        //                    ActionTypeName = isTransfer.ActionTypeName,
        //                    Amount = isTransfer.Amount,
        //                    FromCashID = isTransfer.FromCashID,
        //                    ToBankAccountID = isTransfer.ToBankAccountID,
        //                    Currency = isTransfer.Currency,
        //                    DocumentDate = isTransfer.DocumentDate,
        //                    Description = isTransfer.Description,
        //                    DocumentNumber = isTransfer.DocumentNumber,
        //                    ExchangeRate = isTransfer.ExchangeRate,
        //                    ID = isTransfer.ID,
        //                    IsActive = isTransfer.IsActive,
        //                    FromLocationID = isTransfer.FromLocationID,
        //                    ToLocationID = isTransfer.ToLocationID,
        //                    OurCompanyID = isTransfer.OurCompanyID,
        //                    RecordDate = isTransfer.RecordDate,
        //                    RecordEmployeeID = isTransfer.RecordEmployeeID,
        //                    RecordIP = isTransfer.RecordIP,
        //                    ReferenceID = isTransfer.ReferenceID,
        //                    SystemAmount = isTransfer.SystemAmount,
        //                    SystemCurrency = isTransfer.SystemCurrency,
        //                    UpdateDate = isTransfer.UpdateDate,
        //                    UpdateEmployeeID = isTransfer.UpdateEmployeeID,
        //                    UpdateIP = isTransfer.UpdateIP,
        //                    StatusID = isTransfer.StatusID,
        //                    UID = isTransfer.UID,
        //                    EnvironmentID = isTransfer.EnvironmentID,
        //                    ResultID = isTransfer.ResultID,
        //                    CarrierEmployeeID = isTransfer.CarrierEmployeeID,
        //                    FromBankAccountID = isTransfer.FromBankAccountID,
        //                    FromCustomerID = isTransfer.FromCustomerID,
        //                    FromDate = isTransfer.FromDate,
        //                    FromEmployeeID = isTransfer.FromEmployeeID,
        //                    FromRecordEmployeeID = isTransfer.FromRecordEmployeeID,
        //                    ToCashID = isTransfer.ToCashID,
        //                    ToCustomerID = isTransfer.ToCustomerID,
        //                    ToDate = isTransfer.ToDate,
        //                    ToEmployeeID = isTransfer.ToEmployeeID,
        //                    ToRecordEmployeeID = isTransfer.ToRecordEmployeeID
        //                };

        //                // önce güncelleme yapılır

        //                isTransfer.Amount = transfer.Amount;
        //                isTransfer.CarrierEmployeeID = transfer.CarrierEmployeeID;
        //                isTransfer.Currency = transfer.Currency;
        //                isTransfer.Description = transfer.Description;
        //                isTransfer.DocumentDate = transfer.DocumentDate;
        //                isTransfer.FromBankAccountID = transfer.FromBankID;
        //                isTransfer.FromCashID = transfer.FromCashID;
        //                isTransfer.FromCustomerID = transfer.FromCustID;
        //                isTransfer.FromEmployeeID = transfer.FromEmplID;
        //                isTransfer.FromLocationID = transfer.FromLocationID;
        //                isTransfer.ToBankAccountID = transfer.ToBankID;
        //                isTransfer.ToCashID = transfer.ToCashID;
        //                isTransfer.ToCustomerID = transfer.ToCustID;
        //                isTransfer.ToEmployeeID = transfer.ToEmplID;
        //                isTransfer.ToLocationID = transfer.ToLocationID;
        //                isTransfer.ExchangeRate = transfer.ExchangeRate;
        //                isTransfer.SystemCurrency = location.Currency;
        //                isTransfer.SystemAmount = (transfer.Amount * transfer.ExchangeRate);
        //                isTransfer.IsActive = transfer.IsActive;
        //                isTransfer.StatusID = transfer.StatusID;
        //                isTransfer.UpdateDate = location.LocalDateTime;
        //                isTransfer.UpdateEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                isTransfer.UpdateIP = ServiceHelper.GetIPAddress();

        //                Db.SaveChanges();


        //                // mevcut kayıtlar silinir

        //                var cashactions = Db.CashActions.Where(x => x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID).ToList();
        //                var bankactions = Db.BankActions.Where(x => x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID).ToList();
        //                var employeeactions = Db.EmployeeCashActions.Where(x => x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID).ToList();
        //                var customeractions = Db.CustomerActions.Where(x => x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID).ToList();

        //                Db.CashActions.RemoveRange(cashactions);
        //                Db.BankActions.RemoveRange(bankactions);
        //                Db.EmployeeCashActions.RemoveRange(employeeactions);
        //                Db.CustomerActions.RemoveRange(customeractions);

        //                Db.SaveChanges();

        //                // statuse göre yeni kayıtlar atılır.

        //                if (transfer.StatusID == 2 || transfer.StatusID == 3)  // teslim edildi
        //                {
        //                    if (transfer.FromBankID > 0)
        //                    {
        //                        var acttype = Db.BankActionType.FirstOrDefault(x => x.ID == 7 && x.IsActive == true);
        //                        ServiceHelper.AddBankAction(transfer.FromLocationID, null, transfer.FromBankID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, -1, 0, transfer.Amount, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                    if (transfer.FromCashID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 35 && x.IsActive == true);
        //                        ServiceHelper.AddCashAction(transfer.FromCashID, transfer.FromLocationID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, -1, 0, transfer.Amount, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                    if (transfer.FromEmplID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 35 && x.IsActive == true); // çalışan carisinin türleri de aynı
        //                        ServiceHelper.AddEmployeeAction(transfer.FromEmplID, transfer.FromLocationID, acttype.ID, acttype.Name, isTransfer.ID, transfer.DocumentDate, transfer.Description, -1, 0, transfer.Amount, transfer.Currency, null, null, 12, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value, isTransfer.DocumentNumber, 14);
        //                    }

        //                    if (transfer.FromCustID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 35 && x.IsActive == true); // customer carisinin türleri de aynı
        //                        ServiceHelper.AddCustomerAction(transfer.FromCustID, transfer.FromLocationID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, -1, 0, transfer.Amount, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                }


        //                if (transfer.StatusID == 3)  // onaylandı
        //                {
        //                    if (transfer.ToBankID > 0)
        //                    {
        //                        var acttype = Db.BankActionType.FirstOrDefault(x => x.ID == 6 && x.IsActive == true);
        //                        ServiceHelper.AddBankAction(transfer.ToLocationID, null, transfer.ToBankID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, 1, transfer.Amount, 0, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                    if (transfer.ToCashID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 34 && x.IsActive == true);
        //                        ServiceHelper.AddCashAction(transfer.ToCashID, transfer.ToLocationID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, 1, transfer.Amount, 0, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                    if (transfer.ToEmplID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 34 && x.IsActive == true); // çalışan carisinin türleri de aynı
        //                        ServiceHelper.AddEmployeeAction(transfer.ToEmplID, transfer.ToLocationID, acttype.ID, acttype.Name, isTransfer.ID, transfer.DocumentDate, transfer.Description, 1, transfer.Amount, 0, transfer.Currency, null, null, 12, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value, isTransfer.DocumentNumber, 13);
        //                    }

        //                    if (transfer.ToCustID > 0)
        //                    {
        //                        var acttype = Db.CashActionType.FirstOrDefault(x => x.ID == 34 && x.IsActive == true); // customer carisinin türleri de aynı
        //                        ServiceHelper.AddCustomerAction(transfer.ToCustID, transfer.ToLocationID, null, acttype.ID, transfer.DocumentDate, acttype.Name, isTransfer.ID, transfer.DocumentDate, isTransfer.DocumentNumber, transfer.Description, 1, transfer.Amount, 0, transfer.Currency, null, null, authentication.ActionEmployee.EmployeeID, location.LocalDateTime, isTransfer.UID.Value);
        //                    }

        //                }

        //                if (transfer.StatusID == 4)  // onaylandı
        //                {
        //                    isTransfer.IsActive = false;
        //                    Db.SaveChanges();

        //                }

        //                result.IsSuccess = true;
        //                result.Message += $"{isTransfer.DocumentNumber} nolu Virman İşlemi Güncellendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentTransfer>(self, isTransfer, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Transfer", "Update", isTransfer.ID.ToString(), "Action", "Transfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isTransfer.DocumentNumber} nolu Virman İşlemi Güncellenemedi. {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Transfer", "Update", isTransfer.ID.ToString(), "Action", "Transfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }


        //    }

        //    return result;
        //}


        //public Result<ActionRowResult> CheckResultBackward(Guid id, AuthenticationModel authentication, bool islocal)
        //{
        //    Result<ActionRowResult> result = new Result<ActionRowResult>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isResult = Db.DayResult.FirstOrDefault(x => x.UID == id);

        //        if (isResult != null)
        //        {

        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isResult.LocationID);
        //            var datelist = Db.DateList.FirstOrDefault(x => x.DateKey == isResult.Date);

        //            var action = Db.Action.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date);
        //            var actionrow = Db.ActionRow.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date);
        //            var actionrowresult = Db.ActionRowResult.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.ResultDate == isResult.Date);

        //            List<int> cashsales = new int[] { 10, 21, 24, 28 }.ToList();
        //            List<int> cashprocess = new int[] { 23, 27 }.ToList();
        //            List<int> cardsales = new int[] { 1, 3, 5 }.ToList();
        //            List<int> maas = new int[] { 3, 31 }.ToList();
        //            List<int> hakedisid = new int[] { 32, 36, 39 }.ToList();
        //            List<int> cashexpense = new int[] { 4, 29 }.ToList();
        //            List<int> cashexchange = new int[] { 25, 40 }.ToList();
        //            List<int> bankeft = new int[] { 11, 30 }.ToList();

        //            var cashActions = Db.VCashActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();
        //            var bankActions = Db.VBankActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();
        //            var emplActions = Db.VEmployeeCashActions.Where(x => x.LocationID == isResult.LocationID && x.ProcessDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();

        //            var cashtotal = cashActions.Where(x => cashsales.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
        //            var credittotal = bankActions.Where(x => cardsales.Contains(x.BankActionTypeID.Value)).Sum(x => x.Amount).Value;
        //            var maastotal = cashActions.Where(x => maas.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
        //            var hakedis = emplActions.Where(x => hakedisid.Contains(x.ActionTypeID.Value)).Sum(x => x.Amount).Value;
        //            var expensetotal = cashActions.Where(x => cashexpense.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;

        //            var cashrecorder = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date);
        //            var envelope = Db.DayResultDocuments.FirstOrDefault(x => x.LocationID == isResult.LocationID && x.Date == isResult.Date && x.ResultID == isResult.ID);


        //            try
        //            {
        //                if (action == null)
        //                {
        //                    var newaction = new Entity.Action()
        //                    {
        //                        ActionDate = isResult.Date,
        //                        ActionUID = Guid.NewGuid(),
        //                        LocationID = isResult.LocationID,
        //                        Metarials = 0,
        //                        RecordDate = location.LocalDateTime,
        //                        StateID = isResult.StateID,
        //                        Week = datelist.WeekNumber,
        //                        Year = datelist.WeekYear
        //                    };
        //                    Db.Action.Add(newaction);
        //                    Db.SaveChanges();

        //                    action = newaction;
        //                }


        //                if (actionrow == null)
        //                {
        //                    var newactionrow = new Entity.ActionRow()
        //                    {
        //                        Date = isResult.Date,
        //                        ActionRowUID = Guid.NewGuid(),
        //                        LocationID = isResult.LocationID,
        //                        ActionID = action.ActionID,
        //                        StateID = isResult.StateID,
        //                        Week = datelist.WeekNumber,
        //                        Year = datelist.WeekYear,
        //                        DateWMonth = $"{datelist.Day} {datelist.MonthName.Substring(0, 3)} {datelist.Year}",
        //                        Day = datelist.Day
        //                    };
        //                    Db.ActionRow.Add(newactionrow);
        //                    Db.SaveChanges();

        //                    actionrow = newactionrow;
        //                }

        //                if (actionrowresult == null)
        //                {
        //                    var newactionrowresult = new Entity.ActionRowResult()
        //                    {
        //                        ResultDate = isResult.Date,
        //                        ActionRowID = actionrow.ID,
        //                        LocationID = isResult.LocationID,
        //                        ActionID = action.ActionID,
        //                        StateID = isResult.StateID,
        //                        StatusID = 1,
        //                        EmployeeID = authentication.ActionEmployee.EmployeeID,
        //                        Cash = cashtotal,
        //                        Credit = credittotal,
        //                        LaborPayed = hakedis,
        //                        LaborPayedPayed = maastotal * -1,
        //                        Expense = expensetotal * -1,
        //                        CashIN = cashtotal - hakedis - (expensetotal * -1),
        //                        ZTime = cashrecorder != null ? cashrecorder.SlipDate : null,
        //                        ZNumber = cashrecorder != null ? cashrecorder.SlipNumber : null,
        //                        ZNetTotal = cashrecorder != null ? cashrecorder.NetAmount : null,
        //                        ZGeneralTotal = cashrecorder != null ? cashrecorder.TotalAmount : null,
        //                        IsSendPackage = false,
        //                        IsReceivePackage = false,
        //                        EnvelopeFile = envelope != null ? envelope.FileName : null,
        //                        IsMaster = true,
        //                        NotOpened = false,
        //                        AdminDescription = isResult.Description
        //                    };

        //                    Db.ActionRowResult.Add(newactionrowresult);
        //                    Db.SaveChanges();

        //                    actionrowresult = newactionrowresult;

        //                    ActionRowDocument ardocument = new ActionRowDocument()
        //                    {
        //                        ActionID = action.ActionID,
        //                        ActionRowID = actionrow.ID,
        //                        Date = isResult.Date,
        //                        DocumentTypeID = 1,
        //                        LocationID = isResult.LocationID,
        //                        EmployeeID = authentication.ActionEmployee.EmployeeID,
        //                        FileName = envelope != null ? envelope.FileName : null,
        //                        Description = "Envelope File",
        //                        RecordDate = location.LocalDateTime
        //                    };

        //                    Db.ActionRowDocument.Add(ardocument);
        //                    Db.SaveChanges();

        //                    // dosyayı kopyala




        //                    if (envelope != null && !string.IsNullOrEmpty(envelope.FileName) && !islocal)
        //                    {
        //                        try
        //                        {
        //                            string fileName = envelope.FileName;
        //                            string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
        //                            string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
        //                            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
        //                            string destFile = System.IO.Path.Combine(targetPath, fileName);
        //                            System.IO.File.Copy(sourceFile, destFile, true);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                        }
        //                    }





        //                    result.IsSuccess = true;
        //                    result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sisteme eklendi.";

        //                    ServiceHelper.AddApplicationLog("Office", "ActionRowResult", "Insert", newactionrowresult.ResultID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, newactionrowresult);

        //                    return result;
        //                }
        //                else
        //                {
        //                    // 1 ocak 2020 den büyükse güncelle küçük ise güncelleme
        //                    if (isResult.Date >= new DateTime(2020, 1, 1))
        //                    {
        //                        ActionRowResult self = new ActionRowResult()
        //                        {
        //                            RecordDate = actionrowresult.RecordDate,
        //                            ActionID = actionrowresult.ActionID,
        //                            NotOpened = actionrowresult.NotOpened,
        //                            Longitude = actionrowresult.Longitude,
        //                            ResultID = actionrowresult.ResultID,
        //                            ActionRowID = actionrowresult.ActionRowID,
        //                            AdminDescription = actionrowresult.AdminDescription,
        //                            Cash = actionrowresult.Cash,
        //                            CashIN = actionrowresult.CashIN,
        //                            Credit = actionrowresult.Credit,
        //                            Currency = actionrowresult.Currency,
        //                            Description = actionrowresult.Description,
        //                            EmployeeID = actionrowresult.EmployeeID,
        //                            EnvelopeFile = actionrowresult.EnvelopeFile,
        //                            Expense = actionrowresult.Expense,
        //                            IsActive = actionrowresult.IsActive,
        //                            IsMaster = actionrowresult.IsMaster,
        //                            IsMobile = actionrowresult.IsMobile,
        //                            IsReceivePackage = actionrowresult.IsReceivePackage,
        //                            IsSendPackage = actionrowresult.IsSendPackage,
        //                            LaborPayed = actionrowresult.LaborPayed,
        //                            LaborPayedPayed = actionrowresult.LaborPayedPayed,
        //                            Latitude = actionrowresult.Latitude,
        //                            LocationID = actionrowresult.LocationID,
        //                            RecordEmployeeID = actionrowresult.RecordEmployeeID,
        //                            ResultDate = actionrowresult.ResultDate,
        //                            ResultState = actionrowresult.ResultState,
        //                            StateID = actionrowresult.StateID,
        //                            StatusID = actionrowresult.StatusID,
        //                            SubStatusID = actionrowresult.SubStatusID,
        //                            Total = actionrowresult.Total,
        //                            UpdateDate = actionrowresult.UpdateDate,
        //                            UpdateEmployeeID = actionrowresult.UpdateEmployeeID,
        //                            ZGeneralTotal = actionrowresult.ZGeneralTotal,
        //                            ZNetTotal = actionrowresult.ZNetTotal,
        //                            ZNumber = actionrowresult.ZNumber,
        //                            ZTime = actionrowresult.ZTime

        //                        };


        //                        actionrowresult.ResultDate = isResult.Date;
        //                        actionrowresult.ActionRowID = actionrow.ID;
        //                        actionrowresult.LocationID = isResult.LocationID;
        //                        actionrowresult.ActionID = action.ActionID;
        //                        actionrowresult.StateID = isResult.StateID;
        //                        actionrowresult.StatusID = 1;
        //                        actionrowresult.EmployeeID = authentication.ActionEmployee.EmployeeID;
        //                        actionrowresult.Cash = cashtotal;
        //                        actionrowresult.Credit = credittotal;
        //                        actionrowresult.LaborPayed = hakedis;
        //                        actionrowresult.LaborPayedPayed = maastotal * -1;
        //                        actionrowresult.Expense = expensetotal * -1;
        //                        actionrowresult.CashIN = cashtotal - hakedis - (expensetotal * -1);

        //                        actionrowresult.ZTime = cashrecorder != null ? cashrecorder.SlipDate : null;
        //                        actionrowresult.ZNumber = cashrecorder != null ? cashrecorder.SlipNumber : null;
        //                        actionrowresult.ZNetTotal = cashrecorder != null ? cashrecorder.NetAmount : null;
        //                        actionrowresult.ZGeneralTotal = cashrecorder != null ? cashrecorder.TotalAmount : null;
        //                        actionrowresult.IsSendPackage = false;
        //                        actionrowresult.IsReceivePackage = false;
        //                        actionrowresult.EnvelopeFile = envelope != null ? envelope.FileName : null;
        //                        actionrowresult.IsMaster = true;
        //                        actionrowresult.NotOpened = false;
        //                        actionrowresult.AdminDescription = isResult.Description;

        //                        Db.SaveChanges();

        //                        result.IsSuccess = true;
        //                        result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sistemde güncellendi.";

        //                        var isequal = ServiceHelper.PublicInstancePropertiesEqual<ActionRowResult>(self, actionrowresult, ServiceHelper.getIgnorelist());
        //                        ServiceHelper.AddApplicationLog("Office", "ActionRowResult", "Update", actionrowresult.ResultID.ToString(), "Result", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);


        //                        if (envelope != null && !string.IsNullOrEmpty(envelope.FileName) && !islocal)
        //                        {
        //                            try
        //                            {
        //                                string fileName = envelope.FileName;
        //                                string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
        //                                string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
        //                                string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
        //                                string destFile = System.IO.Path.Combine(targetPath, fileName);
        //                                System.IO.File.Copy(sourceFile, destFile, true);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                            }
        //                        }

        //                    }
        //                    else
        //                    {
        //                        result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarf eski sisteme 1 Ocak 2020 tarihinden önce eklendiği için güncellenmedi.";
        //                        ServiceHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //                    }


        //                }


        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarfta hata oluştu. {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);

        //            }
        //        }


        //    }

        //    return result;
        //}


        //public Result<DocumentEmployeePermit> AddEmployeePermit(EmployeePermit permit, AuthenticationModel authentication)
        //{
        //    Result<DocumentEmployeePermit> result = new Result<DocumentEmployeePermit>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (permit != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                // aynı tarih ile çakışan başka izni var mı kontrol edilir.
        //                var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID != permit.ID && x.UID != permit.UID && x.Date == permit.Date && x.EmployeeID == permit.EmployeeID && x.IsActive == true && (x.DateBegin >= permit.DateBegin || x.DateEnd <= permit.DateEnd));

        //                if (isotherpermit == null)
        //                {


        //                    var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
        //                    var permittype = Db.PermitType.FirstOrDefault(x => x.ID == permit.PermitTypeID);
        //                    string prefix = location.OurCompanyID == 1 ? "PRM" : "IZN";
        //                    var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == permit.LocationID && x.StatsID == 2 && x.OptionID == 3);


        //                    var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == permit.EmployeeID && x.DateStart <= permit.Date && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                    double unithour = empunits?.Hourly ?? 0;


        //                    if (location.OurCompanyID == 1 && locationstats != null)
        //                    {
        //                        unithour = unithour + 1;
        //                    }

        //                    DocumentEmployeePermit employeePermit = new DocumentEmployeePermit();

        //                    employeePermit.ActionTypeID = permit.ActinTypeID;
        //                    employeePermit.ActionTypeName = permit.ActionTypeName;
        //                    employeePermit.Currency = authentication.ActionEmployee.OurCompany.Currency;
        //                    employeePermit.Date = permit.Date;
        //                    employeePermit.DateBegin = permit.DateBegin;
        //                    employeePermit.DateEnd = permit.DateEnd;
        //                    employeePermit.Description = permit.Description;
        //                    employeePermit.DocumentNumber = ServiceHelper.GetDocumentNumber(location.OurCompanyID, prefix);
        //                    employeePermit.EmployeeID = permit.EmployeeID;
        //                    employeePermit.EnvironmentID = permit.EnvironmentID;
        //                    employeePermit.IsActive = permit.IsActive;
        //                    employeePermit.IsPaidTo = permittype.IsPaidTo;
        //                    employeePermit.LocationID = permit.LocationID;
        //                    double minuteDuration = (int)(permit.DateEnd - permit.DateBegin).TotalMinutes; // tabloda computed alan
        //                    employeePermit.OurCompanyID = location.OurCompanyID;
        //                    employeePermit.PermitTypeID = permit.PermitTypeID;
        //                    employeePermit.QuantityHour = (double)(minuteDuration / (double)60);
        //                    employeePermit.RecordDate = location.LocalDateTime;
        //                    employeePermit.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    employeePermit.RecordIP = ServiceHelper.GetIPAddress();
        //                    employeePermit.ReferenceID = permit.ReferanceID ?? null;
        //                    employeePermit.ResultID = permit.ResultID ?? null;
        //                    employeePermit.ReturnWorkDate = permit.ReturnWorkDate;
        //                    employeePermit.StatusID = permit.StatusID;
        //                    employeePermit.UID = permit.UID;
        //                    employeePermit.UnitPrice = unithour;

        //                    if (permit.PermitTypeID == 2)
        //                    {
        //                        employeePermit.TotalAmount = employeePermit.IsPaidTo == false ? (employeePermit.UnitPrice * employeePermit.QuantityHour) : 0;
        //                    }
        //                    else
        //                    {
        //                        employeePermit.TotalAmount = 0;
        //                    }

        //                    Db.DocumentEmployeePermit.Add(employeePermit);
        //                    Db.SaveChanges();

        //                    // eğer status 1 ise ve izin saatlik ücretsiz kesintili ise carisine kesinti işlenir.
        //                    if (employeePermit.StatusID == 1 && employeePermit.IsPaidTo == false && employeePermit.TotalAmount > 0 && permit.PermitTypeID == 2)
        //                    {
        //                        ServiceHelper.AddEmployeeAction(employeePermit.EmployeeID, employeePermit.LocationID, employeePermit.ActionTypeID, employeePermit.ActionTypeName, employeePermit.ID, employeePermit.Date, employeePermit.Description, 1, (-1 * employeePermit.TotalAmount), 0, employeePermit.Currency, null, null, 3, employeePermit.RecordEmployeeID, employeePermit.RecordDate, employeePermit.UID.Value, employeePermit.DocumentNumber, 16);
        //                    }

        //                    result.IsSuccess = true;
        //                    result.Message = "Çalışan izini başarı ile eklendi";

        //                    // log atılır
        //                    ServiceHelper.AddApplicationLog("Office", "Permit", "Insert", employeePermit.ID.ToString(), "Salary", "Permit", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, employeePermit);
        //                }
        //                else
        //                {
        //                    result.Message = $"Çalışanın izini başka bir mevcut izini ile çakıştı. Lütfen kontrol ediniz.";
        //                    ServiceHelper.AddApplicationLog("Office", "Permit", "Insert", "-1", "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, permit);

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan izini eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Permit", "Insert", "-1", "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, permit);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<DocumentEmployeePermit> EditEmployeePermit(EmployeePermit permit, AuthenticationModel authentication)
        //{
        //    Result<DocumentEmployeePermit> result = new Result<DocumentEmployeePermit>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID == permit.ID && x.UID == permit.UID);


        //        if (permit != null && authentication != null && isPermit != null)
        //        {
        //            try
        //            {

        //                DocumentEmployeePermit self = new DocumentEmployeePermit()
        //                {
        //                    ActionTypeID = isPermit.ActionTypeID,
        //                    ActionTypeName = isPermit.ActionTypeName,
        //                    Currency = isPermit.Currency,
        //                    Date = isPermit.Date,
        //                    DateBegin = isPermit.DateBegin,
        //                    DateEnd = isPermit.DateEnd,
        //                    StatusID = isPermit.StatusID,
        //                    Description = isPermit.Description,
        //                    DocumentNumber = isPermit.DocumentNumber,
        //                    EmployeeID = isPermit.EmployeeID,
        //                    EnvironmentID = isPermit.EnvironmentID,
        //                    ID = isPermit.ID,
        //                    IsActive = isPermit.IsActive,
        //                    IsPaidTo = isPermit.IsPaidTo,
        //                    LocationID = isPermit.LocationID,
        //                    MinuteDuration = isPermit.MinuteDuration,
        //                    OurCompanyID = isPermit.OurCompanyID,
        //                    PermitTypeID = isPermit.PermitTypeID,
        //                    QuantityHour = isPermit.QuantityHour,
        //                    RecordDate = isPermit.RecordDate,
        //                    RecordEmployeeID = isPermit.RecordEmployeeID,
        //                    RecordIP = isPermit.RecordIP,
        //                    ReferenceID = isPermit.ReferenceID,
        //                    ResultID = isPermit.ResultID,
        //                    ReturnWorkDate = isPermit.ReturnWorkDate,
        //                    TotalAmount = isPermit.TotalAmount,
        //                    UID = isPermit.UID,
        //                    UnitPrice = isPermit.UnitPrice,
        //                    UpdateDate = isPermit.UpdateDate,
        //                    UpdateEmployeeID = isPermit.UpdateEmployeeID,
        //                    UpdateIP = isPermit.UpdateIP
        //                };

        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
        //                var permittype = Db.PermitType.FirstOrDefault(x => x.ID == permit.PermitTypeID);
        //                var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == permit.LocationID && x.StatsID == 2 && x.OptionID == 3);

        //                var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == permit.EmployeeID && x.DateStart <= permit.Date && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                double unithour = empunits?.Hourly ?? 0;

        //                if (location.OurCompanyID == 1 && locationstats != null)
        //                {
        //                    unithour = unithour + 1;
        //                }


        //                //var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.Date == isPermit.Date && x.UID != isPermit.UID && x.ID != isPermit.ID && (x.DateBegin >= isPermit.DateBegin || x.DateEnd <= isPermit.DateEnd));
        //                var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID != permit.ID && x.UID != permit.UID && x.Date == permit.Date && x.EmployeeID == permit.EmployeeID && x.IsActive == true && (x.DateBegin >= permit.DateBegin || x.DateEnd <= permit.DateEnd));

        //                if (isotherpermit == null)
        //                {

        //                    double minuteDuration = ServiceHelper.CalculatePermitDuration(permit.DateBegin, permit.DateEnd, permit.EmployeeID, permit.LocationID);// (int)(permit.DateEnd - permit.DateBegin).TotalMinutes;

        //                    isPermit.LocationID = permit.LocationID;
        //                    isPermit.EmployeeID = permit.EmployeeID;
        //                    isPermit.Date = permit.Date;
        //                    isPermit.DateBegin = permit.DateBegin;
        //                    isPermit.DateEnd = permit.DateEnd;
        //                    isPermit.Description = permit.Description;
        //                    isPermit.StatusID = permit.StatusID;
        //                    isPermit.PermitTypeID = permit.PermitTypeID;
        //                    isPermit.IsPaidTo = permittype.IsPaidTo;
        //                    isPermit.IsActive = permit.IsActive;
        //                    isPermit.ReturnWorkDate = permit.ReturnWorkDate;
        //                    isPermit.QuantityHour = (double)(minuteDuration / (double)60);
        //                    isPermit.UpdateDate = location.LocalDateTime;
        //                    isPermit.UpdateEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    isPermit.UpdateIP = ServiceHelper.GetIPAddress();
        //                    isPermit.UnitPrice = unithour;

        //                    if (permit.PermitTypeID == 2)
        //                    {
        //                        isPermit.TotalAmount = isPermit.IsPaidTo == false ? (isPermit.UnitPrice * isPermit.QuantityHour) : 0;
        //                    }
        //                    else
        //                    {
        //                        isPermit.TotalAmount = 0;
        //                    }

        //                    Db.SaveChanges();

        //                    // önce cari hesap kaydı varsa silinir.
        //                    var iscari = Db.EmployeeCashActions.FirstOrDefault(x => x.ProcessID == isPermit.ID && x.ProcessUID == isPermit.UID);
        //                    if (iscari != null)
        //                    {
        //                        Db.EmployeeCashActions.Remove(iscari);
        //                        Db.SaveChanges();
        //                    }

        //                    // eğer status 1 ise ve izin saatlik ve ücretsiz kesintili ise carisine kesinti işlenir.
        //                    if (isPermit.StatusID == 1 && isPermit.IsPaidTo == false && isPermit.TotalAmount > 0 && permit.PermitTypeID == 2)
        //                    {
        //                        ServiceHelper.AddEmployeeAction(isPermit.EmployeeID, isPermit.LocationID, isPermit.ActionTypeID, isPermit.ActionTypeName, isPermit.ID, isPermit.Date, isPermit.Description, 1, (-1 * isPermit.TotalAmount), 0, isPermit.Currency, null, null, 3, isPermit.UpdateEmployeeID, isPermit.UpdateDate, isPermit.UID.Value, isPermit.DocumentNumber, 16);
        //                    }

        //                    if (isPermit.StatusID == 2)
        //                    {
        //                        isPermit.IsActive = false;
        //                        Db.SaveChanges();
        //                    }


        //                    result.IsSuccess = true;
        //                    result.Message = $"{isPermit.DocumentNumber} nolu Çalışan İzni başarı ile güncellendi";

        //                    // log atılır
        //                    var isequal = ServiceHelper.PublicInstancePropertiesEqual<DocumentEmployeePermit>(self, isPermit, ServiceHelper.getIgnorelist());
        //                    ServiceHelper.AddApplicationLog("Office", "Permit", "Update", isPermit.ID.ToString(), "Salary", "Permit", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //                else
        //                {
        //                    result.Message = $"Çalışan izini mevcut başka izini ile çakıştı.";
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan izini güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Permit", "Update", isPermit.ID.ToString(), "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, permit);
        //            }


        //        }
        //    }
        //    return result;
        //}




        //public Result<Employee> AddEmployee(Employees isemployee, AuthenticationModel authentication)
        //{
        //    Result<Employee> result = new Result<Employee>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    EmployeeControlModel model = new EmployeeControlModel();
        //    if (isemployee != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {

        //                Employee emp = new Employee();

        //                emp.FullName = isemployee.FullName;
        //                emp.IdentityType = isemployee.IdentityType;
        //                emp.IdentityNumber = isemployee.IdentityNumber;
        //                emp.EMail = isemployee.EMail;
        //                emp.Mobile = isemployee.Mobile;
        //                emp.RecordDate = DateTime.Now;
        //                emp.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                emp.RecordIP = ServiceHelper.GetIPAddress();
        //                emp.AreaCategoryID = isemployee.AreaCategoryID;
        //                emp.DepartmentID = isemployee.DepartmentID;
        //                emp.Description = isemployee.Description;
        //                emp.EmployeeUID = Guid.NewGuid();
        //                emp.IsActive = isemployee.IsActive;
        //                emp.IsTemp = isemployee.IsTemp;
        //                emp.Mobile2 = isemployee.Mobile2;
        //                emp.Username = isemployee.Username;
        //                emp.Password = isemployee.Password;
        //                emp.PositionID = isemployee.PositionID;
        //                emp.RoleGroupID = isemployee.RoleGroupID;
        //                emp.SalaryCategoryID = isemployee.SalaryCategoryID;
        //                emp.SequenceID = isemployee.SequenceID;
        //                emp.ShiftTypeID = isemployee.ShiftTypeID;
        //                emp.StatusID = isemployee.StatusID;
        //                emp.Title = isemployee.Title;
        //                emp.Whatsapp = isemployee.Whatsapp;
        //                emp.OurCompanyID = isemployee.OurCompanyID;

        //                //try
        //                //{
        //                //    if (isemployee != null && !string.IsNullOrEmpty(isemployee.FotoFile))
        //                //    {
        //                //        try
        //                //        {
        //                //            string fileName = isemployee.FotoFile;
        //                //            string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
        //                //            string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
        //                //            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
        //                //            string destFile = System.IO.Path.Combine(targetPath, fileName);
        //                //            System.IO.File.Copy(sourceFile, destFile, true);
        //                //            emp.FotoFile = isemployee.FotoFile;
        //                //        }
        //                //        catch (Exception ex)
        //                //        {
        //                //        }
        //                //    }
        //                //}
        //                //catch (Exception)
        //                //{

        //                //    throw;
        //                //}

        //                Db.Employee.Add(emp);
        //                Db.SaveChanges();

        //                var our = Db.OurCompany.FirstOrDefault(x => x.CompanyID == isemployee.OurCompanyID);

        //                model.empID = emp.EmployeeID;

        //                result.IsSuccess = true;
        //                result.Message = "Çalışan başarı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "Employee", "Insert", emp.EmployeeID.ToString(), "Employee", "AddEmployee", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(our.TimeZone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, emp);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan izini eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Employee", "Insert", "-1", "Employee", "AddEmployee", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isemployee);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<Employee> EditEmployee(Employees employee, AuthenticationModel authentication)
        //{
        //    Result<Employee> result = new Result<Employee>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };
        //    EmployeeControlModel model = new EmployeeControlModel();
        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeUID == employee.EmployeeUID);
        //        if (employee != null && authentication != null && isEmployee != null)
        //        {
        //            try
        //            {
        //                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == isEmployee.OurCompanyID);
        //                Employee self = new Employee()
        //                {
        //                    EmployeeID = isEmployee.EmployeeID,
        //                    FullName = isEmployee.FullName,
        //                    IdentityNumber = isEmployee.IdentityNumber,
        //                    EMail = isEmployee.EMail,
        //                    Mobile = isEmployee.Mobile,
        //                    RecordDate = isEmployee.RecordDate,
        //                    RecordEmployeeID = isEmployee.RecordEmployeeID,
        //                    RecordIP = isEmployee.RecordIP,
        //                    AreaCategoryID = employee.AreaCategoryID,
        //                    DepartmentID = isEmployee.DepartmentID,
        //                    Description = isEmployee.Description,
        //                    EmployeeUID = isEmployee.EmployeeUID,
        //                    IsActive = isEmployee.IsActive,
        //                    IsTemp = isEmployee.IsTemp,
        //                    Mobile2 = isEmployee.Mobile2,
        //                    Username = isEmployee.Username,
        //                    Password = isEmployee.Password,
        //                    PositionID = isEmployee.PositionID,
        //                    RoleGroupID = isEmployee.RoleGroupID,
        //                    SalaryCategoryID = isEmployee.SalaryCategoryID,
        //                    SequenceID = isEmployee.SequenceID,
        //                    ShiftTypeID = isEmployee.ShiftTypeID,
        //                    StatusID = isEmployee.StatusID,
        //                    Title = isEmployee.Title = employee.Title,
        //                    Whatsapp = isEmployee.Whatsapp,
        //                    OurCompanyID = isEmployee.OurCompanyID,

        //                };
        //                isEmployee.AreaCategoryID = employee.AreaCategoryID;
        //                isEmployee.DepartmentID = employee.DepartmentID;
        //                isEmployee.Description = employee.Description;
        //                isEmployee.Mobile2 = employee.Mobile2;
        //                isEmployee.Username = employee.Username;
        //                isEmployee.Password = employee.Password;
        //                isEmployee.PositionID = employee.PositionID;
        //                isEmployee.RoleGroupID = employee.RoleGroupID;
        //                isEmployee.SalaryCategoryID = employee.SalaryCategoryID;
        //                isEmployee.SequenceID = employee.SequenceID;
        //                isEmployee.ShiftTypeID = employee.ShiftTypeID;
        //                isEmployee.StatusID = employee.StatusID;
        //                isEmployee.Title = employee.Title;
        //                isEmployee.Whatsapp = employee.Whatsapp;
        //                isEmployee.OurCompanyID = employee.OurCompanyID;
        //                isEmployee.UpdateDate = DateTime.UtcNow.AddHours(ourcompany.TimeZone.Value);
        //                isEmployee.UpdateEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                isEmployee.UpdateIP = ServiceHelper.GetIPAddress();
        //                isEmployee.IdentityType = employee.IdentityType;
        //                isEmployee.IdentityNumber = employee.IdentityNumber;
        //                isEmployee.Mobile = employee.Mobile;
        //                isEmployee.FullName = employee.FullName;


        //                //try
        //                //{
        //                //    if (employee != null && !string.IsNullOrEmpty(employee.FotoFile))
        //                //    {
        //                //        try
        //                //        {
        //                //            string fileName = employee.FotoFile;
        //                //            string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
        //                //            string targetPath = @"C:\inetpub\wwwroot\Office\Documents";
        //                //            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
        //                //            string destFile = System.IO.Path.Combine(targetPath, fileName);
        //                //            System.IO.File.Copy(sourceFile, destFile, true);
        //                //            isEmployee.FotoFile = employee.FotoFile;
        //                //        }
        //                //        catch (Exception ex)
        //                //        {
        //                //        }
        //                //    }
        //                //}
        //                //catch (Exception)
        //                //{

        //                //    throw;
        //                //}
        //                Db.SaveChanges();

        //                model.empID = isEmployee.EmployeeID;

        //                result.IsSuccess = true;
        //                result.Message = $"{isEmployee.EmployeeID} nolu Çalışan başarı ile Eklendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<Employee>(self, isEmployee, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Employee", "Update", isEmployee.EmployeeID.ToString(), "Employee", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {

        //                result.Message = $"Çalışan güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "Employee", "Update", isEmployee.EmployeeID.ToString(), "Employee", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, employee);
        //            }
        //        }
        //    }


        //    return result;
        //}



        //public Result<EmployeeLocation> AddEmployeeLocation(EmployeesLocation location, AuthenticationModel authentication)
        //{
        //    Result<EmployeeLocation> result = new Result<EmployeeLocation>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (location != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {

        //                EmployeeLocation emp = new EmployeeLocation();

        //                emp.EmployeeID = location.EmployeeID;
        //                emp.LocationID = location.LocationID;
        //                emp.PositionID = location.PositionID;
        //                emp.IsMaster = location.IsMaster;
        //                emp.IsActive = location.IsActive;
        //                emp.RoleID = 1;

        //                Db.EmployeeLocation.Add(emp);
        //                Db.SaveChanges();



        //                result.IsSuccess = true;
        //                result.Message = "Çalışan - Lokasyon ilişkisi başarılı ile eklendi";

        //                // log atılır
        //                ServiceHelper.AddApplicationLog("Office", "EmployeeLocation", "Insert", emp.EmployeeID.ToString(), "Employee", "AddEmployeeLocation", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, emp);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan - Lokasyon ilişkisi eklenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "EmployeeLocation", "Insert", "-1", "Employee", "AddEmployeeLocation", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, location);
        //            }

        //        }
        //    }

        //    return result;
        //}

        //public Result<EmployeeLocation> EditEmployeeLocation(EmployeesLocation location, AuthenticationModel authentication)
        //{
        //    Result<EmployeeLocation> result = new Result<EmployeeLocation>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        try
        //        {
        //            var isEmployee = Db.EmployeeLocation.FirstOrDefault(x => x.EmployeeID == location.EmployeeID && x.LocationID == location.LocationID);
        //            if (location != null && authentication != null && isEmployee != null)
        //            {
        //                EmployeeLocation emp = new EmployeeLocation()
        //                {
        //                    EmployeeID = isEmployee.EmployeeID,
        //                    LocationID = isEmployee.LocationID,
        //                    IsActive = isEmployee.IsActive,
        //                    IsMaster = isEmployee.IsMaster,
        //                    RoleID = isEmployee.RoleID
        //                };

        //                isEmployee.LocationID = location.LocationID;
        //                isEmployee.IsMaster = location.IsMaster;
        //                isEmployee.IsActive = location.IsActive;
        //                isEmployee.RoleID = emp.RoleID == null ? 1 : emp.RoleID;

        //                Db.SaveChanges();



        //                result.IsSuccess = true;
        //                result.Message = $"{isEmployee.EmployeeID} nolu Çalışan {isEmployee.LocationID} nolu lokasyona başarı ile güncellendi";

        //                // log atılır
        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<EmployeeLocation>(emp, isEmployee, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "Employee", "Update", isEmployee.EmployeeID.ToString(), "Employee", "AddEmployeeLocation", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            result.Message = $"Çalışan izini eklenemedi : {ex.Message}";
        //            ServiceHelper.AddApplicationLog("Office", "EmployeeLocation", "Update", "-1", "Employee", "AddEmployeeLocation", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, location);
        //        }

        //    }

        //    return result;
        //}


        //public Result<EmployeeShift> EditLocationShift(LShift shift, AuthenticationModel authentication)
        //{
        //    Result<EmployeeShift> result = new Result<EmployeeShift>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isShift = Db.LocationShift.FirstOrDefault(x => x.ID == shift.ID);

        //        if (isShift != null && authentication != null && (shift.StartDate != null || shift.EndDate != null))
        //        {
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);
        //            try
        //            {
        //                LocationShift self = new LocationShift()
        //                {
        //                    EnvironmentID = isShift.EnvironmentID,
        //                    ID = isShift.ID,
        //                    CloseEnvironmentID = isShift.CloseEnvironmentID,
        //                    Duration = isShift.Duration,
        //                    DurationMinute = isShift.DurationMinute,
        //                    EmployeeID = isShift.EmployeeID,
        //                    FromMobileFinish = isShift.FromMobileFinish,
        //                    FromMobileStart = isShift.FromMobileStart,
        //                    LatitudeFinish = isShift.LatitudeFinish,
        //                    LatitudeStart = isShift.LatitudeStart,
        //                    LocationID = isShift.LocationID,
        //                    LongitudeFinish = isShift.LongitudeFinish,
        //                    LongitudeStart = isShift.LongitudeStart,
        //                    RecordDate = isShift.RecordDate,
        //                    RecordEmployeeID = isShift.RecordEmployeeID,
        //                    ShiftDate = isShift.ShiftDate,
        //                    ShiftDateFinish = isShift.ShiftDateFinish,
        //                    ShiftDateStart = isShift.ShiftDateStart,
        //                    EmployeeIDFinish = isShift.EmployeeIDFinish,
        //                    ShiftFinish = isShift.ShiftFinish,
        //                    ShiftStart = isShift.ShiftStart,
        //                    UpdateDate = isShift.UpdateDate,
        //                    UpdateEmployeeID = isShift.UpdateEmployeeID

        //                };

        //                isShift.ShiftDateStart = shift.StartDate;
        //                isShift.ShiftStart = shift.StartDate?.TimeOfDay ?? null;
        //                isShift.ShiftDateFinish = shift.EndDate;
        //                isShift.ShiftFinish = shift.EndDate?.TimeOfDay ?? null;

        //                Db.SaveChanges();

        //                result.IsSuccess = true;
        //                result.Message = $"{shift.ID} id li lokasyon mesaisi başarı ile gündellendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<LocationShift>(self, isShift, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "LocationShift", "Update", isShift.ID.ToString(), "Shift", "UpdateLocationShift", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"lokasyon mesaisi güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "LocationShift", "Update", shift.ID.ToString(), "Shift", "UpdateLocationShift", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //        else if (isShift != null && authentication != null && shift.StartDate == null && shift.EndDate == null)
        //        {
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);

        //            result.IsSuccess = true;
        //            result.Message = $"{shift.ID} id li lokasyon mesaisi başarı ile silindi";

        //            ServiceHelper.AddApplicationLog("Office", "LocationShift", "Delete", shift.ID.ToString(), "Shift", "UpdateLocationShift", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isShift);

        //            Db.LocationShift.Remove(isShift);
        //            Db.SaveChanges();
        //        }
        //    }


        //    return result;
        //}

        //public Result<EmployeeShift> EditEmployeeShift(EShift shift, AuthenticationModel authentication)
        //{
        //    Result<EmployeeShift> result = new Result<EmployeeShift>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isShift = Db.EmployeeShift.FirstOrDefault(x => x.ID == shift.ID);

        //        if (isShift != null && authentication != null && (shift.StartDate != null || shift.EndDate != null))
        //        {
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);
        //            try
        //            {
        //                EmployeeShift self = new EmployeeShift()
        //                {
        //                    BreakDateEnd = isShift.BreakDateEnd,
        //                    EnvironmentID = isShift.EnvironmentID,
        //                    ID = isShift.ID,
        //                    BreakDateStart = isShift.BreakDateStart,
        //                    BreakDuration = isShift.BreakDuration,
        //                    BreakDurationMinute = isShift.BreakDurationMinute,
        //                    BreakEnd = isShift.BreakEnd,
        //                    BreakStart = isShift.BreakStart,
        //                    BreakTypeID = isShift.BreakTypeID,
        //                    CloseEnvironmentID = isShift.CloseEnvironmentID,
        //                    Duration = isShift.Duration,
        //                    DurationMinute = isShift.DurationMinute,
        //                    EmployeeID = isShift.EmployeeID,
        //                    FromMobileFinish = isShift.FromMobileFinish,
        //                    FromMobileStart = isShift.FromMobileStart,
        //                    IsBreakTime = isShift.IsBreakTime,
        //                    IsWorkTime = isShift.IsWorkTime,
        //                    LatitudeFinish = isShift.LatitudeFinish,
        //                    LatitudeStart = isShift.LatitudeStart,
        //                    LocationID = isShift.LocationID,
        //                    LongitudeFinish = isShift.LongitudeFinish,
        //                    LongitudeStart = isShift.LongitudeStart,
        //                    RecordDate = isShift.RecordDate,
        //                    RecordEmployeeID = isShift.RecordEmployeeID,
        //                    ShiftDate = isShift.ShiftDate,
        //                    ShiftDateEnd = isShift.ShiftDateEnd,
        //                    ShiftDateStart = isShift.ShiftDateStart,
        //                    ShiftDuration = isShift.ShiftDuration,
        //                    ShiftEnd = isShift.ShiftEnd,
        //                    ShiftStart = isShift.ShiftStart,
        //                    UpdateDate = isShift.UpdateDate,
        //                    UpdateEmployeeID = isShift.UpdateEmployeeID

        //                };

        //                isShift.ShiftDateStart = shift.StartDate;
        //                isShift.ShiftStart = shift.StartDate?.TimeOfDay ?? null;
        //                isShift.ShiftDateEnd = shift.EndDate;
        //                isShift.ShiftEnd = shift.EndDate?.TimeOfDay ?? null;

        //                Db.SaveChanges();

        //                result.IsSuccess = true;
        //                result.Message = $"{shift.ID} id li mesai başarı ile gündellendi";

        //                var isequal = ServiceHelper.PublicInstancePropertiesEqual<EmployeeShift>(self, isShift, ServiceHelper.getIgnorelist());
        //                ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Update", isShift.ID.ToString(), "Shift", "UpdateEmployeeShift", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"mesai güncellenemedi : {ex.Message}";
        //                ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Update", shift.ID.ToString(), "Shift", "UpdateEmployeeShift", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //            }
        //        }
        //        else if (isShift != null && authentication != null && shift.StartDate == null && shift.EndDate == null)
        //        {
        //            var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);

        //            result.IsSuccess = true;
        //            result.Message = $"{shift.ID} id li mesai başarı ile silindi";

        //            ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Delete", shift.ID.ToString(), "Shift", "UpdateEmployeeShift", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isShift);

        //            Db.EmployeeShift.Remove(isShift);
        //            Db.SaveChanges();
        //        }
        //    }


        //    return result;
        //}

        //public Result<EmployeeShift> EditEmployeeBreak(List<EBreak> breaks, AuthenticationModel authentication)
        //{
        //    Result<EmployeeShift> result = new Result<EmployeeShift>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        foreach (var shift in breaks)
        //        {
        //            var isShift = Db.EmployeeShift.FirstOrDefault(x => x.ID == shift.ID);

        //            if (isShift != null && authentication != null && (shift.StartDate != null || shift.EndDate != null))
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);
        //                try
        //                {
        //                    EmployeeShift self = new EmployeeShift()
        //                    {
        //                        BreakDateEnd = isShift.BreakDateEnd,
        //                        EnvironmentID = isShift.EnvironmentID,
        //                        ID = isShift.ID,
        //                        BreakDateStart = isShift.BreakDateStart,
        //                        BreakDuration = isShift.BreakDuration,
        //                        BreakDurationMinute = isShift.BreakDurationMinute,
        //                        BreakEnd = isShift.BreakEnd,
        //                        BreakStart = isShift.BreakStart,
        //                        BreakTypeID = isShift.BreakTypeID,
        //                        CloseEnvironmentID = isShift.CloseEnvironmentID,
        //                        Duration = isShift.Duration,
        //                        DurationMinute = isShift.DurationMinute,
        //                        EmployeeID = isShift.EmployeeID,
        //                        FromMobileFinish = isShift.FromMobileFinish,
        //                        FromMobileStart = isShift.FromMobileStart,
        //                        IsBreakTime = isShift.IsBreakTime,
        //                        IsWorkTime = isShift.IsWorkTime,
        //                        LatitudeFinish = isShift.LatitudeFinish,
        //                        LatitudeStart = isShift.LatitudeStart,
        //                        LocationID = isShift.LocationID,
        //                        LongitudeFinish = isShift.LongitudeFinish,
        //                        LongitudeStart = isShift.LongitudeStart,
        //                        RecordDate = isShift.RecordDate,
        //                        RecordEmployeeID = isShift.RecordEmployeeID,
        //                        ShiftDate = isShift.ShiftDate,
        //                        ShiftDateEnd = isShift.ShiftDateEnd,
        //                        ShiftDateStart = isShift.ShiftDateStart,
        //                        ShiftDuration = isShift.ShiftDuration,
        //                        ShiftEnd = isShift.ShiftEnd,
        //                        ShiftStart = isShift.ShiftStart,
        //                        UpdateDate = isShift.UpdateDate,
        //                        UpdateEmployeeID = isShift.UpdateEmployeeID

        //                    };

        //                    isShift.BreakDateStart = shift.StartDate;
        //                    isShift.BreakStart = shift.StartDate?.TimeOfDay ?? null;
        //                    isShift.BreakDateEnd = shift.EndDate;
        //                    isShift.BreakEnd = shift.EndDate?.TimeOfDay ?? null;

        //                    Db.SaveChanges();

        //                    result.IsSuccess = true;
        //                    result.Message += $"{shift.ID} id li mola başarı ile gündellendi";

        //                    var isequal = ServiceHelper.PublicInstancePropertiesEqual<EmployeeShift>(self, isShift, ServiceHelper.getIgnorelist());
        //                    ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Update", isShift.ID.ToString(), "Shift", "UpdateEmployeeBreaks", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //                catch (Exception ex)
        //                {
        //                    result.Message += $"mola güncellenemedi : {ex.Message}";
        //                    ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Update", shift.ID.ToString(), "Shift", "UpdateEmployeeBreaks", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, null);
        //                }
        //            }
        //            else if (isShift != null && authentication != null && shift.StartDate == null && shift.EndDate == null)
        //            {
        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == isShift.LocationID);

        //                result.IsSuccess = true;
        //                result.Message += $"{shift.ID} id li mola başarı ile silindi";

        //                ServiceHelper.AddApplicationLog("Office", "EmployeeShift", "Delete", shift.ID.ToString(), "Shift", "UpdateEmployeeBreaks", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, ServiceHelper.GetIPAddress(), string.Empty, isShift);

        //                Db.EmployeeShift.Remove(isShift);
        //                Db.SaveChanges();
        //            }
        //        }
        //    }


        //    return result;
        //}


        ////Location


    }
}