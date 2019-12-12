using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class DefinitionController : BaseController
    {

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            

            TempData["filter"] = model;

            return RedirectToAction("Index", "Definition");
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            DefinitionControlModel model = new DefinitionControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult LocationSearch(string key, string active) //
        {
            DefinitionControlModel model = new DefinitionControlModel();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.LocationName).ToList();

            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToUpper().Replace("İ", "I").Replace("Ü", "U").Replace("Ğ", "G").Replace("Ş", "S").Replace("Ç", "C").Replace("Ö", "O");
                model.LocationList = model.LocationList.Where(x => x.LocationNameSearch.Contains(key)).ToList();
            }

            if (!string.IsNullOrEmpty(active))
            {
                if (active == "act")
                {
                    model.LocationList = model.LocationList.Where(x => x.IsActive == true).ToList();
                }
                else if (active == "psv")
                {
                    model.LocationList = model.LocationList.Where(x => x.IsActive == false).ToList();
                }

            }

            return PartialView("_PartialLocationList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SaveLocationCash(NewLocationCash locCash)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (locCash != null)
            {
                var our = Db.Location.FirstOrDefault(x => x.LocationID == locCash.LocationID);
                //var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == our.OurCompanyID);

                var blockedAmount = Convert.ToDouble(locCash.BlockedAmount.Replace(".", ","));

                var isCash = Db.Cash.FirstOrDefault(x => x.LocationID == locCash.LocationID && x.Currency == locCash.Currency);

                if (isCash == null)
                {
                    try
                    {
                        Cash newLocCash = new Cash();

                        newLocCash.LocationID = locCash.LocationID;
                        newLocCash.CashName = locCash.CashName;
                        newLocCash.BlockedAmount = blockedAmount;
                        newLocCash.Currency = locCash.Currency;
                        newLocCash.IsActive = true;
                        newLocCash.IsMaster = true;

                        Db.Cash.Add(newLocCash);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{our.LocationName} Lokasyonu {locCash.CashName} kasası başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", newLocCash.ID.ToString(), "Definition", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newLocCash);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{our.LocationName} Lokasyonu {locCash.CashName} kasası başarı ile eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", "-1", "Definition", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"{ locCash.CashName } Kasa Mevcuttur.";

                }
                model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == locCash.LocationID && x.cIsActive == true).ToList();

            }


            TempData["result"] = result;

            model.Result = result;

            return PartialView("_PartialLocationCashList", model);
        }

        [AllowAnonymous]
        public PartialViewResult DeleteLocationCash(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            DefinitionControlModel model = new DefinitionControlModel();
            int? locID = 0;

            if (id != null)
            {

                var isCash = Db.Cash.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        locID = isCash.LocationID;
                        Cash self = new Cash()
                        {
                            LocationID = isCash.LocationID,
                            CashName = isCash.CashName,
                            BlockedAmount = isCash.BlockedAmount,
                            Currency = isCash.Currency,
                            IsMaster = isCash.IsMaster,
                            IsActive = isCash.IsActive,
                            SortBy = isCash.SortBy
                        };

                        isCash.IsActive = false;

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{isCash.CashName} kasası başarı ile silindi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Remove", id.ToString(), "Definition", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isCash.ID} ID'li {isCash.CashName} kasası silinemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Remove", "-1", "Definition", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }
                model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == locID && x.cIsActive == true).ToList();

            }

            TempData["result"] = result;

            model.Result = result;

            return PartialView("_PartialLocationCashList", model);

        }

        [AllowAnonymous]
        public PartialViewResult LocationCash(int? id)
        {
            DefinitionControlModel model = new DefinitionControlModel();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == id && x.cIsActive == true).ToList();
            model.Cash = model.CashList.FirstOrDefault();
            return PartialView("_PartialAddLocationCash", model);
        }



        [AllowAnonymous]
        public ActionResult Bank()
        {
            DefinitionControlModel model = new DefinitionControlModel();

            if (TempData["result"] != null)
            {
                model.Results = TempData["result"] as Result<BankActions> ?? null;
            }

            model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.Name).ToList();
            model.Bank = model.BankList.FirstOrDefault();
            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult BankSearch(string active) //
        {
            DefinitionControlModel model = new DefinitionControlModel();

            model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.Name).ToList();
            

            if (!string.IsNullOrEmpty(active))
            {
                if (active == "act")
                {
                    model.BankList = model.BankList.Where(x => x.IsActive == true).ToList();
                }
                else if (active == "psv")
                {
                    model.BankList = model.BankList.Where(x => x.IsActive == false).ToList();
                }

            }
            model.Bank = model.BankList.FirstOrDefault();
            return PartialView("_PartialBankList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SaveBank(NewBank _bank)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (_bank != null)
            {

                var isBank = Db.Bank.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.Name == _bank.Name);

                if (isBank == null)
                {
                    try
                    {
                        Bank newBank = new Bank();

                        newBank.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;
                        newBank.Name = _bank.Name;
                        newBank.Code = _bank.Code;
                        newBank.IsActive = true;

                        Db.Bank.Add(newBank);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{_bank.Name} banka başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", newBank.ID.ToString(), "Definition", "Bank", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newBank);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{_bank.Name} banka başarı ile eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", "-1", "Definition", "Bank", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"{ _bank.Name } Banka Mevcuttur.";

                }
                model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();

            }


            TempData["result"] = result;

            model.Results = result;

            return PartialView("_PartialBankList", model);
        }

        [AllowAnonymous]
        public PartialViewResult BankList(int? id)
        {
            DefinitionControlModel model = new DefinitionControlModel();
            model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID == id && x.IsActive == true).ToList();
            model.Bank = model.BankList.FirstOrDefault();
            return PartialView("_PartialAddBank", model);
        }


        [AllowAnonymous]
        public ActionResult BankAccount()
        {
            DefinitionControlModel model = new DefinitionControlModel();

            if (TempData["result"] != null)
            {
                model.Results = TempData["result"] as Result<BankActions> ?? null;
            }

            model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.Name).ToList();
            model.Bank = model.BankList.FirstOrDefault();
            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult BankAccountSearch(string active) //
        {
            DefinitionControlModel model = new DefinitionControlModel();

            model.BankList = Db.Bank.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.Name).ToList();


            if (!string.IsNullOrEmpty(active))
            {
                if (active == "act")
                {
                    model.BankList = model.BankList.Where(x => x.IsActive == true).ToList();
                }
                else if (active == "psv")
                {
                    model.BankList = model.BankList.Where(x => x.IsActive == false).ToList();
                }

            }
            model.Bank = model.BankList.FirstOrDefault();
            return PartialView("_PartialBanksList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SaveBankAccount(NewBankAccount newAccount)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (newAccount != null)
            {
                var bAcc = newAccount.BankID;
                var aType = newAccount.AccountTypeID;
                var currency = newAccount.Currency;
                var accNr = newAccount.AccountNumber;
                var ibn = newAccount.IBAN;

                var isBank = Db.BankAccount.FirstOrDefault(x => x.BankID == bAcc && x.AccountTypeID == aType && x.Currency == currency && x.AccountNumber == accNr);

                if (isBank == null)
                {
                    try
                    {
                        BankAccount newBank = new BankAccount();

                        newBank.BankID = bAcc;
                        newBank.AccountTypeID = aType;
                        newBank.BranchName = newAccount.BranchName;
                        newBank.AccountName = newAccount.AccountName;
                        newBank.BranchCode = newAccount.BranchCode;
                        newBank.RoutingNumber = newAccount.RoutingNumber;
                        newBank.AccountNumber = accNr;
                        newBank.Currency = currency;
                        newBank.IBAN = ibn;
                        newBank.IsMaster = true;
                        newBank.IsActive = true;

                        Db.BankAccount.Add(newBank);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{accNr} hesap numaralı { newAccount.BranchName } şubeli { newAccount.AccountName } banka hesabı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", newBank.ID.ToString(), "Definition", "BankAccount", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newAccount);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{accNr} hesap numaralı { newAccount.BranchName } şubeli { newAccount.AccountName } banka hesabı başarı ile eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", "-1", "Definition", "BankAccount", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    try
                    {
                        BankAccount newBank = new BankAccount()
                        {
                            BankID = isBank.BankID,
                            AccountTypeID = isBank.AccountTypeID,
                            BranchName = isBank.BranchName,
                            AccountName = isBank.AccountName,
                            BranchCode = isBank.BranchCode,
                            RoutingNumber = isBank.RoutingNumber,
                            AccountNumber = isBank.AccountNumber,
                            Currency = isBank.Currency,
                            IBAN = isBank.IBAN,
                            IsMaster = isBank.IsMaster,
                            IsActive = isBank.IsActive
                        };

                        isBank.AccountTypeID = aType;
                        isBank.BranchName = newAccount.BranchName;
                        isBank.AccountName = newAccount.AccountName;
                        isBank.BranchCode = newAccount.BranchCode;
                        isBank.RoutingNumber = newAccount.RoutingNumber;
                        isBank.AccountNumber = accNr;
                        isBank.Currency = currency;
                        isBank.IBAN = ibn;

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{accNr} hesap numaralı { newAccount.BranchName } şubeli { newAccount.AccountName } banka hesabı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<BankAccount>(newBank, isBank, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", isBank.ID.ToString(), "Definition", "BankAccount", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{accNr} hesap numaralı { newAccount.BranchName } şubeli { newAccount.AccountName } banka hesabı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", "-1", "Definition", "BankAccount", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    
                }
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && x.BankID == bAcc).ToList();

            }

            TempData["result"] = result;

            model.Results = result;

            return PartialView("_PartialBankAccountList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult DeleteBankAccount(int? id)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (id != null)
            {

                var isBank = Db.BankAccount.FirstOrDefault(x => x.ID == id);

                if (isBank != null)
                {
                    try
                    {
                        BankAccount newBank = new BankAccount()
                        {
                            BankID = isBank.BankID,
                            AccountTypeID = isBank.AccountTypeID,
                            BranchName = isBank.BranchName,
                            AccountName = isBank.AccountName,
                            BranchCode = isBank.BranchCode,
                            RoutingNumber = isBank.RoutingNumber,
                            AccountNumber = isBank.AccountNumber,
                            Currency = isBank.Currency,
                            IBAN = isBank.IBAN,
                            IsMaster = isBank.IsMaster,
                            IsActive = isBank.IsActive
                        };
                        isBank.IsMaster = false;
                        isBank.IsActive = false;

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{isBank.AccountNumber} hesap numaralı { isBank.BranchName } şubeli { isBank.AccountName } banka hesabı başarı ile silindi";
                        
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Remove", isBank.ID.ToString(), "Definition", "BankAccount", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isBank.AccountNumber} hesap numaralı { isBank.BranchName } şubeli { isBank.AccountName } banka hesabı silinemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Remove", "-1", "Definition", "BankAccount", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && x.BankID == isBank.BankID).ToList();

            }


            TempData["result"] = result;

            model.Results = result;

            return PartialView("_PartialBankAccountList", model);
        }

        [AllowAnonymous]
        public PartialViewResult BankAccountList(int? id)
        {
            DefinitionControlModel model = new DefinitionControlModel();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID == id && x.IsActive == true).ToList();
            model.BankAccount = model.BankAccountList.FirstOrDefault();
            model.AccountType = Db.BankAccountType.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            return PartialView("_PartialAddBankAccount", model);
        }


        [AllowAnonymous]
        public ActionResult CashRecord()
        {
            DefinitionControlModel model = new DefinitionControlModel();

            if (TempData["result"] != null)
            {
                model.Results = TempData["result"] as Result<BankActions> ?? null;
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SaveCashRecord(NewCashRecord newCash)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (newCash != null)
            {
                var loc = newCash.LocationID;
                var serial = newCash.SerialNumber;

                var isCash = Db.CashRecorders.FirstOrDefault(x => x.LocationID == loc && x.SerialNumber == serial);

                if (isCash == null)
                {
                    try
                    {
                        CashRecorders newRecord = new CashRecorders();

                        newRecord.LocationID = loc;
                        newRecord.Name = newCash.Name;
                        newRecord.SerialNumber = serial;

                        Db.CashRecorders.Add(newRecord);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", newRecord.ID.ToString(), "Definition", "CashRecord", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCash);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", "-1", "Definition", "CashRecord", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    try
                    {
                        CashRecorders self = new CashRecorders()
                        {
                            LocationID = isCash.LocationID,
                            Name = isCash.Name,
                            SerialNumber = isCash.SerialNumber
                        };

                        isCash.Name = newCash.Name;
                        isCash.SerialNumber = serial;

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa başarı ile Güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<CashRecorders>(isCash, self, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", isCash.ID.ToString(), "Definition", "CashRecord", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", "-1", "Definition", "CashRecord", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
                model.CashRecordsList = Db.CashRecorders.Where(x => x.LocationID == loc).ToList();

            }

            TempData["result"] = result;

            model.Results = result;

            return PartialView("_PartialCashRecordList", model);
        }

        [AllowAnonymous]
        public PartialViewResult CashRecordList(int? id)
        {
            DefinitionControlModel model = new DefinitionControlModel();
            model.CashRecordList = Db.VCashRecorders.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == id).ToList();
            model.CashRecordsList = Db.CashRecorders.Where(x => x.LocationID == id).ToList();
            model.CashRedord = model.CashRecordList.FirstOrDefault();
            return PartialView("_PartialAddCashRecord", model);
        }
        


        [AllowAnonymous]
        public ActionResult PosTerminal()
        {
            DefinitionControlModel model = new DefinitionControlModel();

            if (TempData["result"] != null)
            {
                model.Results = TempData["result"] as Result<BankActions> ?? null;
            }
            model.AccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult PosTerminalList(int? id)
        {
            DefinitionControlModel model = new DefinitionControlModel();

            model.AccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.LocPosTerminalList = Db.VLocationPosTerminal.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == id).ToList();
            
            model.LocPosTerminal = model.LocPosTerminalList.FirstOrDefault();
            return PartialView("_PartialAddPosTerminal", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SavePosTerminal(NewPosTerminal newPos)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            DefinitionControlModel model = new DefinitionControlModel();

            if (newPos != null)
            {
                var loc = newPos.LocationID;
                var terminal = Convert.ToInt32(newPos.TerminalID);
                var ter = newPos.TerminalID;

                var isLocPos = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == loc && x.TerminalID == terminal);

                if (isLocPos == null)
                {
                    try
                    {
                        var isPos = Db.PosTerminal.FirstOrDefault(x => x.TerminalID == ter);
                        PosTerminal ps = new PosTerminal();
                        if (isPos == null)
                        {
                            
                            ps.TerminalID = newPos.TerminalID;
                            ps.ClientID = newPos.ClientID;
                            ps.BankAccountID = newPos.BankAccountID;
                            ps.BrandName = newPos.BrandName;
                            ps.ModelName = newPos.ModelName;
                            ps.SerialNumber = newPos.SerialNumber;

                            Db.PosTerminal.Add(ps);
                            Db.SaveChanges();

                            //result.IsSuccess = true;
                            //result.Message = $"{newPos.TerminalID} { newPos.SerialNumber } serial numaralı pos cihazı başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", isPos.ID.ToString(), "Definition", "PosTerminal", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isPos);
                        }
                        LocationPosTerminal locPos = new LocationPosTerminal();

                        locPos.LocationID = loc;
                        locPos.TerminalID = terminal;
                        locPos.IsMaster = true;
                        locPos.IsActive = true;

                        Db.LocationPosTerminal.Add(locPos);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{newPos.TerminalID} { newPos.SerialNumber } serial numaralı pos Cihazı başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", isLocPos.ID.ToString(), "Definition", "LocationPosTerminal", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isLocPos);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{newPos.TerminalID} { newPos.SerialNumber } serial numaralı pos cihazı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Definition", "Insert", "-1", "Definition", "LocationPosTerminal", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                //else
                //{
                //    try
                //    {
                //        CashRecorders self = new CashRecorders()
                //        {
                //            LocationID = isCash.LocationID,
                //            Name = isCash.Name,
                //            SerialNumber = isCash.SerialNumber
                //        };

                //        isCash.Name = newCash.Name;
                //        isCash.SerialNumber = serial;

                //        Db.SaveChanges();

                //        result.IsSuccess = true;
                //        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa başarı ile Güncellendi";


                //        var isequal = OfficeHelper.PublicInstancePropertiesEqual<CashRecorders>(isCash, self, OfficeHelper.getIgnorelist());
                //        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", isCash.ID.ToString(), "Definition", "CashRecord", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                //    }
                //    catch (Exception ex)
                //    {

                //        result.Message = $"{newCash.Name} { newCash.SerialNumber } serial numaralı yazarkasa güncellenemedi : {ex.Message}";
                //        OfficeHelper.AddApplicationLog("Office", "Definition", "Update", "-1", "Definition", "CashRecord", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                //    }

                //}
                model.LocPosTerminalList = Db.VLocationPosTerminal.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == loc).ToList();

            }

            TempData["result"] = result;

            model.Results = result;

            return PartialView("_PartialPosTerminalList", model);
        }
    }
}