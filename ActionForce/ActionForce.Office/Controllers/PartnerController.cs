using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class PartnerController : BaseController
    {

        [AllowAnonymous]
        public ActionResult Index()
        {
            PartnerControlModel model = new PartnerControlModel();

            model.VPartners = Db.VPartner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }



        [AllowAnonymous]
        public ActionResult NewPartner()
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Countries = Db.Country.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            int firstCountryId = model.Countries.FirstOrDefault().ID;
            model.States = Db.State.Where(x => x.CountryID == firstCountryId).ToList();
            int firstStateId = model.States.FirstOrDefault().ID;
            model.Cities = Db.City.Where(x => x.CountryID == firstCountryId && x.StateID == firstStateId).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AddPartner(PartnerFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {

                Partner partner = new Partner();

                partner.UID = form.UID;
                partner.IsActive = true;
                partner.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID.Value;
                partner.Phone = form.Phone;
                partner.AccountCode = form.AccountCode;
                partner.Address = form.Address;
                partner.City = form.CityID;
                partner.Country = form.Country;
                partner.EMail = form.EMail;
                partner.FullName = form.FullName;
                partner.PhoneCode = form.PhoneCode;
                partner.PostCode = form.PostCode;
                partner.State = form.StateID;
                partner.TaxNumber = form.TaxNumber;
                partner.TaxOffice = form.TaxOffice;

                Db.Partner.Add(partner);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Partner Eklendi";

                OfficeHelper.AddApplicationLog("Office", "Partner", "Insert", partner.ID.ToString(), "Partner", "AddPartner", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, partner);

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Partner", new { id = form.UID });
        }





        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            model.VPartner = Db.VPartner.FirstOrDefault(x => x.UID == id);
            if (model.VPartner == null)
            {
                return RedirectToAction("Index");
            }

            List<int> discardLocIds = new List<int>() { 175, 179, 212 }.ToList();

            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID == model.VPartner.ID).ToList();
            model.PartnerUsers = Db.PartnerUser.Where(x => x.PartnerID == model.VPartner.ID).ToList();
            model.Locations = Db.Location.Where(x => x.IsActive == true && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID.Value && x.LocationTypeID != 5 && x.LocationTypeID != 6 && !discardLocIds.Contains(x.LocationID)).ToList();

            model.Countries = Db.Country.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            int firstCountryId = model.Countries.FirstOrDefault().ID;
            model.States = Db.State.Where(x => x.CountryID == firstCountryId).ToList();
            int firstStateId = model.States.FirstOrDefault().ID;
            model.Cities = Db.City.Where(x => x.CountryID == firstCountryId && x.StateID == firstStateId).ToList();





            return View(model);

        }

        [AllowAnonymous]
        public ActionResult PrePartnership(PartnershipFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (form.LocationID == 0)
            {
                return RedirectToAction("Index");
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == form.LocationID);
            model.VPartner = Db.VPartner.FirstOrDefault(x => x.ID == form.PartnerID);
            model.Partnership = Db.VPartnership.FirstOrDefault(x => x.LocationID == model.Location.LocationID && x.PartnerID == model.VPartner.ID);
            model.UFEPartnership = Db.VPartnership.FirstOrDefault(x => x.LocationID == model.Location.LocationID && x.PartnerID == 0);


            return View(model);
        }

        //AddUser

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditPartnership(PartnershipFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null && form.LocationID > 0 && form.PartnerID >= 0)
            {

                var partner = Db.Partner.FirstOrDefault(x => x.UID == form.PartnerUID && x.ID == form.PartnerID);
                var location = Db.Location.FirstOrDefault(x => x.LocationUID == form.LocationUID && x.LocationID == form.LocationID);

                if (partner != null && location != null)
                {

                    if (form.SubmitUpdate == "1")
                    {
                        var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);
                        var ufePartnership = Db.Partnership.FirstOrDefault(x => x.ID == form.UFEPartnershipID);

                        var periodbegin = Db.ExpensePeriod.FirstOrDefault(x => x.DateYear == form.DateStart.Year && x.DateMonth == form.DateStart.Month);

                        DateTime? dateEnd = null;
                        if (form.DateEnd != null)
                        {
                            dateEnd = Db.ExpensePeriod.FirstOrDefault(x => x.DateYear == form.DateEnd.Value.Year && x.DateMonth == form.DateEnd.Value.Month)?.DateEnd;
                        }

                        var differentRate = form.PartnershipRate - partnership.PartnershipRate;

                        if (partnership != null)
                        {
                            Partnership self = new Partnership()
                            {
                                IsActive = partnership.IsActive,
                                ID = partnership.ID,
                                DateEnd = partnership.DateEnd,
                                LocationID = partnership.LocationID,
                                PartnerID = partnership.PartnerID,
                                DateStart = partnership.DateStart,
                                PartnershipRate = partnership.PartnershipRate
                            };

                            partnership.DateEnd = dateEnd;
                            partnership.DateStart = periodbegin.DateBegin;
                            partnership.LocationID = form.LocationID;
                            partnership.PartnerID = form.PartnerID;
                            partnership.PartnershipRate = form.PartnershipRate;

                            //ufePartnership.DateStart = partnership.DateStart;
                            //ufePartnership.DateEnd = null;
                            ufePartnership.PartnershipRate -= differentRate;

                            Db.SaveChanges();


                            model.Result.IsSuccess = true;
                            model.Result.Message = "Partnership Güncellendi";

                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<Partnership>(self, partnership, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Partnership", "Update", partner.ID.ToString(), "Partner", "EditPartner", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                        else
                        {
                            model.Result.IsSuccess = false;
                            model.Result.Message = "Partner Bulunamadı";
                        }
                    }

                    if (form.SubmitAdd == "1")
                    {
                        var ufePartnership = Db.Partnership.FirstOrDefault(x => x.ID == form.UFEPartnershipID);

                        var periodbegin = Db.ExpensePeriod.FirstOrDefault(x => x.DateYear == form.DateStart.Year && x.DateMonth == form.DateStart.Month);

                        DateTime? dateEnd = null;
                        if (form.DateEnd != null)
                        {
                            dateEnd = Db.ExpensePeriod.FirstOrDefault(x => x.DateYear == form.DateEnd.Value.Year && x.DateMonth == form.DateEnd.Value.Month)?.DateEnd;
                        }

                        var differentRate = form.PartnershipRate;

                        Partnership partnership = new Partnership()
                        {
                            IsActive = true,
                            DateEnd = dateEnd,
                            LocationID = form.LocationID,
                            PartnerID = form.PartnerID,
                            DateStart = periodbegin.DateBegin,
                            PartnershipRate = form.PartnershipRate
                        };

                        Db.Partnership.Add(partnership);
                        Db.SaveChanges();

                        ufePartnership.PartnershipRate -= differentRate;

                        Db.SaveChanges();

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Partnership Eklendi";

                        OfficeHelper.AddApplicationLog("Office", "Partnership", "Insert", partner.ID.ToString(), "Partner", "EditPartnership", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, partnership);

                    }

                    if (form.SubmitEnd == "1")
                    {
                        var ufePartnership = Db.Partnership.FirstOrDefault(x => x.ID == form.UFEPartnershipID);
                        var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);

                        if (partnership != null)
                        {

                            Partnership self = new Partnership()
                            {
                                IsActive = partnership.IsActive,
                                ID = partnership.ID,
                                DateEnd = partnership.DateEnd,
                                LocationID = partnership.LocationID,
                                PartnerID = partnership.PartnerID,
                                DateStart = partnership.DateStart,
                                PartnershipRate = partnership.PartnershipRate
                            };



                            partnership.DateEnd = DateTime.Now.Date;
                            Db.SaveChanges();

                            model.Result.IsSuccess = true;
                            model.Result.Message = "Partnership Sonlandırıldı";

                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<Partnership>(self, partnership, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Partner", "Update", partner.ID.ToString(), "Partner", "EditPartner", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                            ufePartnership.PartnershipRate += partnership.PartnershipRate;

                            Db.SaveChanges();


                        }
                        else
                        {
                            model.Result.IsSuccess = false;
                            model.Result.Message = "Partner Bulunamadı";
                        }
                    }

                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Partner", new { id = form.PartnerUID });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditPartner(PartnerFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {


                var partner = Db.Partner.FirstOrDefault(x => x.UID == form.UID && x.ID == form.PartnerID);

                if (partner != null)
                {

                    Partner self = new Partner()
                    {
                        IsActive = partner.IsActive,
                        OurCompanyID = partner.OurCompanyID,
                        UID = partner.UID,
                        ID = partner.ID,
                        AccountCode = partner.AccountCode,
                        Address = partner.Address,
                        City = partner.City,
                        Country = partner.Country,
                        EMail = partner.EMail,
                        FullName = partner.FullName,
                        Phone = partner.Phone,
                        PhoneCode = partner.PhoneCode,
                        PostCode = partner.PostCode,
                        State = partner.State,
                        TaxNumber = partner.TaxNumber,
                        TaxOffice = partner.TaxOffice

                    };


                    partner.AccountCode = form.AccountCode;
                    partner.Address = form.Address;
                    partner.City = form.CityID;
                    partner.Country = form.Country;
                    partner.EMail = form.EMail;
                    partner.FullName = form.FullName;
                    partner.IsActive = form.IsActive == "1" ? true : false;
                    partner.Phone = form.Phone;
                    partner.PhoneCode = form.PhoneCode;
                    partner.PostCode = form.PostCode;
                    partner.State = form.StateID;
                    partner.TaxNumber = form.TaxNumber;
                    partner.TaxOffice = form.TaxOffice;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Partner Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Partner>(self, partner, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Partner", "Update", partner.ID.ToString(), "Partner", "EditPartner", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Partner Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Partner", new { id = form.UID });
        }


        [AllowAnonymous]
        public ActionResult NewPartnerUser(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();
            model.VPartner = Db.VPartner.FirstOrDefault(x => x.UID == id);

            return View(model);
        }

        //DetailPartnerUser
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AddPartnerUser(PartnerUserFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null && form.PartnerUID != null && !string.IsNullOrEmpty(form.UserFullname))
            {
                var partner = Db.Partner.FirstOrDefault(x => x.UID == form.PartnerUID);

                if (partner != null)
                {
                    string username = form.Username.Trim();
                    if (Db.PartnerUser.Any(x => x.Username == username))
                    {
                        username = username + "_1";
                    };

                    string password = string.Empty;

                    if (form.Password.Trim() == form.Password2.Trim())
                    {
                        password = OfficeHelper.makeMD5(form.Password.Trim());

                        PartnerUser partneruser = new PartnerUser();

                        partneruser.UID = Guid.NewGuid();
                        partneruser.IsActive = true;
                        partneruser.UserFullname = form.UserFullname;
                        partneruser.Username = username;
                        partneruser.Password = password;
                        partneruser.Email = form.Email;
                        partneruser.PartnerID = partner.ID;

                        Db.PartnerUser.Add(partneruser);
                        Db.SaveChanges();

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Partner User Eklendi";

                        OfficeHelper.AddApplicationLog("Office", "PartnerUser", "Insert", partneruser.ID.ToString(), "Partner", "AddPartnerUser", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, partneruser);
                    }
                    else
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Girilen Şifreler Farklı";
                    }
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "İşortağı bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Partner", new { id = form.PartnerUID });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetStateList(int countryid)
        {
            var statelist = Db.State.Where(x => x.CountryID == countryid && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var state in statelist)
            {
                list.Add(new SelectListItem()
                {
                    Value = state.ID.ToString(),
                    Text = state.StateName
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetCityList(int stateid)
        {
            var citylist = Db.City.Where(x => x.StateID == stateid && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var city in citylist)
            {
                list.Add(new SelectListItem()
                {
                    Value = city.ID.ToString(),
                    Text = city.CityName
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public ActionResult DetailPartnerUser(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            model.PartnerUser = Db.PartnerUser.FirstOrDefault(x => x.UID == id);
            if (model.PartnerUser == null)
            {
                return RedirectToAction("Index");
            }

            model.Partner = Db.Partner.FirstOrDefault(x => x.ID == model.PartnerUser.PartnerID);

            return View(model);
        }

        //EditPartnerUser

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditPartnerUser(PartnerUserFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null && form.PartnerUserID > 0)
            {
                var partner = Db.Partner.FirstOrDefault(x => x.UID == form.PartnerUID);

                if (partner != null)
                {
                    var partneruser = Db.PartnerUser.FirstOrDefault(x => x.ID == form.PartnerUserID);

                    if (partneruser != null)
                    {


                        if (form.SubmitUpdate == "1")
                        {
                            string username = form.Username.Trim();

                            if (Db.PartnerUser.Any(x => x.Username == username && x.ID != form.PartnerUserID))
                            {
                                username = username + "_1";
                            };

                            string password = string.Empty;

                            if (form.Password.Trim() == form.Password2.Trim())
                            {
                                password = OfficeHelper.makeMD5(form.Password.Trim());

                                PartnerUser self = new PartnerUser()
                                {
                                    IsActive = partneruser.IsActive,
                                    UID = partneruser.UID,
                                    ID = partneruser.ID,
                                    Email = partneruser.Email,
                                    Username = partneruser.Username,
                                    UserFullname = partneruser.UserFullname,
                                    PartnerID = partneruser.PartnerID,
                                    Password = partneruser.Password
                                };

                                partneruser.UserFullname = form.UserFullname;
                                partneruser.Username = username;
                                partneruser.Password = password;
                                partneruser.Email = form.Email;

                                Db.SaveChanges();

                                model.Result.IsSuccess = true;
                                model.Result.Message = "Partner Kullanıcısı Güncellendi";

                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<PartnerUser>(self, partneruser, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "PartnerUser", "Update", partneruser.ID.ToString(), "Partner", "EditPartnerUser", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else
                            {
                                model.Result.IsSuccess = false;
                                model.Result.Message = "Girilen Şifreler Farklı";
                            }
                        }

                        if (form.SubmitRemove == "1")
                        {
                            partneruser.IsActive = false;
                            Db.SaveChanges();

                            model.Result.IsSuccess = false;
                            model.Result.Message = "Kullanıcı Pasife Çekildi";
                        }

                        if (form.SubmitRemove == "0")
                        {
                            partneruser.IsActive = true;
                            Db.SaveChanges();

                            model.Result.IsSuccess = false;
                            model.Result.Message = "Kullanıcı Aktif Edildi";
                        }
                    }
                    else
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Partner Kullanıcısı Bulunamadı";
                    }
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Partner Bulunamadı";
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Partner", new { id = form.PartnerUID });
        }





        [AllowAnonymous]
        public ActionResult Payments(int? PartnerID, int? LocationID, string ExpensePeriodCode)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
                filterModel.DateBegin = new DateTime(DateTime.Now.Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            IQueryable<VDocumentPartnerPayment> paymentDocuments;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || model.Filters.DateBegin != null || model.Filters.DateEnd != null || !string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
            {
                paymentDocuments = Db.VDocumentPartnerPayment.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    paymentDocuments = paymentDocuments.Where(x => x.PeriodCode == model.Filters.ExpensePeriodCode);
                }

                if (model.Filters.DateBegin != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.DocumentDate >= model.Filters.DateBegin);
                }

                if (model.Filters.DateEnd != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.DocumentDate <= model.Filters.DateEnd);
                }

                model.DocumentPartnerPayments = paymentDocuments.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PaymentFilter(int? PartnerID, int? LocationID, string ExpensePeriodCode, DateTime? DateBegin, DateTime? DateEnd)
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID ?? null;
            model.LocationID = LocationID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
            model.DateBegin = DateBegin != null ? DateBegin : new DateTime(DateTime.Now.Year, 1, 1);
            model.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Payments", "Partner");
        }

        [AllowAnonymous]
        public ActionResult NewPaymentDocument()
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPayment(PartnerPaymentFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var totalAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);
                var period = Db.ExpensePeriod.FirstOrDefault(x => x.PeriodCode == form.ExpensePeriodCode);
                var exchange = OfficeHelper.GetExchange(form.DocumentDate);
                var currency = model.Authentication.ActionEmployee.OurCompany.Currency;

                var document = new DocumentPartnerPayment();

                form.UID = Guid.NewGuid();

                document.UID = form.UID;
                document.DocumentNumber = OfficeHelper.GetDocumentNumber(model.Authentication.ActionEmployee.OurCompanyID ?? 2, "PP");
                document.RecordDate = DateTime.UtcNow.AddHours(3);
                document.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                document.RecordIP = OfficeHelper.GetIPAddress();
                document.ReferenceNumber = form.DocumentSource;
                document.Description = form.PaymentDescription;
                document.Amount = totalAmount;
                document.Currency = currency;
                document.DocumentDate = form.DocumentDate;
                document.ActionDate = period?.DateEnd;
                document.PeriodCode = form.ExpensePeriodCode;
                document.IsActive = true;
                document.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;
                document.LocationID = partnership?.LocationID;
                document.PartnerID = partnership?.PartnerID;
                document.PayMethodID = form.PayMethodID;
                document.PartnerActionTypeID = 2;
                document.ActionTypeName = "Ödeme";
                document.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                document.SystemAmount = document.Amount * document.ExchangeRate;
                document.SystemCurrency = currency;

                Db.DocumentPartnerPayment.Add(document);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Partner Ödeme Dokümanı Eklendi";

                OfficeHelper.AddApplicationLog("Office", "PaymentDocument", "Insert", document.ID.ToString(), "Partner", "AddPayment", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, document);

                // partner cari hareketi eklenir
                try
                {
                    Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("PaymentDetail", "Partner", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult PaymentDetail(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }


            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
            model.DocumentPartnerPayment = Db.VDocumentPartnerPayment.FirstOrDefault(x => x.UID == id);

            if (model.DocumentPartnerPayment == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Payments");
            }

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdatePayment(PartnerPaymentFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var totalAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);
                var period = Db.ExpensePeriod.FirstOrDefault(x => x.PeriodCode == form.ExpensePeriodCode);
                var exchange = OfficeHelper.GetExchange(form.DocumentDate);
                var currency = model.Authentication.ActionEmployee.OurCompany.Currency;

                var document = Db.DocumentPartnerPayment.FirstOrDefault(x => x.UID == form.UID && x.ID == form.DocumentPaymentID);

                if (document != null)
                {

                    DocumentPartnerPayment self = new DocumentPartnerPayment()
                    {
                        RecordDate = document.RecordDate,
                        RecordEmployeeID = document.RecordEmployeeID,
                        RecordIP = document.RecordIP,
                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        Currency = document.Currency,
                        UID = document.UID,
                        DocumentNumber = document.DocumentNumber,
                        DocumentDate = document.DocumentDate,
                        ID = document.ID,
                        UpdateEmployee = document.UpdateEmployee,
                        ActionTypeName = document.ActionTypeName,
                        Amount = document.Amount,
                        Description = document.Description,
                        ExchangeRate = document.ExchangeRate,
                        FromBankAccountID = document.FromBankAccountID,
                        LocationID = document.LocationID,
                        PartnerActionTypeID = document.PartnerActionTypeID,
                        PartnerID = document.PartnerID,
                        PayMethodID = document.PayMethodID,
                        PeriodCode = document.PeriodCode,
                        ReferenceNumber = document.ReferenceNumber,
                        SystemAmount = document.SystemAmount,
                        SystemCurrency = document.SystemCurrency,
                        UpdateDate = document.UpdateDate,
                        UpdateIP = document.UpdateIP,
                        ActionDate = document.ActionDate
                    };


                    document.UpdateDate = DateTime.UtcNow.AddHours(3);
                    document.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                    document.UpdateIP = OfficeHelper.GetIPAddress();
                    document.ReferenceNumber = form.DocumentSource;
                    document.Description = form.PaymentDescription;
                    document.Amount = totalAmount;
                    document.DocumentDate = form.DocumentDate;
                    document.ActionDate = period.DateEnd;
                    document.PeriodCode = form.ExpensePeriodCode;
                    document.IsActive = form.IsActive == "1" ? true : false;
                    document.LocationID = partnership?.LocationID;
                    document.PartnerID = partnership?.PartnerID;
                    document.PayMethodID = form.PayMethodID;
                    document.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    document.SystemAmount = document.Amount * document.ExchangeRate;
                    document.SystemCurrency = currency;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Partner Ödeme Dokümanı Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPartnerPayment>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "PaymentDocument", "Update", document.ID.ToString(), "Partner", "PaymentDetail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    // partner cari hareketi eklenir
                    try
                    {
                        Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Doküman Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("PaymentDetail", "Partner", new { id = form.UID });
        }


        //Earns
        [AllowAnonymous]
        public ActionResult Earns(int? PartnerID, int? LocationID, string ExpensePeriodCode)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
                filterModel.DateBegin = new DateTime(DateTime.Now.Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            IQueryable<VDocumentPartnerEarn> earnDocuments;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || model.Filters.DateBegin != null || model.Filters.DateEnd != null || !string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
            {
                earnDocuments = Db.VDocumentPartnerEarn.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    earnDocuments = earnDocuments.Where(x => x.PeriodCode == model.Filters.ExpensePeriodCode);
                }

                if (model.Filters.DateBegin != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.DocumentDate >= model.Filters.DateBegin);
                }

                if (model.Filters.DateEnd != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.DocumentDate <= model.Filters.DateEnd);
                }

                model.DocumentPartnerEarns = earnDocuments.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EarnsFilter(int? PartnerID, int? LocationID, string ExpensePeriodCode, DateTime? DateBegin, DateTime? DateEnd)
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID ?? null;
            model.LocationID = LocationID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
            model.DateBegin = DateBegin != null ? DateBegin : new DateTime(DateTime.Now.Year, 1, 1);
            model.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Earns", "Partner");
        }

        [AllowAnonymous]
        public ActionResult EarnDetail(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }


            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
            model.DocumentPartnerEarn = Db.VDocumentPartnerEarn.FirstOrDefault(x => x.UID == id);

            if (model.DocumentPartnerEarn == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Earns");
            }

            model.DocumentPartnerEarnRows = Db.VDocumentPartnerEarnRow.Where(x => x.DocumentID == model.DocumentPartnerEarn.ID).ToList();

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateEarn(PartnerEarnFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {


                var document = Db.DocumentPartnerEarn.FirstOrDefault(x => x.UID == form.UID && x.ID == form.DocumentEarnID);

                if (document != null)
                {

                    DocumentPartnerEarn self = new DocumentPartnerEarn()
                    {
                        RecordDate = document.RecordDate,
                        RecordEmployeeID = document.RecordEmployeeID,
                        RecordIP = document.RecordIP,
                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        Currency = document.Currency,
                        UID = document.UID,
                        DocumentNumber = document.DocumentNumber,
                        DocumentDate = document.DocumentDate,
                        ActionDate = document.ActionDate,
                        ID = document.ID,
                        UpdateEmployee = document.UpdateEmployee,
                        ActionTypeName = document.ActionTypeName,
                        Amount = document.Amount,
                        Description = document.Description,
                        ExchangeRate = document.ExchangeRate,
                        FromBankAccountID = document.FromBankAccountID,
                        LocationID = document.LocationID,
                        PartnerActionTypeID = document.PartnerActionTypeID,
                        PartnerID = document.PartnerID,
                        PeriodCode = document.PeriodCode,
                        ReferenceNumber = document.ReferenceNumber,
                        SystemAmount = document.SystemAmount,
                        SystemCurrency = document.SystemCurrency,
                        UpdateDate = document.UpdateDate,
                        UpdateIP = document.UpdateIP

                    };

                    try
                    {
                        Db.AddPartnerShipEarnDocument(document.PartnerID, document.LocationID, document.ID, document.PeriodCode, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                    document.UpdateDate = DateTime.UtcNow.AddHours(3);
                    document.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                    document.UpdateIP = OfficeHelper.GetIPAddress();

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Partner Hakediş Dokümanı Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPartnerEarn>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "EarnDocument", "Update", document.ID.ToString(), "Partner", "EarnDetail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    // partner cari hareketi eklenir
                    try
                    {
                        Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Doküman Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("EarnDetail", "Partner", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult NewEarnDocument()
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Partners = Db.Partner.Where(x => x.ID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEarn(PartnerEarnFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null && form.PartnerID != null && !string.IsNullOrEmpty(form.ExpensePeriodCode))
            {

                var partner = Db.Partner.FirstOrDefault(x => x.ID == form.PartnerID);
                var period = Db.ExpensePeriod.FirstOrDefault(x => x.PeriodCode == form.ExpensePeriodCode);

                if (partner != null && period != null)
                {
                    try
                    {
                        string ipAddress = OfficeHelper.GetIPAddress();

                        var sresult = Db.AddPartnerEarnDocument(partner.ID, period.PeriodCode, model.Authentication.ActionEmployee.EmployeeID, ipAddress).FirstOrDefault();

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Partner Hakediş Dokümanı Eklendi / Güncelleştirildi";
                        OfficeHelper.AddApplicationLog("Office", "EarnDocument", "Insert", null, "Partner", "AddEarn", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Partner Hakediş Dokümanı İşlenemedi!";
                    }
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Earns", "Partner");
        }

        //Actions
        [AllowAnonymous]
        public ActionResult Actions(int? PartnerID, int? LocationID, string PeriodCodeEnd, string PeriodCodeBegin = "2022-01")
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.PeriodCodeBegin = !string.IsNullOrEmpty(PeriodCodeBegin) ? PeriodCodeBegin : "2022-01";
                filterModel.PeriodCodeEnd = !string.IsNullOrEmpty(PeriodCodeEnd) ? PeriodCodeEnd : DateTime.Now.ToString("yyyy-MM");

                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            var beginperiod = model.ExpensePeriods.FirstOrDefault(x => x.PeriodCode == model.Filters.PeriodCodeBegin);
            var endperiod = model.ExpensePeriods.FirstOrDefault(x => x.PeriodCode == model.Filters.PeriodCodeEnd);

            if (beginperiod.DateBegin > endperiod.DateBegin)
            {
                beginperiod = endperiod;
                model.Filters.PeriodCodeBegin = beginperiod.PeriodCode;
            }

            IQueryable<VPartnerActions> oldactions;
            IQueryable<VPartnerActions> actions;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || !string.IsNullOrEmpty(model.Filters.PeriodCodeEnd) || !string.IsNullOrEmpty(model.Filters.PeriodCodeBegin))
            {
                oldactions = Db.VPartnerActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ActionDate < beginperiod.DateBegin);
                actions = Db.VPartnerActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    oldactions = oldactions.Where(x => x.PartnerID == model.Filters.PartnerID);
                    actions = actions.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    oldactions = oldactions.Where(x => x.LocationID == model.Filters.LocationID);
                    actions = actions.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.PeriodCodeBegin))
                {
                    actions = actions.Where(x => x.ActionDate >= beginperiod.DateBegin);
                }

                if (!string.IsNullOrEmpty(model.Filters.PeriodCodeEnd))
                {
                    actions = actions.Where(x => x.ActionDate <= endperiod.DateEnd);
                }

                model.OldPartnerActions = oldactions.ToList();
                model.PartnerActions = actions.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ActionFilter(int? PartnerID, int? LocationID, string PeriodCodeEnd, string PeriodCodeBegin = "2022-01")
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID;
            model.LocationID = LocationID ?? null;
            model.PeriodCodeBegin = !string.IsNullOrEmpty(PeriodCodeBegin) ? PeriodCodeBegin : string.Empty;
            model.PeriodCodeEnd = !string.IsNullOrEmpty(PeriodCodeEnd) ? PeriodCodeEnd : string.Empty;


            if (string.IsNullOrEmpty(model.PeriodCodeBegin))
            {
                model.PeriodCodeBegin = "2022-01";
            }

            if (string.IsNullOrEmpty(model.PeriodCodeEnd))
            {
                model.PeriodCodeEnd = DateTime.Now.ToString("yyyy-MM");
            }

            TempData["filter"] = model;

            return RedirectToAction("Actions", "Partner");
        }

    }
}