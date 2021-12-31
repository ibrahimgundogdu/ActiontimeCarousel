using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class CardController : BaseController
    {

        public CardController() : base()
        {

        }

        public ActionResult Index(string cardinfo)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

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
            model.Card = null;
            model.CardReader = model.CardReader = Db.CardReader.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

            if (!string.IsNullOrEmpty(cardinfo))
            {
                model.Comment = cardinfo;

                string[] cardinfolist = cardinfo.Split(';').ToArray();

                string serino = cardinfolist[0];
                string macano = cardinfolist[1];
                int process = Convert.ToInt32(cardinfolist[2]);
                model.Process = process;

                //model.CardComments = Db.AddCardComment(model.Authentication.CurrentLocation.ID, process, cardinfo, DateTime.UtcNow.AddHours(3)).ToList();

                if (process == 1) // Müşteri Kartı Okunma bölümü //00119D9B;CC:50:E3:11:9D:9B;1;4528C2F3;1;100
                {
                    string cardno = cardinfolist[3];
                    int processtype = Convert.ToInt32(cardinfolist[4]);
                    string credit = cardinfolist[5];
                    double? existscredit = (Convert.ToDouble(credit) / 100);

                    model.CardNumber = cardno;

                    if (!Db.Card.Any(x=> x.CardNumber == cardno))
                    {
                        Db.AddCard(cardno);
                    }



                    model.Card = Db.AddEditCard(cardno, existscredit).FirstOrDefault();

                    model.CardActions = Db.VCardActions.Where(x => x.CardNumber == cardno).ToList();
                    model.ActionDates = model.CardActions.Select(x => x.DateOnly.Value.Date).Distinct().OrderByDescending(x => x).ToList();
                    model.PriceList = Db.GetLocationCurrentProductPrices(model.Authentication.CurrentLocation.ID).ToList();

                    if (model.Card.CardTypeID == 1)
                    {
                        model.PriceList = model.PriceList.Where(x => x.IsBase == true).ToList();
                    }

                    model.CardStatus = model.Card.CardStatusName ?? "Bilinmiyor";

                    model.CardReader = Db.CardReader.FirstOrDefault(x => x.SerialNumber == serino && x.MACAddress == macano && x.LocationID == model.Authentication.CurrentLocation.ID && x.CardReaderTypeID == 1 && x.IsActive == true);

                    model.CardBalanceAction = 0;

                    if (Db.CardActions.Any(x => x.CardID == model.Card.ID))
                    {
                        model.CardBalanceAction = Db.CardActions.Where(x => x.CardID == model.Card.ID)?.Sum(x => x.Credit) ?? 0.0;
                    }

                    model.CardBalance = existscredit ?? 0;


                    model.BasketList = Db.GetLocationCardBasket(model.Authentication.CurrentLocation.ID, model.Card?.CardNumber).ToList();
                    model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
                    var currentBasketTotal = Db.GetLocationCardBasketTotal(model.Authentication.CurrentLocation.ID, model.Card?.CardNumber).FirstOrDefault(x => x.Money == model.Authentication.CurrentLocation.Currency);

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

                if (process == 2) // Personel Kartı Okunma bölümü //00E268A5;E8:DB:84:E2:68:A5;2;A59ED793;1;254;0
                {
                    string cardno = cardinfolist[3];
                    int processtype = Convert.ToInt32(cardinfolist[4]);
                    string setupconfig = cardinfolist[5];  // 254 personel kartı

                    model.CardNumber = cardno;

                    if (!Db.Card.Any(x => x.CardNumber == cardno))
                    {
                        Db.AddCard(cardno);
                    }

                    model.Card = Db.AddEditCard(cardno, 0).FirstOrDefault();

                    if (model.Card.CardTypeID != 2)
                    {
                        Db.SetCardType(cardno, 2);

                        model.Card = Db.VCard.FirstOrDefault(x => x.CardNumber == cardno);
                    }

                    model.EmployeeModel = null;

                    if (model.Card != null && model.Card.CardTypeID == 2 && model.Card.CardStatusID == 1)
                    {
                       // model.Card = Db.VCard.FirstOrDefault(x => x.CardNumber == cardno);

                        if (setupconfig == "254")
                        {
                            var isemployeecard = Db.EmployeeCard.Where(x => x.CardNumber == model.Card.CardNumber).OrderByDescending(x => x.RecordDate).FirstOrDefault();

                            if (isemployeecard != null)
                            {
                                PosManager manager = new PosManager();

                                var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);
                                model.Employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == isemployeecard.EmployeeID);
                                var employees = manager.GetLocationEmployeeModelsToday(location);

                                model.EmployeeModel = employees.FirstOrDefault(x => x.ID == isemployeecard.EmployeeID);
                            }
                        }
                    }

                }

                if (process == 3)
                {
                    string cardno = cardinfolist[3];

                    model.CardNumber = cardno;

                    if (!Db.Card.Any(x => x.CardNumber == cardno))
                    {
                        Db.AddCard(cardno);
                    }

                    model.Card = Db.AddEditCard(cardno, 0).FirstOrDefault();
                }
            }

            return View(model);
        }

        public PartialViewResult AddBasket(int id, int cardpriceid, string cardnumber, int cardreaderid, int process = 0)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            model.CardNumber = cardnumber;

            model.Price = Db.VProductPriceLastList.FirstOrDefault(x => x.ID == id);

            if (model.Price != null)
            {
                var added = Db.AddCardBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, id, cardpriceid, cardnumber, cardreaderid, 7, process);
            }

            model.BasketList = Db.GetLocationCardBasket(model.Authentication.CurrentLocation.ID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(model.Authentication.CurrentLocation.ID, cardnumber).FirstOrDefault(x => x.Money == model.Authentication.CurrentLocation.Currency);
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
            model.Authentication = this.AuthenticationData;

            model.CardNumber = cardnumber;

            var removed = Db.RemoveBasketItem(id);

            model.BasketList = Db.GetLocationCardBasket(model.Authentication.CurrentLocation.ID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(model.Authentication.CurrentLocation.ID, cardnumber).FirstOrDefault(x => x.Money == model.Authentication.CurrentLocation.Currency);
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
            model.Authentication = this.AuthenticationData;

            model.CardNumber = cardnumber;

            var clean = Db.CleanBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID);

            model.BasketList = Db.GetLocationCardBasket(model.Authentication.CurrentLocation.ID, cardnumber).ToList();
            model.EmployeeBasketCount = model.BasketList.Sum(x => x.Quantity);
            var currentBasketTotal = Db.GetLocationCardBasketTotal(model.Authentication.CurrentLocation.ID, cardnumber).FirstOrDefault(x => x.Money == model.Authentication.CurrentLocation.Currency);
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

        public string MakeCashCard(string cardnumber)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            string message = string.Empty;

            if (!string.IsNullOrEmpty(cardnumber))
            {
                try
                {
                    var card = Db.Card.FirstOrDefault(x => x.CardNumber == cardnumber);

                    card.CardTypeID = 1;
                    card.CardStatusID = 1;

                    Db.SaveChanges();

                    message = "Kasa Kartı Tanımlandı";
                }
                catch (Exception ex)
                {
                    message = "Kasa Kartı Tanımlanamadı!";
                }

            }

            return message;
        }

        public string CompleteEmployeeSet(string cardnumber, int? employeeid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            string message = string.Empty;

            if (!string.IsNullOrEmpty(cardnumber) && employeeid > 0)
            {
                try
                {
                    Db.AddEditEmployeeCard(employeeid, 2, cardnumber);

                    message = "Personel Kartı Tanımlandı";
                }
                catch (Exception ex)
                {
                    message = "Personel Kartı Tanımlanamadı!";
                }

            }

            return message;
        }

        public ActionResult MakePersonelCard(string comment)
        {
            //00EF6963;A4:CF:12:EF:69:63;2;A59E37EC;1;254;0;
            //00EF6963;A4:CF:12:EF:69:63;2;A59E37EC;1;254;0;

            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;
            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.CardTypes = Db.CardType.Where(x => x.IsActive == true).ToList();

            if (!string.IsNullOrEmpty(comment))
            {
                model.Comment = comment;

                string[] cardinfolist = comment.Split(';').ToArray();

                string serino = cardinfolist[0];
                string macano = cardinfolist[1];
                string process = cardinfolist[2];
                string cardno = cardinfolist[3];
                model.CardNumber = cardno;

                model.CardReader = Db.CardReader.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.SerialNumber == serino && x.MACAddress == macano);
                model.Card = Db.VCard.FirstOrDefault(x => x.CardNumber == cardno);

                model.Employees = manager.GetLocationEmployeesToday(model.Authentication.CurrentLocation.ID);
                model.EmployeeCard = Db.EmployeeCard.Where(x => x.CardNumber == cardno).OrderByDescending(x => x.RecordDate).FirstOrDefault();
            }
            return View(model);
        }

        public string EmployeeShiftStart(int locationid, int employeeid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeShiftStart(employee.EmployeeUID.ToString(), processDate, locationid, employeeid);

            return result.Message;
        }

        public string EmployeeShiftEnd(int locationid, int employeeid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeShiftEnd(employee.EmployeeUID.ToString(), processDate, locationid, employeeid);

            return result.Message;
        }

        public string EmployeeBreakStart(int locationid, int employeeid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeBreakStart(employee.EmployeeUID.ToString(), processDate, locationid, employeeid);

            return result.Message;
        }

        public string EmployeeBreakEnd(int locationid, int employeeid)
        {
            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeBreakEnd(employee.EmployeeUID.ToString(), processDate, locationid, employeeid);

            return result.Message;
        }

        [HttpPost]
        public ActionResult MakePersonelCardUpdate(string comment, string CardNumber, int? EmployeeID)
        {
            //00EF6963;A4:CF:12:EF:69:63;2;A59E37EC;1;254;0;
            //00EF6963;A4:CF:12:EF:69:63;2;A59E37EC;1;254;0;

            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;
            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }


            if (!string.IsNullOrEmpty(CardNumber) && EmployeeID > 0)
            {

                try
                {
                    Db.AddEditEmployeeCard(EmployeeID, 2, CardNumber);
                    comment = comment.Replace(";1;254;", ";7;254;");
                }
                catch (Exception ex)
                {
                    comment = comment.Replace(";1;254;", ";6;254;");

                }
               
            }
            else
            {
                comment = comment.Replace(";1;254;", ";6;254;");
            }

            return RedirectToAction("Index", new { cardinfo = comment });
        }


        //CardReader
        public ActionResult CardReader()
        {

            CardControlModel model = new CardControlModel();
            model.Authentication = this.AuthenticationData;
            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.CardTypes = Db.CardType.Where(x => x.IsActive == true).ToList();

            model.CardReaderTypes = Db.CardReaderType.Where(x => x.IsActive == true).ToList();
            var locationparts = Db.GetLocationPartList(model.Authentication.CurrentLocation.ID).ToList();

            model.LocationParts = locationparts.Select(x => new LocationPart()
            {
                LocationID = x.LocationID.Value,
                LocationTypeID = x.LocationTypeID,
                PartID = x.PartID,
                FinishDate = x.FinishDate,
                PartName = x.PartName,
                StartDate = x.StartDate
            }).ToList();

            model.CardReaders = Db.CardReader.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID).ToList();

            model.NFCCardLogs = Db.NFCCardLog.Where(x => x.Message.Contains(";86;0")).OrderByDescending(x => x.RecordDate).Take(10).ToList();

            return View(model);
        }
    }
}