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
                model.Result.Message = "Terminal Bilgisi Silinemedi! : " + ex.Message;
            }


            TempData["result"] = model.Result;

            return RedirectToAction("Pos", "Inventory");
        }





        [AllowAnonymous]
        public ActionResult Animal(int? id, int? LocationID)
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.CostumeList = Db.Costume.ToList();
            model.CostumeTypeList = Db.CostumeType.ToList();

            model.Locations = Db.Location.Where(x => x.LocationTypeID == 3 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).Select(x => new LocationDataModel()
            {
                LocationID = x.LocationID,
                LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                SortCode = x.SortBy
            }).OrderBy(z => z.SortCode).ToList();

            model.FilterLocations = model.Locations;

            model.LocationAnimals = Db.VLocationAnimals.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (id != null && id > 0)
            {
                model.LocationAnimals = model.LocationAnimals.Where(x => x.CostumeTypeID == id).ToList();
                model.CostumeTypeID = id;
            }

            if (LocationID != null && LocationID > 0)
            {
                model.LocationAnimals = model.LocationAnimals.Where(x => x.LocationID == LocationID).ToList();
                model.Locations = model.Locations.Where(x => x.LocationID == LocationID).ToList();
                model.LocationID = LocationID;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCostumeType(CostumeTypeFormModel form)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            InventoryControlModel model = new InventoryControlModel();

            if (form != null)
            {

                var isCostume = Db.CostumeType.FirstOrDefault(x => x.TypeNameEN.Trim() == form.TypeNameEN.Trim());

                if (isCostume == null)
                {
                    try
                    {
                        CostumeType newCostumeType = new CostumeType();

                        newCostumeType.TypeNameEN = form.TypeNameEN;
                        newCostumeType.TypeNameTR = form.TypeNameTR;
                        newCostumeType.IsActive = true;
                        newCostumeType.SortBy = "9999";

                        Db.CostumeType.Add(newCostumeType);
                        Db.SaveChanges();

                        newCostumeType.SortBy = newCostumeType.ID.ToString();
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{form.TypeNameEN} - { form.TypeNameTR } kostüm türü başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", newCostumeType.ID.ToString(), "Inventory", "CostumeType", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCostumeType);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{form.TypeNameEN} - { form.TypeNameTR } kostüm türü eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "-1", "Inventory", "PosTerminal", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"{form.TypeNameEN} - { form.TypeNameTR } kostüm türü daha önce zaten kaydedilmiş.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Animal", "Inventory");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterLocationAnimal(AnimalFilterModel filter)
        {
            if (filter != null)
            {
                return RedirectToAction("Animal", "Inventory", new { id = filter.CostumeTypeID, LocationID = filter.LocationID });
            }
            return RedirectToAction("Animal");
        }

        [AllowAnonymous]
        public ActionResult RemoveAnimal(int? id)
        {
            if (id != null)
            {
                var locationAnimal = Db.LocationAnimals.FirstOrDefault(x => x.ID == id);

                if (locationAnimal != null)
                {
                    int LocationID = locationAnimal.LocationID.Value;

                    Db.LocationAnimals.Remove(locationAnimal);
                    Db.SaveChanges();

                    return RedirectToAction("Animal", "Inventory", new { LocationID = LocationID });

                }

            }
            return RedirectToAction("Animal");
        }

        [AllowAnonymous]
        public ActionResult LocationAnimals(int? id)
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            if (id != null && id > 0)
            {


                model.CostumeTypeList = Db.CostumeType.ToList();

                model.Location = Db.Location.Where(x => x.LocationID == id).Select(x => new LocationDataModel()
                {
                    LocationID = x.LocationID,
                    LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                    SortCode = x.SortBy
                }).OrderBy(z => z.SortCode).FirstOrDefault();

                model.LocationID = model.Location?.LocationID ?? id;
                model.LocationAnimals = Db.VLocationAnimals.Where(x => x.LocationID == model.LocationID).ToList();

                model.Locations = Db.Location.Where(x => x.LocationID == model.LocationID).Select(x => new LocationDataModel()
                {
                    LocationID = x.LocationID,
                    LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                    SortCode = x.SortBy
                }).OrderBy(z => z.SortCode).ToList();
            }


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddLocationAnimals(LocationAnimalFormModel form)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            InventoryControlModel model = new InventoryControlModel();

            if (form != null)
            {

                var location = Db.Location.FirstOrDefault(x => x.LocationID == form.LocationID);

                if (location != null)
                {
                    try
                    {
                        foreach (var item in form.CostumeTypeID)
                        {
                            //Chassis
                            Chassis newChasis = new Chassis();
                            newChasis.ChassisNumber = "0000";
                            newChasis.SerialNumber = "0000";
                            newChasis.ChassisTypeID = 2;
                            newChasis.ConstractDate = DateTime.UtcNow.AddHours(3).Date;
                            newChasis.IsActive = true;

                            Db.Chassis.Add(newChasis);
                            Db.SaveChanges();

                            //Costume
                            Costume newCostume = new Costume();
                            newCostume.CostumeTypeID = item;
                            newCostume.ConstractDate = DateTime.UtcNow.AddHours(3).Date;
                            newCostume.IsActive = true;
                            newCostume.SerialNumber = "0000";

                            Db.Costume.Add(newCostume);
                            Db.SaveChanges();


                            //Animal
                            Animal newAnimal = new Animal();

                            newAnimal.ChassisID = newChasis.ID;
                            newAnimal.ConstractDate = DateTime.UtcNow.AddHours(3).Date;
                            newAnimal.CostumeID = newCostume.ID;
                            newAnimal.IsActive = true;
                            newAnimal.Number = string.Empty;

                            Db.Animal.Add(newAnimal);
                            Db.SaveChanges();

                            //LocationAnimal
                            LocationAnimals newLocAnimal = new LocationAnimals();

                            newLocAnimal.AnimalID = newAnimal.ID;
                            newLocAnimal.IsActive = true;
                            newLocAnimal.LocationID = form.LocationID;
                            newLocAnimal.StartDate = DateTime.UtcNow.AddHours(3).Date;

                            Db.LocationAnimals.Add(newLocAnimal);
                            Db.SaveChanges();

                            result.Message += $"{item} - { form.LocationID } kostüm türü lokasyona başarı ile eklendi";

                        }

                        result.IsSuccess = true;

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "", "Inventory", "LocationAnimals", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Seçilenler - { form.LocationID } kostüm türü lokasyona eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "-1", "Inventory", "LocationAnimals", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"Lokasyon bulunamadı.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("LocationAnimals", "Inventory", new { id = form.LocationID});
        }



        [AllowAnonymous]
        public ActionResult Mallmoto(int? id, int? LocationID)
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.MallMotoColors = Db.MallMotoColor.ToList();
            model.MallMotos = Db.MallMoto.ToList();

            model.Locations = Db.Location.Where(x => x.LocationTypeID == 11 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).Select(x => new LocationDataModel()
            {
                LocationID = x.LocationID,
                LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                SortCode = x.SortBy
            }).OrderBy(z => z.SortCode).ToList();

            model.FilterLocations = model.Locations;

            model.LocationMallMotos = Db.VLocationMallMoto.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (id != null && id > 0)
            {
                model.LocationMallMotos = model.LocationMallMotos.Where(x => x.MallMotoColorID == id).ToList();
                model.MallMotoColorID = id;
            }

            if (LocationID != null && LocationID > 0)
            {
                model.LocationMallMotos = model.LocationMallMotos.Where(x => x.LocationID == LocationID).ToList();
                model.Locations = model.Locations.Where(x => x.LocationID == LocationID).ToList();
                model.LocationID = LocationID;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddMallMotoColor(MallMotoColorFormModel form)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            InventoryControlModel model = new InventoryControlModel();

            if (form != null)
            {

                var isColor = Db.MallMotoColor.FirstOrDefault(x => x.ColorNameEN.Trim() == form.ColorNameEN.Trim());

                if (isColor == null)
                {
                    try
                    {
                        MallMotoColor newColor = new MallMotoColor();

                        newColor.ColorNameEN = form.ColorNameEN;
                        newColor.ColorNameTR = form.ColorNameTR;
                        newColor.IsActive = true;

                        Db.MallMotoColor.Add(newColor);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{form.ColorNameEN} - { form.ColorNameTR } renk türü başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", newColor.ID.ToString(), "Inventory", "MallMotoColor", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newColor);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{form.ColorNameEN} - { form.ColorNameTR } renk türü eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "-1", "Inventory", "MallMotoColor", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"{form.ColorNameEN} - { form.ColorNameTR } renk türü daha önce zaten kaydedilmiş.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Mallmoto", "Inventory");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterLocationMallMoto(MallMotoFilterModel filter)
        {
            if (filter != null)
            {
                return RedirectToAction("Mallmoto", "Inventory", new { id = filter.MallMotoColorID, LocationID = filter.LocationID });
            }
            return RedirectToAction("Mallmoto");
        }

        [AllowAnonymous]
        public ActionResult LocationMalMotos(int? id)
        {
            InventoryControlModel model = new InventoryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            if (id != null && id > 0)
            {


                model.MallMotoColors = Db.MallMotoColor.ToList();

                model.Location = Db.Location.Where(x => x.LocationID == id).Select(x => new LocationDataModel()
                {
                    LocationID = x.LocationID,
                    LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                    SortCode = x.SortBy
                }).OrderBy(z => z.SortCode).FirstOrDefault();

                model.LocationID = model.Location?.LocationID ?? id;

                model.LocationMallMotos = Db.VLocationMallMoto.Where(x => x.LocationID == model.LocationID).ToList();

                model.Locations = Db.Location.Where(x => x.LocationID == model.LocationID).Select(x => new LocationDataModel()
                {
                    LocationID = x.LocationID,
                    LocationName = x.LocationFullName ?? x.SortBy + " " + x.LocationName,
                    SortCode = x.SortBy
                }).OrderBy(z => z.SortCode).ToList();
            }


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddLocationMallMotos(LocationMallMotoFormModel form)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            InventoryControlModel model = new InventoryControlModel();

            if (form != null)
            {

                var location = Db.Location.FirstOrDefault(x => x.LocationID == form.LocationID);

                if (location != null)
                {
                    try
                    {
                        foreach (var item in form.MallMotoColorID)
                        {

                            //MallMoto
                            MallMoto newMallmoto = new MallMoto();

                            newMallmoto.MallMotoColorID = item;
                            newMallmoto.ConstractDate = DateTime.UtcNow.AddHours(3).Date;
                            newMallmoto.IsActive = true;
                            newMallmoto.MotoNumber = "000";

                            Db.MallMoto.Add(newMallmoto);
                            Db.SaveChanges();

                            newMallmoto.MotoNumber = "00" + newMallmoto.ID.ToString();
                            Db.SaveChanges();


                            //LocationMallmoto
                            LocationMallMoto newLocMallMoto = new LocationMallMoto();

                            newLocMallMoto.MallMotoID = newMallmoto.ID;
                            newLocMallMoto.IsActive = true;
                            newLocMallMoto.LocationID = form.LocationID;
                            newLocMallMoto.StartDate = DateTime.UtcNow.AddHours(3).Date;

                            Db.LocationMallMoto.Add(newLocMallMoto);
                            Db.SaveChanges();

                            result.Message += $"{item} - { form.LocationID } Renk MallMoto lokasyona başarı ile eklendi";

                        }

                        result.IsSuccess = true;

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "", "Inventory", "LocationMallMoto", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Seçilenler - { form.LocationID } id li lokasyona eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Inventory", "Insert", "-1", "Inventory", "LocationMallMoto", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"Lokasyon bulunamadı.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("LocationMalMotos", "Inventory", new { id = form.LocationID });
        }

        //RemoveMalMoto
        [AllowAnonymous]
        public ActionResult RemoveMalMoto(int? id)
        {
            if (id != null)
            {
                var locationmallmoto = Db.LocationMallMoto.FirstOrDefault(x => x.ID == id);

                if (locationmallmoto != null)
                {
                    int LocationID = locationmallmoto.LocationID.Value;

                    Db.LocationMallMoto.Remove(locationmallmoto);
                    Db.SaveChanges();

                    return RedirectToAction("Mallmoto", "Inventory", new { LocationID = LocationID });

                }

            }
            return RedirectToAction("Mallmoto");
        }


        //Trampoline














    }
}