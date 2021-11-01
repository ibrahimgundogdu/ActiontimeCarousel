using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class UfePlusCardController : BaseController
    {
        // GET: Ufecard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Payment(long? id)
        {
            UfePlusCardControlModel model = new UfePlusCardControlModel();
            model.Authentication = this.AuthenticationData;

            if (id == null)
            {
                return RedirectToAction("Index", "Sales");
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.SaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ID == id);

            if (TempData["CardFilter"] != null)
            {
                model.CardFilter = TempData["CardFilter"] as FormCardFilter;

                if (!string.IsNullOrEmpty(model.CardFilter.QRCode))
                {
                   model.CustomerCards = Db.VCustomerCard.Where(x => x.UID.ToString() == model.CardFilter.QRCode).ToList();
                }

                if (!string.IsNullOrEmpty(model.CardFilter.CardNumber))
                {
                    model.CustomerCards = Db.VCustomerCard.Where(x => x.CardNumber.ToString() == model.CardFilter.CardNumber.Trim().Replace(" ","")).ToList();
                }

                if (!string.IsNullOrEmpty(model.CardFilter.PhoneNumber))
                {
                    model.CustomerCards = Db.VCustomerCard.Where(x => x.PhoneNumber.ToString() == model.CardFilter.CardNumber.Trim().Replace(" ", "")).ToList();
                }

            }

            if (model.CustomerCards != null && model.CustomerCards.Count > 0)
            {
                foreach (var item in model.CustomerCards)
                {
                    var cardCreditModels = Db.GetCardBalance(item.CardID, model.Authentication.CurrentLocation.Currency).FirstOrDefault();
                    item.Credit = cardCreditModels?.Balance ?? 0;
                    item.Currency = cardCreditModels?.Currency ?? model.Authentication.CurrentLocation.Currency;
                }
            }

            return View(model);
        }

        //FindCustomer
        [HttpPost]
        public ActionResult FindCustomer(FormCardFilter cardform)
        {
           
            TempData["CardFilter"] = cardform;

            return RedirectToAction("Payment", new { id = cardform.OrderID });
        }

    }
}