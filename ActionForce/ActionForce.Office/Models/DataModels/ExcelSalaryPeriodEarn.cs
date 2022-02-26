using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExcelSalaryPeriodEarn
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? SalaryTotal { get; set; }
        public double? PermitTotal { get; set; }
        public double? ExtraShiftTotal { get; set; }
        public double? PremiumTotal { get; set; }
        public double? FormalTotal { get; set; }
        public double? OtherTotal { get; set; }

        public double? PrePaymentAmount { get; set; }
        public double? SalaryCutAmount { get; set; }
        public double? PermitPaymentAmount { get; set; }
        public double? ExtraShiftPaymentAmount { get; set; }
        public double? PremiumPaymentAmount { get; set; }
        public double? FormalPaymentAmount { get; set; }
        public double? OtherPaymentAmount { get; set; }

        public string Currency { get; set; }

    }

    public class ExcelSalaryPeriodPayment
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? BankPaymentAmount { get; set; }
        public double? ManuelPaymentAmount { get; set; }
        public double? TransferBalance { get; set; }
        public string Currency { get; set; }

    }

 public class ExcelSalaryPeriodCost
    {
        public int MaasPeriodID { get; set; }
        public int CalisanID { get; set; }
        public string AdiSoyadi { get; set; }
        public string TCKN { get; set; }
        public string Telefon { get; set; }
        public string YemekKarti { get; set; }
        public string IBAN { get; set; }
        public string Banka { get; set; }
        public short? OdemeTuru { get; set; }
        public string SGKSube { get; set; }
        public string Lokasyon { get; set; }

        public double? HakedisToplam { get; set; }
        public double? IzinToplam { get; set; }
        public double? FMesaiToplam { get; set; }
        public double? PrimToplam { get; set; }
        public double? ResmiToplam { get; set; }
        public double? DigerToplam { get; set; }
        public double? ToplamHakedis { get; set; }
        public double? AvansOdeme { get; set; }
        public double? MaasKesinti { get; set; }
        public double? IzinOdeme { get; set; }
        public double? FMesaiOdeme { get; set; }
        public double? PrimOdeme { get; set; }
        public double? ResmiOdeme { get; set; }
        public double? DigerOdeme { get; set; }
        public double? ToplamOdeme { get; set; }
        public double? ToplamBakiye { get; set; }
        public double? BankadanOdeme { get; set; }
        public double? EldenOdeme { get; set; }
        public double? DevirBakiye { get; set; }
        public double? ToplamFinal { get; set; }
        public double? YemekKartiHakedis { get; set; }
        public double? YemekKartiOdeme { get; set; }
        public double? ToplamYemekKartiBakiye { get; set; }
        public double? NetMaliyet { get; set; }
        public double? Tahakkuk { get; set; }
        public double? SSK { get; set; }
        public double? GV { get; set; }
        public double? DV { get; set; }
        public double? Kidem { get; set; }
        public double? Ihbar { get; set; }
        public double? Izin { get; set; }
        public double? ToplamMaliyet { get; set; }
        public string TesvikNo { get; set; }
        public double? TesvikIndirim { get; set; }
        public int? SSKGunSayisi { get; set; }
        public string Birim { get; set; }

    }



    public class ExcelSalaryPeriodFoodEarn
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? FoodCardTotal { get; set; }
        public string Currency { get; set; }

    }

    public class ExcelSalaryPeriodFoodPayment
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? FoodCardPaymentAmount { get; set; }
        public string Currency { get; set; }

    }

    public class ExcelParameterRent
    {
        public int LocationID { get; set; }
        public string Kodu { get; set; }
        public string Adi { get; set; }
        public string Turu { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime? Bitis { get; set; }
        public double? Tutar { get; set; }
        public string Birim { get; set; }

    }
}

