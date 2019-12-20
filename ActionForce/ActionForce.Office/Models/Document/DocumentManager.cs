using ActionForce.Entity;
using System;
using System.Collections.Generic;
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

                        Db.DocumentCashCollections.Add(newCashColl);
                        Db.SaveChanges();


                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{newCashColl.Date} tarihli { newCashColl.Amount } {newCashColl.Currency} tutarındaki kasa tahsilatı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentCashCollections> DeleteCashCollection(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                        var balance = Db.GetCashBalance(sale.LocationID, sale.CashID).FirstOrDefault() ?? 0;

                        if (balance >= sale.Amount)
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
                            ticketSale.EnvironmentID = 2;
                            ticketSale.UID = Guid.NewGuid();




                            Db.DocumentTicketSales.Add(ticketSale);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, ticketSale.Amount, 0, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate);




                            result.IsSuccess = true;
                            result.Message = "Bilet satış başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, ticketSale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { sale.Amount } { sale.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { sale.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Bilet satış eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                            EnvironmentID = isCash.EnvironmentID
                        };
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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Sale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satışı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentTicketSales> DeleteCashSale(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış başarı ile iptal edildi";
                            
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }



        public Result<DocumentSaleExchange> AddSaleExchange(SaleExchange saleExchange, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        var balance = Db.GetCashBalance(saleExchange.LocationID, saleExchange.FromCashID).FirstOrDefault() ?? 0;
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
                            sale.EnvironmentID = 2;
                            sale.UID = Guid.NewGuid();

                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                sale.SlipDocument = filename;
                                string folder = "Document/Exchange";
                                saleExchange.SlipPath = saleExchange.SlipPath + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(saleExchange.SlipPath, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }



                            Db.DocumentSaleExchange.Add(sale);
                            Db.SaveChanges();


                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(sale.FromCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, -1, 0, sale.Amount, sale.Currency, null, null, sale.RecordEmployeeID, sale.RecordDate);
                            OfficeHelper.AddCashAction(sale.ToCashID, sale.LocationID, null, sale.ActionTypeID, sale.Date, sale.ActionTypeName, sale.ID, sale.Date, sale.DocumentNumber, sale.Description, 1, sale.ToAmount, 0, sale.ToCurrency, null, null, sale.RecordEmployeeID, sale.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{sale.Date} tarihli { sale.Amount } {sale.Currency} kasa döviz satışı başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", sale.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, sale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { saleExchange.Amount } { saleExchange.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { saleExchange.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }

                        

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{saleExchange.Amount} {saleExchange.Currency} kasa döviz satışı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }


                }

            }

            return result;
        }

        public Result<DocumentSaleExchange> EditSaleExchange(SaleExchange saleExchange, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        EnvironmentID = isExchange.EnvironmentID
                    };
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
                    isExchange.UpdateDate = DateTime.UtcNow.AddHours(3);
                    isExchange.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                    isExchange.UpdateIP = OfficeHelper.GetIPAddress();

                    if (file != null && file.ContentLength > 0)
                    {

                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        isExchange.SlipDocument = filename;
                        string folder = "Document/Exchange";
                        saleExchange.SlipPath = saleExchange.SlipPath + folder;
                        try
                        {
                            file.SaveAs(Path.Combine(saleExchange.SlipPath, filename));
                        }
                        catch (Exception ex)
                        {
                        }
                    }

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
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isExchange.ID.ToString(), "Cash", "ExchangeSale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
                catch (Exception ex)
                {
                    result.Message = $"{saleExchange.Amount} {saleExchange.Currency} kasa döviz satışı Güncellenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }


            }

            return result;
        }

        public Result<DocumentSaleExchange> DeleteSaleExchange(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
                            OfficeHelper.AddCashAction(isCash.ToCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.ToAmount, 0, isCash.ToCurrency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa döviz satışı iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                    var docDate = new DateTime(DateTime.Now.Year, 1, 1);
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
                            open.Description = open.Description;
                            open.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "COS");
                            open.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            open.IsActive = true;
                            open.LocationID = open.LocationID;
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
                            OfficeHelper.AddCashAction(open.CashID, open.LocationID, null, open.ActionTypeID, open.Date, open.ActionTypeName, open.ID, open.Date, open.DocumentNumber, open.Description, 1, open.Amount, 0, open.Currency, null, null, open.RecordEmployeeID, open.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{open.Date} tarihli { open.Amount } {open.Currency} tutarındaki kasa açılış fişi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", open.ID.ToString(), "Cash", "Open", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, open);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi eklenemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Open", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

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



                            isOpen.Amount = amount;
                            isOpen.Description = cashOpen.Description;
                            isOpen.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
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
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "CashOpen", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                        var exchange = OfficeHelper.GetExchange(cashOpen.DocumentDate.Value);
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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "CashOpen", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isOpen.Amount} {isOpen.Currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }


            }

            return result;
        }

        public Result<DocumentCashOpen> DeleteCashOpen(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentCashOpen.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa açılış fişi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashOpen", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa açılış fişi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                        var balance = Db.GetCashBalance(payment.LocationID, payment.CashID).FirstOrDefault() ?? 0;

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
                            OfficeHelper.AddCashAction(cashPayment.CashID, cashPayment.LocationID, null, cashPayment.ActionTypeID, cashPayment.Date, cashPayment.ActionTypeName, cashPayment.ID, cashPayment.Date, cashPayment.DocumentNumber, cashPayment.Description, -1, 0, cashPayment.Amount, cashPayment.Currency, null, null, cashPayment.RecordEmployeeID, cashPayment.RecordDate);
                            


                            result.IsSuccess = true;
                            result.Message = "Kasa Ödemesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashPayment.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashPayment);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { payment.Amount } { payment.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { payment.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Kasa Ödemesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isPayment.ID.ToString(), "Cash", "CashPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{payment.Amount} {payment.Currency} tutarındaki kasa ödemesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
            }

            return result;
        }

        public Result<DocumentCashPayments> DeleteCashPayment(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentCashPayments.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {
                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentTicketSaleReturns> AddCashSaleReturn(SaleReturn sale,  AuthenticationModel authentication)
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
                        var balance = Db.GetCashBalance(sale.LocationID, sale.CashID).FirstOrDefault() ?? 0;

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
                            OfficeHelper.AddCashAction(ticketSale.CashID, ticketSale.LocationID, null, ticketSale.ActionTypeID, ticketSale.Date, ticketSale.ActionTypeName, ticketSale.ID, ticketSale.Date, ticketSale.DocumentNumber, ticketSale.Description, -1, 0, ticketSale.Amount, ticketSale.Currency, null, null, ticketSale.RecordEmployeeID, ticketSale.RecordDate);

                            


                            result.IsSuccess = true;
                            result.Message = "Bilet satış iadesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", ticketSale.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, ticketSale);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { sale.Amount } { sale.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { sale.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Bilet satış iadesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "SaleReturn", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }

            return result;
        }

        public Result<DocumentTicketSaleReturns> DeleteCashSaleReturn(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iade iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentCashExpense> AddCashExpense(CashExpense expense, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        var balance = Db.GetCashBalance(expense.LocationID, expense.CashID).FirstOrDefault() ?? 0;

                        if (balance >= expense.Amount)
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

                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                cashExpense.SlipDocument = filename;
                                string folder = "Document/Expense";
                                expense.SlipPath = expense.SlipPath + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(expense.SlipPath, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }



                            Db.DocumentCashExpense.Add(cashExpense);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate);
                            


                            result.IsSuccess = true;
                            result.Message = "Masraf ödeme fişi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashExpense.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashExpense);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { expense.Amount } { expense.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { expense.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashExpense> EditCashExpense(CashExpense expense, HttpPostedFileBase file, AuthenticationModel authentication)
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
                            EnvironmentID = isExpense.EnvironmentID
                        };

                        string FileName = string.Empty;

                        if (file != null && file.ContentLength > 0)
                        {

                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            isExpense.SlipDocument = filename;
                            string folder = "Document/Expense";
                            expense.SlipPath = expense.SlipPath + folder;
                            try
                            {
                                file.SaveAs(Path.Combine(expense.SlipPath, filename));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isExpense.ID.ToString(), "Cash", "Expense", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Masraf ödeme fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                

            }

            return result;
        }

        public Result<DocumentCashExpense> DeleteCashExpense(long? id, AuthenticationModel authentication)
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
                    var isCash = Db.DocumentCashExpense.FirstOrDefault(x => x.ID == id);
                    if (isCash != null)
                    {
                        try
                        {
                            var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                            isCash.IsActive = false;
                            isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isCash.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isCash.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();

                            OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                            result.IsSuccess = true;
                            result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi başarı ile iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        catch (Exception ex)
                        {

                            result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi iptal edilemedi : {ex.Message}";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                }
            }

            return result;
        }




        public Result<DocumentBankTransfer> AddBankTransfer(BankTransfer transfer, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        bankTransfer.StatusID = 1;
                        bankTransfer.EnvironmentID = 2;
                        bankTransfer.ReferenceID = transfer.ReferanceID;
                        bankTransfer.UID = transfer.UID;
                        


                        if (file != null && file.ContentLength > 0)
                        {

                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            bankTransfer.SlipDocument = filename;
                            string folder = "Document/Bank";
                            bankTransfer.SlipPath = folder;
                            transfer.SlipPath = transfer.SlipPath + folder;
                            try
                            {
                                file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        bankTransfer.ReferenceCode = OfficeHelper.BankReferenceCode(bankTransfer.OurCompanyID.Value);

                        Db.DocumentBankTransfer.Add(bankTransfer);
                        Db.SaveChanges();


                        result.IsSuccess = true;
                        result.Message = "Havale / EFT bildirimi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", bankTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, bankTransfer);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentBankTransfer> EditBankTransfer(BankTransfer transfer, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        var locId = isTransfer.LocationID;
                        var docDate = isTransfer.Date;
                        var exchange = OfficeHelper.GetExchange(transfer.DocumentDate.Value);

                        var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29);

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
                            EnvironmentID = isTransfer.EnvironmentID

                        };

                        string FileName = string.Empty;
                        if (transfer.StatusID == 2)
                        {


                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                isTransfer.SlipDocument = filename;
                                string folder = "Document/Bank";
                                transfer.SlipPath = transfer.SlipPath + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }
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

                            Db.SaveChanges();





                            var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isTransfer.ActionTypeID && x.ProcessID == isTransfer.ID);
                            if (cashaction == null)
                            {
                                OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, isTransfer.NetAmount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);
                            }

                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi başarı ile lokasyon takip etti";

                            var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isTransfer.ID && x.Date == isTransfer.Date && x.LocationID == isTransfer.LocationID);
                            if (isExpense == null)
                            {
                                DocumentCashExpense cashExpense = new DocumentCashExpense();

                                cashExpense.ActionTypeID = actType.ID;
                                cashExpense.ActionTypeName = actType.Name;
                                cashExpense.Amount = isTransfer.Amount;
                                cashExpense.CashID = isTransfer.FromCashID;
                                cashExpense.Currency = isTransfer.Currency;
                                cashExpense.Date = isTransfer.Date;
                                cashExpense.Description = isTransfer.Description;
                                cashExpense.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXP");
                                cashExpense.ExchangeRate = isTransfer.ExchangeRate;
                                cashExpense.ToBankAccountID = isTransfer.ToBankAccountID ?? (int?)null;
                                cashExpense.ToEmployeeID = (int?)null;
                                cashExpense.ToCustomerID = (int?)null;
                                cashExpense.IsActive = true;
                                cashExpense.LocationID = isTransfer.LocationID;
                                cashExpense.OurCompanyID = isTransfer.OurCompanyID;
                                cashExpense.RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                                cashExpense.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                                cashExpense.RecordIP = OfficeHelper.GetIPAddress();
                                cashExpense.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == isTransfer.Currency ? isTransfer.Amount : isTransfer.Amount * cashExpense.ExchangeRate;
                                cashExpense.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                                cashExpense.SlipNumber = isTransfer.SlipNumber;
                                cashExpense.SlipDate = isTransfer.SlipDate;
                                cashExpense.ReferenceID = isTransfer.ID;
                                cashExpense.EnvironmentID = 2;
                                cashExpense.UID = Guid.NewGuid();

                                if (file != null && file.ContentLength > 0)
                                {

                                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                    cashExpense.SlipDocument = filename;
                                    string folder = "Document/Expense";
                                    transfer.SlipPath = transfer.SlipPath + folder;
                                    try
                                    {
                                        file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }



                                Db.DocumentCashExpense.Add(cashExpense);
                                Db.SaveChanges();

                                // cari hesap işlemesi
                                OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate);

                                var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == cashExpense.LocationID && x.CashActionTypeID == cashExpense.ID && x.ProcessID == cashExpense.ID);
                                if (expaction == null)
                                {

                                    OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate);
                                }
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashExpense.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else if (transfer.StatusID == 3)
                        {


                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                isTransfer.SlipDocument = filename;
                                string folder = "Document/Bank";
                                transfer.SlipPath = transfer.SlipPath + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }
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

                            Db.SaveChanges();
                            

                            var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == locId && x.CashActionTypeID == isTransfer.ActionTypeID && x.ProcessID == isTransfer.ID);
                            if (cashaction == null)
                            {
                                OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, isTransfer.NetAmount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);
                            }

                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi başarı ile lokasyon onayladı";

                            var isExpense = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isTransfer.ID && x.Date == isTransfer.Date && x.LocationID == isTransfer.LocationID);
                            if (isExpense == null)
                            {
                                DocumentCashExpense cashExpense = new DocumentCashExpense();

                                cashExpense.ActionTypeID = actType.ID;
                                cashExpense.ActionTypeName = actType.Name;
                                cashExpense.Amount = isTransfer.Amount;
                                cashExpense.CashID = isTransfer.FromCashID;
                                cashExpense.Currency = isTransfer.Currency;
                                cashExpense.Date = isTransfer.Date;
                                cashExpense.Description = isTransfer.Description;
                                cashExpense.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXP");
                                cashExpense.ExchangeRate = isTransfer.ExchangeRate;
                                cashExpense.ToBankAccountID = isTransfer.ToBankAccountID ?? (int?)null;
                                cashExpense.ToEmployeeID = (int?)null;
                                cashExpense.ToCustomerID = (int?)null;
                                cashExpense.IsActive = true;
                                cashExpense.LocationID = isTransfer.LocationID;
                                cashExpense.OurCompanyID = isTransfer.OurCompanyID;
                                cashExpense.RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                                cashExpense.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
                                cashExpense.RecordIP = OfficeHelper.GetIPAddress();
                                cashExpense.SystemAmount = authentication.ActionEmployee.OurCompany.Currency == isTransfer.Currency ? isTransfer.Amount : isTransfer.Amount * cashExpense.ExchangeRate;
                                cashExpense.SystemCurrency = authentication.ActionEmployee.OurCompany.Currency;
                                cashExpense.SlipNumber = isTransfer.SlipNumber;
                                cashExpense.SlipDate = isTransfer.SlipDate;
                                cashExpense.ReferenceID = isTransfer.ID;
                                cashExpense.EnvironmentID = 2;
                                cashExpense.UID = Guid.NewGuid();

                                if (file != null && file.ContentLength > 0)
                                {

                                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                    cashExpense.SlipDocument = filename;
                                    string folder = "Document/Expense";
                                    transfer.SlipPath = transfer.SlipPath + folder;
                                    try
                                    {
                                        file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }



                                Db.DocumentCashExpense.Add(cashExpense);
                                Db.SaveChanges();

                                // cari hesap işlemesi
                                OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate);

                                var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == cashExpense.LocationID && x.CashActionTypeID == cashExpense.ID && x.ProcessID == cashExpense.ID);
                                if (expaction == null)
                                {

                                    OfficeHelper.AddCashAction(cashExpense.CashID, cashExpense.LocationID, null, cashExpense.ActionTypeID, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.ID, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, null, null, cashExpense.RecordEmployeeID, cashExpense.RecordDate);
                                }
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", cashExpense.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else if (transfer.StatusID == 4)
                        {
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

                            Db.SaveChanges();


                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi başarı ile muhasebe takip etti";


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else if (transfer.StatusID == 5)
                        {
                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                isTransfer.SlipDocument = filename;
                                string folder = "Document/Bank";
                                transfer.SlipPath = transfer.SlipPath + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(transfer.SlipPath, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }
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

                            Db.SaveChanges();

                            OfficeHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);


                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi başarı ile muhasebe onayladı";


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isTransfer, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isTransfer.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else if (transfer.StatusID == 6)
                        {
                            isTransfer.IsActive = false;
                            isTransfer.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isTransfer.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isTransfer.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();
                            

                            OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, -1 * isTransfer.NetAmount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);
                            OfficeHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, -1 * isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi muhasebe red etti";

                            var isExp = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isTransfer.ID && x.Date == docDate && x.LocationID == isTransfer.LocationID);
                            if (isExp != null)
                            {
                                isExp.IsActive = false;
                                isExp.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isExp.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                                isExp.UpdateIP = OfficeHelper.GetIPAddress();

                                Db.SaveChanges();

                                var expaction = Db.CashActions.FirstOrDefault(x => x.LocationID == isExp.LocationID && x.CashActionTypeID == isExp.ID && x.ProcessID == isExp.ID);
                                if (expaction != null)
                                {

                                    OfficeHelper.AddCashAction(isExp.CashID, isExp.LocationID, null, isExp.ActionTypeID, isExp.Date, isExp.ActionTypeName, isExp.ID, isExp.Date, isExp.DocumentNumber, isExp.Description, -1, 0, -1 * isExp.Amount, isExp.Currency, null, null, isExp.RecordEmployeeID, isExp.RecordDate);
                                }

                                OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isExp.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else if (transfer.StatusID == 7)
                        {
                            isTransfer.IsActive = false;
                            isTransfer.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isTransfer.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                            isTransfer.UpdateIP = OfficeHelper.GetIPAddress();

                            Db.SaveChanges();


                            OfficeHelper.AddCashAction(isTransfer.FromCashID, isTransfer.LocationID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, -1, 0, -1 * isTransfer.NetAmount, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);
                            OfficeHelper.AddBankAction(isTransfer.LocationID, null, isTransfer.ToBankAccountID, null, isTransfer.ActionTypeID, isTransfer.Date, isTransfer.ActionTypeName, isTransfer.ID, isTransfer.Date, isTransfer.DocumentNumber, isTransfer.Description, 1, -1 * isTransfer.NetAmount, 0, isTransfer.Currency, null, null, isTransfer.RecordEmployeeID, isTransfer.RecordDate);

                            result.IsSuccess = true;
                            result.Message = $"{isTransfer.ID} ID li {isTransfer.Amount} {isTransfer.Currency} tutarındaki havale eft işlemi iptal edildi";

                            OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isTransfer.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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


                        Db.DocumentSalaryEarn.Add(salaryEarn);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, salaryEarn.ActionTypeID, salaryEarn.ActionTypeName, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmount, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate);

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
                        var balance = Db.GetCashBalance(payment.LocationID, payment.FromCashID).FirstOrDefault() ?? 0;

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

                            Db.DocumentSalaryPayment.Add(salaryPayment);
                            Db.SaveChanges();

                            // cari hesap işlemesi

                            if (salaryPayment.FromBankAccountID > 0)
                            {
                                OfficeHelper.AddBankAction(salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.FromBankAccountID, null, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate);
                            }
                            else
                            {
                                OfficeHelper.AddCashAction(salaryPayment.FromCashID, salaryPayment.LocationID, salaryPayment.ToEmployeeID, salaryPayment.ActionTypeID, salaryPayment.Date, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.DocumentNumber, salaryPayment.Description, -1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate);
                            }

                            //maaş hesap işlemi
                            OfficeHelper.AddEmployeeAction(salaryPayment.ToEmployeeID, salaryPayment.LocationID, salaryPayment.ActionTypeID, salaryPayment.ActionTypeName, salaryPayment.ID, salaryPayment.Date, salaryPayment.Description, 1, 0, salaryPayment.Amount, salaryPayment.Currency, null, null, salaryPayment.SalaryType, salaryPayment.RecordEmployeeID, salaryPayment.RecordDate);

                            result.IsSuccess = true;
                            result.Message = "Maaş Avans ödemesi başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", salaryPayment.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, salaryPayment);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { payment.Amount } { payment.Currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { payment.Currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }


        public Result<DayResultDocuments> AddResultDocument(long? id, HttpPostedFileBase file, string path, int? typeid, string description, AuthenticationModel authentication)
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




                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                resultDocuments.FileName = filename;
                                string folder = "Document/CashRecorder";
                                resultDocuments.FilePath = folder;
                                path = path + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(path, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            Db.DayResultDocuments.Add(resultDocuments);
                            Db.SaveChanges();


                            result.IsSuccess = true;
                            result.Message = $"{resultDocuments.ID} ID li {resultDocuments.Date} tarihli {resultDocuments.FileName} isimli dosya başarı ile eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", resultDocuments.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{id} {file} dosyası eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Result Document", "Insert", "-1", "Result", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

        public Result<DocumentCashRecorderSlip> AddCashRecorder(long? id, HttpPostedFileBase file, string path, int? typeid, string description, string slipnumber, string slipdate, string sliptime, string slipamount, string sliptotalmount, AuthenticationModel authentication)
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
                            double? netamount = Convert.ToDouble(slipamount.Replace(".", ""));
                            double? totalamount = Convert.ToDouble(sliptotalmount.Replace(".", ""));
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

                            if (file != null && file.ContentLength > 0)
                            {

                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                resultCashSlip.SlipFile = filename;
                                string folder = "Document/CashRecorder";
                                resultCashSlip.SlipPath = folder;
                                path = path + folder;
                                try
                                {
                                    file.SaveAs(Path.Combine(path, filename));
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            Db.DocumentCashRecorderSlip.Add(resultCashSlip);
                            Db.SaveChanges();


                            result.IsSuccess = true;
                            result.Message = $"{resultCashSlip.ID} ID li {resultCashSlip.SlipDate} tarihli {resultCashSlip.SlipFile} isimli dosya ile beraber kayıt başarı ile eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Result Cash Recorder", "Insert", resultCashSlip.ID.ToString(), "Result", "Detail", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(OfficeHelper.GetTimeZone(locationid)), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{id} {file} dosyası eklenemedi : {ex.Message}";
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




                        Db.DocumentPosCollections.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.BankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, posCollection.Amount, 0, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Pos Tahsilatı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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




                        Db.DocumentPosCancel.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Pos iptali başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos iptali eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
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




                        Db.DocumentPosRefund.Add(posCollection);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddBankAction(posCollection.LocationID, null, posCollection.FromBankAccountID, null, posCollection.ActionTypeID, posCollection.Date, posCollection.ActionTypeName, posCollection.ID, posCollection.Date, posCollection.DocumentNumber, posCollection.Description, 1, 0, posCollection.Amount, posCollection.Currency, null, null, posCollection.RecordEmployeeID, posCollection.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Pos iadesi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", posCollection.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, posCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Pos iadesi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }


        public Result<DocumentCashRecorderSlip> AddCashRecorder(CashRecorder record, HttpPostedFileBase file, AuthenticationModel authentication)
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
                        cashRedord.EnvironmentID = 2;
                        cashRedord.UID = Guid.NewGuid();

                        if (file != null && file.ContentLength > 0)
                        {

                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            cashRedord.SlipFile = filename;
                            string folder = "Document/CashRecorder";

                            record.SlipPath = record.SlipPath + folder;
                            try
                            {
                                file.SaveAs(Path.Combine(record.SlipPath, filename));

                                cashRedord.SlipPath = folder;
                            }
                            catch (Exception ex)
                            {
                            }
                        }


                        Db.DocumentCashRecorderSlip.Add(cashRedord);
                        Db.SaveChanges();
                        

                        result.IsSuccess = true;
                        result.Message = "Yazarkasa fişi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Insert", cashRedord.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cashRedord);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Yazarkasa fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Insert", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
            }

            return result;
        }

    }
}