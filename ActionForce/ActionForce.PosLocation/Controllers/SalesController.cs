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

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

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

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

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

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == id);
            model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == id).ToList();
            model.PaymentAmount = Db.GetTicketSalePaymentAmount(id).FirstOrDefault() ?? 0;
            model.ExpenseSlip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ReferenceID == id);
            if (model.ExpenseSlip != null && model.ExpenseSlip.CustomerID > 0)
            {
                model.Customer = Db.Customer.FirstOrDefault(x => x.ID == model.ExpenseSlip.CustomerID);
            }

            return View(model);
        }


        [HttpPost]
        public JsonResult GetCustomerInfo(string id)
        {
            Customer customer = new Customer();

            if (string.IsNullOrEmpty(id))
            {
                return Json(customer, JsonRequestBehavior.AllowGet);
            }
            //90(533) 303 10 40  (533) 303 10 40

            string code = string.Empty;
            string number = string.Empty;

            if (id.Contains("("))
            {
                var phonenumber = id.Split('(').ToArray();
                code = phonenumber[0];
                number = phonenumber[1].Replace(")", "").Replace(" ", "").Replace("(", "");
            }
            else
            {
                code = "90";
                number = id.Substring(2, id.Length - 2).Replace(")", "").Replace(" ", "").Replace("(", "");
            }


            customer = Db.Customer.FirstOrDefault(x => x.IsActive == true && x.PhoneCode == code && x.PhoneNumber == number);

            return Json(customer, JsonRequestBehavior.AllowGet);
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


            int CustomerID = Db.CheckCustomer(form.CustomerIdentityNumber.Trim(), form.CustomerName.Trim(), form.CustomerMail, form.PhoneNumberCountry.Trim(), form.CountryCode.Trim(), phonenumber, form.PostAddress.Trim(), 2).FirstOrDefault() ?? 2;

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == form.OrderID);
            var ResultID = Db.GetDayResultID(model.Authentication.CurrentLocation.ID, form.DocumentDate, 1, 3, model.Authentication.CurrentEmployee.EmployeeID, string.Empty, PosManager.GetIPAddress()).FirstOrDefault();
            var PaymentAmount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? 0;

            DocumentExpenseSlip slip = new DocumentExpenseSlip();

            if (form.ExpenseSlipID > 0)
            {
                slip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ID == form.ExpenseSlipID);
                slip.CustomerAddress = form.PostAddress;
                slip.Description = form.Description;

                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Gider Pusulası Bulundu. Doğrulayınız!";
            }
            else
            {
                try
                {
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
                    slip.CustomerID = CustomerID;
                    slip.CustomerAddress = form.PostAddress;

                    Db.DocumentExpenseSlip.Add(slip);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Gider Pusulası Kaydedildi. Doğrulayınız!";
                }
                catch (Exception ex)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Gider Pusulası Kaydedilemedi : " + ex.Message;
                }
            }


            if (slip != null)
            {
                var Customer = Db.Customer.FirstOrDefault(x => x.ID == slip.CustomerID);

                if (Customer.PhoneNumber != phonenumber)
                {
                    Customer.PhoneNumber = phonenumber;
                    Db.ChangeCustomerPhone(Customer.ID, phonenumber);
                }

                Random _random = new Random();
                var code = _random.Next(100000, 999999);

                string smsphonenumber = Customer.PhoneCode == "90" ? Customer.PhoneNumber : Customer.SMSNumber;
                bool isinternational = Customer.PhoneCode == "90" ? false : true;
                var sendtime = DateTime.UtcNow.AddHours(3);

                Db.AddConfirmMessage(12, slip.UID, smsphonenumber, code, sendtime);

                string Message = $"Degerli ziyaretcimiz. {code} kodunuzu UFE GRUP dan aldiginiz hizmetin iptali icin kullanabilirsiniz.";
                // SMS gönderme
                SMSManager smsmanager = new SMSManager();
                smsmanager.SendSMS(Message, smsphonenumber, isinternational);

                return RedirectToAction("ConfirmRefund", new { id = slip.ID });

            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Refund", new { id = form.OrderID });
        }

        public ActionResult ConfirmRefund(long? id)
        {
            SalesControlModel model = new SalesControlModel();

            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            model.ExpenseSlip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ID == id);
            if (model.ExpenseSlip != null)
            {
                model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == model.ExpenseSlip.ReferenceID);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult CheckConfirm(ConfirmFormModel form)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (form == null)
            {
                return RedirectToAction("Index");
            }

            var expenseslip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ID == form.SlipID);

            if (expenseslip != null)
            {
                var confirm = Db.ConfirmMessage.Where(x => x.DocumentUID == expenseslip.UID && x.DocumentTypeID == 12).OrderByDescending(x => x.DateSend).FirstOrDefault();

                if (confirm != null)
                {
                    if (confirm.ConfirmCode.ToString() == form.SMSCode)
                    {
                        var DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
                        var resuldid = Db.ExpenseSlipMessageConfirmed(confirm.ID, expenseslip.ID, form.OrderID, DocumentDate).FirstOrDefault();

                        if (resuldid > 0)
                        {
                            model.Result.IsSuccess = true;
                            model.Result.Message = "Gider Pusulası Eklendi";
                        }
                        else
                        {
                            model.Result.IsSuccess = false;
                            model.Result.Message = "Gider Pusulası Eklenemedi";
                        }
                    }
                    else
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Doğrulama Kodu hatalı";
                    }
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Doğrulama Kaydı Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Gider Pusulası Bulunamadı";
            }

            return RedirectToAction("Refund", new { id = form.OrderID });
        }

        public ActionResult Payment(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == id);
            
            model.TicketSalePosReceipt = Db.TicketSalePosReceipt.FirstOrDefault(x => x.SaleID == id);
            model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == id).ToList();

            model.IsManuel = model.TicketSalePosPaymentSummary.Any(x => x.FromPosTerminal == false);

            model.PaymentAmount = model.TicketSaleSummary.Total;
            model.BalanceAmount = model.TicketSaleSummary.BalanceAmount;
            model.PosPaymentType = Db.PosPaymentType.Where(x => x.IsManual == true).ToList();
            model.Banks = Db.Bank.Where(x => x.EFTCode != null).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddPayment(PaymentFormModel form)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (form == null)
            {
                return RedirectToAction("Index");
            }

            var order = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == form.OrderID);

            if (order != null)
            {
                try
                {
                    DateTime receiptdate = form.ReceiptDate;
                    TimeSpan receipttime = (TimeSpan)form.ReceiptTime.TimeOfDay;

                    var paymentdate = Convert.ToDateTime(receiptdate.Add(receipttime));

                    var receiptid = Db.AddTicketSalePosReceipt(order.ID, form.ReceiptNo, form.ZNo, form.EkuNo, paymentdate.ToString(), paymentdate.Date, paymentdate.TimeOfDay, 1, order.Total);

                    if (!string.IsNullOrEmpty(form.PaymentAmount))
                    {
                        var amount = PosManager.GetStringToAmount(form.PaymentAmount);

                        if (amount <= 0)
                        {
                            model.Result.Message = "Tutar 0 veya daha küçük olamaz!";
                            TempData["Result"] = model.Result;

                            return RedirectToAction("Detail", new { id = form.OrderID });
                        }

                        if (amount > order.BalanceAmount)
                        {
                            amount = order.BalanceAmount;
                        }

                        var paymenttype = form.PosPaymentType;
                        var subpaymenttype = 0;
                        var noi = 0;
                        short bkmid = 0;

                        if (form.PosPaymentType == 4)
                        {
                            subpaymenttype = 1;
                            noi = form.Installment ?? 1;
                            bkmid = (short?)form.BankId ?? (short)46;
                        }

                        var paymentid = Db.AddTicketSalePosPayment(order.ID, paymenttype, subpaymenttype.ToString(), noi, amount, null, 949, null, paymentdate.ToString(), paymentdate.Date, paymentdate.TimeOfDay, bkmid, "0", "0", null, null, null, null, "", false);

                        var posStatusID = 3;

                        var paidamount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? (double)0;
                        if (paidamount < order.Total)
                        {
                            posStatusID = 1;
                        }

                        using (ActionTimeEntities _db = new ActionTimeEntities())
                        {
                            _db.CheckLocationPosTicketSale(order.ID);
                        }

                        var sicilno = Db.GetLocationCurrentPosTerminal(model.Authentication.CurrentLocation.ID).FirstOrDefault();

                        Db.SetTicketSaleStatus(order.ID, posStatusID, sicilno);
                        Db.SetTicketSalePosSend(order.ID, false);

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Ödeme Bilgisi Eklendi";

                    }
                }
                catch (Exception ex)
                {
                    model.Result.Message = "Ödeme Bilgisi Eklenemedi : " + ex.Message;
                }
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }


        [HttpPost]
        public ActionResult EditPayment(PaymentFormModel form)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (form == null)
            {
                return RedirectToAction("Index");
            }

            var order = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == form.OrderID);
            var receipt = Db.TicketSalePosReceipt.FirstOrDefault(x => x.ID == form.PosReceiptID);

            if (order != null && receipt != null)
            {
                try
                {
                    DateTime receiptdate = form.ReceiptDate;
                    TimeSpan receipttime = (TimeSpan)form.ReceiptTime.TimeOfDay;
                    var paymentdate = Convert.ToDateTime(receiptdate.Add(receipttime));

                    if (order.IsSendPosTerminal == false)
                    {
                        receipt.EkuNo = form.EkuNo;
                        receipt.ReceiptNo = form.ReceiptNo;
                        receipt.ZNo = form.ZNo;
                        receipt.TransDateTime = $"{paymentdate.ToString("yyyy-MM-dd")}T{paymentdate.ToString("hh:mm:ss:fffffff")}";
                        receipt.ReceiptDate = paymentdate.Date;
                        receipt.ReceiptTime = paymentdate.TimeOfDay;

                        Db.SaveChanges();
                    }
                    
                    if (!string.IsNullOrEmpty(form.PaymentAmount))
                    {
                        var amount = PosManager.GetStringToAmount(form.PaymentAmount);

                        if (amount <= 0)
                        {
                            model.Result.Message = "Tutar 0 veya daha küçük olamaz!";
                            TempData["Result"] = model.Result;

                            return RedirectToAction("Detail", new { id = form.OrderID });
                        }

                        if (amount > order.BalanceAmount)
                        {
                            amount = order.BalanceAmount;
                        }

                        var paymenttype = form.PosPaymentType;
                        var subpaymenttype = 0;
                        var noi = 0;
                        short bkmid = 0;

                        if (form.PosPaymentType == 4)
                        {
                            subpaymenttype = 1;
                            noi = form.Installment ?? 1;
                            bkmid = (short?)form.BankId ?? (short)46;
                        }

                        var paymentid = Db.AddTicketSalePosPayment(order.ID, paymenttype, subpaymenttype.ToString(), noi, amount, null, 949, null, paymentdate.ToString(), paymentdate.Date, paymentdate.TimeOfDay, bkmid, "0", "0", null, null, null, null, "", false);

                        var posStatusID = 3;

                        var paidamount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? (double)0;
                        if (paidamount < order.Total)
                        {
                            posStatusID = 1;
                        }

                        using (ActionTimeEntities _db = new ActionTimeEntities())
                        {
                            _db.CheckLocationPosTicketSale(order.ID);
                        }

                        var sicilno = Db.GetLocationCurrentPosTerminal(model.Authentication.CurrentLocation.ID).FirstOrDefault();

                        Db.SetTicketSaleStatus(order.ID, posStatusID, sicilno);
                        Db.SetTicketSalePosSend(order.ID, false);

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Ödeme Bilgisi Eklendi";

                    }
                }
                catch (Exception ex)
                {
                    model.Result.Message = "Ödeme Bilgisi Eklenemedi : " + ex.Message;
                }
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }





    }
}