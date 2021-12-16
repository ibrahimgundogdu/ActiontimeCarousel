using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class CardController : BaseController
    {
        private int _EmployeeID { get; set; }
        private int _LocationID { get; set; }
        private string _Currency { get; set; }

        public CardController() : base()
        {

        }
        // GET: Default
        public ActionResult Index()
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;


            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }


            model.PriceList = Db.GetLocationCurrentProductPrices(_LocationID).ToList();


            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(_LocationID, _EmployeeID).FirstOrDefault(x => x.Money == _Currency);
            model.BasketTotal = new BasketTotal()
            {
                Total = currentBasketTotal?.Total ?? 0,
                Discount = currentBasketTotal?.Discount ?? 0,
                SubTotal = currentBasketTotal?.SubTotal ?? 0,
                TaxTotal = currentBasketTotal?.TaxTotal ?? 0,
                GeneralTotal = currentBasketTotal?.GeneralTotal ?? 0,
                Currency = currentBasketTotal?.Money,
                Sign = currentBasketTotal?.Sign
            };

            return View(model);
        }

        public PartialViewResult AddBasket(int id, int cardid)
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;

            model.Price = Db.VProductPriceLastList.FirstOrDefault(x => x.ID == id);

            if (model.Price != null)
            {
                var added = Db.AddCardBasket(_LocationID, _EmployeeID, id, cardid, 7);
            }

            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(_LocationID, _EmployeeID).FirstOrDefault(x => x.Money == _Currency);
            model.BasketTotal = new BasketTotal()
            {
                Total = currentBasketTotal?.Total ?? 0,
                Discount = currentBasketTotal?.Discount ?? 0,
                SubTotal = currentBasketTotal?.SubTotal ?? 0,
                TaxTotal = currentBasketTotal?.TaxTotal ?? 0,
                GeneralTotal = currentBasketTotal?.GeneralTotal ?? 0,
                Currency = currentBasketTotal?.Money,
                Sign = currentBasketTotal?.Sign
            };

            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Price.ProductName} sepete eklendi.";
            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        public PartialViewResult RemoveBasketItem(int id)
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;

            var removed = Db.RemoveBasketItem(id);

            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(_LocationID, _EmployeeID).FirstOrDefault(x => x.Money == _Currency);
            model.BasketTotal = new BasketTotal()
            {
                Total = currentBasketTotal?.Total ?? 0,
                Discount = currentBasketTotal?.Discount ?? 0,
                SubTotal = currentBasketTotal?.SubTotal ?? 0,
                TaxTotal = currentBasketTotal?.TaxTotal ?? 0,
                GeneralTotal = currentBasketTotal?.GeneralTotal ?? 0,
                Currency = currentBasketTotal?.Money,
                Sign = currentBasketTotal?.Sign
            };

            model.Result.IsSuccess = true;
            model.Result.Message = $"Bilet sepetten kaldırıldı.";

            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        public PartialViewResult ClearBasket()
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;

            var clean = Db.CleanBasket(_LocationID, _EmployeeID);

            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(_LocationID, _EmployeeID).FirstOrDefault(x => x.Money == _Currency);
            model.BasketTotal = new BasketTotal()
            {
                Total = currentBasketTotal?.Total ?? 0,
                Discount = currentBasketTotal?.Discount ?? 0,
                SubTotal = currentBasketTotal?.SubTotal ?? 0,
                TaxTotal = currentBasketTotal?.TaxTotal ?? 0,
                GeneralTotal = currentBasketTotal?.GeneralTotal ?? 0,
                Currency = currentBasketTotal?.Money,
                Sign = currentBasketTotal?.Sign
            };

            model.Result.IsSuccess = true;
            model.Result.Message = $"Sepet temizlendi.";
            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        public ActionResult Detail(string cardinfo)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;


            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }


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

                   


                    model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == cardno);
                    model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == serino && x.MACAddress == macano && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

                    model.CardBalanceAction = 0;

                    if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
                    {
                        model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
                    }

                    model.CardBalance = existscredit ?? 0;
                }

            }


            return View(model);
        }
    }
}