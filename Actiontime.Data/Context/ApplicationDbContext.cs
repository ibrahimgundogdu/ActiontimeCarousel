using System;
using System.Collections.Generic;
using Actiontime.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.Data.Context;

public partial class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<BankAction> BankActions { get; set; }

    public virtual DbSet<BankActionType> BankActionTypes { get; set; }

    public virtual DbSet<Basket> Baskets { get; set; }

    public virtual DbSet<Cash> Cashes { get; set; }

    public virtual DbSet<CashAction> CashActions { get; set; }

    public virtual DbSet<CashActionType> CashActionTypes { get; set; }

    public virtual DbSet<CashDocument> CashDocuments { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<DatePeriod> DatePeriods { get; set; }

    public virtual DbSet<DayResult> DayResults { get; set; }

    public virtual DbSet<DayResultState> DayResultStates { get; set; }

    public virtual DbSet<DocumentNumber> DocumentNumbers { get; set; }

    public virtual DbSet<DrawerDevice> DrawerDevices { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeBreak> EmployeeBreaks { get; set; }

    public virtual DbSet<EmployeeSchedule> EmployeeSchedules { get; set; }

    public virtual DbSet<EmployeeShift> EmployeeShifts { get; set; }

    public virtual DbSet<Inspection> Inspections { get; set; }

    public virtual DbSet<InspectionCategory> InspectionCategories { get; set; }

    public virtual DbSet<InspectionItem> InspectionItems { get; set; }

    public virtual DbSet<InspectionItemImage> InspectionItemImages { get; set; }

    public virtual DbSet<InspectionRow> InspectionRows { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<LocationParameter> LocationParameters { get; set; }

    public virtual DbSet<LocationPartial> LocationPartials { get; set; }

    public virtual DbSet<LocationSchedule> LocationSchedules { get; set; }

    public virtual DbSet<LocationShift> LocationShifts { get; set; }

    public virtual DbSet<MqttTicketLog> MqttTicketLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderPosPayment> OrderPosPayments { get; set; }

    public virtual DbSet<OrderPosRefund> OrderPosRefunds { get; set; }

    public virtual DbSet<OrderRow> OrderRows { get; set; }

    public virtual DbSet<OrderRowStatus> OrderRowStatuses { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderType> OrderTypes { get; set; }

    public virtual DbSet<OurCompany> OurCompanies { get; set; }

    public virtual DbSet<OurLocation> OurLocations { get; set; }

    public virtual DbSet<PayMethod> PayMethods { get; set; }

    public virtual DbSet<PaymentTerminal> PaymentTerminals { get; set; }

    public virtual DbSet<PosTerminal> PosTerminals { get; set; }

    public virtual DbSet<PriceCategory> PriceCategories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<Qrreader> Qrreaders { get; set; }

    public virtual DbSet<QrreaderParameter> QrreaderParameters { get; set; }

    public virtual DbSet<RecordHistory> RecordHistories { get; set; }

    public virtual DbSet<SaleChannel> SaleChannels { get; set; }

    public virtual DbSet<SyncProcess> SyncProcesses { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripConfirm> TripConfirms { get; set; }

    public virtual DbSet<TripHistory> TripHistories { get; set; }

    public virtual DbSet<Vaction> Vactions { get; set; }

    public virtual DbSet<VbankAction> VbankActions { get; set; }

    public virtual DbSet<VcashAction> VcashActions { get; set; }

    public virtual DbSet<VinspectionItem> VinspectionItems { get; set; }

    public virtual DbSet<VinspectionRow> VinspectionRows { get; set; }

    public virtual DbSet<Vorder> Vorders { get; set; }

    public virtual DbSet<VorderRow> VorderRows { get; set; }

    public virtual DbSet<VorderRowSummary> VorderRowSummaries { get; set; }

    public virtual DbSet<Vtrip> Vtrips { get; set; }

    public virtual DbSet<VtripConfirm> VtripConfirms { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Turkish_CI_AS");

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.ToTable("Bank");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Eftcode).HasColumnName("EFTCode");
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.ShortName)
                .HasMaxLength(80)
                .HasComputedColumnSql("(upper([Name]))", false);
            entity.Property(e => e.SortBy)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<BankAction>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.Amount).HasComputedColumnSql("([Collection]-[Payment])", false);
            entity.Property(e => e.BankActionTypeId).HasColumnName("BankActionTypeID");
            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
        });

        modelBuilder.Entity<BankActionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_BankActionType_1");

            entity.ToTable("BankActionType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SortBy)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketBasket");

            entity.ToTable("Basket");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.MasterPrice).HasDefaultValueSql("((0))");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.PromoPrice).HasDefaultValueSql("((0))");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
        });

        modelBuilder.Entity<Cash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Cash_1");

            entity.ToTable("Cash");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CashName).HasMaxLength(50);
            entity.Property(e => e.CashTypeId).HasColumnName("CashTypeID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(4)
                .IsFixedLength();
        });

        modelBuilder.Entity<CashAction>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.Amount).HasComputedColumnSql("([Collection]-[Payment])", false);
            entity.Property(e => e.CashActionTypeId).HasColumnName("CashActionTypeID");
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.Collection).HasDefaultValueSql("((0))");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("OrderID");
            entity.Property(e => e.Payment).HasDefaultValueSql("((0))");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
        });

        modelBuilder.Entity<CashActionType>(entity =>
        {
            entity.ToTable("CashActionType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.IsMobile).HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.MobileTag).HasMaxLength(50);
            entity.Property(e => e.Module).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<CashDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProcessDocument");

            entity.ToTable("CashDocument");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CashActionTypeId).HasColumnName("CashActionTypeID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentDate).HasColumnType("date");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PayMethodId).HasColumnName("PayMethodID");
            entity.Property(e => e.PhotoFile).HasMaxLength(80);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("Currency");

            entity.Property(e => e.Code).HasMaxLength(4);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Sign).HasMaxLength(1);
        });

        modelBuilder.Entity<DatePeriod>(entity =>
        {
            entity.HasKey(e => e.Date).HasName("PK_DATE");

            entity.ToTable("DatePeriod");

            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Day).HasComputedColumnSql("(datepart(day,[Date]))", false);
            entity.Property(e => e.DayName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(80);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Month).HasComputedColumnSql("(datepart(month,[Date]))", false);
            entity.Property(e => e.MonthName).HasMaxLength(50);
            entity.Property(e => e.PeriodNumber)
                .HasMaxLength(9)
                .HasComputedColumnSql("((CONVERT([nvarchar](4),[WeekYear])+'-')+CONVERT([nvarchar](4),[WeekNumber]))", false);
            entity.Property(e => e.Quarter).HasComputedColumnSql("(datepart(quarter,[Date]))", false);
            entity.Property(e => e.Week).HasComputedColumnSql("([WeekNumber])", false);
            entity.Property(e => e.Year).HasComputedColumnSql("(datepart(year,[Date]))", false);
        });

        modelBuilder.Entity<DayResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Result");

            entity.ToTable("DayResult");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PhotoFile).HasMaxLength(80);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<DayResultState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RESULT_STATE");

            entity.ToTable("DayResultState");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StateName).HasMaxLength(50);
        });

        modelBuilder.Entity<DocumentNumber>(entity =>
        {
            entity.ToTable("DocumentNumber");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Prefix).HasMaxLength(2);
        });

        modelBuilder.Entity<DrawerDevice>(entity =>
        {
            entity.ToTable("DrawerDevice");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateRecord)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(20)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.SerialNumber).HasMaxLength(20);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.Version).HasMaxLength(20);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_EMPLOYEE");

            entity.ToTable("Employee");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CountryPhoneCode).HasMaxLength(8);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("EMail");
            entity.Property(e => e.EmployeeUid).HasColumnName("EmployeeUID");
            entity.Property(e => e.FotoFile).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Mobile2).HasMaxLength(50);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.RoleGroupId).HasColumnName("RoleGroupID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.SmsNumber)
                .HasMaxLength(4000)
                .HasComputedColumnSql("(replace(replace([Mobile],'+',''),' ',''))", false);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Whatsapp).HasMaxLength(50);
        });

        modelBuilder.Entity<EmployeeBreak>(entity =>
        {
            entity.ToTable("EmployeeBreak");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BreakDate).HasColumnType("date");
            entity.Property(e => e.BreakDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[BreakEnd]-[BreakStart]))", false);
            entity.Property(e => e.BreakEnd).HasColumnType("datetime");
            entity.Property(e => e.BreakStart).HasColumnType("datetime");
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasComputedColumnSql("((((CONVERT([varchar](5),datediff(second,[BreakStart],[BreakEnd])/(3600))+':')+CONVERT([varchar](5),(datediff(second,[BreakStart],[BreakEnd])%(3600))/(60)))+':')+CONVERT([varchar](5),datediff(second,[BreakStart],[BreakEnd])%(60)))", false);
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[BreakStart],[BreakEnd]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<EmployeeSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ScheduleRequest");

            entity.ToTable("EmployeeSchedule");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ShiftStart],[ShiftEnd]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ScheduleDate)
                .HasComputedColumnSql("(CONVERT([date],[ShiftStart]))", false)
                .HasColumnType("date");
            entity.Property(e => e.ScheduleWeek).HasMaxLength(9);
            entity.Property(e => e.ShiftDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ShiftEnd]-[ShiftStart]))", false);
            entity.Property(e => e.ShiftEnd).HasColumnType("datetime");
            entity.Property(e => e.ShiftStart).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<EmployeeShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_EMPLOYEE_SHIFT_HISTORY");

            entity.ToTable("EmployeeShift");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasComputedColumnSql("((((CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftEnd])/(3600))+':')+CONVERT([varchar](5),(datediff(second,[ShiftStart],[ShiftEnd])%(3600))/(60)))+':')+CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftEnd])%(60)))", false);
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ShiftStart],[ShiftEnd]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.ShiftDate).HasColumnType("date");
            entity.Property(e => e.ShiftDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ShiftEnd]-[ShiftStart]))", false);
            entity.Property(e => e.ShiftEnd).HasColumnType("datetime");
            entity.Property(e => e.ShiftStart).HasColumnType("datetime");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.InspectionTypeId, e.InspectorId, e.InspectionDate }).HasName("PK_InspectionHeader");

            entity.ToTable("Inspection");

            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.InspectionTypeId).HasColumnName("InspectionTypeID");
            entity.Property(e => e.InspectorId).HasColumnName("InspectorID");
            entity.Property(e => e.InspectionDate).HasColumnType("date");
            entity.Property(e => e.DateBegin).HasColumnType("datetime");
            entity.Property(e => e.DateEnd).HasColumnType("datetime");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<InspectionCategory>(entity =>
        {
            entity.ToTable("InspectionCategory");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<InspectionItem>(entity =>
        {
            entity.ToTable("InspectionItem");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AnswerType).HasComment("1 evet hayır, 2 değer girme");
            entity.Property(e => e.EstimatedAnswer).HasMaxLength(20);
            entity.Property(e => e.InspectionCatId).HasColumnName("InspectionCatID");
            entity.Property(e => e.InspectionTypeId).HasColumnName("InspectionTypeID");
            entity.Property(e => e.IsPart).HasDefaultValueSql("((0))");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ItemNameTr)
                .HasMaxLength(250)
                .HasColumnName("ItemNameTR");
            entity.Property(e => e.Number).HasMaxLength(4);
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<InspectionItemImage>(entity =>
        {
            entity.ToTable("InspectionItemImage");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ImageName).HasMaxLength(50);
            entity.Property(e => e.InspectionItemId).HasColumnName("InspectionItemID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<InspectionRow>(entity =>
        {
            entity.HasKey(e => new { e.InspectionId, e.InspectionItemId, e.InspectionCategoryId, e.LocationPartId });

            entity.ToTable("InspectionRow");

            entity.Property(e => e.InspectionId).HasColumnName("InspectionID");
            entity.Property(e => e.InspectionItemId).HasColumnName("InspectionItemID");
            entity.Property(e => e.InspectionCategoryId).HasColumnName("InspectionCategoryID");
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.Description).HasMaxLength(120);
            entity.Property(e => e.EstimatedValue).HasMaxLength(20);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.InpectionDate).HasColumnType("datetime");
            entity.Property(e => e.InspectionItemName).HasMaxLength(250);
            entity.Property(e => e.InspectionValue).HasMaxLength(20);
            entity.Property(e => e.InspectorId).HasColumnName("InspectorID");
            entity.Property(e => e.LanguageCode).HasMaxLength(2);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Currency).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasMaxLength(20);
            entity.Property(e => e.LocalDate)
                .HasComputedColumnSql("(CONVERT([date],dateadd(hour,[Timezone],getutcdate())))", false)
                .HasColumnType("date");
            entity.Property(e => e.LocalDateTime)
                .HasComputedColumnSql("(dateadd(hour,[Timezone],getutcdate()))", false)
                .HasColumnType("datetime");
            entity.Property(e => e.LocationCode).HasMaxLength(20);
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LocationTypeId).HasColumnName("LocationTypeID");
            entity.Property(e => e.LocationTypeName).HasMaxLength(50);
            entity.Property(e => e.LocationUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("LocationUID");
            entity.Property(e => e.Longitude).HasMaxLength(20);
            entity.Property(e => e.MapUrl)
                .HasMaxLength(1000)
                .HasColumnName("MapURL");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PriceCatId).HasColumnName("PriceCatID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StateId).HasColumnName("StateID");
        });

        modelBuilder.Entity<LocationParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_LOCATION_RENT");

            entity.ToTable("LocationParameter");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.DateFinish).HasColumnType("date");
            entity.Property(e => e.DateStart).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<LocationPartial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrampolinePart");

            entity.ToTable("LocationPartial");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Direction).HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.PartialId).HasColumnName("PartialID");
            entity.Property(e => e.PartialTypeId).HasColumnName("PartialTypeID");
        });

        modelBuilder.Entity<LocationSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ScheduleLocation");

            entity.ToTable("LocationSchedule");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ScheduleStart],[ScheduleEnd]))", false);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ScheduleDate)
                .HasComputedColumnSql("(CONVERT([date],[ScheduleStart]))", false)
                .HasColumnType("date");
            entity.Property(e => e.ScheduleDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ScheduleEnd]-[ScheduleStart]))", false);
            entity.Property(e => e.ScheduleEnd).HasColumnType("datetime");
            entity.Property(e => e.ScheduleStart).HasColumnType("datetime");
            entity.Property(e => e.ScheduleWeek).HasMaxLength(9);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UnitPriceMultiplier).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<LocationShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_LOCATION_SHIFT_HISTORY");

            entity.ToTable("LocationShift");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasComputedColumnSql("((((CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftFinish])/(3600))+':')+CONVERT([varchar](5),(datediff(second,[ShiftStart],[ShiftFinish])%(3600))/(60)))+':')+CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftFinish])%(60)))", false);
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ShiftStart],[ShiftFinish]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EmployeeIdfinish).HasColumnName("EmployeeIDFinish");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.ShiftDate).HasColumnType("date");
            entity.Property(e => e.ShiftDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ShiftFinish]-[ShiftStart]))", false);
            entity.Property(e => e.ShiftFinish).HasColumnType("datetime");
            entity.Property(e => e.ShiftStart).HasColumnType("datetime");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<MqttTicketLog>(entity =>
        {
            entity.ToTable("MqttTicketLog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MachineName).HasMaxLength(350);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.ResponseMessage).HasMaxLength(500);
            entity.Property(e => e.ResponseTopic).HasMaxLength(50);
            entity.Property(e => e.Topic).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sale");

            entity.ToTable("Order");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CardReaderId).HasColumnName("CardReaderID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.KioskTerminalId).HasColumnName("KioskTerminalID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderNumber).HasMaxLength(40);
            entity.Property(e => e.OrderStatusId).HasColumnName("OrderStatusID");
            entity.Property(e => e.OrderTypeId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("OrderTypeID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.PartnerUserId).HasColumnName("PartnerUserID");
            entity.Property(e => e.PaymentTerminalId).HasColumnName("PaymentTerminalID");
            entity.Property(e => e.PosStatusId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("PosStatusID");
            entity.Property(e => e.PosTerminalId).HasColumnName("PosTerminalID");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.PrintCount).HasDefaultValueSql("((0))");
            entity.Property(e => e.ReasonId)
                .HasComment("iptal veya iade durumlarında durum kodu seçmek için")
                .HasColumnName("ReasonID");
            entity.Property(e => e.ReceiptNumber).HasMaxLength(10);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SaleChannelD).HasComment("1 location on app, 2 office, 3 diğer");
            entity.Property(e => e.SendPaymentTerminal).HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.SyncDate).HasColumnType("datetime");
            entity.Property(e => e.TourGroupId).HasColumnName("TourGroupID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<OrderPosPayment>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.PaymentType, e.PaymentAmount, e.Currency, e.PaymentDate });

            entity.ToTable("OrderPosPayment");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.AuthorizationCode).HasMaxLength(20);
            entity.Property(e => e.BatchNumber).HasMaxLength(20);
            entity.Property(e => e.DocumentNumber).HasMaxLength(40);
            entity.Property(e => e.FromPosTerminal).HasDefaultValueSql("(CONVERT([bit],(1)))");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.MaskedPan).HasMaxLength(50);
            entity.Property(e => e.MerchantId)
                .HasMaxLength(20)
                .HasColumnName("MerchantID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.TerminalId)
                .HasMaxLength(20)
                .HasColumnName("TerminalID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<OrderPosRefund>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.RefundType, e.RefundAmount, e.Currency, e.RefoundDate });

            entity.ToTable("OrderPosRefund");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.RefoundDate).HasColumnType("datetime");
            entity.Property(e => e.DocumentNumber).HasMaxLength(40);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<OrderRow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SaleRows");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount).HasComputedColumnSql("(([Quantity]*(([Price]-[Discount])+[ExtraPrice]))/((1)+[TaxRate]/(100)))", false);
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DateKey)
                .HasComputedColumnSql("(CONVERT([date],[Date]))", false)
                .HasColumnType("date");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(50)
                .HasColumnName("DeviceID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymethodId).HasColumnName("PaymethodID");
            entity.Property(e => e.PrePaid).HasDefaultValueSql("((0))");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.QrreaderId).HasColumnName("QRReaderID");
            entity.Property(e => e.Quantity).HasDefaultValueSql("((1))");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RowStatusId).HasColumnName("RowStatusID");
            entity.Property(e => e.SyncDate).HasColumnType("datetime");
            entity.Property(e => e.TaxRate).HasDefaultValueSql("((8))");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TicketTripId).HasColumnName("TicketTripID");
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketTypeID");
            entity.Property(e => e.TimeKey).HasComputedColumnSql("(CONVERT([time],[Date]))", false);
            entity.Property(e => e.Total).HasComputedColumnSql("([Quantity]*(([Price]-[Discount])+[ExtraPrice]))", false);
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<OrderRowStatus>(entity =>
        {
            entity.ToTable("OrderRowStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.StatusColor).HasMaxLength(10);
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketSaleStatus");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SaleStatusName).HasMaxLength(20);
            entity.Property(e => e.SortBy)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<OrderType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SaleType");

            entity.ToTable("OrderType");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SaleTypeName).HasMaxLength(40);
            entity.Property(e => e.SortBy)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<OurCompany>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OUR_COMPANY");

            entity.ToTable("OurCompany");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AccountCode).HasMaxLength(10);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Culture).HasMaxLength(8);
            entity.Property(e => e.Currency).HasMaxLength(20);
            entity.Property(e => e.CurrencySign).HasMaxLength(1);
            entity.Property(e => e.TaxNumber).HasMaxLength(18);
            entity.Property(e => e.TaxOffice).HasMaxLength(50);
        });

        modelBuilder.Entity<OurLocation>(entity =>
        {
            entity.ToTable("OurLocation");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Currency).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LocalDate)
                .HasComputedColumnSql("(CONVERT([date],dateadd(hour,[Timezone],getutcdate())))", false)
                .HasColumnType("date");
            entity.Property(e => e.LocalDateTime)
                .HasComputedColumnSql("(dateadd(hour,[Timezone],getutcdate()))", false)
                .HasColumnType("datetime");
            entity.Property(e => e.LocationCode).HasMaxLength(20);
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LocationTypeId).HasColumnName("LocationTypeID");
            entity.Property(e => e.LocationTypeName).HasMaxLength(50);
            entity.Property(e => e.LocationUid).HasColumnName("LocationUID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PriceCatId).HasColumnName("PriceCatID");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketTypeID");
        });

        modelBuilder.Entity<PayMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Paymethod");

            entity.ToTable("PayMethod");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MethodName).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentTerminal>(entity =>
        {
            entity.ToTable("PaymentTerminal");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BrandName).HasMaxLength(20);
            entity.Property(e => e.KioskTerminalId).HasColumnName("KioskTerminalID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ModelName).HasMaxLength(20);
            entity.Property(e => e.OurCompayId).HasColumnName("OurCompayID");
            entity.Property(e => e.PosTerminalId).HasColumnName("PosTerminalID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SerialNumber).HasMaxLength(40);
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<PosTerminal>(entity =>
        {
            entity.ToTable("PosTerminal");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BrandName).HasMaxLength(20);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ModelName).HasMaxLength(20);
            entity.Property(e => e.OurCompayId).HasColumnName("OurCompayID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SerialNumber).HasMaxLength(40);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
        });

        modelBuilder.Entity<PriceCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PriceCategory_1");

            entity.ToTable("PriceCategory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryCode).HasMaxLength(8);
            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketTypeID");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AllowSale).HasComment("Satışta mı");
            entity.Property(e => e.AllowZeroStock).HasComment("Sıfır stok ile çalışılsın mı");
            entity.Property(e => e.Barcode).HasMaxLength(20);
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.IsEnvanter).HasComment("Stok Sayımı Yapılırmı");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.ProductCategoryId).HasColumnName("ProductCategoryID");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Property).HasMaxLength(40);
            entity.Property(e => e.PropertyValue).HasMaxLength(20);
            entity.Property(e => e.Sku)
                .HasMaxLength(40)
                .HasColumnName("SKU");
            entity.Property(e => e.StockCode).HasMaxLength(40);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UnitCode).HasMaxLength(10);
            entity.Property(e => e.UnitId).HasColumnName("UnitID");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.ToTable("ProductPrice");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.PriceCategoryName).HasMaxLength(50);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ProductName).HasMaxLength(150);
        });

        modelBuilder.Entity<Qrreader>(entity =>
        {
            entity.ToTable("QRReader");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(20)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.LocationTypeId).HasColumnName("LocationTypeID");
            entity.Property(e => e.Macaddress)
                .HasMaxLength(20)
                .HasColumnName("MACAddress");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PartGroupName).HasMaxLength(50);
            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.QrreaderTypeId).HasColumnName("QRReaderTypeID");
            entity.Property(e => e.SerialNumber).HasMaxLength(20);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Version).HasMaxLength(20);
        });

        modelBuilder.Entity<QrreaderParameter>(entity =>
        {
            entity.ToTable("QRReaderParameter");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.QrreaderTypeId).HasColumnName("QRReaderTypeID");
        });

        modelBuilder.Entity<RecordHistory>(entity =>
        {
            entity.HasKey(e => new { e.TableName, e.RowId, e.RowUid });

            entity.ToTable("RecordHistory");

            entity.Property(e => e.TableName).HasMaxLength(40);
            entity.Property(e => e.RowId).HasColumnName("RowID");
            entity.Property(e => e.RowUid).HasColumnName("RowUID");
            entity.Property(e => e.Crud)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CrudDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CrudIp)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("CrudIP");
            entity.Property(e => e.CrudUserFullName).HasMaxLength(100);
            entity.Property(e => e.CrudUserId).HasColumnName("CrudUserID");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
        });

        modelBuilder.Entity<SaleChannel>(entity =>
        {
            entity.ToTable("SaleChannel");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.SaleChannelName).HasMaxLength(20);
            entity.Property(e => e.SortBy)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<SyncProcess>(entity =>
        {
            entity.ToTable("SyncProcess");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateCreate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Entity).HasMaxLength(40);
            entity.Property(e => e.EntityId).HasColumnName("EntityID");
            entity.Property(e => e.EntityUid).HasColumnName("EntityUID");
            entity.Property(e => e.FilePath).HasMaxLength(250);
            entity.Property(e => e.Process).HasComment("CRUD Create Update Delete");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.ToTable("TicketType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.TicketTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketTrip");

            entity.ToTable("Trip");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ConfirmId).HasColumnName("ConfirmID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.ReaderSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TripCancel).HasColumnType("datetime");
            entity.Property(e => e.TripDate).HasColumnType("date");
            entity.Property(e => e.TripDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[TripEnd]-[TripStart]))", false);
            entity.Property(e => e.TripDurationSecond).HasComputedColumnSql("(datediff(second,[TripStart],[TripEnd]))", false);
            entity.Property(e => e.TripEnd).HasColumnType("datetime");
            entity.Property(e => e.TripStart).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<TripConfirm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketTripConfirm");

            entity.ToTable("TripConfirm");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ConfirmNumber).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConfirmTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.ReaderSerialNumber).HasMaxLength(20);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SaleOrderId).HasColumnName("SaleOrderID");
            entity.Property(e => e.SaleOrderRowId).HasColumnName("SaleOrderRowID");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<TripHistory>(entity =>
        {
            entity.ToTable("TripHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ConfirmId).HasColumnName("ConfirmID");
            entity.Property(e => e.ConfirmTime).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreaterId).HasColumnName("CreaterID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.ReaderSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.SaleOrderId).HasColumnName("SaleOrderID");
            entity.Property(e => e.SaleOrderRowId).HasColumnName("SaleOrderRowID");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TripCancel).HasColumnType("datetime");
            entity.Property(e => e.TripDate).HasColumnType("date");
            entity.Property(e => e.TripDuration).HasPrecision(0);
            entity.Property(e => e.TripEnd).HasColumnType("datetime");
            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.TripStart).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<Vaction>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VActions");

            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessName).HasMaxLength(50);
            entity.Property(e => e.ProcessType)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.SourceName).HasMaxLength(80);
        });

        modelBuilder.Entity<VbankAction>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VBankActions");

            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.BankActionTypeId).HasColumnName("BankActionTypeID");
            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.BankName).HasMaxLength(80);
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
        });

        modelBuilder.Entity<VcashAction>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VCashActions");

            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.CashActionTypeId).HasColumnName("CashActionTypeID");
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.CashName).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
        });

        modelBuilder.Entity<VinspectionItem>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VInspectionItem");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.EstimatedAnswer).HasMaxLength(20);
            entity.Property(e => e.InspectionCatId).HasColumnName("InspectionCatID");
            entity.Property(e => e.InspectionTypeId).HasColumnName("InspectionTypeID");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ItemNameTr)
                .HasMaxLength(250)
                .HasColumnName("ItemNameTR");
            entity.Property(e => e.Number).HasMaxLength(4);
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<VinspectionRow>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VInspectionRow");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(120);
            entity.Property(e => e.EstimatedValue).HasMaxLength(20);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.InpectionDate).HasColumnType("datetime");
            entity.Property(e => e.InspectionCategoryId).HasColumnName("InspectionCategoryID");
            entity.Property(e => e.InspectionId).HasColumnName("InspectionID");
            entity.Property(e => e.InspectionItemId).HasColumnName("InspectionItemID");
            entity.Property(e => e.InspectionItemName).HasMaxLength(250);
            entity.Property(e => e.InspectionValue).HasMaxLength(20);
            entity.Property(e => e.InspectorId).HasColumnName("InspectorID");
            entity.Property(e => e.LanguageCode).HasMaxLength(2);
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.Number).HasMaxLength(4);
            entity.Property(e => e.PartialId).HasColumnName("PartialID");
        });

        modelBuilder.Entity<Vorder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VOrder");

            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.EmployeeFullName).HasMaxLength(150);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderNumber).HasMaxLength(40);
            entity.Property(e => e.OrderStatusId).HasColumnName("OrderStatusID");
            entity.Property(e => e.PayMethodName).HasMaxLength(50);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(10);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SaleStatusName).HasMaxLength(20);
            entity.Property(e => e.Sign).HasMaxLength(1);
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<VorderRow>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VOrderRow");

            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DateKey).HasColumnType("date");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.MethodName).HasMaxLength(50);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.RowStatusId).HasColumnName("RowStatusID");
            entity.Property(e => e.Sign).HasMaxLength(1);
            entity.Property(e => e.StatusName).HasMaxLength(50);
            entity.Property(e => e.TicketDuration).HasMaxLength(10);
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TicketTypeName).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.TripDuration).HasMaxLength(10);
            entity.Property(e => e.Uid).HasColumnName("UID");
        });

        modelBuilder.Entity<VorderRowSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VOrderRowSummary");

            entity.Property(e => e.DateKey).HasColumnType("date");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.MethodName).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.StatusName).HasMaxLength(50);
            entity.Property(e => e.TicketTypeName).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<Vtrip>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VTrip");

            entity.Property(e => e.ConfirmId).HasColumnName("ConfirmID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.ReaderSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TripCancel).HasColumnType("datetime");
            entity.Property(e => e.TripDate).HasColumnType("date");
            entity.Property(e => e.TripDuration).HasPrecision(0);
            entity.Property(e => e.TripEnd).HasColumnType("datetime");
            entity.Property(e => e.TripStart).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<VtripConfirm>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VTripConfirm");

            entity.Property(e => e.ConfirmId).HasColumnName("ConfirmID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.LocalDateTime).HasColumnType("datetime");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.PartialId).HasColumnName("PartialID");
            entity.Property(e => e.ReaderSerialNumber).HasMaxLength(20);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.SaleOrderId).HasColumnName("SaleOrderID");
            entity.Property(e => e.SaleOrderRowId).HasColumnName("SaleOrderRowID");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TripDuration).HasPrecision(0);
            entity.Property(e => e.TripEnd).HasColumnType("datetime");
            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.TripStart).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
