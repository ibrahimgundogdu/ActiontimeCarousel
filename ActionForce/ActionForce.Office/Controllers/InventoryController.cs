using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class InventoryController : BaseController
    {
        // GET: Inventory
        public ActionResult Pos()
        {
            InventoryControlModel model = new InventoryControlModel();

            model.PosTerminals = Db.PosTerminal.ToList();


            return View();
        }
    }
}