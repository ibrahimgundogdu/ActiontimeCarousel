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
        public ActionResult Index(long? id, string cardinfo)
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
            model.TicketSaleRow = Db.TicketSaleRows.Where(x => x.SaleID == id).ToList();
            model.TicketSalePosPayments = Db.TicketSalePosPayment.Where(x => x.SaleID == id).ToList();

            model.CreditPaymentAmount = model.TicketSalePosPayments.Sum(x => x.PaymentAmount) ?? 0;
            model.MasterCredit = model.TicketSaleRow.Sum(x => x.MasterCredit) ?? 0;
            model.PromoCredit = model.TicketSaleRow.Sum(x => x.PromoCredit) ?? 0;
            model.TotalCredit = model.MasterCredit + model.PromoCredit;

            if (!string.IsNullOrEmpty(cardinfo))
            {
                //00119D9B;CC:50:E3:11:9D:9B;4528C2F3;100

                string[] cardinfolist = cardinfo.Split(';').ToArray();

                if (cardinfolist.Count() == 4)
                {
                    string serino = cardinfolist[0];
                    string macano = cardinfolist[1];
                    string cardno = cardinfolist[2];
                    string credit = cardinfolist[3];
                    double? existscredit = (Convert.ToDouble(credit) / 100);
                    model.CardNumber = cardno;

                    var creditLoad = Db.AddTicketSaleCreditLoad(id, cardno, existscredit, serino, macano, model.Authentication.CurrentEmployee.EmployeeID, PosManager.GetIPAddress()).FirstOrDefault();
                    model.CreditLoad = creditLoad;

                    model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == cardno);
                    model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == serino && x.MACAddress == macano && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);
                }

            }

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

            }

            return View(model);
        }



    }
}