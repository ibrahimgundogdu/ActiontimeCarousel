using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace ActionForce.PosLocation.Controllers
{
    public class DefaultController : BaseController
    {
        private int _EmployeeID { get; set; }
        private int _LocationID { get; set; }
        private string _Currency { get; set; }

        public DefaultController() : base()   
        {
            
        }
        // GET: Default
        public ActionResult Index()
        {
            DefaultControlModel model = new DefaultControlModel();
            model.Authentication = this.AuthenticationData;

            if (model.Authentication.IsCardSystem == true)
            {
                return RedirectToAction("Index", "Card");
            }

            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;
            //string sql = "SELECT * FROM VPriceLastList Where ";
            //using (var connection = new SqlConnection(PosManager.GetConnectionString()))
            //{
            //    var invoices = connection.Query<CurrentPriceList>(sql);   @(Model.Authentication.IsCardSystem == true ? "Card":"/")
            //}
            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }




            model.PriceList = Db.GetLocationCurrentPrices(_LocationID).ToList();
            model.PromotionList = Db.GetLocationCurrentPromotions(_LocationID).ToList();
            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x=> x.Quantity);
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

        public PartialViewResult AddBasket(int id)
        {
            DefaultControlModel model = new DefaultControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;

            model.Price = Db.VPriceLastList.FirstOrDefault(x => x.ID == id);

            if (model.Price != null)
            {
                var added = Db.AddPosBasket(_LocationID, _EmployeeID, id, null, null, null,7);
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
            DefaultControlModel model = new DefaultControlModel();
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

        public PartialViewResult AddBasketPromotion(int id)
        {
            DefaultControlModel model = new DefaultControlModel();
            _EmployeeID = this.AuthenticationData.CurrentEmployee.EmployeeID;
            _LocationID = this.AuthenticationData.CurrentLocation.ID;
            _Currency = this.AuthenticationData.CurrentLocation.Currency;

            model.Promotion = Db.VTicketPromotion.FirstOrDefault(x => x.ID == id);

            if (model.Promotion != null)
            {
                var added = Db.AddBasket(_LocationID, _EmployeeID, model.Promotion.MainPriceID, id, null, null);
            }

            model.BasketList = Db.GetLocationCurrentBasket(_LocationID, _EmployeeID).ToList();
            model.Result.IsSuccess = true;
            model.Result.Message = $"{model.Promotion.ProductName} sepete eklendi.";

            TempData["Result"] = model.Result;

            return PartialView("_PartialBasketList", model);
        }


        public PartialViewResult ClearBasket()
        {
            DefaultControlModel model = new DefaultControlModel();
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
    }
}