using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class InventoryController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Pos()
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.Banks = Db.VBankAccount.Where(x => x.OurCompanyID == 2 && x.AccountTypeID == 2).Select(x => new BankDataModel()
            {
                BankAccountID = x.ID,
                BankName = x.Name
            }).ToList();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == 2).Select(x => new LocationDataModel()
            {
                LocationID = x.LocationID,
                LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                SortCode = x.SortBy
            }).OrderBy(z => z.SortCode).ToList();

            model.PosTerminals = Db.PosTerminal.Select(x => new PosTerminalDataModel()
            {
                BrandName = x.BrandName,
                BankName = string.Empty,
                BankAccountID = x.BankAccountID,
                ClientID = x.ClientID,
                ID = x.ID,
                LocationID = null,
                LocationName = string.Empty,
                ModelName = x.ModelName,
                SerialNumber = x.SerialNumber,
                SicilNumber = x.SicilNumber

            }).ToList();

            var locationposterminals = Db.VLocationPosTerminal.ToList();

            foreach (var item in model.PosTerminals)
            {
                var terminal = locationposterminals.Where(x => x.TerminalID == item.ID && x.SicilNumber == item.SicilNumber && x.SerialNumber == item.SerialNumber).OrderByDescending(x => x.RecordDate).FirstOrDefault();
                if (terminal != null && terminal.IsMaster == true && terminal.IsActive == true)
                {
                    item.BankName = terminal.AccountName;
                    item.LocationID = terminal.LocationID;
                    item.LocationName = terminal.LocationFullName;
                }
                else if (terminal != null && terminal.IsMaster == false)
                {
                    item.BankName = terminal.AccountName;
                }
            }

            #region Filter
            if (TempData["PosFilter"] != null)
            {
                model.FilterModel = TempData["PosFilter"] as PosFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.SerialNumber))
                {
                    model.PosTerminals = model.PosTerminals.Where(x => x.SerialNumber.Contains(model.FilterModel.SerialNumber) || x.SicilNumber.Contains(model.FilterModel.SerialNumber)).OrderBy(x => x.ID).ToList();
                }
                if (model.FilterModel.BankAccountID != null)
                {
                    model.PosTerminals = model.PosTerminals.Where(x => x.BankAccountID == model.FilterModel.BankAccountID).OrderBy(x => x.ID).ToList();
                }
                if (model.FilterModel.LocationID != null)
                {
                    model.PosTerminals = model.PosTerminals.Where(x => x.LocationID == model.FilterModel.LocationID).OrderBy(x => x.ID).ToList();
                }
            }
            #endregion

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult PosFilter(PosFilterModel getFilterModel)
        {
            TempData["PosFilter"] = getFilterModel;

            return RedirectToAction("Pos");
        }

        [AllowAnonymous]
        public ActionResult AddPos()
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.Banks = Db.VBankAccount.Where(x => x.OurCompanyID == 2 && x.AccountTypeID == 2).Select(x => new BankDataModel()
            {
                BankAccountID = x.ID,
                BankName = x.Name
            }).ToList();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == 2).Select(x => new LocationDataModel()
            {
                LocationID = x.LocationID,
                LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                SortCode = x.SortBy
            }).OrderBy(z => z.SortCode).ToList();


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosTerminal(PosFormModel form)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            InventoryControlModel model = new InventoryControlModel();

            if (form != null)
            {

                var isSerial = Db.PosTerminal.FirstOrDefault(x => x.SicilNumber.Trim() == form.SicilNumber.Trim() || x.SerialNumber.Trim() == form.SerialNumber.Trim());

                if (isSerial == null)
                {
                    try
                    {
                        PosTerminal newTerminal = new PosTerminal();

                        newTerminal.BankAccountID = form.BankAccountID ?? null;
                        newTerminal.BrandName = form.BrandName;
                        newTerminal.ClientID = form.ClientID;
                        newTerminal.ModelName = form.ModelName;
                        newTerminal.SerialNumber = form.SerialNumber.Trim();
                        newTerminal.SicilNumber = form.SicilNumber.Trim();

                        Db.PosTerminal.Add(newTerminal);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali başarı ile eklendi";

                        if (form.LocationID != null && form.LocationID > 0)
                        {
                            var isexistslocterm = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == form.LocationID && x.IsActive == true && x.IsMaster == true);

                            if (isexistslocterm == null)
                            {
                                LocationPosTerminal locterm = new LocationPosTerminal();
                                locterm.IsActive = true;
                                locterm.IsMaster = true;
                                locterm.LocationID = form.LocationID;
                                locterm.RecordDate = DateTime.UtcNow.AddHours(3);
                                locterm.TerminalID = newTerminal.ID;

                                Db.LocationPosTerminal.Add(locterm);
                                Db.SaveChanges();
                            }
                            else
                            {
                                isexistslocterm.IsMaster = false;
                                Db.SaveChanges();

                                LocationPosTerminal locterm = new LocationPosTerminal();
                                locterm.IsActive = true;
                                locterm.IsMaster = true;
                                locterm.LocationID = form.LocationID;
                                locterm.RecordDate = DateTime.UtcNow.AddHours(3);
                                locterm.TerminalID = newTerminal.ID;

                                Db.LocationPosTerminal.Add(locterm);
                                Db.SaveChanges();
                            }

                        }

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", newTerminal.ID.ToString(), "Inventory", "PosTerminal", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newTerminal);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "-1", "Inventory", "PosTerminal", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali daha önce zaten kaydedilmiş.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Pos", "Inventory");
        }

        //EditPosTerminal
        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosTerminal(PosFormModel form)
        {
            InventoryControlModel model = new InventoryControlModel();

            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };



            if (form != null && form.ID != null)
            {

                var isSerial = Db.PosTerminal.FirstOrDefault(x => x.ID == form.ID && x.SicilNumber.Trim() == form.SicilNumber.Trim() || x.SerialNumber.Trim() == form.SerialNumber.Trim());

                if (isSerial != null)
                {
                    try
                    {


                        PosTerminal self = new PosTerminal()
                        {
                            BankAccountID = isSerial.BankAccountID,
                            BrandName = isSerial.BrandName,
                            ClientID = isSerial.ClientID,
                            ID = isSerial.ID,
                            ModelName = isSerial.ModelName,
                            SerialNumber = isSerial.SerialNumber,
                            SicilNumber = isSerial.SicilNumber
                        };


                        isSerial.ModelName = form.ModelName;
                        isSerial.BankAccountID = form.BankAccountID;
                        isSerial.BrandName = form.BrandName;
                        isSerial.ClientID = form.ClientID;

                        Db.SaveChanges();

                        model.Result.IsSuccess = true;
                        model.Result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali başarı ile güncellendi";


                        if (form.LocationID != null && form.LocationID > 0)
                        {
                            var isexistslocterm = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == form.LocationID && x.TerminalID == isSerial.ID && x.IsActive == true && x.IsMaster == true);

                            if (isexistslocterm == null)
                            {
                                // master kaldır
                                Db.RemovePosTerminalMaster(isSerial.ID, form.LocationID);

                                LocationPosTerminal locterm = new LocationPosTerminal();
                                locterm.IsActive = true;
                                locterm.IsMaster = true;
                                locterm.LocationID = form.LocationID;
                                locterm.RecordDate = DateTime.UtcNow.AddHours(3);
                                locterm.TerminalID = isSerial.ID;

                                Db.LocationPosTerminal.Add(locterm);
                                Db.SaveChanges();
                            }
                            else
                            {
                                var isexistsloctermdif = Db.LocationPosTerminal.FirstOrDefault(x => x.TerminalID == isSerial.ID && x.IsActive == true && x.IsMaster == true);

                                if (isexistsloctermdif != null && isexistsloctermdif.LocationID != form.LocationID)
                                {
                                    Db.RemovePosTerminalMaster(isSerial.ID, form.LocationID);

                                    LocationPosTerminal locterm = new LocationPosTerminal();
                                    locterm.IsActive = true;
                                    locterm.IsMaster = true;
                                    locterm.LocationID = form.LocationID;
                                    locterm.RecordDate = DateTime.UtcNow.AddHours(3);
                                    locterm.TerminalID = isSerial.ID;

                                    Db.LocationPosTerminal.Add(locterm);
                                    Db.SaveChanges();
                                }


                            }

                        }

                        // log atılır
                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<PosTerminal>(self, isSerial, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Update", isSerial.ID.ToString(), "Inventory", "EditPosTerminal", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        model.Result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Update", "-1", "Inventory", "EditPosTerminal", null, false, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali bulunamadı.";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"{form.SicilNumber} sicil nolu { form.SerialNumber } serili Pos Terminali form bilgisi bulunamadı.";
            }

            TempData["result"] = model.Result;

            return RedirectToAction("Pos", "Inventory");
        }


        //Detail
        [AllowAnonymous]
        public ActionResult Detail(string id)
        {
            InventoryControlModel model = new InventoryControlModel();


            if (String.IsNullOrEmpty(id))
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Sicil no boş olamaz";

                TempData["result"] = model.Result;

                return RedirectToAction("Pos", "Inventory");
            }


            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.Banks = Db.VBankAccount.Where(x => x.OurCompanyID == 2 && x.AccountTypeID == 2).Select(x => new BankDataModel()
            {
                BankAccountID = x.ID,
                BankName = x.Name
            }).ToList();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == 2).Select(x => new LocationDataModel()
            {
                LocationID = x.LocationID,
                LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                SortCode = x.SortBy
            }).OrderBy(z => z.SortCode).ToList();



            model.PosTerminal = Db.PosTerminal.Where(x => x.SicilNumber == id).Select(x => new PosTerminalDataModel()
            {
                BrandName = x.BrandName,
                BankName = string.Empty,
                BankAccountID = x.BankAccountID,
                ClientID = x.ClientID,
                ID = x.ID,
                LocationID = null,
                LocationName = string.Empty,
                ModelName = x.ModelName,
                SerialNumber = x.SerialNumber,
                SicilNumber = x.SicilNumber

            }).FirstOrDefault();

            if (model.PosTerminal != null)
            {
                model.VLocationPosTerminals = Db.VLocationPosTerminal.Where(x => x.TerminalID == model.PosTerminal.ID).OrderByDescending(y => y.RecordDate).ToList();

                model.PosTerminal.LocationID = model.VLocationPosTerminals.FirstOrDefault(x => x.IsActive == true && x.IsMaster == true)?.LocationID;
            }
            else
            {
                return RedirectToAction("Pos", "Inventory");
            }
            


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Remove(int? id)
        {
            InventoryControlModel model = new InventoryControlModel();
            model.Result = new Result();

            if (id == null || id <= 0)
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "ID bulunamadı!";

                TempData["result"] = model.Result;

                return RedirectToAction("Pos", "Inventory");
            }

            try
            {
                Db.RemovePosTerminal(id);
                model.Result.IsSuccess = true;
                model.Result.Message = "Terminal Bilgisi Silindi!";
            }
            catch (Exception ex)
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Terminal Bilgisi Silinemedi! : "+ex.Message;
            }


            TempData["result"] = model.Result;

            return RedirectToAction("Pos", "Inventory");
        }
    }
}