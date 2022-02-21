using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CardControlModel : LayoutControlModel
    {
        public IEnumerable<VProductPriceLastList> PriceList { get; set; }
        public IEnumerable<VTicketBasket> BasketList { get; set; }
        public VProductPriceLastList Price { get; set; }
        public VTicketBasket BasketItem { get; set; }
        public BasketTotal BasketTotal { get; set; }
        public int? EmployeeBasketCount { get; set; }
        public EmployeeModel EmployeeModel { get; set; }
        public VCard Card { get; set; }
        public CardReader CardReader { get; set; }
        public List<TicketSaleCreditLoad> CreditLoads { get; set; }
        public List<VCardActions> CardActions { get; set; }
        public List<CardType> CardTypes { get; set; }
        public List<DateTime> ActionDates { get; set; }
        public double CardBalance { get; set; } = 0;
        public double CardBalanceAction { get; set; } = 0;
        public TicketSaleCreditLoad CreditLoad { get; set; }
        public string CardNumber { get; set; }
        public string CardStatus { get; set; }
        public string CardTypeName { get; set; }
        public string Comment { get; set; }
        public List<CardComment> CardComments { get; set; }
        public Employee Employee { get; set; }
        public List<DataEmployee> Employees { get; set; }
        public EmployeeCard EmployeeCard { get; set; }
        public int Process { get; set; }
        public int ProcessType { get; set; }

        public List<CardReaderType> CardReaderTypes { get; set; }
        public List<LocationPart> LocationParts { get; set; }
        public List<CardReader> CardReaders { get; set; }
        public List<NFCCardLog> NFCCardLogs { get; set; }


    }
}