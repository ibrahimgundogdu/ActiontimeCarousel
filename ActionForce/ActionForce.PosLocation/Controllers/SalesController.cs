using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SalesController : BaseController
    {
        // GET: Sales
        public ActionResult Index()
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            var DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            model.SaleSummary = Db.VTicketSaleSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).ToList();


            return View(model);
        }

        public ActionResult Detail(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            model.DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == id);
            model.TicketSale = Db.TicketSale.FirstOrDefault(x => x.ID == id);
            model.TicketSaleRows = Db.VTicketSaleRowSummary.Where(x => x.SaleID == id).ToList();
            model.TicketSalePosPayment = Db.TicketSalePosPayment.Where(x => x.SaleID == id).ToList();
            model.TicketSalePosStatus = Db.TicketSalePosStatus.Where(x => x.IsActive == true).ToList();
            model.PosPaymentType = Db.PosPaymentType.ToList();
            model.PosPaymentSubType = Db.PosPaymentSubType.ToList();
            model.Environments = Db.Environment.ToList();
            model.Currencys = Db.Currency.ToList();
            model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == id).ToList();
            model.DocumentActions = Db.VTicketSaleDocumentAction.Where(x => x.SaleID == id).ToList();
            model.DocumentNumbers = string.Join(",", model.DocumentActions.Select(x => x.DocumentNumber).ToArray());


            return View(model);
        }

        public ActionResult CheckDocument(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;
            try
            {
                Db.CheckLocationPosTicketSale(id);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Detail", new { id });
        }

        public ActionResult Refund(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == id);
            model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == id).ToList();
            model.PaymentAmount = Db.GetTicketSalePaymentAmount(id).FirstOrDefault() ?? 0;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddRefund(RefundFormModel form)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (form == null)
            {
                return RedirectToAction("Index");
            }


            string phonenumber = form.CustomerPhone.Replace("(", "").Replace(")", "").Replace(" ", "");


            int CustomerID = Db.CheckCustomer(form.CustomerIdentityNumber.Trim(), form.CustomerName.Trim(), form.CustomerMail, form.PhoneNumberCountry.Trim(), form.CountryCode.Trim(), phonenumber, 2).FirstOrDefault() ?? 2;

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == form.OrderID);
            var ResultID = Db.GetDayResultID(model.Authentication.CurrentLocation.ID,form.DocumentDate,1,3,model.Authentication.CurrentEmployee.EmployeeID,string.Empty,PosManager.GetIPAddress()).FirstOrDefault();
            var PaymentAmount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? 0;

            DocumentExpenseSlip slip = new DocumentExpenseSlip();

            slip.ActionTypeID = 41;
            slip.ActionTypeName = "Gider Pusulası";
            slip.Amount = PaymentAmount;
            slip.Currency = model.TicketSaleSummary.Currency;
            slip.Description = form.Description;
            slip.DocumentDate = form.DocumentDate;
            slip.DocumentNumber = form.DocumentNumber;
            slip.EnvironmentID = 7;
            slip.ExchangeRate = 1;
            slip.IsActive = true;
            slip.IsConfirmed = false;
            slip.LocationID = model.Authentication.CurrentLocation.ID;
            slip.OurCompanyID = model.Authentication.CurrentLocation.OurCompanyID;
            slip.RecordDate = DateTime.UtcNow.AddHours(3);
            slip.RecordEmployeeID = model.Authentication.CurrentEmployee.EmployeeID;
            slip.RecordIP = PosManager.GetIPAddress();
            slip.ReferenceID = form.OrderID;
            slip.ResultID = ResultID;
            slip.SystemAmount = PaymentAmount;
            slip.SystemCurrency = model.TicketSaleSummary.Currency;
            slip.UID = Guid.NewGuid();

            try
            {
                Db.DocumentExpenseSlip.Add(slip);
                Db.SaveChanges();



                // burdan devam
            }
            catch (Exception ex)
            {

            }




            SMSManager smsmanager = new SMSManager();
            smsmanager.SendSMS("Deneme Mesajıdır.", "5335975566", null);


            return View(model);
        }
    }
}