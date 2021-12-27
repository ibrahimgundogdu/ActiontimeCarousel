using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class CardActionController : BaseController
    {
        public ActionResult Index(long? id)
        {
            CardActionControlModel model = new CardActionControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id == null)
            {
                return RedirectToAction("Index", "Card");
            }

            model.TicketSale = Db.TicketSale.FirstOrDefault(x => x.ID == id);

            if (model.TicketSale == null || string.IsNullOrEmpty(model.TicketSale?.CardNumber))
            {
                return RedirectToAction("Index", "Card");
            }

            model.CardReader = Db.CardReader.FirstOrDefault(x => x.ID == model.TicketSale.CardReaderID);

            model.TicketSaleRow = Db.TicketSaleRows.Where(x => x.SaleID == id).ToList();
            model.TicketSalePosPayments = Db.TicketSalePosPayment.Where(x => x.SaleID == id).ToList();

            model.CreditPaymentAmount = model.TicketSalePosPayments.Sum(x => x.PaymentAmount) ?? 0;
            model.MasterCredit = model.TicketSaleRow.Sum(x => x.MasterCredit) ?? 0;
            model.PromoCredit = model.TicketSaleRow.Sum(x => x.PromoCredit) ?? 0;
            model.TotalCredit = model.MasterCredit + model.PromoCredit;
            model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == model.TicketSale.CardNumber);

            model.CardNumber = model.Card.CardNumber;

            var creditLoad = Db.AddTicketSaleCreditLoad(id, model.Card.CardNumber, model.Card.Credit, model.CardReader.SerialNumber, model.CardReader.MACAddress, model.Authentication.CurrentEmployee.EmployeeID, PosManager.GetIPAddress(), 3, null, null).FirstOrDefault();
            model.CreditLoad = creditLoad;
            model.Comment = $"{model.CreditLoad.SerialNumber};{model.CreditLoad.MACAddress};1;{model.CreditLoad.CardNumber};{(int)model.CreditLoad.FinalCredit * 100};";

            if (creditLoad.IsSuccess == true)
            {
                return RedirectToAction("LoadResult", new { id = creditLoad.UID });
            }



            model.CardBalanceAction = 0;

            if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
            {
                model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
            }

            model.CardBalance = model.Card?.Credit ?? 0;

            return View(model);
        }

        public ActionResult CreditRefund(long? id)
        {
            CardActionControlModel model = new CardActionControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id == null)
            {
                return RedirectToAction("Index", "Card");
            }

            model.CardAction = Db.VCardActions.FirstOrDefault(x => x.ID == id);

            if (model.CardAction == null || string.IsNullOrEmpty(model.CardAction?.CardNumber))
            {
                return RedirectToAction("Index", "Card");
            }

            model.CardReader = Db.CardReader.FirstOrDefault(x => x.ID == model.CardAction.CardReaderID);
            model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == model.CardAction.CardNumber);

            model.CardNumber = model.Card.CardNumber;

            var creditLoad = Db.AddTicketSaleCreditLoad(null, model.Card.CardNumber, model.Card.Credit, model.CardReader.SerialNumber, model.CardReader.MACAddress, model.Authentication.CurrentEmployee.EmployeeID, PosManager.GetIPAddress(), 4, model.CardAction.ProcessID, model.CardAction.ID).FirstOrDefault();
            model.CreditLoad = creditLoad;

            if (creditLoad.IsSuccess == true)
            {
                return RedirectToAction("RefundResult", new { id = creditLoad.UID });
            }

            model.CardBalanceAction = 0;
            model.RefundCredit = Convert.ToInt32(model.CreditLoad.RefundCredit);

            if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
            {
                model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
            }

            model.CardBalance = model.Card?.Credit ?? 0;

            return View(model);
        }

        public ActionResult LoadResult(Guid? id)
        {
            CardActionControlModel model = new CardActionControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id == null)
            {
                return RedirectToAction("Index", "Card");
            }

            model.CreditLoad = Db.TicketSaleCreditLoad.FirstOrDefault(x => x.UID == id);

            if (model.CreditLoad != null)
            {



                model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == model.CreditLoad.CardNumber);
                model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == model.CreditLoad.SerialNumber && x.MACAddress == model.CreditLoad.MACAddress && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);



                model.TicketSale = Db.TicketSale.FirstOrDefault(x => x.ID == model.CreditLoad.SaleID);
                model.TicketSaleRow = Db.TicketSaleRows.Where(x => x.SaleID == model.CreditLoad.SaleID).ToList();
                model.TicketSalePosPayments = Db.TicketSalePosPayment.Where(x => x.SaleID == model.CreditLoad.SaleID).ToList();

                model.CreditPaymentAmount = model.TicketSalePosPayments.Sum(x => x.PaymentAmount) ?? 0;
                model.MasterCredit = model.TicketSaleRow.Sum(x => x.MasterCredit) ?? 0;
                model.PromoCredit = model.TicketSaleRow.Sum(x => x.PromoCredit) ?? 0;
                model.TotalCredit = model.MasterCredit + model.PromoCredit;

                model.CardBalanceAction = 0;

                if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
                {
                    model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
                }




            }

            return View(model);
        }

        public ActionResult RefundResult(Guid? id)
        {
            CardActionControlModel model = new CardActionControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id == null)
            {
                return RedirectToAction("Index", "Card");
            }

            model.CreditLoad = Db.TicketSaleCreditLoad.FirstOrDefault(x => x.UID == id);

            if (model.CreditLoad != null)
            {

                model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == model.CreditLoad.CardNumber);
                model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == model.CreditLoad.SerialNumber && x.MACAddress == model.CreditLoad.MACAddress && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

                model.CardAction = Db.VCardActions.FirstOrDefault(x => x.ID == model.CreditLoad.CardActionID);

                model.RefundCredit = Convert.ToInt32(model.CreditLoad.RefundCredit);
                model.CardBalanceAction = 0;

                if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
                {
                    model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
                }

                model.CardBalance = model.Card?.Credit ?? 0;
            }

            return View(model);
        }

        //CompleteCardLoad
        public string CompleteCardLoad(Guid? uid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            string message = string.Empty;

            if (uid != null)
            {
                try
                {
                    CardServiceClient csclient = new CardServiceClient();

                    message = csclient.CardLoad(uid.Value, 1, "Tamamlandı");
                }
                catch (Exception ex)
                {
                    message = "Kart Yüklenemedi!";
                }

            }

            return message;
        }
    }
}