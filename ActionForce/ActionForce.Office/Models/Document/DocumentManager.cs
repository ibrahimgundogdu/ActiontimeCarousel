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

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID);

                        if (cashaction != null)
                        {
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
                        var balance = Db.GetCashBalance(payment.LocationID, payment.FromCashID).FirstOrDefault().Value;

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

                        bankTransfer.ActionTypeID = transfer.ActionTypeID;
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


    }
}