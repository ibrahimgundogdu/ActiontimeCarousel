using System;
using System.Collections.Generic;
using Actiontime.DataCloud.Entities;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.DataCloud.Context;

public partial class ApplicationCloudDbContext : DbContext
{
    public ApplicationCloudDbContext()
    {
    }

    public ApplicationCloudDbContext(DbContextOptions<ApplicationCloudDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CashAction> CashActions { get; set; }

    public virtual DbSet<DayResult> DayResults { get; set; }

    public virtual DbSet<DayResultDocument> DayResultDocuments { get; set; }

    public virtual DbSet<DocumentCashExpense> DocumentCashExpenses { get; set; }

    public virtual DbSet<DocumentExpenseSlip> DocumentExpenseSlips { get; set; }

    public virtual DbSet<DocumentSalaryPayment> DocumentSalaryPayments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeShift> EmployeeShifts { get; set; }

    public virtual DbSet<Inspection> Inspections { get; set; }

    public virtual DbSet<InspectionRow> InspectionRows { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Location1> Locations1 { get; set; }

    public virtual DbSet<LocationPartTrip> LocationPartTrips { get; set; }

    public virtual DbSet<LocationShift> LocationShifts { get; set; }

    public virtual DbSet<TicketSale> TicketSales { get; set; }

    public virtual DbSet<TicketSalePosPayment> TicketSalePosPayments { get; set; }

    public virtual DbSet<TicketSaleRow> TicketSaleRows { get; set; }

    public virtual DbSet<TicketTrip> TicketTrips { get; set; }

    public virtual DbSet<TicketTripConfirm> TicketTripConfirms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=144.126.132.166;Initial Catalog=ActionTimeDb; Persist Security Info=True; User ID=actiontime;Password=7C242B8A6C464D8FB8F553FDA850D24D!;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Turkish_CI_AS");

        modelBuilder.Entity<CashAction>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentNumber, e.SaleId, e.CashId, e.LocationId, e.ActionDate, e.ProcessDate }, "NonClusteredIndex-20211017-185141");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.Amount).HasComputedColumnSql("([Collection]-[Payment])", false);
            entity.Property(e => e.CashActionTypeId).HasColumnName("CashActionTypeID");
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.Collection).HasDefaultValueSql("((0))");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentNumber).HasMaxLength(50);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Payment).HasDefaultValueSql("((0))");
            entity.Property(e => e.ProcessDate).HasColumnType("date");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.ProcessName).HasMaxLength(50);
            entity.Property(e => e.ProcessUid).HasColumnName("ProcessUID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.SaleId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("SaleID");
            entity.Property(e => e.TicketSalePosPaymentId).HasColumnName("TicketSalePosPaymentID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
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
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<DayResultDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ResultDocuments");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentTypeID");
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.FileName)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.FilePath)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RecordIP");
            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<DocumentCashExpense>(entity =>
        {
            entity.ToTable("DocumentCashExpense");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.ActionTypeName).HasMaxLength(80);
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.DocumentNumber).HasMaxLength(16);
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.ExpenseTypeId).HasColumnName("ExpenseTypeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.ReferenceTableModel).HasMaxLength(50);
            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.SlipDate).HasColumnType("datetime");
            entity.Property(e => e.SlipDocument).HasMaxLength(80);
            entity.Property(e => e.SlipNumber).HasMaxLength(20);
            entity.Property(e => e.SlipPath).HasMaxLength(50);
            entity.Property(e => e.SystemCurrency).HasMaxLength(4);
            entity.Property(e => e.ToBankAccountId).HasColumnName("ToBankAccountID");
            entity.Property(e => e.ToCustomerId).HasColumnName("ToCustomerID");
            entity.Property(e => e.ToEmployeeId).HasColumnName("ToEmployeeID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<DocumentExpenseSlip>(entity =>
        {
            entity.ToTable("DocumentExpenseSlip");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.ActionTypeName).HasMaxLength(80);
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.CustomerAddress).HasMaxLength(350);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.DocumentDate).HasColumnType("date");
            entity.Property(e => e.DocumentNumber).HasMaxLength(16);
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PayMethodId).HasColumnName("PayMethodID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.SaleId).HasColumnName("SaleID");
            entity.Property(e => e.SaleRowId).HasColumnName("SaleRowID");
            entity.Property(e => e.SystemCurrency).HasMaxLength(4);
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<DocumentSalaryPayment>(entity =>
        {
            entity.ToTable("DocumentSalaryPayment");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.ActionTypeName).HasMaxLength(80);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.DocumentFile).HasMaxLength(50);
            entity.Property(e => e.DocumentNumber).HasMaxLength(16);
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.FromBankAccountId).HasColumnName("FromBankAccountID");
            entity.Property(e => e.FromCashId).HasColumnName("FromCashID");
            entity.Property(e => e.IsLumpSum).HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.SalaryTypeId).HasColumnName("SalaryTypeID");
            entity.Property(e => e.SystemCurrency).HasMaxLength(4);
            entity.Property(e => e.ToEmployeeId).HasColumnName("ToEmployeeID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK_EMPLOYEE");

            entity.ToTable("Employee");

            entity.HasIndex(e => new { e.EmployeeId, e.IdentityNumber, e.Email, e.Mobile, e.Mobile2, e.Whatsapp, e.Username, e.Password, e.FotoFile, e.RoleId, e.OurCompanyId, e.IsTemp, e.IsActive, e.IsDismissal, e.RecordEmployeeId, e.StatusId }, "NonClusteredIndex-20190409-171426");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Address).HasMaxLength(350);
            entity.Property(e => e.AreaCategoryId).HasColumnName("AreaCategoryID");
            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.CountryPhoneCode).HasMaxLength(8);
            entity.Property(e => e.DateEnd).HasColumnType("datetime");
            entity.Property(e => e.DateStart).HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("EMail");
            entity.Property(e => e.EmployeeUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("EmployeeUID");
            entity.Property(e => e.FoodCardNumber).HasMaxLength(30);
            entity.Property(e => e.FotoFile).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.FullNameSearch)
                .HasMaxLength(4000)
                .HasComputedColumnSql("(upper(replace(replace(replace(replace(replace(replace(isnull([FullName],''),'İ','I'),'Ç','C'),'Ü','U'),'Ö','O'),'Ğ','G'),'Ş','S')))", false);
            entity.Property(e => e.Iban)
                .HasMaxLength(30)
                .HasColumnName("IBAN");
            entity.Property(e => e.IdentityNumber).HasMaxLength(50);
            entity.Property(e => e.IdentityType).HasMaxLength(16);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Mobile2).HasMaxLength(50);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PositionId).HasColumnName("PositionID");
            entity.Property(e => e.PostCode).HasMaxLength(10);
            entity.Property(e => e.RecordDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.RoleGroupId).HasColumnName("RoleGroupID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.SalaryCategoryId).HasColumnName("SalaryCategoryID");
            entity.Property(e => e.SalaryPaymentTypeId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("SalaryPaymentTypeID");
            entity.Property(e => e.SequenceId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("SequenceID");
            entity.Property(e => e.Sgkbranch)
                .HasMaxLength(40)
                .HasColumnName("SGKBranch");
            entity.Property(e => e.ShiftTypeId).HasColumnName("ShiftTypeID");
            entity.Property(e => e.SmsNumber)
                .HasMaxLength(4000)
                .HasComputedColumnSql("(replace(replace([Mobile],'+',''),' ',''))", false);
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("StatusID");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Whatsapp).HasMaxLength(50);
        });

        modelBuilder.Entity<EmployeeShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_EMPLOYEE_SHIFT_HISTORY");

            entity.ToTable("EmployeeShift");

            entity.HasIndex(e => new { e.Id, e.EmployeeId, e.LocationId, e.ShiftDate }, "NonClusteredIndex-20171119-163621");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BreakDateEnd).HasColumnType("datetime");
            entity.Property(e => e.BreakDateStart).HasColumnType("datetime");
            entity.Property(e => e.BreakDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),(CONVERT([datetime],[ShiftDate])+CONVERT([datetime],[BreakEnd]))-(CONVERT([datetime],[ShiftDate])+CONVERT([datetime],[BreakStart]))))", false);
            entity.Property(e => e.BreakDurationMinute).HasComputedColumnSql("(datediff(minute,[BreakDateStart],[BreakDateEnd]))", false);
            entity.Property(e => e.BreakEnd).HasPrecision(0);
            entity.Property(e => e.BreakStart).HasPrecision(0);
            entity.Property(e => e.BreakTypeId).HasColumnName("BreakTypeID");
            entity.Property(e => e.CloseEnvironmentId).HasColumnName("CloseEnvironmentID");
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasComputedColumnSql("((((CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftEnd])/(3600))+':')+CONVERT([varchar](5),(datediff(second,[ShiftStart],[ShiftEnd])%(3600))/(60)))+':')+CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftEnd])%(60)))", false);
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ShiftDateStart],[ShiftDateEnd]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.ShiftDate).HasColumnType("date");
            entity.Property(e => e.ShiftDateEnd).HasColumnType("datetime");
            entity.Property(e => e.ShiftDateStart).HasColumnType("datetime");
            entity.Property(e => e.ShiftDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ShiftDateEnd]-[ShiftDateStart]))", false);
            entity.Property(e => e.ShiftEnd).HasPrecision(0);
            entity.Property(e => e.ShiftStart).HasPrecision(0);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
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

            entity.HasIndex(e => new { e.LocationId, e.LocationTypeId, e.OurCompanyId, e.SortBy }, "NonClusteredIndex-20171119-163748");

            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Address).HasMaxLength(120);
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Currency).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Distance).HasMaxLength(20);
            entity.Property(e => e.EnforcedWarning).HasColumnType("ntext");
            entity.Property(e => e.ExpenseCenter)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(1)))");
            entity.Property(e => e.ImageFile).HasMaxLength(50);
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasColumnName("IP");
            entity.Property(e => e.Latitude).HasMaxLength(20);
            entity.Property(e => e.LocalDate)
                .HasComputedColumnSql("(CONVERT([date],dateadd(hour,[Timezone],getutcdate())))", false)
                .HasColumnType("date");
            entity.Property(e => e.LocalDateTime)
                .HasComputedColumnSql("(dateadd(hour,[Timezone],getutcdate()))", false)
                .HasColumnType("datetime");
            entity.Property(e => e.LocationCode).HasMaxLength(20);
            entity.Property(e => e.LocationFullName)
                .HasMaxLength(613)
                .HasComputedColumnSql("((((((rtrim([SORTBY])+' ')+isnull([LocationName],''))+' ')+isnull([Description],''))+' ')+isnull([State],''))", false);
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LocationNameSearch)
                .HasMaxLength(4000)
                .HasComputedColumnSql("(upper(replace(replace(replace(replace(replace(replace([LocationName],'İ','I'),'Ç','C'),'Ü','U'),'Ö','O'),'Ğ','G'),'Ş','S')))", false);
            entity.Property(e => e.LocationTypeId).HasColumnName("LocationTypeID");
            entity.Property(e => e.LocationUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("LocationUID");
            entity.Property(e => e.Longitude).HasMaxLength(20);
            entity.Property(e => e.MallId).HasColumnName("MallID");
            entity.Property(e => e.MapUrl)
                .HasMaxLength(1000)
                .HasColumnName("MapURL");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PosaccountId).HasColumnName("POSAccountID");
            entity.Property(e => e.PriceCatId).HasColumnName("PriceCatID");
            entity.Property(e => e.ProductPriceCatId).HasColumnName("ProductPriceCatID");
            entity.Property(e => e.ProfitCenter)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(1)))");
            entity.Property(e => e.RecordDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(15)
                .HasColumnName("RecordIP");
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(15)
                .HasColumnName("UpdateIP");
            entity.Property(e => e.UseCardSysteme).HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.Weight).HasMaxLength(20);
        });

        modelBuilder.Entity<Location1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Location", "Office");

            entity.Property(e => e.Currency).HasMaxLength(20);
            entity.Property(e => e.DateId).HasColumnName("DateID");
            entity.Property(e => e.DateKey).HasColumnType("date");
            entity.Property(e => e.DayName).HasMaxLength(50);
            entity.Property(e => e.DayNameTr)
                .HasMaxLength(50)
                .HasColumnName("DayNameTR");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Distance).HasMaxLength(20);
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.EnforcedWarning).HasColumnType("ntext");
            entity.Property(e => e.ImageFile).HasMaxLength(50);
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasColumnName("IP");
            entity.Property(e => e.Latitude).HasMaxLength(20);
            entity.Property(e => e.LocalDate).HasColumnType("date");
            entity.Property(e => e.LocalDateTime).HasColumnType("datetime");
            entity.Property(e => e.LocationCode).HasMaxLength(20);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LocationNameSearch).HasMaxLength(4000);
            entity.Property(e => e.LocationTypeId).HasColumnName("LocationTypeID");
            entity.Property(e => e.LocationUid).HasColumnName("LocationUID");
            entity.Property(e => e.Longitude).HasMaxLength(20);
            entity.Property(e => e.MapUrl)
                .HasMaxLength(1000)
                .HasColumnName("MapURL");
            entity.Property(e => e.MonthName).HasMaxLength(50);
            entity.Property(e => e.MonthNameTr)
                .HasMaxLength(50)
                .HasColumnName("MonthNameTR");
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PriceCatId).HasColumnName("PriceCatID");
            entity.Property(e => e.ShiftDate).HasColumnType("date");
            entity.Property(e => e.ShiftFinish).HasPrecision(0);
            entity.Property(e => e.ShiftStart).HasPrecision(0);
            entity.Property(e => e.SortBy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Weight).HasMaxLength(20);
        });

        modelBuilder.Entity<LocationPartTrip>(entity =>
        {
            entity.ToTable("LocationPartTrip");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.EmployeeName).HasMaxLength(150);
            entity.Property(e => e.LocalTime)
                .HasComputedColumnSql("(dateadd(hour,[TimeZone],getutcdate()))", false)
                .HasColumnType("datetime");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TripEnd)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TripStart)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UnitDuration).HasDefaultValueSql("((180))");
        });

        modelBuilder.Entity<LocationShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_LOCATION_SHIFT_HISTORY");

            entity.ToTable("LocationShift");

            entity.HasIndex(e => new { e.LocationId, e.ShiftDate }, "NonClusteredIndex-20200117-190257");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CloseEnvironmentId).HasColumnName("CloseEnvironmentID");
            entity.Property(e => e.Duration)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasComputedColumnSql("((((CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftFinish])/(3600))+':')+CONVERT([varchar](5),(datediff(second,[ShiftStart],[ShiftFinish])%(3600))/(60)))+':')+CONVERT([varchar](5),datediff(second,[ShiftStart],[ShiftFinish])%(60)))", false);
            entity.Property(e => e.DurationMinute).HasComputedColumnSql("(datediff(minute,[ShiftDateStart],[ShiftDateFinish]))", false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EmployeeIdfinish).HasColumnName("EmployeeIDFinish");
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.ShiftDate).HasColumnType("date");
            entity.Property(e => e.ShiftDateFinish).HasColumnType("datetime");
            entity.Property(e => e.ShiftDateStart).HasColumnType("datetime");
            entity.Property(e => e.ShiftDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[ShiftDateFinish]-[ShiftDateStart]))", false);
            entity.Property(e => e.ShiftFinish).HasPrecision(0);
            entity.Property(e => e.ShiftStart).HasPrecision(0);
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<TicketSale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sale");

            entity.ToTable("TicketSale");

            entity.HasIndex(e => e.OrderNumber, "NonClusteredIndex-20181125-143213");

            entity.HasIndex(e => e.CardNumber, "NonClusteredIndex-20220203-023150");

            entity.HasIndex(e => new { e.Date, e.LocationId, e.CardNumber }, "NonClusteredIndex-20220203-030151");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CardReaderId).HasColumnName("CardReaderID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.CustomerData).HasMaxLength(500);
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EnvironmentId).HasColumnName("EnvironmentID");
            entity.Property(e => e.IdentityCard).HasMaxLength(50);
            entity.Property(e => e.IsSendPosTerminal).HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.LocalOrderId).HasColumnName("LocalOrderID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.OurCompanyId).HasColumnName("OurCompanyID");
            entity.Property(e => e.PaymethodId).HasColumnName("PaymethodID");
            entity.Property(e => e.PosRegistryNumber).HasMaxLength(20);
            entity.Property(e => e.PosStatusId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("PosStatusID");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.ReasonId)
                .HasComment("iptal veya iade durumlarında durum kodu seçmek için")
                .HasColumnName("ReasonID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.RecordIp)
                .HasMaxLength(20)
                .HasColumnName("RecordIP");
            entity.Property(e => e.SaleChannelD).HasComment("1 location on app, 2 office, 3 diğer");
            entity.Property(e => e.SaleTypeId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("SaleTypeID");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("StatusID");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
            entity.Property(e => e.UpdateIp)
                .HasMaxLength(20)
                .HasColumnName("UpdateIP");
        });

        modelBuilder.Entity<TicketSalePosPayment>(entity =>
        {
            entity.HasKey(e => new { e.SaleId, e.PaymentType, e.PaymentAmount, e.PaymentCurrency, e.PaymentDate }).HasName("PK_TicketSalePosPayment_1");

            entity.ToTable("TicketSalePosPayment");

            entity.HasIndex(e => new { e.SaleId, e.PaymentType, e.PaymentAmount, e.PaymentDateTime }, "NonClusteredIndex-20210930-193141");

            entity.HasIndex(e => e.SaleId, "NonClusteredIndex-20210930-193525");

            entity.Property(e => e.SaleId).HasColumnName("SaleID");
            entity.Property(e => e.PaymentDate).HasColumnType("date");
            entity.Property(e => e.AuthorizationCode).HasMaxLength(20);
            entity.Property(e => e.BankBkmid).HasColumnName("BankBKMID");
            entity.Property(e => e.BatchNumber).HasMaxLength(20);
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasComputedColumnSql("(case when [PaymentCurrency]='949' then 'TRL' else 'USD' end)", false);
            entity.Property(e => e.FromPosTerminal).HasDefaultValueSql("(CONVERT([bit],(1)))");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.MaskedPan).HasMaxLength(50);
            entity.Property(e => e.MerchantId)
                .HasMaxLength(20)
                .HasColumnName("MerchantID");
            entity.Property(e => e.PaymentDateTime).HasMaxLength(30);
            entity.Property(e => e.PaymentDesc).HasMaxLength(100);
            entity.Property(e => e.PaymentInfo).HasMaxLength(150);
            entity.Property(e => e.PaymentSubType).HasMaxLength(20);
            entity.Property(e => e.PaymentTime).HasPrecision(0);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(40);
            entity.Property(e => e.StanNumber).HasMaxLength(20);
            entity.Property(e => e.TerminalId)
                .HasMaxLength(20)
                .HasColumnName("TerminalID");
        });

        modelBuilder.Entity<TicketSaleRow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SaleRows");

            entity.HasIndex(e => e.TicketNumber, "NonClusteredIndex-20181124-194738");

            entity.HasIndex(e => new { e.SaleId, e.DateKey, e.TimeKey, e.LocationId, e.ProductId, e.TicketProductId }, "NonClusteredIndex-20220220-021529");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AnimalCostumeTypeId).HasColumnName("AnimalCostumeTypeID");
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CardReaderId).HasColumnName("CardReaderID");
            entity.Property(e => e.Currency).HasMaxLength(4);
            entity.Property(e => e.CustomerData).HasMaxLength(500);
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DateKey)
                .HasComputedColumnSql("(CONVERT([date],[Date]))", false)
                .HasColumnType("date");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(50)
                .HasColumnName("DeviceID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocalRowId).HasColumnName("LocalRowID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.MallMotoColorId).HasColumnName("MallMotoColorID");
            entity.Property(e => e.MasterCredit).HasDefaultValueSql("((0))");
            entity.Property(e => e.ParentId)
                .HasDefaultValueSql("((0))")
                .HasColumnName("ParentID");
            entity.Property(e => e.PaymethodId).HasColumnName("PaymethodID");
            entity.Property(e => e.PrePaid).HasDefaultValueSql("((0))");
            entity.Property(e => e.PriceCategoryId).HasColumnName("PriceCategoryID");
            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.PromoCredit).HasDefaultValueSql("((0))");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.Quantity).HasDefaultValueSql("((1))");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.SaleId).HasColumnName("SaleID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TaxRate).HasDefaultValueSql("((8))");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TicketProductId).HasColumnName("TicketProductID");
            entity.Property(e => e.TicketTripId).HasColumnName("TicketTripID");
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketTypeID");
            entity.Property(e => e.TimeKey).HasComputedColumnSql("(CONVERT([time],[Date]))", false);
            entity.Property(e => e.Total).HasComputedColumnSql("([Quantity]*(([Price]-[Discount])+[ExtraPrice]))", false);
            entity.Property(e => e.TotalCredit).HasComputedColumnSql("(isnull([MasterCredit],(0))+isnull([PromoCredit],(0)))", false);
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateEmployeeId).HasColumnName("UpdateEmployeeID");
        });

        modelBuilder.Entity<TicketTrip>(entity =>
        {
            entity.ToTable("TicketTrip");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AnimalId).HasColumnName("AnimalID");
            entity.Property(e => e.ConfirmId).HasColumnName("ConfirmID");
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.IdentityCard).HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.PartNumber).HasMaxLength(50);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.RecordEmployeeId).HasColumnName("RecordEmployeeID");
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TicketTypeId).HasColumnName("TicketTypeID");
            entity.Property(e => e.TripCancel).HasColumnType("datetime");
            entity.Property(e => e.TripDate).HasColumnType("date");
            entity.Property(e => e.TripDuration)
                .HasPrecision(0)
                .HasComputedColumnSql("(CONVERT([time](0),[TripEnd]-[TripStart]))", false);
            entity.Property(e => e.TripDurationSn).HasComputedColumnSql("(datediff(second,[TripStart],[TripEnd]))", false);
            entity.Property(e => e.TripEnd).HasColumnType("datetime");
            entity.Property(e => e.TripStart).HasColumnType("datetime");
        });

        modelBuilder.Entity<TicketTripConfirm>(entity =>
        {
            entity.ToTable("TicketTripConfirm");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ConfirmNumber).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConfirmTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationPartId).HasColumnName("LocationPartID");
            entity.Property(e => e.QrreaderId).HasColumnName("QRReaderID");
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.TicketSaleId).HasColumnName("TicketSaleID");
            entity.Property(e => e.TicketSaleRowId).HasColumnName("TicketSaleRowID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
