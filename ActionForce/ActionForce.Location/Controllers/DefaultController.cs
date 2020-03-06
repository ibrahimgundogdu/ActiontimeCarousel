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
            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.EmployeeBasketCount = Db.GetBasketCount(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID)?.FirstOrDefault().Value ?? 0;
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

            if (basketitem != null)
            {
                bool isChecked = ischecked == 1 ? true : false;
                var added = Db.AddBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, id, null, null, null);
            }

            model.BasketList = Db.GetLocationCurrentBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Price.ProductName} kullanım durumu güncellendi.";

            return PartialView("_PartialBasketList", model);
        }
    }
}