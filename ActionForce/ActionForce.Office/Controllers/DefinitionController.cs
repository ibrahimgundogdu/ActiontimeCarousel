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
    }
}