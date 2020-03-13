using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class DefaultController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            DefaultControlModel model = new DefaultControlModel();

            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            model.PromotionList = Db.GetLocationCurrentPromotions(model.Authentication.CurrentLocation.ID).ToList();
            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.EmployeeBasketCount = Db.GetBasketCount(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID)?.FirstOrDefault().Value ?? 0;
            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).FirstOrDefault(x => x.Money == model.Authentication.CurrentOurCompany.Currency);
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

        [AllowAnonymous]
        public PartialViewResult AddBasket(int id)
        {
            DefaultControlModel model = new DefaultControlModel();

            model.Price = Db.VPriceLastList.FirstOrDefault(x => x.ID == id);

            if (model.Price != null)
            {
                var added = Db.AddBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, id, null, null, null);
            }

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Price.ProductName} sepete eklendi.";

            return PartialView("_PartialBasketList", model);
        }

        [AllowAnonymous]
        public PartialViewResult RemoveBasketItem(int id)
        {
            DefaultControlModel model = new DefaultControlModel();

            var removed = Db.RemoveBasketItem(id);

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"Bilet sepetten kaldırıldı.";

            return PartialView("_PartialBasketList", model);
        }
        [AllowAnonymous]
        public PartialViewResult AddBasketPromotion(int id)
        {
            DefaultControlModel model = new DefaultControlModel();

            model.Promotion = Db.VTicketPromotion.FirstOrDefault(x => x.ID == id);

            if (model.Promotion != null)
            {
                var added = Db.AddBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, model.Promotion.MainPriceID, id, null, null);
            }

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Promotion.ProductName} sepete eklendi.";

            return PartialView("_PartialBasketList", model);
        }

        [AllowAnonymous]
        public PartialViewResult ClearBasket()
        {
            DefaultControlModel model = new DefaultControlModel();

            var clean = Db.CleanBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID);

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"Sepet temizlendi.";

            return PartialView("_PartialBasketList", model);
        }

        [AllowAnonymous]
        public PartialViewResult BasketItemDetail(int id)
        {
            DefaultControlModel model = new DefaultControlModel();

            model.BasketItem = Db.VTicketBasket.FirstOrDefault(x => x.ID == id);

            return PartialView("_PartialBasketItemDetail", model);
        }

        [AllowAnonymous]
        public PartialViewResult CheckImmediately(int id, int ischecked)
        {
            DefaultControlModel model = new DefaultControlModel();

            var basketitem = Db.TicketBasket.FirstOrDefault(x => x.ID == id);
            string usinginfo = string.Empty;

            if (basketitem != null)
            {
                bool isChecked = ischecked == 1 ? true : false;

                var added = Db.SetBasketItemUseImmediately(id, isChecked);

                usinginfo = isChecked == true ? "Hemen kullanılacak" : "Daha sonra kullanılacak";
            }

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"Bilet kullanım durumu : {usinginfo} olarak güncellendi.";

            return PartialView("_PartialBasketList", model);
        }

        [AllowAnonymous]
        public PartialViewResult BasketTotal()
        {
            DefaultControlModel model = new DefaultControlModel();

            var currentBasketTotal = Db.GetLocationCurrentBasketTotal(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).FirstOrDefault(x => x.Money == model.Authentication.CurrentOurCompany.Currency);
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

            return PartialView("_PartialBasketTotal", model);
        }

        [AllowAnonymous]
        public PartialViewResult ReadTicketDetail(string qcode)
        {
            DefaultControlModel model = new DefaultControlModel();

            if (!string.IsNullOrEmpty(qcode))
            {
                model.TicketInfo = LocationHelper.GetScannedTicketInfo(qcode);
            }

            return PartialView("_PartialReadTicketDetail", model);
        }
    }
}