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
        public ActionResult CardReader(int? id)
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

            model.CardReaders = Db.VCardReader.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID).ToList();
            model.NewCardReaders = Db.VCardReader.Where(x => x.LocationID == 0 && x.IsActive == true).ToList();

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
                    wReader.PartName = part.PartName;
                    wReader.PartGroupName = part.PartName;

                    existReader.LocationID = 0;
                    existReader.LocationPartID = 0;

                    Db.SaveChanges();

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

            return RedirectToAction("CardReader", new { id = reader.CardReaderID});
        }
    }
}