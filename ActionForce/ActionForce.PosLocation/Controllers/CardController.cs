using ActionForce.Entity;
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
        public ActionResult Index(string cardinfo)
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
            model.CardTypes = Db.CardType.Where(x => x.IsActive == true).ToList();

            model.BasketTotal = new BasketTotal()
            {
                Total = 0,
                Discount = 0,
                SubTotal = 0,
                TaxTotal = 0,
                GeneralTotal = 0,
                Currency = "TRL",
                Sign = "₺"
            };

            model.BasketList = new List<VTicketBasket>();


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

                    if (!Db.Card.Any(x => x.CardNumber == cardno))
                    {
                        Db.AddCard(cardno);
                    }

                    model.Card = Db.VCard.FirstOrDefault(x => x.CardNumber == cardno);

                    if (model.Card.CardTypeID <= 1)
                    {
                        model.CardActions = Db.VCardActions.Where(x => x.CardNumber == cardno).ToList();
                        model.ActionDates = model.CardActions.Select(x => x.DateOnly.Value.Date).Distinct().OrderByDescending(x => x).ToList();
                    }
                    if (model.Card.CardTypeID == 2)
                    {
                        var isemployeecard = Db.EmployeeCard.FirstOrDefault(x => x.CardNumber == model.Card.CardNumber);

                        if (isemployeecard != null)
                        {
                            if (isemployeecard.CardStatusID == 1)
                            {
                                PosManager manager = new PosManager();

                                var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);
                                var employees = manager.GetLocationEmployeeModelsToday(location);

                                model.EmployeeModel = employees.FirstOrDefault(x => x.ID == isemployeecard.EmployeeID);
                            }
                        }

                       

                    }

                    model.CardStatus = model.Card.CardStatusName ?? "Bilinmiyor";
                    model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == serino && x.MACAddress == macano && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

                    model.CardBalanceAction = 0;

                    if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
                    {
                        model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
                    }

                    model.CardBalance = existscredit ?? 0;
                }

                model.BasketList = Db.GetLocationCardBasket(_LocationID, model.Card?.CardNumber).ToList();
                model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
                var currentBasketTotal = Db.GetLocationCardBasketTotal(_LocationID, model.Card?.CardNumber).FirstOrDefault(x => x.Money == _Currency);

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

            }

            return View(model);
        }

        public PartialViewResult AddBasket(int id, int cardpriceid, string cardnumber, int cardreaderid)
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;
            model.CardNumber = cardnumber;

            model.Price = Db.VProductPriceLastList.FirstOrDefault(x => x.ID == id);

            if (model.Price != null)
            {
                var added = Db.AddCardBasket(_LocationID, _EmployeeID, id, cardpriceid, cardnumber, cardreaderid, 7);
            }

            model.BasketList = Db.GetLocationCardBasket(_LocationID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(_LocationID, cardnumber).FirstOrDefault(x => x.Money == _Currency);
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

            model.CardReader = Db.CardReader.FirstOrDefault(x => x.ID == cardreaderid);
            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Price.ProductName} sepete eklendi.";
            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        public PartialViewResult RemoveBasketItem(int id, string cardnumber, int cardreaderid)
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;
            model.CardNumber = cardnumber;

            var removed = Db.RemoveBasketItem(id);

            model.BasketList = Db.GetLocationCardBasket(_LocationID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(_LocationID, cardnumber).FirstOrDefault(x => x.Money == _Currency);
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

            model.CardReader = Db.CardReader.FirstOrDefault(x => x.ID == cardreaderid);
            model.Result.IsSuccess = true;
            model.Result.Message = $"Bilet sepetten kaldırıldı.";

            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        public PartialViewResult ClearBasket(string cardnumber, int cardreaderid)
        {
            CardControlModel model = new CardControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;
            model.CardNumber = cardnumber;

            var clean = Db.CleanBasket(_LocationID, _EmployeeID);

            model.BasketList = Db.GetLocationCardBasket(_LocationID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(_LocationID, cardnumber).FirstOrDefault(x => x.Money == _Currency);
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

            model.CardReader = Db.CardReader.FirstOrDefault(x => x.ID == cardreaderid);
            model.Result.IsSuccess = true;
            model.Result.Message = $"Sepet temizlendi.";
            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }

        //public ActionResult Detail(string cardinfo)
        //{
        //    CardControlModel model = new CardControlModel();
        //    model.Authentication = this.AuthenticationData;

        //    _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
        //    _LocationID = this.AuthenticationData.CurrentLocation.ID;
        //    _Currency = this.AuthenticationData.CurrentLocation.Currency;


        //    if (TempData["Result"] != null)
        //    {
        //        model.Result = TempData["Result"] as Result;
        //    }


        //    if (!string.IsNullOrEmpty(cardinfo))
        //    {
        //        //00119D9B;CC:50:E3:11:9D:9B;4528C2F3;100

        //        string[] cardinfolist = cardinfo.Split(';').ToArray();

        //        if (cardinfolist.Count() == 4)
        //        {
        //            string serino = cardinfolist[0];
        //            string macano = cardinfolist[1];
        //            string cardno = cardinfolist[2];
        //            string credit = cardinfolist[3];
        //            double? existscredit = (Convert.ToDouble(credit) / 100);
        //            model.CardNumber = cardno;



        //            model.Card = Db.Card.FirstOrDefault(x => x.CardNumber == cardno);
        //            model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == serino && x.MACAddress == macano && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

        //            model.CardBalanceAction = 0;

        //            if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
        //            {
        //                model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
        //            }

        //            model.CardBalance = existscredit ?? 0;
        //        }

        //    }


        //    return View(model);
        //}
    }
}