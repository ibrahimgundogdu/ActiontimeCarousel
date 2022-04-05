using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SettingsController : BaseController
    {
        // GET: Settings
        public ActionResult Index()
        {
            SettingsControlModel model = new SettingsControlModel();
            model.Authentication = this.AuthenticationData;
            return View(model);
        }

        //CardReader
        public ActionResult CardReader(int? id, string SearchKey)
        {

            SettingsControlModel model = new SettingsControlModel();
            model.Authentication = this.AuthenticationData;
            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.CardTypes = Db.CardType.Where(x => x.IsActive == true).ToList();
            model.CurrentCardReader = Db.VCardReader.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.IsActive == true && x.LocationPartID == 0);
            model.CardReaderTypes = Db.CardReaderType.Where(x => x.IsActive == true).ToList();
            var locationparts = Db.GetLocationPartList(model.Authentication.CurrentLocation.ID).ToList();



            model.LocationParts = locationparts.Select(x => new LocationPart()
            {
                LocationID = x.LocationID.Value,
                LocationTypeID = x.LocationTypeID,
                PartID = x.PartID,
                FinishDate = x.FinishDate,
                PartName = x.PartName,
                StartDate = x.StartDate
            }).ToList();

            if (model.LocationParts == null || model.LocationParts.Count == 0)
            {
                model.LocationParts.Add(new LocationPart()
                {
                    PartID = 0,
                    PartName = "Kasa"
                });
            }

            model.CardReaders = Db.VCardReader.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID).ToList();
            model.NewCardReaders = Db.VCardReader.Where(x => x.LocationID == 0 && x.IsActive == true).ToList();

            if (!string.IsNullOrEmpty(SearchKey))
            {
                model.SearchKey = SearchKey.Trim().ToUpper();

                model.NewCardReaders = model.NewCardReaders.Where(x => x.SerialNumber.Contains(model.SearchKey)).ToList();
            }

            model.CardReader = model.CardReaders.FirstOrDefault();

            if (id != null && id > 0)
            {
                model.CardReader = Db.VCardReader.FirstOrDefault(x => x.ID == id);
            }

            return View(model);
        }

        //CardReaderUpdate
        [HttpPost]
        public ActionResult CardReaderUpdate(FormCardReader reader)
        {
            CashControlModel model = new CashControlModel();
            model.Authentication = this.AuthenticationData;

            if (reader != null)
            {
                var wReader = Db.CardReader.FirstOrDefault(x => x.ID == reader.CardReaderID);
                var existReader = Db.CardReader.FirstOrDefault(x => x.ID != reader.CardReaderID && x.LocationID == reader.LocationID && x.LocationTypeID == reader.LocationTypeID && x.LocationPartID == reader.LocationPartID && x.CardReaderTypeID == reader.CardReaderTypeID && x.IsActive == true);

                if (wReader != null)
                {
                    var part = Db.GetLocationPartList(model.Authentication.CurrentLocation.ID).ToList().Where(x => x.PartID == reader.LocationPartID).FirstOrDefault();

                    wReader.LocationID = reader.LocationID;
                    wReader.LocationTypeID = reader.LocationTypeID;
                    wReader.CardReaderTypeID = reader.CardReaderTypeID;
                    wReader.LocationPartID = reader.LocationPartID;
                    wReader.PartName = part?.PartName;
                    wReader.PartGroupName = part?.PartName;

                    Db.SaveChanges();

                    if (existReader != null)
                    {
                        existReader.LocationID = 0;
                        existReader.LocationPartID = 0;

                        Db.SaveChanges();
                    }

                    if (wReader.CardReaderTypeID == 2)
                    {

                        var newparam = new LocationCardReaderParameter();

                        if (!Db.LocationCardReaderParameter.Any(x => x.LocationID == reader.LocationID && x.LocationTypeID == reader.LocationTypeID))
                        {
                            var locCardReaderParam = Db.LocationCardReaderParameter.FirstOrDefault(x => x.LocationID == null && x.LocationTypeID == reader.LocationTypeID);


                            newparam.LocationID = reader.LocationID;
                            newparam.LocationTypeID = reader.LocationTypeID;
                            newparam.OurCompanyID = 2;
                            newparam.MiliSecond = locCardReaderParam?.MiliSecond ?? 100;
                            newparam.ReadCount = locCardReaderParam?.ReadCount ?? 1;
                            newparam.StartDate = locCardReaderParam?.StartDate ?? new DateTime(2022, 1, 1);
                            newparam.UnitDuration = locCardReaderParam?.UnitDuration ?? 10;
                            newparam.UnitPrice = locCardReaderParam?.UnitPrice ?? 20;


                            Db.LocationCardReaderParameter.Add(newparam);
                            Db.SaveChanges();

                        }
                        else
                        {
                            newparam = Db.LocationCardReaderParameter.FirstOrDefault(x => x.LocationID == reader.LocationID && x.LocationTypeID == reader.LocationTypeID);
                        }


                        if (!Db.CardReaderParameter.Any(x => x.LocationID == reader.LocationID && x.CardReaderID == reader.CardReaderID && x.SerialNumber == wReader.SerialNumber && x.MACAddress == wReader.MACAddress))
                        {

                            var newcrparam = new CardReaderParameter()
                            {
                                LocationID = reader.LocationID,
                                CardReaderID = reader.CardReaderID,
                                MiliSecond = newparam?.MiliSecond ?? 100,
                                ReadCount = newparam?.ReadCount ?? 1,
                                StartDate = newparam?.StartDate ?? new DateTime(2022, 1, 1),
                                UnitDuration = newparam?.UnitDuration ?? 10,
                                UnitPrice = newparam?.UnitPrice ?? 20,
                                SerialNumber = wReader.SerialNumber,
                                MACAddress = wReader.MACAddress,
                                RecordDate = DateTime.UtcNow.AddHours(3),
                                Version = "2.22"
                            };

                            Db.CardReaderParameter.Add(newcrparam);
                            Db.SaveChanges();

                        }

                    }

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Tanımlama Tamamlandı";
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Seçilen Okuyucu Bulunamadı";
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Boş Olamaz";
            }


            TempData["Result"] = model.Result;

            return RedirectToAction("CardReader", new { id = reader.CardReaderID });
        }
    }
}