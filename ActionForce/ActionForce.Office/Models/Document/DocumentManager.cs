using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

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
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

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



            return result;
        }

        public Result<DocumentCashCollections> DeleteCashCollection(CashCollection collection, AuthenticationModel authentication)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };



            return result;
        }

    }
}