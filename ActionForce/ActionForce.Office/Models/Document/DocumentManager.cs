using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class DocumentManager
    {

        public DocumentManager()
        {

        }

        public Result<DocumentCashCollections> AddCashCollection(CashCollection collection, AuthenticationModel authentication)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (collection != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {

                    var actType = Db.CashActionType.FirstOrDefault(x => x.ID == collection.ActinTypeID);
                    var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                    var amount = collection.Amount;
                    var currency = collection.Currency;
                    var docDate = DateTime.Now.Date;
                    int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                    if (collection.DocumentDate != null)
                    {
                        docDate = collection.DocumentDate.Value.Date;
                    }
                    var cash = OfficeHelper.GetCash(collection.LocationID, collection.Currency);
                    var refID = collection.ReferanceID ?? (long?)null;
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(docDate);

                        DocumentCashCollections newCashColl = new DocumentCashCollections();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.Amount = amount;
                        newCashColl.CashID = cash.ID;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = collection.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "CC");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.FromBankAccountID = collection.FromBankAccountID ?? (int?)null;
                        newCashColl.FromEmployeeID = collection.FromEmployeeID ?? (int?)null;
                        newCashColl.FromCustomerID = collection.FromCustomerID ?? (int?)null;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = collection.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        newCashColl.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.EnvironmentID = collection.EnvironmentID;
                        newCashColl.UID = Guid.NewGuid();
                        newCashColl.ResultID = collection.ResultID;
                        newCashColl.ReferenceID = refID;

                        Db.DocumentCashCollections.Add(newCashColl);
                        Db.SaveChanges();




                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate, newCashColl.UID.Value);

                        result.IsSuccess = true;
                        result.Message = $"{newCashColl.Date} tarihli { newCashColl.Amount } {newCashColl.Currency} tutarındaki kasa tahsilatı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(collection.LocationID)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }

                }
            }

            return result;
        }

        public Result<DocumentCashCollections> EditCashCollection(CashCollection collection, AuthenticationModel authentication)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == collection.UID);

                if (isCash != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(collection.LocationID, collection.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                        var locId = isCash.LocationID;
                        var exchange = OfficeHelper.GetExchange(collection.DocumentDate.Value);
                        var refID = collection.ReferanceID ?? (long?)null;
                        DocumentCashCollections self = new DocumentCashCollections()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            EnvironmentID = isCash.EnvironmentID,
                            FromBankAccountID = isCash.FromBankAccountID,
                            FromCustomerID = isCash.FromCustomerID,
                            FromEmployeeID = isCash.FromEmployeeID,
                            ResultID = isCash.ResultID,
                            UID = isCash.UID
                        };
                        isCash.ReferenceID = refID;
                        isCash.LocationID = collection.LocationID;
                        isCash.CashID = cash.ID;
                        isCash.Date = collection.DocumentDate;
                        isCash.FromBankAccountID = collection.FromBankAccountID ?? (int?)null;
                        isCash.FromEmployeeID = collection.FromEmployeeID ?? (int?)null;
                        isCash.FromCustomerID = collection.FromCustomerID ?? (int?)null;
                        isCash.Amount = collection.Amount;
                        isCash.Currency = collection.Currency;
                        isCash.Description = collection.Description;
                        isCash.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Collection = isCash.Amount;
                            cashaction.CashID = cash.ID;
                            cashaction.Currency = collection.Currency;
                            cashaction.ActionDate = collection.DocumentDate;
                            cashaction.ProcessDate = collection.DocumentDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(collection.LocationID)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashCollections> DeleteCashCollection(Guid? id, AuthenticationModel authentication)
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
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentTicketSales> AddCashSale(CashSale sale, AuthenticationModel authentication)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (sale != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentTicketSales ticketSale = new DocumentTicketSales();

                        ticketSale.ActionTypeID = sale.ActinTypeID;
                        ticketSale.ActionTypeName = sale.ActionTypeName;
                        ticketSale.Amount = sale.Amount;
                        ticketSale.CashID = sale.CashID;
                        ticketSale.Currency = sale.Currency;
                        ticketSale.Date = sale.DocumentDate;
                        ticketSale.Description = sale.Description;
                        ticketSale.DocumentNumber = OfficeHelper.GetDocumentNumber(sale.OurCompanyID, "TS");
                        ticketSale.ExchangeRate = sale.ExchangeRate;
                        ticketSale.FromCustomerID = sale.FromCustomerID;
                        ticketSale.IsActive = true;
                        ticketSale.LocationID = sale.LocationID;
                        ticketSale.OurCompanyID = sale.OurCompanyID;
                        ticketSale.RecordDate = DateTime.UtcNow.AddHours(sale.TimeZone.Value);
                        ticketSale.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        ticketSale.RecordIP = OfficeHelper.GetIPAddress();
                        ticketSale.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * sale.ExchangeRate;
                        ticketSale.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        ticketSale.ReferenceID = sale.ReferanceID;
                        ticketSale.Quantity = sale.Quantity;
                        ticketSale.PayMethodID = sale.PayMethodID;
                        ticketSale.EnvironmentID = sale.EnvironmentID;
                        ticketSale.UID = Guid.NewGuid();
                        ticketSale.ResultID = sale.ResultID;

                        Db.DocumentTicketSales.Add(ticketSale);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, ticketSale.Amount, 0, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate, ticketSale.UID.Value);

                        result.IsSuccess = true;
                        result.Message = "Bilet satış başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, ticketSale);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Bilet satış eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(sale.LocationID)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentTicketSales> EditCashSale(CashSale sale, AuthenticationModel authentication)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == sale.UID);

                if (isCash != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(sale.LocationID, sale.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == sale.LocationID);
                        var locId = isCash.LocationID;
                        var exchange = OfficeHelper.GetExchange(sale.DocumentDate.Value);

                        DocumentTicketSales self = new DocumentTicketSales()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            PayMethodID = isCash.PayMethodID,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.ReferenceID = sale.ReferanceID;
                        isCash.LocationID = sale.LocationID;
                        isCash.CashID = cash.ID;
                        isCash.Date = sale.DocumentDate;
                        isCash.Quantity = sale.Quantity;
                        isCash.PayMethodID = sale.PayMethodID;
                        isCash.FromCustomerID = sale.FromCustomerID ?? (int?)null;
                        isCash.Amount = sale.Amount;
                        isCash.Description = sale.Description;
                        isCash.ExchangeRate = sale.ExchangeRate != null ? sale.ExchangeRate : self.ExchangeRate;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Collection = isCash.Amount;
                            cashaction.CashID = cash.ID;
                            cashaction.Currency = sale.Currency;
                            cashaction.ActionDate = sale.DocumentDate;
                            cashaction.ProcessDate = sale.DocumentDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satışı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Sale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satışı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentTicketSales> DeleteCashSale(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentSaleExchange> AddSaleExchange(SaleExchange saleExchange, AuthenticationModel authentication)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (saleExchange != null && authentication != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {

                    try
                    {
                        var balance = Db.GetCashBalance(saleExchange.LocationID, saleExchange.FromCashID, saleExchange.DocumentDate).FirstOrDefault() ?? 0;
                        if (balance >= saleExchange.Amount)
                        {
                            DocumentSaleExchange sale = new DocumentSaleExchange();

                            sale.ActionTypeID = saleExchange.ActinTypeID;
                            sale.ActionTypeName = saleExchange.ActionTypeName;
                            sale.Amount = saleExchange.Amount;
                            sale.FromCashID = saleExchange.FromCashID;
                            sale.Currency = saleExchange.Currency;
                            sale.ToCashID = saleExchange.ToCashID;
                            sale.ToCurrency = saleExchange.ToCurrency;
                            sale.ToAmount = saleExchange.ToAmount;
                            sale.Date = saleExchange.DocumentDate;
                            sale.Description = saleExchange.Description;
                            sale.DocumentNumber = OfficeHelper.GetDocumentNumber(saleExchange.OurCompanyID, "EXS");
                            sale.ExchangeRate = saleExchange.ExchangeRate;
                            sale.SaleExchangeRate = saleExchange.SaleExchangeRate;
                            sale.IsActive = true;
                            sale.LocationID = saleExchange.LocationID;
                            sale.OurCompanyID = saleExchange.OurCompanyID;
                            sale.RecordDate = DateTime.UtcNow.AddHours(saleExchange.TimeZone.Value);
                            sale.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            sale.RecordIP = OfficeHelper.GetIPAddress();
                            sale.ReferenceID = saleExchange.ReferanceID;
                            sale.EnvironmentID = saleExchange.EnvironmentID;
                            sale.UID = Guid.NewGuid();
                            sale.SlipNumber = saleExchange.SlipNumber;
                            sale.SlipDate = saleExchange.SlipDate;
                            sale.ResultID = saleExchange.ResultID;
                            sale.SlipDocument = saleExchange.SlipDocument;
                            sale.SlipPath = saleExchange.SlipPath;

                            Db.DocumentSaleExchange.Add(sale);
                            Db.SaveChanges();


                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(sale.FromCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, -1, 0, sale.Amount, sale.Currency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);
                            OfficeHelper.AddCashAction(sale.ToCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, 1, sale.ToAmount, 0, sale.ToCurrency, null, null, sale.RecordEmployeeID, sale.RecordDate, sale.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{sale.Date} tarihli { sale.Amount } {sale.Currency} kasa döviz satışı başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", sale.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(saleExchange.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, sale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { saleExchange.Amount } { saleExchange.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { saleExchange.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(saleExchange.LocationID)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }



                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{saleExchange.Amount} {saleExchange.Currency} kasa döviz satışı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(saleExchange.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }


                }

            }

            return result;
        }

        public Result<DocumentSaleExchange> EditSaleExchange(SaleExchange saleExchange, AuthenticationModel authentication)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            using (ActionTimeEntities Db = new ActionTimeEntities())
            {

                var isExchange = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == saleExchange.UID);

                try
                {
                    var location = Db.Location.FirstOrDefault(x => x.LocationID == saleExchange.LocationID);
                    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                    var currencyo = saleExchange.Currency;
                    var currencyi = location.Currency != null ? location.Currency : ourcompany.Currency;

                    var cash = OfficeHelper.GetCash(saleExchange.LocationID, currencyo);

                    var casho = OfficeHelper.GetCash(saleExchange.LocationID, saleExchange.Currency);
                    var cashi = OfficeHelper.GetCash(saleExchange.LocationID, currencyi);

                    var locId = saleExchange.LocationID;
                    var exchange = OfficeHelper.GetExchange(saleExchange.DocumentDate.Value);


                    var isKasa = Convert.ToInt32(isExchange.ToCashID);
                    var isKasao = Convert.ToInt32(isExchange.FromCashID);

                    DocumentSaleExchange self = new DocumentSaleExchange()
                    {
                        ActionTypeID = isExchange.ActionTypeID,
                        ActionTypeName = isExchange.ActionTypeName,

                        FromCashID = isExchange.FromCashID,
                        Amount = isExchange.Amount,
                        Currency = isExchange.Currency,

                        SaleExchangeRate = isExchange.SaleExchangeRate,
                        ToCashID = isExchange.ToCashID,

                        ToAmount = (isExchange.Amount * isExchange.SaleExchangeRate),
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
                    isExchange.ReferenceID = saleExchange.ReferanceID;
                    isExchange.LocationID = saleExchange.LocationID;
                    isExchange.FromCashID = casho.ID;
                    isExchange.ToCashID = cashi.ID;
                    isExchange.Date = saleExchange.DocumentDate;
                    isExchange.Amount = saleExchange.Amount;
                    isExchange.ToCurrency = currencyi;
                    isExchange.Currency = currencyo;
                    isExchange.ToAmount = (saleExchange.Amount * saleExchange.SaleExchangeRate);
                    isExchange.Description = saleExchange.Description;
                    isExchange.ExchangeRate = saleExchange.ExchangeRate != null ? saleExchange.ExchangeRate : self.ExchangeRate;
                    isExchange.UpdateDate = DateTime.UtcNow.AddHours(saleExchange.TimeZone.Value);
                    isExchange.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                    isExchange.UpdateIP = OfficeHelper.GetIPAddress();

                    isExchange.SlipDocument = !string.IsNullOrEmpty(saleExchange.SlipDocument) ? saleExchange.SlipDocument : self.SlipDocument;
                    isExchange.SlipPath = !string.IsNullOrEmpty(saleExchange.SlipPath) ? saleExchange.SlipPath : self.SlipPath;

                    Db.SaveChanges();


                    var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasao && x.LocationID == locId && x.CashActionTypeID == isExchange.ActionTypeID && x.ProcessID == isExchange.ID);
                    var cashactioni = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == locId && x.CashActionTypeID == isExchange.ActionTypeID && x.ProcessID == isExchange.ID);
                    if (cashaction != null)
                    {
                        cashaction.LocationID = isExchange.LocationID;
                        cashaction.CashID = isExchange.FromCashID;
                        cashaction.Collection = isExchange.Amount;
                        cashaction.Currency = isExchange.Currency;
                        cashaction.ActionDate = saleExchange.DocumentDate;
                        cashaction.ProcessDate = saleExchange.DocumentDate;
                        cashaction.UpdateDate = isExchange.UpdateDate;
                        cashaction.UpdateEmployeeID = isExchange.UpdateEmployee;

                        Db.SaveChanges();

                    }
                    if (cashactioni != null)
                    {
                        cashactioni.LocationID = isExchange.LocationID;
                        cashactioni.CashID = isExchange.ToCashID;
                        cashactioni.Collection = isExchange.ToAmount;
                        cashactioni.Currency = isExchange.ToCurrency;
                        cashactioni.ActionDate = saleExchange.DocumentDate;
                        cashactioni.ProcessDate = saleExchange.DocumentDate;
                        cashactioni.UpdateDate = isExchange.UpdateDate;
                        cashactioni.UpdateEmployeeID = isExchange.UpdateEmployee;

                        Db.SaveChanges();

                    }
                    result.IsSuccess = true;
                    result.Message = $"{isExchange.ID} ID li {isExchange.Date} tarihli {isExchange.Amount} {isExchange.Currency} kasa döviz satışı başarı ile güncellendi";


                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSaleExchange>(self, isExchange, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isExchange.ID.ToString(), "Cash", "ExchangeSale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
                catch (Exception ex)
                {
                    result.Message = $"{saleExchange.Amount} {saleExchange.Currency} kasa döviz satışı Güncellenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isExchange.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }


            }

            return result;
        }

        public Result<DocumentSaleExchange> DeleteSaleExchange(Guid? id, AuthenticationModel authentication)
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
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            OfficeHelper.AddCashAction(isCash.ToCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.ToAmount, 0, isCash.ToCurrency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }

            }

            return result;
        }



        public Result<DocumentCashOpen> AddCashOpen(CashOpen cashOpen, AuthenticationModel authentication)
        {
            Result<DocumentCashOpen> result = new Result<DocumentCashOpen>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (cashOpen != null && authentication != null)
            {

                using (ActionTimeEntities Db = new ActionTimeEntities())
                {

                    var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashOpen.ActinTypeID);
                    var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                    var amount = cashOpen.Amount;
                    var currency = cashOpen.Currency;
                    
                    var docDate = new DateTime(cashOpen.docDate ?? DateTime.Now.Year, 1, 1);
                    int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
                    var cash = OfficeHelper.GetCash(cashOpen.LocationID, cashOpen.Currency);

                    var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.LocationID == cashOpen.LocationID && x.CashID == cash.ID && x.ActionTypeID == cashOpen.ActinTypeID);
                    if (isOpen == null)
                    {
                        try
                        {


                            DocumentCashOpen open = new DocumentCashOpen();

                            open.ActionTypeID = actType.ID;
                            open.ActionTypeName = actType.Name;
                            open.Amount = amount;
                            open.CashID = cash.ID;
                            open.Currency = currency;
                            open.Date = docDate;
                            open.Description = cashOpen.Description;
                            open.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "COS");
                            open.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            open.IsActive = true;
                            open.LocationID = cashOpen.LocationID;
                            open.OurCompanyID = location.OurCompanyID;
                            open.RecordDate = DateTime.UtcNow.AddHours(timezone);
                            open.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            open.RecordIP = OfficeHelper.GetIPAddress();
                            open.SystemAmount = ourcompany.Currency == currency ? amount : amount * cashOpen.ExchangeRate;
                            open.SystemCurrency = ourcompany.Currency;
                            open.EnvironmentID = 2;
                            open.ReferenceID = cashOpen.ReferanceID;
                            open.UID = Guid.NewGuid();

                            Db.DocumentCashOpen.Add(open);
                            Db.SaveChanges();


                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(open.CashID, open.LocationID, null, open.ActionTypeID, open.Date, open.ActionTypeName, open.ID, open.Date, open.DocumentNumber, open.Description, 1, open.Amount, 0, open.Currency, null, null, open.RecordEmployeeID, open.RecordDate, open.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{open.Date} tarihli { open.Amount } {open.Currency} tutarındaki kasa açılış fişi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", open.ID.ToString(), "Cash", "Open", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, open);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi eklenemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Open", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isOpen.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                    }
                    else
                    {
                        try
                        {
                            DocumentCashOpen self = new DocumentCashOpen()
                            {
                                ActionTypeID = isOpen.ActionTypeID,
                                ActionTypeName = isOpen.ActionTypeName,
                                Amount = isOpen.Amount,
                                CashID = isOpen.CashID,
                                Currency = isOpen.Currency,
                                Date = isOpen.Date,
                                Description = isOpen.Description,
                                DocumentNumber = isOpen.DocumentNumber,
                                ExchangeRate = isOpen.ExchangeRate,
                                ID = isOpen.ID,
                                IsActive = isOpen.IsActive,
                                LocationID = isOpen.LocationID,
                                OurCompanyID = isOpen.OurCompanyID,
                                RecordDate = isOpen.RecordDate,
                                RecordEmployeeID = isOpen.RecordEmployeeID,
                                RecordIP = isOpen.RecordIP,
                                ReferenceID = isOpen.ReferenceID,
                                SystemAmount = isOpen.SystemAmount,
                                SystemCurrency = isOpen.SystemCurrency,
                                UpdateDate = isOpen.UpdateDate,
                                UpdateEmployee = isOpen.UpdateEmployee,
                                UpdateIP = isOpen.UpdateIP,
                                EnvironmentID = isOpen.EnvironmentID
                            };
                            
                            isOpen.ReferenceID = cashOpen.ReferanceID;
                            isOpen.Amount = amount;
                            isOpen.Description = cashOpen.Description;
                            isOpen.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            isOpen.UpdateDate = DateTime.UtcNow.AddHours(cashOpen.TimeZone.Value);
                            isOpen.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                            isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                            isOpen.SystemCurrency = ourcompany.Currency;

                            Db.SaveChanges();

                            var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.CashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isOpen.Date && x.DocumentNumber == isOpen.DocumentNumber);

                            if (cashaction != null)
                            {
                                cashaction.Collection = isOpen.Amount;
                                cashaction.UpdateDate = isOpen.UpdateDate;
                                cashaction.UpdateEmployeeID = isOpen.UpdateEmployee;

                                Db.SaveChanges();

                            }

                            result.IsSuccess = true;
                            result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki kasa açılış fişi başarı ile güncellendi";


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashOpen>(self, isOpen, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "Open", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Open", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(cashOpen.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }

                    }


                }

            }

            return result;
        }

        public Result<DocumentCashOpen> EditCashOpen(CashOpen cashOpen, AuthenticationModel authentication)
        {
            Result<DocumentCashOpen> result = new Result<DocumentCashOpen>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {

                var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.UID == cashOpen.UID);
                if (isOpen != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(cashOpen.LocationID, cashOpen.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                        var locId = cashOpen.LocationID;
                        var exchange = OfficeHelper.GetExchange(isOpen.Date.Value);
                        DocumentCashOpen self = new DocumentCashOpen()
                        {
                            ActionTypeID = isOpen.ActionTypeID,
                            ActionTypeName = isOpen.ActionTypeName,
                            Amount = isOpen.Amount,
                            CashID = isOpen.CashID,
                            Currency = isOpen.Currency,
                            Date = isOpen.Date,
                            Description = isOpen.Description,
                            DocumentNumber = isOpen.DocumentNumber,
                            ExchangeRate = isOpen.ExchangeRate,
                            ID = isOpen.ID,
                            IsActive = isOpen.IsActive,
                            LocationID = isOpen.LocationID,
                            OurCompanyID = isOpen.OurCompanyID,
                            RecordDate = isOpen.RecordDate,
                            RecordEmployeeID = isOpen.RecordEmployeeID,
                            RecordIP = isOpen.RecordIP,
                            ReferenceID = isOpen.ReferenceID,
                            SystemAmount = isOpen.SystemAmount,
                            SystemCurrency = isOpen.SystemCurrency,
                            UpdateDate = isOpen.UpdateDate,
                            UpdateEmployee = isOpen.UpdateEmployee,
                            UpdateIP = isOpen.UpdateIP,
                            EnvironmentID = isOpen.EnvironmentID
                        };
                        isOpen.ReferenceID = cashOpen.ReferanceID;
                        isOpen.LocationID = cashOpen.LocationID;
                        isOpen.Amount = cashOpen.Amount;
                        isOpen.Description = cashOpen.Description;
                        isOpen.ExchangeRate = cashOpen.ExchangeRate != null ? cashOpen.ExchangeRate : self.ExchangeRate;
                        isOpen.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isOpen.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                        isOpen.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == cashOpen.Currency ? cashOpen.Amount : cashOpen.Amount * isOpen.ExchangeRate;
                        isOpen.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isOpen.LocationID;
                            cashaction.Collection = isOpen.Amount;
                            cashaction.UpdateDate = isOpen.UpdateDate;
                            cashaction.UpdateEmployeeID = isOpen.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isOpen.ID} ID li {isOpen.Amount} {isOpen.Currency} tutarındaki kasa açılış fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashOpen>(self, isOpen, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "Open", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(cashOpen.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isOpen.Amount} {isOpen.Currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Open", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(cashOpen.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentCashOpen> DeleteCashOpen(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentCashOpen> result = new Result<DocumentCashOpen>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentCashOpen.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa açılış fişi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashOpen", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa açılış fişi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentCashPayments> AddCashPayment(CashPayment payment, AuthenticationModel authentication)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (payment != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var balance = Db.GetCashBalance(payment.LocationID, payment.CashID, payment.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= payment.Amount)
                        {
                            DocumentCashPayments cashPayment = new DocumentCashPayments();

                            cashPayment.ActionTypeID = payment.ActinTypeID;
                            cashPayment.ActionTypeName = payment.ActionTypeName;
                            cashPayment.Amount = payment.Amount;
                            cashPayment.CashID = payment.CashID;
                            cashPayment.Currency = payment.Currency;
                            cashPayment.Date = payment.DocumentDate;
                            cashPayment.Description = payment.Description;
                            cashPayment.DocumentNumber = OfficeHelper.GetDocumentNumber(payment.OurCompanyID, "CPY");
                            cashPayment.ExchangeRate = payment.ExchangeRate;
                            cashPayment.ToEmployeeID = payment.ToEmployeeID;
                            cashPayment.ToCustomerID = payment.ToCustomerID;
                            cashPayment.IsActive = true;
                            cashPayment.LocationID = payment.LocationID;
                            cashPayment.OurCompanyID = payment.OurCompanyID;
                            cashPayment.RecordDate = DateTime.UtcNow.AddHours(payment.TimeZone.Value);
                            cashPayment.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            cashPayment.RecordIP = OfficeHelper.GetIPAddress();
                            cashPayment.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == payment.Currency ? payment.Amount : payment.Amount * payment.ExchangeRate;
                            cashPayment.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                            cashPayment.ReferenceID = payment.ReferanceID;
                            cashPayment.EnvironmentID = 2;
                            cashPayment.UID = Guid.NewGuid();



                            Db.DocumentCashPayments.Add(cashPayment);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(cashPayment.CashID, cashPayment.LocationID, null, cashPayment.ActionTypeID, cashPayment.Date, cashPayment.ActionTypeName, cashPayment.ID, cashPayment.Date, cashPayment.DocumentNumber, cashPayment.Description, -1, 0, cashPayment.Amount, cashPayment.Currency, null, null, cashPayment.RecordEmployeeID, cashPayment.RecordDate, cashPayment.UID.Value);



                            result.IsSuccess = true;
                            result.Message = "Kasa Ödemesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashPayment.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashPayment);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { payment.Amount } { payment.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { payment.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Kasa Ödemesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashPayments> EditCashPayment(CashPayment payment, AuthenticationModel authentication)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPayment = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == payment.UID);

                if (isPayment != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(payment.LocationID, payment.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == payment.LocationID);
                        var locId = isPayment.LocationID;
                        var exchange = OfficeHelper.GetExchange(payment.DocumentDate.Value);

                        DocumentCashPayments self = new DocumentCashPayments()
                        {
                            ActionTypeID = isPayment.ActionTypeID,
                            ActionTypeName = isPayment.ActionTypeName,
                            Amount = isPayment.Amount,
                            CashID = isPayment.CashID,
                            Currency = isPayment.Currency,
                            Date = isPayment.Date,
                            Description = isPayment.Description,
                            DocumentNumber = isPayment.DocumentNumber,
                            ExchangeRate = isPayment.ExchangeRate,
                            ID = isPayment.ID,
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
                            EnvironmentID = isPayment.EnvironmentID
                        };
                        isPayment.ReferenceID = payment.ReferanceID;
                        isPayment.LocationID = payment.LocationID;
                        isPayment.CashID = cash.ID;
                        isPayment.Date = payment.DocumentDate;
                        isPayment.ToEmployeeID = payment.ToEmployeeID ?? (int?)null;
                        isPayment.ToCustomerID = payment.ToCustomerID ?? (int?)null;
                        isPayment.Amount = payment.Amount;
                        isPayment.Currency = payment.Currency;
                        isPayment.Description = payment.Description;
                        isPayment.ExchangeRate = payment.ExchangeRate != null ? payment.ExchangeRate : self.ExchangeRate;
                        isPayment.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isPayment.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isPayment.UpdateIP = OfficeHelper.GetIPAddress();
                        isPayment.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == payment.Currency ? payment.Amount : payment.Amount * isPayment.ExchangeRate;
                        isPayment.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isPayment.ActionTypeID && x.ProcessID == isPayment.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isPayment.LocationID;
                            cashaction.Collection = isPayment.Amount;
                            cashaction.CashID = cash.ID;
                            cashaction.Currency = payment.Currency;
                            cashaction.ActionDate = payment.DocumentDate;
                            cashaction.ProcessDate = payment.DocumentDate;
                            cashaction.UpdateDate = isPayment.UpdateDate;
                            cashaction.UpdateEmployeeID = isPayment.UpdateEmployee;

                            Db.SaveChanges();

                        }


                        result.IsSuccess = true;
                        result.Message = $"{isPayment.ID} ID li {isPayment.Date} tarihli {isPayment.Amount} {isPayment.Currency} tutarındaki kasa ödemesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashPayments>(self, isPayment, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isPayment.ID.ToString(), "Cash", "CashPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{payment.Amount} {payment.Currency} tutarındaki kasa ödemesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
            }

            return result;
        }

        public Result<DocumentCashPayments> DeleteCashPayment(Guid? id, AuthenticationModel authentication)
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
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentTicketSaleReturns> AddCashSaleReturn(SaleReturn sale, AuthenticationModel authentication)
        {
            Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (sale != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var balance = Db.GetCashBalance(sale.LocationID, sale.CashID, sale.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= sale.Amount)
                        {
                            DocumentTicketSaleReturns ticketSale = new DocumentTicketSaleReturns();

                            ticketSale.ActionTypeID = sale.ActinTypeID;
                            ticketSale.ActionTypeName = sale.ActionTypeName;
                            ticketSale.Amount = sale.Amount;
                            ticketSale.CashID = sale.CashID;
                            ticketSale.Currency = sale.Currency;
                            ticketSale.Date = sale.DocumentDate;
                            ticketSale.Description = sale.Description;
                            ticketSale.DocumentNumber = OfficeHelper.GetDocumentNumber(sale.OurCompanyID, "TSR");
                            ticketSale.ExchangeRate = sale.ExchangeRate;
                            ticketSale.ToCustomerID = sale.ToCustomerID;
                            ticketSale.IsActive = true;
                            ticketSale.LocationID = sale.LocationID;
                            ticketSale.OurCompanyID = sale.OurCompanyID;
                            ticketSale.RecordDate = DateTime.UtcNow.AddHours(sale.TimeZone.Value);
                            ticketSale.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            ticketSale.RecordIP = OfficeHelper.GetIPAddress();
                            ticketSale.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * sale.ExchangeRate;
                            ticketSale.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                            ticketSale.ReferenceID = sale.ReferanceID;
                            ticketSale.Quantity = sale.Quantity;
                            ticketSale.PayMethodID = sale.PayMethodID;
                            ticketSale.EnvironmentID = 2;
                            ticketSale.UID = Guid.NewGuid();




                            Db.DocumentTicketSaleReturns.Add(ticketSale);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, 0, ticketSale.Amount, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate, ticketSale.UID.Value);




                            result.IsSuccess = true;
                            result.Message = "Bilet satış iadesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, ticketSale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { sale.Amount } { sale.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { sale.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Bilet satış iadesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentTicketSaleReturns> EditCashSaleReturn(SaleReturn sale, AuthenticationModel authentication)
        {
            Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.UID == sale.UID);

                if (isCash != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(sale.LocationID, sale.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == sale.LocationID);
                        var locId = isCash.LocationID;
                        var exchange = OfficeHelper.GetExchange(sale.DocumentDate.Value);

                        DocumentTicketSaleReturns self = new DocumentTicketSaleReturns()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.ReferenceID = sale.ReferanceID;
                        isCash.LocationID = sale.LocationID;
                        isCash.CashID = cash.ID;
                        isCash.Date = sale.DocumentDate;
                        isCash.Quantity = sale.Quantity;
                        isCash.PayMethodID = sale.PayMethodID;
                        isCash.ToCustomerID = sale.ToCustomerID ?? (int?)null;
                        isCash.Amount = sale.Amount;
                        isCash.Description = sale.Description;
                        isCash.ExchangeRate = sale.ExchangeRate != null ? sale.ExchangeRate : self.ExchangeRate;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == sale.Currency ? sale.Amount : sale.Amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Collection = isCash.Amount;
                            cashaction.CashID = cash.ID;
                            cashaction.Currency = sale.Currency;
                            cashaction.ActionDate = sale.DocumentDate;
                            cashaction.ProcessDate = sale.DocumentDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSaleReturns>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "SaleReturn", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(sale.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentTicketSaleReturns> DeleteCashSaleReturn(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iade iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentCashExpense> AddCashExpense(CashExpense expense, AuthenticationModel authentication)
        {
            Result<DocumentCashExpense> result = new Result<DocumentCashExpense>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (expense != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {

                        DocumentCashExpense cashExpense = new DocumentCashExpense();

                        cashExpense.ActionTypeID = expense.ActinTypeID;
                        cashExpense.ActionTypeName = expense.ActionTypeName;
                        cashExpense.Amount = expense.Amount;
                        cashExpense.CashID = expense.CashID;
                        cashExpense.Currency = expense.Currency;
                        cashExpense.Date = expense.DocumentDate;
                        cashExpense.Description = expense.Description;
                        cashExpense.DocumentNumber = OfficeHelper.GetDocumentNumber(expense.OurCompanyID, "EXP");
                        cashExpense.ExchangeRate = expense.ExchangeRate;
                        cashExpense.ToBankAccountID = expense.ToBankAccountID ?? (int?)null;
                        cashExpense.ToEmployeeID = expense.ToEmployeeID ?? (int?)null;
                        cashExpense.ToCustomerID = expense.ToCustomerID ?? (int?)null;
                        cashExpense.IsActive = true;
                        cashExpense.LocationID = expense.LocationID;
                        cashExpense.OurCompanyID = expense.OurCompanyID;
                        cashExpense.RecordDate = DateTime.UtcNow.AddHours(expense.TimeZone.Value);
                        cashExpense.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        cashExpense.RecordIP = OfficeHelper.GetIPAddress();
                        cashExpense.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == expense.Currency ? expense.Amount : expense.Amount * expense.ExchangeRate;
                        cashExpense.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        cashExpense.SlipNumber = expense.SlipNumber;
                        cashExpense.SlipDate = expense.SlipDate;
                        cashExpense.ReferenceID = expense.ReferanceID;
                        cashExpense.EnvironmentID = 2;
                        cashExpense.UID = Guid.NewGuid();
                        cashExpense.ExpenseTypeID = expense.ExpenseTypeID;
                        cashExpense.SlipPath = expense.SlipPath;
                        cashExpense.SlipDocument = expense.SlipDocument;
                        cashExpense.ReferenceTableModel = expense.ReferanceModel;

                        Db.DocumentCashExpense.Add(cashExpense);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate, cashExpense.UID.Value);



                        result.IsSuccess = true;
                        result.Message = "Masraf ödeme fişi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashExpense.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(expense.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashExpense);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(expense.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashExpense> EditCashExpense(CashExpense expense, AuthenticationModel authentication)
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
                        var cash = OfficeHelper.GetCash(expense.LocationID, expense.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == expense.LocationID);
                        var locId = isExpense.LocationID;
                        var exchange = OfficeHelper.GetExchange(expense.DocumentDate.Value);

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
                        isExpense.ExchangeRate = expense.ExchangeRate != null ? expense.ExchangeRate : self.ExchangeRate;
                        isExpense.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isExpense.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isExpense.UpdateIP = OfficeHelper.GetIPAddress();
                        isExpense.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == expense.Currency ? expense.Amount : expense.Amount * isExpense.ExchangeRate;
                        isExpense.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isExpense.ActionTypeID && x.ProcessID == isExpense.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isExpense.LocationID;
                            cashaction.Payment = isExpense.Amount;
                            cashaction.Currency = isExpense.Currency;
                            cashaction.CashID = cash.ID;
                            cashaction.ActionDate = isExpense.Date;
                            cashaction.ProcessDate = isExpense.Date;
                            cashaction.UpdateDate = isExpense.UpdateDate;
                            cashaction.UpdateEmployeeID = isExpense.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isExpense.ID} ID li {isExpense.Date} tarihli {isExpense.Amount} {isExpense.Currency} tutarındaki masraf ödeme fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashExpense>(self, isExpense, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isExpense.ID.ToString(), "Cash", "Expense", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(expense.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(expense.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentCashExpense> DeleteCashExpense(Guid? id, AuthenticationModel authentication)
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
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentBankTransfer> AddBankTransfer(BankTransfer transfer, AuthenticationModel authentication)
        {
            Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (transfer != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(transfer.LocationID, transfer.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
                        var exchange = OfficeHelper.GetExchange(transfer.DocumentDate.Value);

                        var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29);

                        var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.LocationID && x.Date == transfer.DocumentDate);

                        DocumentBankTransfer bankTransfer = new DocumentBankTransfer();

                        bankTransfer.ActionTypeID = transfer.ActinTypeID;
                        bankTransfer.ActionTypeName = transfer.ActionTypeName;
                        bankTransfer.Amount = transfer.Amount;
                        bankTransfer.Commission = transfer.Commission;
                        bankTransfer.FromCashID = transfer.FromCashID;
                        bankTransfer.Currency = transfer.Currency;
                        bankTransfer.Date = transfer.DocumentDate;
                        bankTransfer.Description = transfer.Description;
                        bankTransfer.DocumentNumber = OfficeHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompanyID.Value, "BT");
                        bankTransfer.ExchangeRate = transfer.ExchangeRate;
                        bankTransfer.ToBankAccountID = transfer.ToBankID;
                        bankTransfer.IsActive = true;
                        bankTransfer.LocationID = transfer.LocationID;
                        bankTransfer.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
                        bankTransfer.RecordDate = DateTime.UtcNow.AddHours(transfer.TimeZone.Value);
                        bankTransfer.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        bankTransfer.RecordIP = OfficeHelper.GetIPAddress();
                        bankTransfer.SystemAmount = bankTransfer.Amount * bankTransfer.ExchangeRate;
                        bankTransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        bankTransfer.SlipNumber = transfer.SlipNumber;
                        bankTransfer.SlipDate = transfer.SlipDate;
                        bankTransfer.StatusID = transfer.StatusID ?? 1;
                        bankTransfer.EnvironmentID = 2;
                        bankTransfer.ReferenceID = transfer.ReferanceID;
                        bankTransfer.UID = transfer.UID;
                        bankTransfer.SlipDocument = transfer.SlipDocument;
                        bankTransfer.SlipPath = transfer.SlipPath;

                        bankTransfer.ReferenceCode = OfficeHelper.BankReferenceCode(bankTransfer.OurCompanyID.Value);

                        Db.DocumentBankTransfer.Add(bankTransfer);
                        Db.SaveChanges();

                        if (new int?[] { 2, 3, 4, 5 }.Contains(bankTransfer.StatusID))
                        {
                            if (bankTransfer.Commission > 0)  // komisyonlu işlem ise
                            {
                                var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == bankTransfer.ID && x.Date == bankTransfer.Date && x.LocationID == bankTransfer.LocationID);
                                if (isExpense == null)
                                {
                                    CashExpense expense = new CashExpense();

                                    expense.ActinTypeID = actType.ID;
                                    expense.ActionTypeName = actType.Name;
                                    expense.Amount = bankTransfer.Commission.Value;
                                    expense.Currency = bankTransfer.Currency;
                                    expense.Description = bankTransfer.Description;
                                    expense.DocumentDate = bankTransfer.Date;
                                    expense.EnvironmentID = bankTransfer.EnvironmentID;
                                    expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
                                    expense.CashID = cash.ID;
                                    expense.LocationID = location.LocationID;
                                    expense.OurCompanyID = location.OurCompanyID;
                                    expense.SlipDate = bankTransfer.SlipDate;
                                    expense.SlipNumber = bankTransfer.SlipNumber;
                                    expense.SlipDocument = bankTransfer.SlipDocument;
                                    expense.TimeZone = location.Timezone.Value;
                                    expense.UID = Guid.NewGuid();
                                    expense.ExpenseTypeID = 25;
                                    expense.ReferanceID = bankTransfer.ID;
                                    expense.ResultID = dayresult?.ID;
                                    expense.ToBankAccountID = bankTransfer.ToBankAccountID;
                                    expense.SlipPath = bankTransfer.SlipPath;
                                    expense.ReferanceModel = transfer.ReferanceModel;

                                    var expenseresult = AddCashExpense(expense, authentication);
                                    result.Message += $" {expenseresult.Message}";
                                }

                            }

                            var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (cashaction == null)
                            {
                                OfficeHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                        }
                        else if (new int?[] { 6, 7 }.Contains(bankTransfer.StatusID))
                        {
                            var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.CashActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (expaction != null)
                            {

                                OfficeHelper.AddCashAction(bankTransfer.FromCashID, bankTransfer.LocationID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, -1, 0, -1 * bankTransfer.NetAmount, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                            var expbank = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (expbank != null)
                            {
                                OfficeHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, -1 * bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }
                        }

                        if (new int?[] { 5 }.Contains(bankTransfer.StatusID))
                        {
                            var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == bankTransfer.LocationID && x.BankActionTypeID == bankTransfer.ActionTypeID && x.ProcessID == bankTransfer.ID && x.ProcessUID == bankTransfer.UID);
                            if (cashaction == null)
                            {
                                OfficeHelper.AddBankAction(bankTransfer.LocationID, null, bankTransfer.ToBankAccountID, null, bankTransfer.ActionTypeID, bankTransfer.Date, bankTransfer.ActionTypeName, bankTransfer.ID, bankTransfer.Date, bankTransfer.DocumentNumber, bankTransfer.Description, 1, bankTransfer.NetAmount, 0, bankTransfer.Currency, null, null, bankTransfer.RecordEmployeeID, bankTransfer.RecordDate, bankTransfer.UID.Value);
                            }

                        }

                        result.IsSuccess = true;
                        result.Message = "Havale / EFT bildirimi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", bankTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, bankTransfer);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentBankTransfer> EditBankTransfer(BankTransfer transfer, AuthenticationModel authentication)
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
                        var cash = OfficeHelper.GetCash(transfer.LocationID, transfer.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
                        var exchange = OfficeHelper.GetExchange(transfer.DocumentDate.Value);

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
                        isTransfer.ExchangeRate = transfer.ExchangeRate != null ? transfer.ExchangeRate : self.ExchangeRate;
                        isTransfer.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isTransfer.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isTransfer.UpdateIP = OfficeHelper.GetIPAddress();
                        isTransfer.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == transfer.Currency ? transfer.Amount : transfer.Amount * isTransfer.ExchangeRate;
                        isTransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        isTransfer.StatusID = transfer.StatusID;
                        isTransfer.TrackingNumber = transfer.TrackingNumber;
                        isTransfer.ReferenceCode = transfer.ReferanceCode;
                        isTransfer.LocationID = transfer.LocationID;
                        isTransfer.Currency = transfer.Currency;
                        isTransfer.IsActive = transfer.IsActive;
                        isTransfer.SlipDocument = !string.IsNullOrEmpty(transfer.SlipDocument) ? transfer.SlipDocument : self.SlipDocument;
                        isTransfer.SlipPath = !string.IsNullOrEmpty(transfer.SlipPath) ? transfer.SlipPath : self.SlipPath;



                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message += "Banka Havale / EFT işlemi Güncellendi";

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);



                        // 01. mevcut kasa çıkış hareketi, banka giriş hareketi ve kasa masraf hareketi varsa sil 

                        var iscashexit = Db.CashActions.FirstOrDefault(x => x.LocationID == self.LocationID && x.CashActionTypeID == self.ActionTypeID && x.ProcessID == self.ID && x.ProcessUID == self.UID);
                        if (iscashexit != null)
                        {
                            Db.CashActions.Remove(iscashexit);
                            Db.SaveChanges();
                        }

                        var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == self.ID && x.Date == self.Date && x.LocationID == self.LocationID);

                        if (isexpenseexists != null)
                        {
                            var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
                            if (iscashexpenseexit != null)
                            {
                                Db.CashActions.Remove(iscashexpenseexit);
                                Db.SaveChanges();
                            }

                            Db.DocumentCashExpense.Remove(isexpenseexists);
                            Db.SaveChanges();
                        }

                        var isbankexists = Db.BankActions.FirstOrDefault(x => x.LocationID == isTransfer.LocationID && x.BankActionTypeID == isTransfer.ActionTypeID && x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID);
                        if (isbankexists != null)
                        {
                            Db.BankActions.Remove(isbankexists);
                            Db.SaveChanges();
                        }

                        // 02. yeni kasa çıkış hareketlerini ekle

                        if (new int?[] { 2, 3, 4, 5 }.Contains(isTransfer.StatusID))
                        {
                            // 01. yeni kasa çıkış hareketi komisyonsuz tutar miktarınca eklenir
                            var mainamount = (isTransfer.Amount - isTransfer.Commission);

                            OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, mainamount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                            result.Message += $" kasa çıkış işlemi yapıldı. ";

                            if (transfer.Commission > 0)  // komisyonlu işlem ise
                            {

                                // 02. yeni kasa masraf evrağı komisyon tutarı miktarınca eklenir 

                                CashExpense expense = new CashExpense();

                                expense.ActinTypeID = actType.ID;
                                expense.ActionTypeName = actType.Name;
                                expense.Amount = isTransfer.Commission.Value;
                                expense.Currency = isTransfer.Currency;
                                expense.Description = isTransfer.Description;
                                expense.DocumentDate = isTransfer.Date;
                                expense.EnvironmentID = isTransfer.EnvironmentID;
                                expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
                                expense.CashID = cash.ID;
                                expense.LocationID = location.LocationID;
                                expense.OurCompanyID = location.OurCompanyID;
                                expense.SlipDate = isTransfer.SlipDate;
                                expense.SlipNumber = isTransfer.SlipNumber;
                                expense.SlipDocument = isTransfer.SlipDocument;
                                expense.TimeZone = location.Timezone.Value;
                                expense.UID = Guid.NewGuid();
                                expense.ExpenseTypeID = 25;
                                expense.ReferanceID = isTransfer.ID;
                                expense.ResultID = dayresult?.ID;
                                expense.ToBankAccountID = isTransfer.ToBankAccountID;
                                expense.SlipPath = isTransfer.SlipPath;
                                expense.ReferanceModel = transfer.ReferanceModel;

                                var expenseresult = AddCashExpense(expense, authentication);
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
                            OfficeHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                            result.Message += $" banka giriş işlemi yapıldı. ";
                        }


                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentBankTransfer> DeleteCashBankTransfer(Guid? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isCash.ID && x.Date == isCash.Date && x.LocationID == isCash.LocationID);

                            if (isexpenseexists != null)
                            {
                                var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
                                if (iscashexpenseexit != null)
                                {
                                    isexpenseexists.IsActive = false;
                                    isexpenseexists.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                                    isexpenseexists.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                                    isexpenseexists.UpdateIP = OfficeHelper.GetIPAddress();
                                    Db.SaveChanges();
                                }

                                Db.DocumentCashExpense.Remove(isexpenseexists);
                                Db.SaveChanges();
                            }

                            var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessUID == isCash.UID);
                            if (expaction != null)
                            {

                                OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.NetAmount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            }
                            var expbank = Db.BankActions.FirstOrDefault(x => x.LocationID == isCash.LocationID && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessUID == isCash.UID);
                            if (expbank != null)
                            {
                                OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.ToBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.NetAmount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            }

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki havale eft başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki havale eft iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentSalaryEarn> AddSalaryEarn(SalaryEarn salary, AuthenticationModel authentication)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (salary != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentSalaryEarn salaryEarn = new DocumentSalaryEarn();

                        salaryEarn.ActionTypeID = salary.ActionTypeID;
                        salaryEarn.ActionTypeName = salary.ActionTypeName;
                        salaryEarn.EmployeeID = salary.EmployeeID;
                        salaryEarn.TotalAmount = salary.TotalAmount;
                        salaryEarn.UnitPrice = salary.UnitPrice;
                        salaryEarn.QuantityHour = salary.QuantityHour;
                        salaryEarn.Currency = salary.Currency;
                        salaryEarn.Date = salary.DocumentDate;
                        salaryEarn.Description = salary.Description;
                        salaryEarn.DocumentNumber = OfficeHelper.GetDocumentNumber(salary.OurCompanyID, "SE");
                        salaryEarn.IsActive = true;
                        salaryEarn.LocationID = salary.LocationID;
                        salaryEarn.OurCompanyID = salary.OurCompanyID;
                        salaryEarn.RecordDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
                        salaryEarn.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        salaryEarn.RecordIP = OfficeHelper.GetIPAddress();
                        salaryEarn.UID = salary.UID;
                        salaryEarn.EnvironmentID = salary.EnvironmentID;
                        salaryEarn.ReferenceID = salary.ReferanceID;
                        salaryEarn.ResultID = salary.ResultID;
                        salaryEarn.SystemQuantityHour = salary.SystemQuantityHour;
                        salaryEarn.SystemTotalAmount = salary.SystemTotalAmount;
                        salaryEarn.SystemUnitPrice = salary.SystemUnitPrice;
                        salaryEarn.CategoryID = salary.CategoryID;

                        Db.DocumentSalaryEarn.Add(salaryEarn);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, salaryEarn.ActionTypeID, salaryEarn.ActionTypeName, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmount, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber);

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", salaryEarn.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, salaryEarn);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Ücret Hakediş eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }

                }
            }

            return result;
        }

        public Result<DocumentSalaryEarn> EditSalaryEarn(SalaryEarn salary, AuthenticationModel authentication)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
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
                        var exchange = OfficeHelper.GetExchange(salary.DocumentDate.Value);

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
                            SystemUnitPrice = isEarn.SystemUnitPrice
                        };
                        isEarn.ReferenceID = salary.ReferanceID;
                        isEarn.CategoryID = salary.CategoryID;
                        isEarn.EmployeeID = salary.EmployeeID;
                        isEarn.TotalAmount = (double)((double?)salary.UnitPrice * (double)salary.QuantityHour);
                        isEarn.UnitPrice = (double?)salary.UnitPrice;
                        isEarn.Description = salary.Description;
                        isEarn.QuantityHour = (double)salary.QuantityHour;
                        isEarn.UpdateDate = DateTime.UtcNow.AddHours(salary.TimeZone.Value);
                        isEarn.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isEarn.UpdateIP = OfficeHelper.GetIPAddress();
                        isEarn.SystemQuantityHour = salary.SystemQuantityHour;
                        isEarn.SystemTotalAmount = salary.SystemTotalAmount;
                        isEarn.SystemUnitPrice = salary.SystemUnitPrice;

                        Db.SaveChanges();

                        var empaction = Db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == isEmp && x.ActionTypeID == isEarn.ActionTypeID && x.ProcessID == isEarn.ID);

                        if (empaction != null)
                        {
                            empaction.Collection = isEarn.TotalAmount;
                            empaction.Currency = isEarn.Currency;
                            empaction.ProcessDate = isEarn.Date;
                            empaction.UpdateDate = isEarn.UpdateDate;
                            empaction.UpdateEmployeeID = isEarn.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş başarı ile güncellendi";

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryEarn>(self, isEarn, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", isEarn.ID.ToString(), "Salary", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Ücret Hakediş güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(salary.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }


            }

            return result;
        }

        public Result<DocumentSalaryEarn> DeleteSalaryEarn(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentSalaryEarn.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            //maaş hesap işlemi
                            OfficeHelper.AddEmployeeAction(isCash.EmployeeID, isCash.LocationID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.TotalAmount, 0, isCash.Currency, null, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value, isCash.DocumentNumber);

                            result.IsSuccess = true;
                            result.Message = "Ücret Hakediş iptal edildi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", isCash.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.TotalAmount} {isCash.Currency} tutarındaki ÜCRET HAKEDİŞ iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }

                }
            }

            return result;
        }




        public Result<DocumentSalaryPayment> AddSalaryPayment(SalaryPayment payment, AuthenticationModel authentication)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (payment != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var balance = Db.GetCashBalance(payment.LocationID, payment.FromCashID, payment.DocumentDate).FirstOrDefault() ?? 0;

                        if (balance >= payment.Amount)
                        {
                            DocumentSalaryPayment salaryPayment = new DocumentSalaryPayment();

                            salaryPayment.ActionTypeID = payment.ActinTypeID;
                            salaryPayment.ActionTypeName = payment.ActionTypeName;
                            salaryPayment.ToEmployeeID = payment.EmployeeID;
                            salaryPayment.Amount = payment.Amount;
                            salaryPayment.FromCashID = payment.FromCashID;
                            salaryPayment.Currency = payment.Currency;
                            salaryPayment.Date = payment.DocumentDate;
                            salaryPayment.Description = payment.Description;
                            salaryPayment.DocumentNumber = OfficeHelper.GetDocumentNumber(payment.OurCompanyID, "SAP");
                            salaryPayment.ExchangeRate = payment.ExchangeRate;
                            salaryPayment.FromBankAccountID = payment.FromBankID;
                            salaryPayment.IsActive = true;
                            salaryPayment.LocationID = payment.LocationID;
                            salaryPayment.OurCompanyID = payment.OurCompanyID;
                            salaryPayment.RecordDate = DateTime.UtcNow.AddHours(payment.TimeZone.Value);
                            salaryPayment.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            salaryPayment.RecordIP = OfficeHelper.GetIPAddress();
                            salaryPayment.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == salaryPayment.Currency ? salaryPayment.Amount : salaryPayment.Amount * salaryPayment.ExchangeRate;
                            salaryPayment.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                            salaryPayment.SalaryType = payment.SalaryTypeID;
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
                                OfficeHelper.AddBankAction(salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.FromBankAccountID, null, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value);
                            }
                            else
                            {
                                OfficeHelper.AddCashAction(salaryPayment.FromCashID, salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value);
                            }

                            //maaş hesap işlemi
                            OfficeHelper.AddEmployeeAction(salaryPayment.ToEmployeeID, salaryPayment.LocationID, salaryPayment.ActionTypeID, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.Description, 1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.SalaryType, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate, salaryPayment.UID.Value, salaryPayment.DocumentNumber);

                            result.IsSuccess = true;
                            result.Message = "Maaş Avans ödemesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", salaryPayment.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, salaryPayment);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { payment.Amount } { payment.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { payment.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentSalaryPayment> EditSalaryPayment(SalaryPayment payment, AuthenticationModel authentication)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPayment = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == payment.UID);

                if (isPayment != null)
                {
                    try
                    {
                        var cash = OfficeHelper.GetCash(payment.LocationID, payment.Currency);
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == payment.LocationID);
                        var locId = payment.LocationID;
                        var exchange = OfficeHelper.GetExchange(payment.DocumentDate.Value);

                        var isKasa = isPayment.FromCashID != null ? Convert.ToInt32(isPayment.FromCashID) : (int?)null;
                        var isBank = isPayment.FromBankAccountID != null ? isPayment.FromBankAccountID : (int?)null;

                        var isEmp = isPayment.ToEmployeeID;

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
                            SalaryType = isPayment.SalaryType,
                            UpdateDate = isPayment.UpdateDate,
                            UpdateEmployee = isPayment.UpdateEmployee,
                            UpdateIP = isPayment.UpdateIP,
                            CategoryID = isPayment.CategoryID,
                            EnvironmentID = isPayment.EnvironmentID
                        };
                        isPayment.ReferenceID = payment.ReferanceID;
                        isPayment.CategoryID = payment.CategoryID;
                        isPayment.LocationID = payment.LocationID;
                        isPayment.Currency = payment.Currency;
                        isPayment.Date = payment.DocumentDate;
                        isPayment.ToEmployeeID = (int?)payment.EmployeeID ?? (int?)null;
                        isPayment.Amount = payment.Amount;
                        isPayment.FromCashID = cash.ID;
                        isPayment.Description = payment.Description;
                        isPayment.ExchangeRate = payment.ExchangeRate != null ? payment.ExchangeRate : self.ExchangeRate;
                        isPayment.FromBankAccountID = payment.FromBankID ?? (int?)null;
                        isPayment.SalaryType = payment.SalaryTypeID;
                        isPayment.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isPayment.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isPayment.UpdateIP = OfficeHelper.GetIPAddress();
                        isPayment.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == payment.Currency ? payment.Amount : payment.Amount * isPayment.ExchangeRate;
                        isPayment.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        if (isKasa != null && payment.FromBankID == 0)
                        {
                            var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.EmployeeID == isEmp && x.CashActionTypeID == isPayment.ActionTypeID && x.ProcessID == isPayment.ID);

                            if (cashaction != null)
                            {
                                cashaction.Payment = isPayment.Amount;
                                cashaction.Currency = isPayment.Currency;
                                cashaction.CashID = (int?)isPayment.FromBankAccountID == 0 ? cash.ID : (int?)null;
                                cashaction.ActionDate = isPayment.Date;
                                cashaction.ProcessDate = isPayment.Date;
                                cashaction.EmployeeID = isPayment.ToEmployeeID;
                                cashaction.UpdateDate = isPayment.UpdateDate;
                                cashaction.UpdateEmployeeID = isPayment.UpdateEmployee;

                                Db.SaveChanges();

                            }
                        }
                        else if (isBank != null && payment.FromBankID > 0)
                        {
                            var bankaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isBank && x.LocationID == locId && x.EmployeeID == isEmp && x.BankActionTypeID == isPayment.ActionTypeID && x.ProcessID == isPayment.ID);

                            if (bankaction != null)
                            {
                                bankaction.Payment = isPayment.Amount;
                                bankaction.Currency = isPayment.Currency;
                                bankaction.BankAccountID = (int?)isPayment.FromBankAccountID > 0 ? isPayment.FromBankAccountID : (int?)null;
                                bankaction.ActionDate = isPayment.Date;
                                bankaction.ProcessDate = isPayment.Date;
                                bankaction.EmployeeID = isPayment.ToEmployeeID;
                                bankaction.UpdateDate = isPayment.UpdateDate;
                                bankaction.UpdateEmployeeID = isPayment.UpdateEmployee;

                                Db.SaveChanges();

                            }
                        }
                        else if (isKasa != null && payment.FromBankID != 0)
                        {
                            OfficeHelper.AddCashAction(isPayment.FromCashID, isPayment.LocationID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.RecordEmployeeID, isPayment.RecordDate, isPayment.UID.Value);
                            OfficeHelper.AddBankAction(isPayment.LocationID, null, isPayment.FromBankAccountID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, -1 * isPayment.Amount, isPayment.Currency, null, null, isPayment.RecordEmployeeID, isPayment.RecordDate, isPayment.UID.Value);

                        }
                        else if (isBank != null && payment.FromBankID == 0)
                        {
                            OfficeHelper.AddCashAction(isPayment.FromCashID, isPayment.LocationID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, -1 * isPayment.Amount, isPayment.Currency, null, null, isPayment.RecordEmployeeID, isPayment.RecordDate, isPayment.UID.Value);
                            OfficeHelper.AddBankAction(isPayment.LocationID, null, isPayment.FromBankAccountID, isPayment.ToEmployeeID, isPayment.ActionTypeID, isPayment.Date, isPayment.ActionTypeName, isPayment.ID, isPayment.Date, isPayment.DocumentNumber, isPayment.Description, -1, 0, isPayment.Amount, isPayment.Currency, null, null, isPayment.RecordEmployeeID, isPayment.RecordDate, isPayment.UID.Value);
                        }


                        var empaction = Db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == isEmp && x.ActionTypeID == isPayment.ActionTypeID && x.ProcessID == isPayment.ID);

                        if (empaction != null)
                        {
                            empaction.ProcessDate = isPayment.Date;
                            empaction.Collection = isPayment.Amount;
                            empaction.Currency = isPayment.Currency;
                            empaction.EmployeeID = isPayment.ToEmployeeID;
                            empaction.UpdateDate = isPayment.UpdateDate;
                            empaction.UpdateEmployeeID = isPayment.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryPayment>(self, isPayment, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", isPayment.ID.ToString(), "Salary", "SalaryPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans güncellenemdi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(payment.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentSalaryPayment> DeleteSalaryPayment(Guid? id, AuthenticationModel authentication)
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
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            if (isCash.FromBankAccountID > 0)
                            {
                                OfficeHelper.AddBankAction(isCash.LocationID, isCash.ToEmployeeID, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            }
                            else
                            {
                                OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);
                            }

                            //maaş hesap işlemi
                            OfficeHelper.AddEmployeeAction(isCash.ToEmployeeID, isCash.LocationID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.SalaryType, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value, isCash.DocumentNumber);

                            result.IsSuccess = true;
                            result.Message = "Maaş Avans ödemesi başarı ile iptal edildi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", isCash.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki maaş avans ödemesi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DayResultDocuments> AddResultDocument(long? id, string filename, string path, int? typeid, string description, AuthenticationModel authentication)
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
                            resultDocuments.RecordDate = DateTime.Now;
                            resultDocuments.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            resultDocuments.RecordIP = OfficeHelper.GetIPAddress();
                            resultDocuments.ResultID = dayresult.ID;
                            resultDocuments.FileName = filename;
                            resultDocuments.FilePath = path;

                            Db.DayResultDocuments.Add(resultDocuments);
                            Db.SaveChanges();


                            result.IsSuccess = true;
                            result.Message = $"{resultDocuments.ID} ID li {resultDocuments.Date} tarihli {resultDocuments.FileName} isimli dosya başarı ile eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", resultDocuments.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{id} {filename} dosyası eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> AddCashRecorder(long? id, string filename, string path, int? typeid, string description, string slipnumber, string slipdate, string sliptime, string slipamount, string cashamount, string cardamount, string sliptotalmount, AuthenticationModel authentication)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
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
                            double? scardamount = Convert.ToDouble(cardamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                            double? scashamount = Convert.ToDouble(cashamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                            double? netamount = scardamount + scashamount; //Convert.ToDouble(slipamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                            double? totalamount = Convert.ToDouble(sliptotalmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                            DateTime? date = Convert.ToDateTime(slipdate);
                            TimeSpan? time = Convert.ToDateTime(sliptime).TimeOfDay;
                            DateTime? slipdatetime = date.Value.Add(time.Value);

                            DocumentCashRecorderSlip resultCashSlip = new DocumentCashRecorderSlip();

                            resultCashSlip.ActionTypeID = typeid;
                            resultCashSlip.ActionTypeName = "Yazar Kasa Z Raporu";
                            resultCashSlip.Currency = authentication.ActionEmployee.OurCompany.Currency;
                            resultCashSlip.DocumentNumber = OfficeHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompany.CompanyID, "CR");
                            resultCashSlip.EnvironmentID = 2;
                            resultCashSlip.IsActive = true;
                            resultCashSlip.LocationID = dayresult.LocationID;
                            resultCashSlip.NetAmount = netamount;
                            resultCashSlip.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
                            resultCashSlip.RecordDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid));
                            resultCashSlip.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                            resultCashSlip.RecordIP = OfficeHelper.GetIPAddress();
                            resultCashSlip.ResultID = id;
                            resultCashSlip.SlipDate = slipdatetime;
                            resultCashSlip.SlipNumber = slipnumber;
                            resultCashSlip.TotalAmount = totalamount;
                            resultCashSlip.UID = Guid.NewGuid();
                            resultCashSlip.SlipFile = filename;
                            resultCashSlip.SlipPath = path;
                            resultCashSlip.CashAmount = scashamount;
                            resultCashSlip.CreditAmount = scardamount;
                            resultCashSlip.Date = slipdatetime?.Date;

                            Db.DocumentCashRecorderSlip.Add(resultCashSlip);
                            Db.SaveChanges();

                            result.IsSuccess = true;
                            result.Message = $"{resultCashSlip.ID} ID li {resultCashSlip.SlipDate} tarihli {resultCashSlip.SlipFile} isimli dosya ile beraber kayıt başarı ile eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Result Cash Recorder", "Insert", resultCashSlip.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{id} {filename} dosyası eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }




        public Result<DocumentPosCollections> AddPosCollection(PosCollection collection, AuthenticationModel authentication)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (collection != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentPosCollections posCollection = new DocumentPosCollections();

                        posCollection.ActionTypeID = collection.ActinTypeID;
                        posCollection.ActionTypeName = collection.ActionTypeName;
                        posCollection.Amount = collection.Amount;
                        posCollection.BankAccountID = collection.BankAccountID;
                        posCollection.Currency = collection.Currency;
                        posCollection.Date = collection.DocumentDate;
                        posCollection.Description = collection.Description;
                        posCollection.DocumentNumber = OfficeHelper.GetDocumentNumber(collection.OurCompanyID, "PC");
                        posCollection.ExchangeRate = collection.ExchangeRate;
                        posCollection.FromCustomerID = collection.FromCustomerID;
                        posCollection.IsActive = true;
                        posCollection.LocationID = collection.LocationID;
                        posCollection.OurCompanyID = collection.OurCompanyID;
                        posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
                        posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        posCollection.RecordIP = OfficeHelper.GetIPAddress();
                        posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
                        posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        posCollection.ReferenceID = collection.ReferanceID;
                        posCollection.TerminalID = collection.TerminalID;
                        posCollection.EnvironmentID = 2;
                        posCollection.UID = Guid.NewGuid();
                        posCollection.Quantity = collection.Quantity;
                        posCollection.ResultID = collection.ResultID;


                        Db.DocumentPosCollections.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.BankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, posCollection.Amount, 0, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

                        result.IsSuccess = true;
                        result.Message = "Pos Tahsilatı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentPosCollections> EditPosCollection(PosCollection collection, AuthenticationModel authentication)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPos = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == collection.UID);
                if (isPos != null)
                {
                    try
                    {
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                        var locId = collection.LocationID;
                        var exchange = OfficeHelper.GetExchange(collection.DocumentDate.Value);

                        DocumentPosCollections self = new DocumentPosCollections()
                        {
                            ActionTypeID = isPos.ActionTypeID,
                            ActionTypeName = isPos.ActionTypeName,
                            Amount = isPos.Amount,
                            Currency = isPos.Currency,
                            Date = isPos.Date,
                            Description = isPos.Description,
                            DocumentNumber = isPos.DocumentNumber,
                            ExchangeRate = isPos.ExchangeRate,
                            ID = isPos.ID,
                            FromCustomerID = isPos.FromCustomerID,
                            IsActive = isPos.IsActive,
                            LocationID = isPos.LocationID,
                            OurCompanyID = isPos.OurCompanyID,
                            RecordDate = isPos.RecordDate,
                            RecordEmployeeID = isPos.RecordEmployeeID,
                            RecordIP = isPos.RecordIP,
                            ReferenceID = isPos.ReferenceID,
                            SystemAmount = isPos.SystemAmount,
                            SystemCurrency = isPos.SystemCurrency,
                            UpdateDate = isPos.UpdateDate,
                            UpdateEmployee = isPos.UpdateEmployee,
                            UpdateIP = isPos.UpdateIP,
                            BankAccountID = isPos.BankAccountID,
                            TerminalID = isPos.TerminalID,
                            EnvironmentID = isPos.EnvironmentID,
                            Quantity = isPos.Quantity
                        };
                        isPos.ReferenceID = collection.ReferanceID;
                        isPos.LocationID = collection.LocationID;
                        isPos.Date = collection.DocumentDate;
                        isPos.BankAccountID = collection.BankAccountID;
                        isPos.FromCustomerID = collection.FromCustomerID ?? (int?)null;
                        isPos.Amount = collection.Amount;
                        isPos.Currency = collection.Currency;
                        isPos.Description = collection.Description;
                        isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
                        isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isPos.UpdateIP = OfficeHelper.GetIPAddress();
                        isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
                        isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        isPos.Quantity = collection.Quantity;
                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isPos.LocationID;
                            cashaction.Collection = isPos.Amount;
                            cashaction.Currency = isPos.Currency;
                            cashaction.BankAccountID = isPos.BankAccountID;
                            cashaction.ActionDate = isPos.Date;
                            cashaction.ProcessDate = isPos.Date;
                            cashaction.UpdateDate = isPos.UpdateDate;
                            cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos tahsilatı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isPos, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos tahsilatı güncellenemdi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentPosCollections> DeletePosCollection(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.BankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = "Pos tahsilatı başarı ile iptal edildi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos tahsilatı iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }


            return result;
        }




        public Result<DocumentPosCancel> AddPosCancel(PosCancel collection, AuthenticationModel authentication)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (collection != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentPosCancel posCollection = new DocumentPosCancel();

                        posCollection.ActionTypeID = collection.ActinTypeID;
                        posCollection.ActionTypeName = collection.ActionTypeName;
                        posCollection.Amount = collection.Amount;
                        posCollection.FromBankAccountID = collection.FromBankAccountID;
                        posCollection.Currency = collection.Currency;
                        posCollection.Date = collection.DocumentDate;
                        posCollection.Description = collection.Description;
                        posCollection.DocumentNumber = OfficeHelper.GetDocumentNumber(collection.OurCompanyID, "PCN");
                        posCollection.ExchangeRate = collection.ExchangeRate;
                        posCollection.ToCustomerID = collection.ToCustomerID;
                        posCollection.IsActive = true;
                        posCollection.LocationID = collection.LocationID;
                        posCollection.OurCompanyID = collection.OurCompanyID;
                        posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
                        posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        posCollection.RecordIP = OfficeHelper.GetIPAddress();
                        posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
                        posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        posCollection.ReferenceID = collection.ReferanceID;
                        posCollection.TerminalID = collection.TerminalID;
                        posCollection.EnvironmentID = 2;
                        posCollection.UID = Guid.NewGuid();
                        posCollection.Quantity = collection.Quantity;



                        Db.DocumentPosCancel.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

                        result.IsSuccess = true;
                        result.Message = "Pos iptali başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos iptali eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentPosCancel> EditPosCancel(PosCancel collection, AuthenticationModel authentication)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPos = Db.DocumentPosCancel.FirstOrDefault(x => x.UID == collection.UID);
                if (isPos != null)
                {
                    try
                    {
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                        var locId = collection.LocationID;
                        var exchange = OfficeHelper.GetExchange(collection.DocumentDate.Value);

                        DocumentPosCancel self = new DocumentPosCancel()
                        {
                            ActionTypeID = isPos.ActionTypeID,
                            ActionTypeName = isPos.ActionTypeName,
                            Amount = isPos.Amount,
                            Currency = isPos.Currency,
                            Date = isPos.Date,
                            Description = isPos.Description,
                            DocumentNumber = isPos.DocumentNumber,
                            ExchangeRate = isPos.ExchangeRate,
                            ID = isPos.ID,
                            ToCustomerID = isPos.ToCustomerID,
                            IsActive = isPos.IsActive,
                            LocationID = isPos.LocationID,
                            OurCompanyID = isPos.OurCompanyID,
                            RecordDate = isPos.RecordDate,
                            RecordEmployeeID = isPos.RecordEmployeeID,
                            RecordIP = isPos.RecordIP,
                            ReferenceID = isPos.ReferenceID,
                            SystemAmount = isPos.SystemAmount,
                            SystemCurrency = isPos.SystemCurrency,
                            UpdateDate = isPos.UpdateDate,
                            UpdateEmployee = isPos.UpdateEmployee,
                            UpdateIP = isPos.UpdateIP,
                            FromBankAccountID = isPos.FromBankAccountID,
                            TerminalID = isPos.TerminalID,
                            EnvironmentID = isPos.EnvironmentID,
                            Quantity = isPos.Quantity
                        };
                        isPos.Quantity = collection.Quantity;
                        isPos.ReferenceID = collection.ReferanceID;
                        isPos.LocationID = collection.LocationID;
                        isPos.Date = collection.DocumentDate;
                        isPos.FromBankAccountID = collection.FromBankAccountID;
                        isPos.ToCustomerID = collection.ToCustomerID ?? (int?)null;
                        isPos.Amount = collection.Amount;
                        isPos.Currency = collection.Currency;
                        isPos.Description = collection.Description;
                        isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
                        isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isPos.UpdateIP = OfficeHelper.GetIPAddress();
                        isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * isPos.ExchangeRate;
                        isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isPos.LocationID;
                            cashaction.Collection = isPos.Amount;
                            cashaction.Currency = isPos.Currency;
                            cashaction.BankAccountID = isPos.FromBankAccountID;
                            cashaction.ActionDate = isPos.Date;
                            cashaction.ProcessDate = isPos.Date;
                            cashaction.UpdateDate = isPos.UpdateDate;
                            cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iptali başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCancel>(self, isPos, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "PosCancel", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iptali güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentPosCancel> DeletePosCancel(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentPosCancel.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = "Pos iptali başarı ile iptal edildi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iptali iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }


            return result;
        }




        public Result<DocumentPosRefund> AddPosRefund(PosRefund collection, AuthenticationModel authentication)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (collection != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentPosRefund posCollection = new DocumentPosRefund();

                        posCollection.ActionTypeID = collection.ActinTypeID;
                        posCollection.ActionTypeName = collection.ActionTypeName;
                        posCollection.Amount = collection.Amount;
                        posCollection.FromBankAccountID = collection.FromBankAccountID;
                        posCollection.Currency = collection.Currency;
                        posCollection.Date = collection.DocumentDate;
                        posCollection.Description = collection.Description;
                        posCollection.DocumentNumber = OfficeHelper.GetDocumentNumber(collection.OurCompanyID, "PRF");
                        posCollection.ExchangeRate = collection.ExchangeRate;
                        posCollection.ToCustomerID = collection.ToCustomerID;
                        posCollection.IsActive = true;
                        posCollection.LocationID = collection.LocationID;
                        posCollection.OurCompanyID = collection.OurCompanyID;
                        posCollection.RecordDate = DateTime.UtcNow.AddHours(collection.TimeZone.Value);
                        posCollection.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        posCollection.RecordIP = OfficeHelper.GetIPAddress();
                        posCollection.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * collection.ExchangeRate;
                        posCollection.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        posCollection.ReferenceID = collection.ReferanceID;
                        posCollection.TerminalID = collection.TerminalID;
                        posCollection.EnvironmentID = 2;
                        posCollection.UID = Guid.NewGuid();
                        posCollection.Quantity = collection.Quantity;


                        Db.DocumentPosRefund.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate, posCollection.UID.Value);

                        result.IsSuccess = true;
                        result.Message = "Pos iadesi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos iadesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentPosRefund> EditPosRefund(PosRefund collection, AuthenticationModel authentication)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isPos = Db.DocumentPosRefund.FirstOrDefault(x => x.UID == collection.UID);
                if (isPos != null)
                {
                    try
                    {
                        var location = Db.Location.FirstOrDefault(x => x.LocationID == collection.LocationID);
                        var locId = collection.LocationID;
                        var exchange = OfficeHelper.GetExchange(collection.DocumentDate.Value);

                        DocumentPosRefund self = new DocumentPosRefund()
                        {
                            ActionTypeID = isPos.ActionTypeID,
                            ActionTypeName = isPos.ActionTypeName,
                            Amount = isPos.Amount,
                            Currency = isPos.Currency,
                            Date = isPos.Date,
                            Description = isPos.Description,
                            DocumentNumber = isPos.DocumentNumber,
                            ExchangeRate = isPos.ExchangeRate,
                            ID = isPos.ID,
                            ToCustomerID = isPos.ToCustomerID,
                            IsActive = isPos.IsActive,
                            LocationID = isPos.LocationID,
                            OurCompanyID = isPos.OurCompanyID,
                            RecordDate = isPos.RecordDate,
                            RecordEmployeeID = isPos.RecordEmployeeID,
                            RecordIP = isPos.RecordIP,
                            ReferenceID = isPos.ReferenceID,
                            SystemAmount = isPos.SystemAmount,
                            SystemCurrency = isPos.SystemCurrency,
                            UpdateDate = isPos.UpdateDate,
                            UpdateEmployee = isPos.UpdateEmployee,
                            UpdateIP = isPos.UpdateIP,
                            FromBankAccountID = isPos.FromBankAccountID,
                            TerminalID = isPos.TerminalID,
                            EnvironmentID = isPos.EnvironmentID,
                            Quantity = isPos.Quantity

                        };
                        isPos.Quantity = collection.Quantity;
                        isPos.ReferenceID = collection.ReferanceID;
                        isPos.LocationID = collection.LocationID;
                        isPos.Date = collection.DocumentDate;
                        isPos.FromBankAccountID = collection.FromBankAccountID;
                        isPos.ToCustomerID = collection.ToCustomerID ?? (int?)null;
                        isPos.Amount = collection.Amount;
                        isPos.Currency = collection.Currency;
                        isPos.Description = collection.Description;
                        isPos.ExchangeRate = collection.ExchangeRate != null ? collection.ExchangeRate : self.ExchangeRate;
                        isPos.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isPos.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isPos.UpdateIP = OfficeHelper.GetIPAddress();
                        isPos.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == collection.Currency ? collection.Amount : collection.Amount * isPos.ExchangeRate;
                        isPos.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.LocationID == locId && x.BankActionTypeID == isPos.ActionTypeID && x.ProcessID == isPos.ID);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isPos.LocationID;
                            cashaction.Collection = isPos.Amount;
                            cashaction.Currency = isPos.Currency;
                            cashaction.BankAccountID = isPos.FromBankAccountID;
                            cashaction.ActionDate = isPos.Date;
                            cashaction.ProcessDate = isPos.Date;
                            cashaction.UpdateDate = isPos.UpdateDate;
                            cashaction.UpdateEmployeeID = isPos.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iadesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosRefund>(self, isPos, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isPos.ID.ToString(), "Bank", "PosRefund", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isPos.ID} ID li {isPos.Date} tarihli {isPos.Amount} {isPos.Currency} tutarındaki pos iadesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(collection.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentPosRefund> DeletePosRefund(Guid? id, AuthenticationModel authentication)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (id != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    var isCash = Db.DocumentPosRefund.FirstOrDefault(x => x.UID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3));
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate, isCash.UID.Value);

                            result.IsSuccess = true;
                            result.Message = "Pos iadesi başarı ile iptal edildi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iptali iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isCash.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }


            return result;
        }




        public Result<DocumentCashRecorderSlip> AddCashRecorder(CashRecorder record, AuthenticationModel authentication)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (record != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        DocumentCashRecorderSlip cashRedord = new DocumentCashRecorderSlip();

                        cashRedord.ActionTypeID = record.ActinTypeID;
                        cashRedord.ActionTypeName = record.ActionTypeName;
                        cashRedord.NetAmount = record.NetAmount;
                        cashRedord.TotalAmount = record.TotalAmount;
                        cashRedord.Currency = record.Currency;
                        cashRedord.Date = record.DocumentDate;
                        cashRedord.DocumentNumber = OfficeHelper.GetDocumentNumber(record.OurCompanyID, "CR");
                        cashRedord.IsActive = true;
                        cashRedord.LocationID = record.LocationID;
                        cashRedord.OurCompanyID = record.OurCompanyID;
                        cashRedord.RecordDate = DateTime.UtcNow.AddHours(record.TimeZone.Value);
                        cashRedord.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        cashRedord.RecordIP = OfficeHelper.GetIPAddress();
                        cashRedord.SlipDate = record.SlipDate;
                        cashRedord.SlipNumber = record.SlipNumber;
                        cashRedord.EnvironmentID = record.EnvironmentID;
                        cashRedord.UID = Guid.NewGuid();
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
                        OfficeHelper.AddApplicationLog("Office", "Result Cash Recorder", "Insert", cashRedord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(record.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashRedord);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Yazarkasa fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(record.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> EditCashRecorder(CashRecorder record, AuthenticationModel authentication)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var location = Db.Location.FirstOrDefault(x => x.LocationID == record.LocationID);
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

                        isRecord.LocationID = isRecord.LocationID;
                        isRecord.NetAmount = record.NetAmount;
                        isRecord.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
                        isRecord.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isRecord.UpdateIP = OfficeHelper.GetIPAddress();
                        isRecord.SlipDate = record.SlipDate;
                        isRecord.SlipNumber = record.SlipNumber;
                        isRecord.TotalAmount = record.TotalAmount;
                        isRecord.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        isRecord.UpdateIP = OfficeHelper.GetIPAddress();
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

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashRecorderSlip>(self, isRecord, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Result Cash Recorder", "Update", isRecord.ID.ToString(), "CashRecorder", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isRecord.ID} {isRecord.SlipFile} dosyası güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Result Document", "Update", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> DeleteCashRecorder(Guid? id, AuthenticationModel authentication)
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
                    if (isRecord != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isRecord.Date));

                            isRecord.IsActive = false;
                            isRecord.UpdateDate = DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isRecord.LocationID ?? 3));
                            isRecord.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isRecord.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            result.IsSuccess = true;
                            result.Message = $"{isRecord.ID} ID li {isRecord.SlipDate} tarihli kayıt başarı ile silindi.";

                            OfficeHelper.AddApplicationLog("Office", "Result Cash Recorder", "Remove", isRecord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isRecord.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isRecord.TotalAmount} {isRecord.Currency} tarihli kayıt başarı ile silinemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Result Document", "Remove", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(isRecord.LocationID ?? 3)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }


            return result;
        }


        public Result<DocumentTransfer> AddTransfer(TransferModel transfer, AuthenticationModel authentication)
        {
            Result<DocumentTransfer> result = new Result<DocumentTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (transfer != null && authentication != null)
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    try
                    {
                        var flocation = Db.Location.FirstOrDefault(x => x.LocationID == transfer.FromLocationID);

                        var actiontype = Db.CashActionType.FirstOrDefault(x => x.ID == 34);

                        var exchange = OfficeHelper.GetExchange(transfer.DocumentDate);

                        var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.FromLocationID && x.Date == transfer.DocumentDate);

                        DocumentTransfer doctransfer = new DocumentTransfer();

                        doctransfer.ActionTypeID = actiontype.ID;
                        doctransfer.ActionTypeName = actiontype.Name;
                        doctransfer.Amount = transfer.Amount;
                        doctransfer.CarrierEmployeeID = transfer.CarrierEmployeeID;
                        doctransfer.FromCashID = transfer.FromCashID;
                        doctransfer.FromBankAccountID = transfer.FromBankID;
                        doctransfer.FromCustomerID = transfer.FromCustID;
                        doctransfer.FromDate = transfer.DocumentDate;
                        doctransfer.FromEmployeeID = transfer.FromEmplID;
                        doctransfer.FromLocationID = transfer.FromLocationID;
                        doctransfer.FromRecordEmployeeID = authentication.ActionEmployee.EmployeeID;

                        doctransfer.ToBankAccountID = transfer.ToBankID;
                        doctransfer.ToCashID = transfer.ToCashID;
                        doctransfer.ToCustomerID = transfer.ToCustID;
                        doctransfer.ToEmployeeID = transfer.ToEmplID;
                        doctransfer.ToLocationID = transfer.ToLocationID;

                        doctransfer.Currency = transfer.Currency;
                        doctransfer.DocumentDate = transfer.DocumentDate;
                        doctransfer.Description = transfer.Description;
                        doctransfer.DocumentNumber = OfficeHelper.GetDocumentNumber(authentication.ActionEmployee.OurCompanyID.Value, "TR");
                        doctransfer.ExchangeRate = transfer.Currency == "USD" ? exchange.USDA : transfer.Currency == "EUR" ? exchange.EURA : 1;
                        doctransfer.ToBankAccountID = transfer.ToBankID;
                        doctransfer.IsActive = true;
                        doctransfer.OurCompanyID = authentication.ActionEmployee.OurCompanyID;
                        doctransfer.RecordDate = DateTime.UtcNow.AddHours(flocation.Timezone.Value);
                        doctransfer.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                        doctransfer.RecordIP = OfficeHelper.GetIPAddress();
                        doctransfer.SystemAmount = doctransfer.Amount * doctransfer.ExchangeRate;
                        doctransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        doctransfer.StatusID = 1;
                        doctransfer.EnvironmentID = 2;
                        doctransfer.UID = transfer.UID;

                        doctransfer.ResultID = dayresult?.ID;

                        Db.DocumentTransfer.Add(doctransfer);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = "Cari Virman kaydı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "DocumentTransfer", "Insert", doctransfer.ID.ToString(), "Action", "AddTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(flocation.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, doctransfer);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Cari Virman kaydı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "DocumentTransfer", "Insert", "-1", "Action", "AddTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(authentication.ActionEmployee.OurCompany.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentTransfer> EditTransfer(TransferModel transfer, AuthenticationModel authentication)
        {
            Result<DocumentTransfer> result = new Result<DocumentTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isTransfer = Db.DocumentTransfer.FirstOrDefault(x => x.UID == transfer.UID);

                if (isTransfer != null)
                {
                    try
                    {
                        //var cash = OfficeHelper.GetCash(transfer.LocationID, transfer.Currency);
                        //var location = Db.Location.FirstOrDefault(x => x.LocationID == transfer.LocationID);
                        //var exchange = OfficeHelper.GetExchange(transfer.DocumentDate.Value);

                        //var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29); // masraf ödeme fişi

                        //DocumentBankTransfer self = new DocumentBankTransfer()
                        //{
                        //    ActionTypeID = isTransfer.ActionTypeID,
                        //    ActionTypeName = isTransfer.ActionTypeName,
                        //    Amount = isTransfer.Amount,
                        //    NetAmount = isTransfer.NetAmount,
                        //    FromCashID = isTransfer.FromCashID,
                        //    ToBankAccountID = isTransfer.ToBankAccountID,
                        //    Currency = isTransfer.Currency,
                        //    Date = isTransfer.Date,
                        //    Description = isTransfer.Description,
                        //    DocumentNumber = isTransfer.DocumentNumber,
                        //    ExchangeRate = isTransfer.ExchangeRate,
                        //    ID = isTransfer.ID,
                        //    IsActive = isTransfer.IsActive,
                        //    LocationID = isTransfer.LocationID,
                        //    OurCompanyID = isTransfer.OurCompanyID,
                        //    RecordDate = isTransfer.RecordDate,
                        //    RecordEmployeeID = isTransfer.RecordEmployeeID,
                        //    RecordIP = isTransfer.RecordIP,
                        //    ReferenceID = isTransfer.ReferenceID,
                        //    SystemAmount = isTransfer.SystemAmount,
                        //    SystemCurrency = isTransfer.SystemCurrency,
                        //    UpdateDate = isTransfer.UpdateDate,
                        //    UpdateEmployee = isTransfer.UpdateEmployee,
                        //    UpdateIP = isTransfer.UpdateIP,
                        //    SlipNumber = isTransfer.SlipNumber,
                        //    SlipDocument = isTransfer.SlipDocument,
                        //    SlipDate = isTransfer.SlipDate,
                        //    StatusID = isTransfer.StatusID,
                        //    TrackingNumber = isTransfer.TrackingNumber,
                        //    Commission = isTransfer.Commission,
                        //    UID = isTransfer.UID,
                        //    EnvironmentID = isTransfer.EnvironmentID,
                        //    ReferenceCode = isTransfer.ReferenceCode,
                        //    ResultID = isTransfer.ResultID,
                        //    SlipPath = isTransfer.SlipPath

                        //};

                        //var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == transfer.LocationID && x.Date == transfer.DocumentDate);


                        //isTransfer.ReferenceID = transfer.ReferanceID;
                        //isTransfer.Commission = transfer.Commission;
                        //isTransfer.Date = transfer.DocumentDate;
                        //isTransfer.FromCashID = cash.ID;
                        //isTransfer.SlipDate = transfer.SlipDate;
                        //isTransfer.SlipNumber = transfer.SlipNumber;
                        //isTransfer.ToBankAccountID = transfer.ToBankID ?? (int?)null;
                        //isTransfer.Amount = transfer.Amount;
                        //isTransfer.Description = transfer.Description;
                        //isTransfer.ExchangeRate = transfer.ExchangeRate != null ? transfer.ExchangeRate : self.ExchangeRate;
                        //isTransfer.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                        //isTransfer.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                        //isTransfer.UpdateIP = OfficeHelper.GetIPAddress();
                        //isTransfer.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == transfer.Currency ? transfer.Amount : transfer.Amount * isTransfer.ExchangeRate;
                        //isTransfer.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                        //isTransfer.StatusID = transfer.StatusID;
                        //isTransfer.TrackingNumber = transfer.TrackingNumber;
                        //isTransfer.ReferenceCode = transfer.ReferanceCode;
                        //isTransfer.LocationID = transfer.LocationID;
                        //isTransfer.Currency = transfer.Currency;
                        //isTransfer.IsActive = transfer.IsActive;
                        //isTransfer.SlipDocument = !string.IsNullOrEmpty(transfer.SlipDocument) ? transfer.SlipDocument : self.SlipDocument;
                        //isTransfer.SlipPath = !string.IsNullOrEmpty(transfer.SlipPath) ? transfer.SlipPath : self.SlipPath;



                        //Db.SaveChanges();

                        //result.IsSuccess = true;
                        //result.Message += "Banka Havale / EFT işlemi Güncellendi";

                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                        //OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);



                        //// 01. mevcut kasa çıkış hareketi, banka giriş hareketi ve kasa masraf hareketi varsa sil 

                        //var iscashexit = Db.CashActions.FirstOrDefault(x => x.LocationID == self.LocationID && x.CashActionTypeID == self.ActionTypeID && x.ProcessID == self.ID && x.ProcessUID == self.UID);
                        //if (iscashexit != null)
                        //{
                        //    Db.CashActions.Remove(iscashexit);
                        //    Db.SaveChanges();
                        //}

                        //var isexpenseexists = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == self.ID && x.Date == self.Date && x.LocationID == self.LocationID);

                        //if (isexpenseexists != null)
                        //{
                        //    var iscashexpenseexit = Db.CashActions.FirstOrDefault(x => x.LocationID == isexpenseexists.LocationID && x.CashActionTypeID == isexpenseexists.ActionTypeID && x.ProcessID == isexpenseexists.ID && x.ProcessUID == isexpenseexists.UID);
                        //    if (iscashexpenseexit != null)
                        //    {
                        //        Db.CashActions.Remove(iscashexpenseexit);
                        //        Db.SaveChanges();
                        //    }

                        //    Db.DocumentCashExpense.Remove(isexpenseexists);
                        //    Db.SaveChanges();
                        //}

                        //var isbankexists = Db.BankActions.FirstOrDefault(x => x.LocationID == isTransfer.LocationID && x.BankActionTypeID == isTransfer.ActionTypeID && x.ProcessID == isTransfer.ID && x.ProcessUID == isTransfer.UID);
                        //if (isbankexists != null)
                        //{
                        //    Db.BankActions.Remove(isbankexists);
                        //    Db.SaveChanges();
                        //}

                        //// 02. yeni kasa çıkış hareketlerini ekle

                        //if (new int?[] { 2, 3, 4, 5 }.Contains(isTransfer.StatusID))
                        //{
                        //    // 01. yeni kasa çıkış hareketi komisyonsuz tutar miktarınca eklenir
                        //    var mainamount = (isTransfer.Amount - isTransfer.Commission);

                        //    OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, mainamount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                        //    result.Message += $" kasa çıkış işlemi yapıldı. ";

                        //    if (transfer.Commission > 0)  // komisyonlu işlem ise
                        //    {

                        //        // 02. yeni kasa masraf evrağı komisyon tutarı miktarınca eklenir 

                        //        CashExpense expense = new CashExpense();

                        //        expense.ActinTypeID = actType.ID;
                        //        expense.ActionTypeName = actType.Name;
                        //        expense.Amount = isTransfer.Commission.Value;
                        //        expense.Currency = isTransfer.Currency;
                        //        expense.Description = isTransfer.Description;
                        //        expense.DocumentDate = isTransfer.Date;
                        //        expense.EnvironmentID = isTransfer.EnvironmentID;
                        //        expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
                        //        expense.CashID = cash.ID;
                        //        expense.LocationID = location.LocationID;
                        //        expense.OurCompanyID = location.OurCompanyID;
                        //        expense.SlipDate = isTransfer.SlipDate;
                        //        expense.SlipNumber = isTransfer.SlipNumber;
                        //        expense.SlipDocument = isTransfer.SlipDocument;
                        //        expense.TimeZone = location.Timezone.Value;
                        //        expense.UID = Guid.NewGuid();
                        //        expense.ExpenseTypeID = 25;
                        //        expense.ReferanceID = isTransfer.ID;
                        //        expense.ResultID = dayresult?.ID;
                        //        expense.ToBankAccountID = isTransfer.ToBankAccountID;
                        //        expense.SlipPath = isTransfer.SlipPath;
                        //        expense.ReferanceModel = transfer.ReferanceModel;

                        //        var expenseresult = AddCashExpense(expense, authentication);
                        //        result.Message += $" {expenseresult.Message}";
                        //    }
                        //}
                        //if (new int?[] { 7 }.Contains(isTransfer.StatusID))
                        //{
                        //    isTransfer.IsActive = false;

                        //    Db.SaveChanges();
                        //}

                        //if (new int?[] { 5 }.Contains(isTransfer.StatusID))
                        //{
                        //    OfficeHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate, isTransfer.UID.Value);
                        //    result.Message += $" banka giriş işlemi yapıldı. ";
                        //}


                    }
                    catch (Exception)
                    {

                        //result.Message = $"Havale / EFT güncellenemedi : {ex.Message}";
                        //OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(transfer.TimeZone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }


        public Result<ActionRowResult> CheckResultBackward(Guid id, AuthenticationModel authentication, bool islocal)
        {
            Result<ActionRowResult> result = new Result<ActionRowResult>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            using (ActionTimeEntities Db = new ActionTimeEntities())
            {
                var isResult = Db.DayResult.FirstOrDefault(x => x.UID == id);

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
                    List<int> cashexpense = new int[] { 4, 29 }.ToList();
                    List<int> cashexchange = new int[] { 25 }.ToList();
                    List<int> bankeft = new int[] { 11, 30 }.ToList();

                    var cashActions = Db.VCashActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();
                    var bankActions = Db.VBankActions.Where(x => x.LocationID == isResult.LocationID && x.ActionDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();
                    var emplActions = Db.VEmployeeCashActions.Where(x => x.LocationID == isResult.LocationID && x.ProcessDate == isResult.Date && x.Currency == authentication.ActionEmployee.OurCompany.Currency).ToList();

                    var cashtotal = cashActions.Where(x => cashsales.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var credittotal = bankActions.Where(x => cardsales.Contains(x.BankActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var maastotal = cashActions.Where(x => maas.Contains(x.CashActionTypeID.Value)).Sum(x => x.Amount).Value;
                    var hakedis = emplActions.Where(x => x.ActionTypeID == 32).Sum(x => x.Amount).Value;
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
                                EmployeeID = authentication.ActionEmployee.EmployeeID,
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
                                EmployeeID = authentication.ActionEmployee.EmployeeID,
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

                            OfficeHelper.AddApplicationLog("Office", "ActionRowResult", "Insert", newactionrowresult.ResultID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newactionrowresult);

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
                                actionrowresult.EmployeeID = authentication.ActionEmployee.EmployeeID;
                                actionrowresult.Cash = cashtotal;
                                actionrowresult.Credit = credittotal;
                                actionrowresult.LaborPayed = hakedis;
                                actionrowresult.LaborPayedPayed = maastotal * -1;
                                actionrowresult.Expense = expensetotal * -1;

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

                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<ActionRowResult>(self, actionrowresult, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "ActionRowResult", "Update", actionrowresult.ResultID.ToString(), "Result", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


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
                                OfficeHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                            }


                        }


                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isResult.Date.ToShortDateString()} tarihli zarfta hata oluştu. {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "DailyResult", "Update", isResult.ID.ToString(), "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }


            }

            return result;
        }

    }
}