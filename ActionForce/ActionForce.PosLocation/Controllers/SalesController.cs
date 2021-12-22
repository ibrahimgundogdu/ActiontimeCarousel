using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SalesController : BaseController
    {
        // GET: Sales
        public ActionResult Index(string id)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            DateTime DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out DocumentDate);
            }

            model.DocumentDate = DocumentDate;

            model.SaleSummary = Db.VTicketSaleSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).ToList();

            model.TicketSaleRowSummary = Db.VTicketSaleSaleRowSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).ToList();

            List<int> priceCategorieIds = model.TicketSaleRowSummary.Select(x => x.PriceCategoryID.Value).Distinct().ToList();
            List<int> priceIds = model.TicketSaleRowSummary.Select(x => x.PriceID).Distinct().ToList();

            model.VPrices = Db.VPrice.Where(x => priceCategorieIds.Contains(x.PriceCategoryID.Value)).ToList();
            model.VPrices.AddRange(Db.VPrice.Where(x => priceIds.Contains(x.ID)).ToList());
            model.VPrices = model.VPrices.Distinct().ToList();

            int[] cashtypes = new int[] { 10, 21, 24, 28, 41 }.ToArray();
            int[] cardtypes = new int[] { 1, 3, 5, 9 }.ToArray();

            model.TicketSalePaymentSummary = Db.VTicketSalePaymentSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).ToList();
            model.CashActions = Db.CashActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == DocumentDate && cashtypes.Contains(x.CashActionTypeID.Value)).ToList();
            model.BankActions = Db.BankActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == DocumentDate && cardtypes.Contains(x.BankActionTypeID.Value)).ToList();
            model.ExpenseSlips = Db.VDocumentExpenseSlip.Where(x=> x.LocationID == model.Authentication.CurrentLocation.ID && x.DocumentDate == DocumentDate && x.IsActive == true).ToList();

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
            model.ExpenseSlips = Db.VDocumentExpenseSlip.Where(x => x.ReferenceID == id && x.IsActive == true).ToList();

            model.PaymentAmount = model.TicketSalePosPaymentSummary.Sum(x => x.PaymentAmount) ?? 0;
            model.RefundedAmount = model.ExpenseSlips.Sum(x => x.Amount) ?? 0;
            model.RefundRate = (100 * model.RefundedAmount) / model.PaymentAmount;
            model.CardActionTypes = Db.CardActionType.ToList();
            model.CreditLoads = Db.TicketSaleCreditLoad.Where(x => x.SaleID == id).ToList();

            return View(model);
        }

        public ActionResult Control(string id)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;
            int counter = 0;

            try
            {


                if (!string.IsNullOrEmpty(id))
                {
                    DateTime.TryParse(id, out DocumentDate);
                }

                var SaleIds = Db.TicketSale.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).Select(x => x.ID).Distinct().ToList();

                foreach (var item in SaleIds)
                {
                    Db.CheckLocationPosTicketSale(item);
                    counter++;
                }

            }
            catch (Exception)
            {
            }

            model.Result.Message = $"{counter} adet Satış kontrol edildi";
            TempData["Result"] = model.Result;

            return RedirectToAction("Index", new { id = DocumentDate.ToString("yyyy-MM-dd") });
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

        public ActionResult Refund(long? id, long? RowID = 0)
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
            model.ExpenseSlips = Db.VDocumentExpenseSlip.Where(x => x.ReferenceID == id && x.IsActive == true).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
            model.TicketSaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.ID == RowID);
            model.RefundedAmount = model.ExpenseSlips.Sum(x => x.Amount) ?? 0;
            model.RefundAmount = model.PaymentAmount - model.RefundedAmount;

            if (model.TicketSaleRow != null)
            {
                model.RefundAmount = model.RefundAmount > model.TicketSaleRow.Total ? model.TicketSaleRow.Total.Value : model.RefundAmount;
            }
           




            return View(model);
        }

        public ActionResult RefundDetail(long? id)
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

            model.ExpenseSlip = Db.VDocumentExpenseSlip.FirstOrDefault(x => x.ID == id);

            if (model.ExpenseSlip != null)
            {
                model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == model.ExpenseSlip.ReferenceID);
                model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == model.ExpenseSlip.ReferenceID).ToList();
                model.PaymentAmount = Db.GetTicketSalePaymentAmount(model.ExpenseSlip.ReferenceID).FirstOrDefault() ?? 0;
                model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
                model.Customer = Db.Customer.FirstOrDefault(x => x.ID == model.ExpenseSlip.CustomerID);
            }
            else
            {
                return RedirectToAction("Index");
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
            var PaymentAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

            if (PaymentAmount > 0)
            {
                model.PaymentAmount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? 0;
                model.RefundedAmount = Db.GetTicketSaleRefundAmount(form.OrderID).FirstOrDefault() ?? 0;

                if ((model.RefundedAmount + PaymentAmount) > model.PaymentAmount)
                {
                    PaymentAmount = (model.PaymentAmount - model.RefundedAmount);
                }

                DocumentExpenseSlip slip = new DocumentExpenseSlip();
                long SlipID = 0;

                try
                {

                    SlipID = Db.AddEditExpenseSlip(
                        null,
                        model.Authentication.CurrentLocation.OurCompanyID,
                        model.Authentication.CurrentLocation.ID,
                        form.DocumentDate,
                        form.DocumentNumber,
                        CustomerID,
                        form.PostAddress,
                        form.PayMethod,
                        PaymentAmount,
                        model.TicketSaleSummary.Currency,
                        1,
                        PaymentAmount,
                        model.TicketSaleSummary.Currency,
                        form.OrderID,
                        form.OrderRowID,
                        "Gider Pusulası",
                        41,
                        form.Description,
                        ResultID,
                        7,
                        Guid.NewGuid(),
                        DateTime.UtcNow.AddHours(3),
                        model.Authentication.CurrentEmployee.EmployeeID,
                        PosManager.GetIPAddress(),
                        null,
                        null,
                        null,
                        true,
                        true
                        ).FirstOrDefault().Value;


                    Db.ExpenseSlipCheck(SlipID);

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Gider Pusulası Kaydedildi.";
                }
                catch (Exception ex)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Gider Pusulası Kaydedilemedi : " + ex.Message;
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Tutar 0 dan büyük olmalı.";
            }
            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }

        [HttpPost]
        public ActionResult EditRefund(RefundFormModel form)
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
            var refundAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            long SlipID = form.ExpenseSlipID ?? 0;

            DocumentExpenseSlip slip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ID == form.ExpenseSlipID);
            SlipID = slip.ID;

            model.PaymentAmount = Db.GetTicketSalePaymentAmount(form.OrderID).FirstOrDefault() ?? 0;
            model.RefundedAmount = Db.GetTicketSaleRefundAmount(form.OrderID).FirstOrDefault() ?? 0;

            model.RefundedAmount = model.RefundedAmount - slip.Amount ?? 0;

            if ((model.RefundedAmount + refundAmount) > model.PaymentAmount)
            {
                refundAmount = (model.PaymentAmount - model.RefundedAmount);
            }

            if (refundAmount > 0)
            {
                try
                {
                    DocumentExpenseSlip self = new DocumentExpenseSlip()
                    {
                        DocumentNumber = slip.DocumentNumber,
                        DocumentDate = slip.DocumentDate,
                        Description = slip.Description,
                        ActionTypeID = slip.ActionTypeID,
                        ActionTypeName = slip.ActionTypeName,
                        Amount = slip.Amount,
                        Currency = slip.Currency,
                        CustomerAddress = slip.CustomerAddress,
                        CustomerID = slip.CustomerID,
                        EnvironmentID = slip.EnvironmentID,
                        ExchangeRate = slip.ExchangeRate,
                        ID = slip.ID,
                        IsActive = slip.IsActive,
                        IsConfirmed = slip.IsConfirmed,
                        LocationID = slip.LocationID,
                        OurCompanyID = slip.OurCompanyID,
                        PayMethodID = slip.PayMethodID,
                        RecordDate = slip.RecordDate,
                        RecordEmployeeID = slip.RecordEmployeeID,
                        RecordIP = slip.RecordIP,
                        ReferenceID = slip.ReferenceID,
                        ResultID = slip.ResultID,
                        SystemAmount = slip.SystemAmount,
                        SystemCurrency = slip.SystemCurrency,
                        UID = slip.UID,
                        UpdateDate = slip.UpdateDate,
                        UpdateEmployee = slip.UpdateEmployee,
                        UpdateIP = slip.UpdateIP,
                        SaleID = slip.SaleID,
                        SaleRowID = slip.SaleRowID
                    };

                    SlipID = Db.AddEditExpenseSlip(
                        slip.ID,
                        slip.OurCompanyID,
                        slip.LocationID,
                        form.DocumentDate,
                        form.DocumentNumber,
                        CustomerID,
                        form.PostAddress,
                        form.PayMethod,
                        refundAmount,
                        model.TicketSaleSummary.Currency,
                        slip.ExchangeRate,
                        refundAmount,
                        model.TicketSaleSummary.Currency,
                        slip.ReferenceID,
                        slip.SaleRowID,
                        slip.ActionTypeName,
                        slip.ActionTypeID,
                        form.Description,
                        ResultID,
                        slip.EnvironmentID,
                        slip.UID,
                        slip.RecordDate,
                        slip.RecordEmployeeID,
                        slip.RecordIP,
                        DateTime.UtcNow.AddHours(3),
                        model.Authentication.CurrentEmployee.EmployeeID,
                        PosManager.GetIPAddress(),
                        slip.IsConfirmed,
                        slip.IsActive
                        ).FirstOrDefault().Value;


                    model.Result.IsSuccess = true;
                    model.Result.Message = "Gider Pusulası Güncellendi";

                    var isequal = PosManager.PublicInstancePropertiesEqual<DocumentExpenseSlip>(self, slip, PosManager.getIgnorelist());
                    PosManager.AddApplicationLog("PosLocation", "ExpenseSlip", "Update", slip.ID.ToString(), "DocumentExpenseSlip", "EditRefund", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.CurrentEmployee.FullName, PosManager.GetIPAddress(), string.Empty, null);

                    Db.ExpenseSlipCheck(SlipID);
                }
                catch (Exception ex)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Gider Pusulası Güncellenemedi : " + ex.Message;
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Tutar 0'dan büyük olmalıdır.";
            }


            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }

        public ActionResult RemoveRefund(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;
            long OrderID = 0;

            DocumentExpenseSlip slip = Db.DocumentExpenseSlip.FirstOrDefault(x => x.ID == id);

            if (slip != null)
            {
                OrderID = slip.ReferenceID.Value;

                try
                {
                    Db.RemoveExpenseSlip(id);

                    Db.ExpenseSlipCheck(id);

                    var RefundedAmount = Db.GetTicketSaleRefundAmount(OrderID).FirstOrDefault() ?? 0;

                    if (RefundedAmount == 0)
                    {
                        Db.SetTicketSalePosStatusR(OrderID, 3);
                    }

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Gider Pusulası Kaldırıldı";
                }
                catch (Exception ex)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Gider Pusulası Kaldırılamadı : " + ex.Message;
                }
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = OrderID });
        }

        public ActionResult ConfirmRefund(long? id)
        {
            SalesControlModel model = new SalesControlModel();

            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            model.ExpenseSlip = Db.VDocumentExpenseSlip.FirstOrDefault(x => x.ID == id);
            if (model.ExpenseSlip != null)
            {
                model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == model.ExpenseSlip.ReferenceID);
            }

            return View(model);
        }

        public ActionResult CheckRefund(long? id, long? oid)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;
            try
            {
                Db.ExpenseSlipCheck(id);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Detail", new { id = oid });
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
                        model.Result.Message = "Ödeme Bilgisi Güncellendi";

                    }
                }
                catch (Exception ex)
                {
                    model.Result.Message = "Ödeme Bilgisi Güncellenemedi : " + ex.Message;
                }
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }

        public ActionResult Remove(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;
            var updateDate = DateTime.UtcNow.AddHours(3);

            var sale = Db.TicketSale.FirstOrDefault(x => x.ID == id);

            if (sale != null && sale.PosStatusID == 0)
            {
                Db.RemoveTicketSale(id, model.Authentication.CurrentEmployee.EmployeeID, updateDate, PosManager.GetIPAddress());

                model.Result.IsSuccess = true;
                model.Result.Message = "Sipariş Kaldırıldı";
                TempData["Result"] = model.Result;

                return RedirectToAction("Index");
            }
            else
            {

                model.Result.IsSuccess = false;
                model.Result.Message = "Sipariş Bulunamadı veya Uygun Değil";

                TempData["Result"] = model.Result;

                return RedirectToAction("Detail", new { id });
            }

        }

        public ActionResult RowDetail(long? id, long? OrderID)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;
            var updateDate = DateTime.UtcNow.AddHours(3);

            model.TicketSalePosStatus = Db.TicketSalePosStatus.ToList();
            model.TicketSaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.ID == id && x.SaleID == OrderID);
            model.TicketSale = Db.TicketSale.FirstOrDefault(x => x.ID == OrderID);
            if (model.TicketSaleRow != null)
            {
                model.Price = Db.Price.FirstOrDefault(x => x.ID == model.TicketSaleRow.PriceID);

                if (model.Price != null)
                {
                    model.TicketProduct = Db.TicketProduct.FirstOrDefault(x => x.ID == model.Price.ProductID);
                    model.Prices = Db.Price.Where(x => x.PriceCategoryID == model.Price.PriceCategoryID && x.Unit >= model.Price.Unit).ToList();
                }
                else
                {
                    model.TicketProduct = new TicketProduct();
                    model.Prices = new List<Price>();
                }
            }
            else
            {
                model.Price = new Price();
            }


            model.PosStatusName = model.TicketSalePosStatus.FirstOrDefault(x => x.ID == model.TicketSale.PosStatusID)?.PosStatusName;

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSaleRow(SaleRowFormModel form)
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            if (form == null)
            {
                return RedirectToAction("Index");
            }

            var ticketSaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.ID == form.OrderRowID && x.SaleID == form.OrderID);
            var ticketSale = Db.TicketSale.FirstOrDefault(x => x.ID == form.OrderID);

            if (ticketSale != null && ticketSaleRow != null)
            {
                try
                {
                    DateTime receiptdate = form.ReceiptDate;
                    TimeSpan receipttime = (TimeSpan)form.ReceiptTime;
                    var saledate = Convert.ToDateTime(receiptdate.Add(receipttime));

                    var price = Db.Price.FirstOrDefault(x => x.ID == ticketSaleRow.PriceID);
                    ticketSaleRow.Date = saledate;

                    if (ticketSaleRow.PriceID != form.PriceID)
                    {
                        price = Db.Price.FirstOrDefault(x => x.ID == form.PriceID);
                    }

                    ticketSaleRow.PriceID = price.ID;
                    ticketSaleRow.Unit = price.Unit;
                    ticketSaleRow.ExtraUnit = form.ExtraUnit;
                    ticketSaleRow.Price = price.Price1.Value;
                    ticketSaleRow.ExtraPrice = (form.ExtraUnit * price.ExtraMultiple);
                    ticketSaleRow.Description = form.Description;

                    Db.SaveChanges();

                    Db.SetTicketSaleAmount(form.OrderID);

                    Db.SetTicketSalePosStatusUpgrade(form.OrderID, 1);

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Satır Güncellendi";

                }
                catch (Exception ex)
                {
                    model.Result.Message = "Satır Güncellenemedi : " + ex.Message;
                }
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = form.OrderID });
        }
    }
}