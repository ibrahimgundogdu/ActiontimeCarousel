using ActionForce.CardService.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class ServiceHelper
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["UfeConnectionString"].ConnectionString;
        }

        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public Result AddCardAction(CardReadModel model)
        {
            Result result = new Result();
            DateTime now = DateTime.UtcNow.AddHours(3);

            result.IsSuccess = false;
            result.Message = string.Empty;

            try
            {

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    // cihaz tanımlı mı ona bakılır değilse eklenir

                    var crParameters = new { Serial = model.SerialNumber, MAC = model.MACAddress };
                    var crSql = "SELECT TOP (1) * FROM [dbo].[CardReader] Where [SerialNumber] = @Serial and [MACAddress] = @MAC";
                    var cardReader = connection.QueryFirstOrDefault<CardReader>(crSql, crParameters);

                    if (cardReader == null)
                    {
                        var guid = Guid.NewGuid();
                        var iparameters = new
                        {
                            OurCompanyID = 2,
                            LocationID = 0,
                            LocationTypeID = 0,
                            LocationPartID = 0,
                            PartName = "Tanımsız",
                            PartGroupName = "Tanımsız",
                            SerialNumber = model.SerialNumber,
                            MACAddress = model.MACAddress,
                            CardReaderTypeID = 2,
                            UID = guid,
                            StartDate = now.Date,
                            IsActive = true
                        };
                        var isql = "INSERT INTO [dbo].[CardReader] ([OurCompanyID],[LocationID],[LocationTypeID],[LocationPartID],[PartName],[PartGroupName],[SerialNumber],[MACAddress],[CardReaderTypeID],[UID],[StartDate],[IsActive]) VALUES(@OurCompanyID, @LocationID, @LocationTypeID, @LocationPartID,@PartName,@PartGroupName, @SerialNumber, @MACAddress, @CardReaderTypeID, @UID, @StartDate, @IsActive)";
                        connection.Execute(isql, iparameters);
                        cardReader = GetCardReader(guid);
                    }

                    // kart tanımlı mı bakılır değilse eklenir


                    var cParameters = new { Number = model.CardNumber, TypeId = model.CardType };
                    var cSql = "SELECT TOP (1) * FROM [dbo].[Card] Where [CardNumber] = @Number";
                    var card = connection.QueryFirstOrDefault<Card>(cSql, cParameters);

                    if (card == null)
                    {
                        var cguid = Guid.NewGuid();
                        var cparameters = new
                        {
                            OurCompanyID = 2,
                            CardTypeID = model.CardType,
                            CardNumber = model.CardNumber,
                            ExpireDate = DateTime.Now.AddYears(99),
                            Credit = 0,
                            Currency = "TRL",
                            CardStatusID = 0,
                            RecordDate = now,
                            UID = cguid
                        };
                        var csql = "INSERT INTO [dbo].[Card] ([OurCompanyID],[CardTypeID],[CardNumber],[ExpireDate],[Credit],[Currency],[CardStatusID],[RecordDate],[ActivateDate],[UID]) VALUES( @OurCompanyID, @CardTypeID, @CardNumber, @ExpireDate, @Credit, @Currency, @CardStatusID, @RecordDate, null, @UID)";
                        connection.Execute(csql, cparameters);

                        card = GetCard(model.CardNumber);
                    }


                    // minderde personel tanımlı mı ona bakılır


                    var eParameters = new { LocationID = cardReader.LocationID, LocationPartID = cardReader.LocationPartID };
                    var eSql = "SELECT TOP (1) [EmployeeID] FROM [dbo].[LocationPartEmployee] WHERE [LocationID] = @LocationID and [LocationPartID] = @LocationPartID Order By [ReadDate] desc";
                    int EmployeeID = connection.QueryFirstOrDefault<int>(eSql, eParameters);


                    // kart okuma hereketi eklenir

                    var criParameters = new
                    {
                        Serial = model.SerialNumber,
                        MAC = model.MACAddress,
                        ProcessType = model.ProcessType,
                        ProcessNumber = model.ProcessNumber,
                        CardType = model.CardType,
                        CardNumber = model.CardNumber,
                        ProcessDate = model.CurrentDate,
                        RideAmount = model.RidePrice,
                        CardBalanceAmount = model.CardBlance,
                        EmployeeID = EmployeeID,
                        CardTypeID = card.CardTypeID
                    };
                    var criSql = "SELECT TOP (1) * FROM [dbo].[CardReaderAction] Where [SerialNumber] = @Serial and [MACAddress] = @MAC and [ProcessType] = @ProcessType and [ProcessNumber] = @ProcessNumber and [CardType] = @CardType and  [CardNumber] = @CardNumber and [ProcessDate] = @ProcessDate and [RideAmount] = @RideAmount and [CardBalanceAmount] = @CardBalanceAmount";
                    var cardReaderAction = connection.QueryFirstOrDefault<CardReaderAction>(criSql, criParameters);

                    if (cardReaderAction == null)
                    {
                        var craguid = Guid.NewGuid();
                        var craparameters = new
                        {
                            CardReaderID = cardReader.ID,
                            CardReaderUID = cardReader.UID,
                            SerialNumber = model.SerialNumber,
                            MACAddress = model.MACAddress,
                            ProcessType = model.ProcessType,
                            ProcessNumber = model.ProcessNumber,
                            CardType = model.CardType,
                            CardNumber = model.CardNumber,
                            ProcessDate = model.CurrentDate,
                            RideAmount = model.RidePrice,
                            CardBalanceAmount = model.CardBlance,
                            LocationID = cardReader.LocationID,
                            UID = craguid,
                            RecordDate = now,
                            EmployeeID = EmployeeID,
                            CardTypeID = card.CardTypeID

                        };
                        var crasql = "  INSERT INTO [dbo].[CardReaderAction] ([CardReaderID] ,[CardReaderUID],[SerialNumber],[MACAddress],[ProcessType],[ProcessNumber],[CardType],[CardNumber],[ProcessDate],[RideAmount],[CardBalanceAmount],[LocationID],[UID],[RecordDate],[EmployeeID],[CardTypeID]) VALUES(@CardReaderID,@CardReaderUID,@SerialNumber,@MACAddress,@ProcessType,@ProcessNumber,@CardType, @CardNumber, @ProcessDate, @RideAmount, @CardBalanceAmount , @LocationID, @UID, @RecordDate, @EmployeeID, @CardTypeID )";
                        var isinsert = connection.Execute(crasql, craparameters);

                        cardReaderAction = GetCardReaderAction(craguid);
                    }




                    // kart hereketi eklenir

                    var caParameters = new { OurCompanyID = 2, CardID = card.ID, LocationID = cardReader.LocationID, ActionTypeID = 2, ActionDate = model.ProcessDate, CreditSpend = model.RidePrice };
                    var caSql = "SELECT TOP (1) * FROM [dbo].[CardActions] Where [OurCompanyID] = @OurCompanyID and [CardID] = @CardID and [LocationID] = @LocationID and [ActionTypeID] = @ActionTypeID and [ActionDate] = @ActionDate and [CreditSpend] = @CreditSpend";
                    var cardAction = connection.QueryFirstOrDefault<CardAction>(caSql, caParameters);

                    if (cardAction == null)
                    {
                        var caguid = Guid.NewGuid();
                        var caparameters = new
                        {
                            OurCompanyID = card.OurCompanyID,
                            CardID = card.ID,
                            LocationID = cardReader.LocationID,
                            CustomerID = 2,
                            ProcessID = cardReaderAction.ID,
                            ProcessUID = cardReaderAction.UID,
                            ActionTypeID = 2,
                            ActionTypeName = "Kredi Kullanım",
                            ActionDate = model.CurrentDate,
                            CreditCharge = 0,
                            CreditSpend = model.RidePrice,
                            Currency = "TRL",
                            CardNumber = model.CardNumber,
                            CardReaderID = cardReader.ID,
                            EmployeeID = EmployeeID,
                            CardTypeID = card.CardTypeID
                        };

                        var casql = "  INSERT INTO [dbo].[CardActions] ([OurCompanyID],[CardID],[LocationID],[CustomerID],[ProcessID],[ProcessUID],[ActionTypeID],[ActionTypeName],[ActionDate],[CreditCharge],[CreditSpend],[Currency],[CardNumber],[CardReaderID],[EmployeeID],[CardTypeID]) VALUES(@OurCompanyID,@CardID,@LocationID,@CustomerID,@ProcessID,@ProcessUID,@ActionTypeID, @ActionTypeName, @ActionDate, @CreditCharge, @CreditSpend , @Currency, @CardNumber,@CardReaderID, @EmployeeID, @CardTypeID )";
                        connection.Execute(casql, caparameters);
                    }

                    // kartın bakiyesi güncellenir.
                    SetCardBalance(model.CardNumber, model.CardBlance);

                    //var cuparameters = new { CardNumber = model.CardNumber, Balance = model.CardBlance };
                    //var cusql = "Update [dbo].[Card] Set [Credit] = @Balance Where [CardNumber] = @CardNumber";
                    //connection.Execute(cusql, cuparameters);

                }

                result.IsSuccess = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Message = "ERROR";
            }

            return result;
        }


        public Result AddPersonalCardAction(CardReadPersonalModel model)
        {
            Result result = new Result();
            DateTime now = DateTime.UtcNow.AddHours(3);

            result.IsSuccess = false;
            result.Message = string.Empty;

            try
            {

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    // cihaz tanımlı mı ona bakılır değilse eklenir

                    var crParameters = new { Serial = model.SerialNumber, MAC = model.MACAddress };
                    var crSql = "SELECT TOP (1) * FROM [dbo].[CardReader] Where [SerialNumber] = @Serial and [MACAddress] = @MAC and [IsActive] = 1";
                    var cardReader = connection.QueryFirstOrDefault<CardReader>(crSql, crParameters);

                    if (cardReader == null)
                    {
                        var guid = Guid.NewGuid();
                        var iparameters = new
                        {
                            OurCompanyID = 2,
                            LocationID = 0,
                            LocationTypeID = 0,
                            LocationPartID = 0,
                            PartName = "Tanımsız",
                            PartGroupName = "Tanımsız",
                            SerialNumber = model.SerialNumber,
                            MACAddress = model.MACAddress,
                            CardReaderTypeID = 2,
                            UID = guid,
                            StartDate = now.Date,
                            IsActive = true
                        };

                        var isql = "INSERT INTO [dbo].[CardReader] ([OurCompanyID],[LocationID],[LocationTypeID],[LocationPartID],[PartName],[PartGroupName],[SerialNumber],[MACAddress],[CardReaderTypeID],[UID],[StartDate],[IsActive]) VALUES(@OurCompanyID, @LocationID, @LocationTypeID, @LocationPartID,@PartName,@PartGroupName, @SerialNumber, @MACAddress, @CardReaderTypeID, @UID, @StartDate, @IsActive)";
                        connection.Execute(isql, iparameters);

                        cardReader = GetCardReader(guid);

                    }


                    // kart tanımlı mı bakılır değilse eklenir

                    var cParameters = new { Number = model.CardNumber };
                    var cSql = "SELECT TOP (1) * FROM [dbo].[Card] Where [CardNumber] = @Number";
                    var card = connection.QueryFirstOrDefault<Card>(cSql, cParameters);

                    if (card == null)
                    {
                        var cguid = Guid.NewGuid();
                        var cparameters = new
                        {
                            OurCompanyID = 2,
                            CardTypeID = 2,
                            CardNumber = model.CardNumber,
                            ExpireDate = DateTime.Now.AddYears(99),
                            Credit = 0,
                            Currency = "TRL",
                            CardStatusID = 1,
                            RecordDate = now,
                            UID = cguid
                        };

                        var csql = "INSERT INTO [dbo].[Card] ([OurCompanyID],[CardTypeID],[CardNumber],[ExpireDate],[Credit],[Currency],[CardStatusID],[RecordDate],[ActivateDate],[UID]) VALUES( @OurCompanyID, @CardTypeID, @CardNumber, @ExpireDate, @Credit, @Currency, @CardStatusID, @RecordDate, null, @UID)";

                        connection.Execute(csql, cparameters);

                        card = GetCard(model.CardNumber);
                    }


                    if (cardReader.CardReaderTypeID == 2) // part set et
                    {
                        var emplyeeparameters = new { CardNumber = model.CardNumber };
                        var emplyeesql = "SELECT TOP (1) * FROM [dbo].[EmployeeCard] Where [CardNumber] = @CardNumber Order By [RecordDate] desc";
                        var cardEmployee = connection.QueryFirstOrDefault<EmployeeCard>(emplyeesql, emplyeeparameters);

                        if (cardEmployee != null && cardEmployee.EmployeeID != null)
                        {
                            var ncparameters = new
                            {
                                OurCompanyID = 2,
                                LocationID = cardReader.LocationID,
                                LocationTypeID = cardReader.LocationTypeID,
                                LocationPartID = cardReader.LocationPartID,
                                EmployeeID = cardEmployee.EmployeeID,
                                ReadDate = DateTime.UtcNow.AddHours(3)
                            };
                            var csql = "INSERT INTO [dbo].[LocationPartEmployee] ([OurCompanyID], [LocationID], [LocationTypeID], [LocationPartID], [EmployeeID], [ReadDate]) VALUES(@OurCompanyID, @LocationID, @LocationTypeID, @LocationPartID, @EmployeeID, @ReadDate)";
                            connection.Execute(csql, ncparameters);

                            result.IsSuccess = true;
                            result.Message = "OK";

                        }
                        else if (cardEmployee != null && (cardEmployee.EmployeeID == null || cardEmployee.EmployeeID <= 0))
                        {
                            result.IsSuccess = false;
                            result.Message = "NO CRDEM";
                        }

                        //else if (cardEmployee == null)
                        //{
                        //    var ncparameters = new
                        //    {
                        //        OurCompanyID = 2,
                        //        EmployeeID = cardEmployee?.EmployeeID,
                        //        CardID = card.ID,
                        //        CardTypeID = card.CardTypeID,
                        //        CardStatusID = card.CardStatusID,
                        //        CardNumber = model.CardNumber,
                        //        RecordDate = DateTime.UtcNow.AddHours(3)
                        //    };
                        //    var csql = "INSERT INTO [dbo].[EmployeeCard] ([OurCompanyID],[EmployeeID],[CardID],[CardTypeID],[CardStatusID],[CardNumber],[RecordDate]) VALUES(@OurCompanyID, @EmployeeID, @CardID, @CardTypeID, @CardStatusID, @CardNumber,@RecordDate)";
                        //    connection.Execute(csql, ncparameters);

                        //    result.IsSuccess = false;
                        //    result.Message = "EDIT CRD";
                        //}
                        else
                        {
                            result.IsSuccess = true;
                            result.Message = "NOTFOUND";
                        }

                    }
                    else if (cardReader.CardReaderTypeID == 1) // mesai set et
                    {
                        result.IsSuccess = true;
                        result.Message = "OK";
                    }

                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "ERROR";
            }

            return result;
        }


        public ParamResult AddCardReaderParameter(CardReaderParameterModel model)
        {
            DateTime now = DateTime.UtcNow.AddHours(3);
            ParamResult paramresult = new ParamResult()
            {
                LocationID = 0,
                IsSameParameter = true,
                MiliSecond = 0,
                ReadCount = 0,
                UnitDuration = 0,
                UnitPrice = 0,
            };



            try
            {

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    var crParameters = new { Serial = model.SerialNumber, MAC = model.MACAddress };
                    var crSql = "SELECT TOP (1) * FROM [dbo].[CardReader] Where [SerialNumber] = @Serial and [MACAddress] = @MAC";
                    var cardReader = connection.QueryFirstOrDefault<CardReader>(crSql, crParameters);

                    if (cardReader == null)
                    {
                        var guid = Guid.NewGuid();
                        var parameters = new
                        {
                            OurCompanyID = 2,
                            LocationID = 0,
                            LocationTypeID = 0,
                            LocationPartID = 0,
                            PartName = "Tanımsız",
                            PartGroupName = "Tanımsız",
                            SerialNumber = model.SerialNumber,
                            MACAddress = model.MACAddress,
                            CardReaderTypeID = 2,
                            UID = guid,
                            StartDate = now.Date,
                            IsActive = true
                        };
                        var isql = "INSERT INTO [dbo].[CardReader] ([OurCompanyID],[LocationID],[LocationTypeID],[LocationPartID],[PartName],[PartGroupName],[SerialNumber],[MACAddress],[CardReaderTypeID],[UID],[StartDate],[IsActive]) VALUES(@OurCompanyID, @LocationID, @LocationTypeID, @LocationPartID,@PartName,@PartGroupName, @SerialNumber, @MACAddress, @CardReaderTypeID, @UID, @StartDate, @IsActive)";
                        connection.Execute(isql, parameters);
                        cardReader = GetCardReader(guid);
                    }


                    var values = new { SerialNumber = model.SerialNumber, MACAddress = model.MACAddress, Version = model.Version, UnitPrice = (decimal)(model.UnitPrice / 100), MiliSecond = model.MiliSecond, ReadCount = model.ReadCount, UnitDuration = model.UnitDuration };
                    var sql = "exec [dbo].[CheckCardReaderParameter] @SerialNumber, @MACAddress, @Version, @UnitPrice, @MiliSecond, @ReadCount, @UnitDuration";

                    paramresult = connection.Query<ParamResult>(sql, values).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
            }

            return paramresult;
        }

        public CardReader GetCardReader(Guid uid)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { UID = uid };
                var sql = "SELECT TOP (1) * FROM [dbo].[CardReader] Where [UID] = @UID and [IsActive] = 1";
                return connection.QueryFirstOrDefault<CardReader>(sql, Parameters);
            }
        }

        public CardReader GetCardReader(string serial, string macaddress)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { SERIAL = serial, MAC = macaddress };
                var sql = "SELECT TOP (1) * FROM [dbo].[CardReader] Where [SerialNumber] = @SERIAL and [MACAddress] = @MAC and [IsActive] = 1";
                return connection.QueryFirstOrDefault<CardReader>(sql, Parameters);
            }
        }

        public CardDeposite GetCardDeposit(long OrderID)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { SaleID = OrderID };
                var sql = "SELECT TOP (1) [ID] ,[SaleID] ,[Date] ,[UID] FROM [dbo].[TicketSaleRows] Where SaleID = @SaleID AND [ProductID] = 1 and StatusID = 2";
                return connection.QueryFirstOrDefault<CardDeposite>(sql, Parameters);
            }
        }

        public CardReaderAction GetCardReaderAction(Guid uid)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { UID = uid };
                var sql = "SELECT TOP (1) * FROM [dbo].[CardReaderAction] Where [UID] = @UID";
                return connection.QueryFirstOrDefault<CardReaderAction>(sql, Parameters);
            }
        }

        public Card GetCard(string Number)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { Number };
                var sql = "SELECT TOP (1) * FROM [dbo].[VCard] Where [CardNumber] = @Number";
                return connection.QueryFirstOrDefault<Card>(sql, Parameters);
            }
        }

        public CardCreditLoad GetCardLoad(Guid uid)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { UID = uid };
                var sql = "SELECT TOP (1) * FROM [dbo].[TicketSaleCreditLoad] Where [UID] = @UID";
                return connection.QueryFirstOrDefault<CardCreditLoad>(sql, Parameters);
            }
        }

        public CardCreditLoad GetCardLoad(long id)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var Parameters = new { ID = id };
                var sql = "SELECT TOP (1) * FROM [dbo].[TicketSaleCreditLoad] Where [ID] = @ID";
                return connection.QueryFirstOrDefault<CardCreditLoad>(sql, Parameters);
            }
        }

        public Result LoadCard(CardCreditLoad load)
        {
            Result result = new Result();

            result.IsSuccess = false;
            result.Message = string.Empty;

            try
            {
                if (load.IsSuccess != true)
                {

                
                var card = GetCard(load.CardNumber);
                var cardReader = GetCardReader(load.SerialNumber, load.MACAddress);

                using (var connection = new SqlConnection(GetConnectionString()))
                {

                    if (load.CardActionTypeID == 5) // kart resetleme işlemi
                    {

                        var caParameters = new { CardNumber = load.CardNumber };
                        //var caSql = "UPDATE [dbo].[CardActions] SET [CreditCharge] = @RefundCredit, [ActionDateUpdate] = @Date, [ActionTypeID] = @ActionTypeID  WHERE [ProcessID] = @CardReaderActionID and [ActionTypeID] = 2 and [CardNumber] = @CardNumber and [CardReaderID] = @CardReaderID ";
                        var caSql = "Exec [dbo].[ResetCard] @CardNumber";
                        connection.Query(caSql, caParameters);

                    }

                    if (load.CardActionTypeID == 4) // kontür iade işlemi
                    {

                        var caParameters = new { Date = DateTime.UtcNow.AddHours(3), RefundCredit = load.RefundCredit, CardReaderActionID = load.CardReaderActionID, ActionTypeID = load.CardActionTypeID, CardNumber = load.CardNumber, CardReaderID = cardReader.ID, CardActionID = load.CardActionID };
                        //var caSql = "UPDATE [dbo].[CardActions] SET [CreditCharge] = @RefundCredit, [ActionDateUpdate] = @Date, [ActionTypeID] = @ActionTypeID  WHERE [ProcessID] = @CardReaderActionID and [ActionTypeID] = 2 and [CardNumber] = @CardNumber and [CardReaderID] = @CardReaderID ";
                        var caSql = "UPDATE [dbo].[CardActions] SET [CreditCharge] = @RefundCredit, [ActionDateUpdate] = @Date, [ActionTypeID] = @ActionTypeID, [ActionTypeName] = 'Kredi İade'  WHERE [ID] = @CardActionID ";
                        connection.Execute(caSql, caParameters);

                    }
                    else if (load.CardActionTypeID == 3)  // kredi yükleme işlemi
                    {

                        // Cart depozitosu için caride başlangıç kaydı amaçlı satır eklenir ve cart statüsü aktif e çekilir.

                        var deposite = GetCardDeposit(load.SaleID);

                        if (deposite != null)
                        {

                            var caParameters = new { OurCompanyID = 2, CardID = card.ID, LocationID = cardReader.LocationID, ActionTypeID = 1, SaleID = load.SaleID, SaleRowID = deposite.ID, UID = load.UID, IsPromotion = false };
                            var caSql = "Delete FROM [dbo].[CardActions] Where [OurCompanyID] = @OurCompanyID and [CardID] = @CardID and [LocationID] = @LocationID and [ActionTypeID] = @ActionTypeID and SaleID = @SaleID and SaleRowID = @SaleRowID and [ProcessUID] = @UID and [IsPromotion] = @IsPromotion";
                            connection.Execute(caSql, caParameters);

                            var caparameters = new
                            {
                                OurCompanyID = card.OurCompanyID,
                                CardID = card.ID,
                                LocationID = cardReader.LocationID,
                                CustomerID = 2,
                                SaleID = deposite.SaleID,
                                SaleRowID = deposite.ID,
                                ProcessID = load.ID,
                                ProcessUID = load.UID,
                                ActionTypeID = 1,
                                ActionTypeName = "Kart Aktif Etme",
                                ActionDate = DateTime.UtcNow.AddHours(3),
                                CreditCharge = 0,
                                CreditSpend = 0,
                                Currency = "TRL",
                                IsPromotion = false,
                                CardNumber = load.CardNumber,
                                CardReaderID = cardReader.ID
                            };

                            var casql = "  INSERT INTO [dbo].[CardActions] ([OurCompanyID],[CardID],[LocationID],[CustomerID],[SaleID],[SaleRowID],[ProcessID],[ProcessUID],[ActionTypeID],[ActionTypeName],[ActionDate],[CreditCharge],[CreditSpend],[Currency],[IsPromotion],[CardNumber],[CardReaderID]) VALUES(@OurCompanyID,@CardID,@LocationID,@CustomerID,@SaleID,@SaleRowID,@ProcessID,@ProcessUID,@ActionTypeID, @ActionTypeName, @ActionDate, @CreditCharge, @CreditSpend , @Currency, @IsPromotion,@CardNumber,@CardReaderID)";
                            connection.Execute(casql, caparameters);

                            var cuparameter = new { CardNumber = card.CardNumber, ActivateDate = DateTime.UtcNow.AddHours(3) };
                            var cupdsql = "Update [dbo].[Card] Set [CardStatusID] = 1, [ActivateDate] = @ActivateDate  Where [CardNumber] = @CardNumber";
                            connection.Execute(cupdsql, cuparameter);
                        }

                        // MasterCredit kart hereketi eklenir
                        if (load.MasterCredit != null && load.MasterCredit > 0)
                        {

                            var caParameters = new { OurCompanyID = 2, CardID = card.ID, LocationID = cardReader.LocationID, ActionTypeID = 3, UID = load.UID, IsPromotion = false };
                            var caSql = "Delete FROM [dbo].[CardActions] Where [OurCompanyID] = @OurCompanyID and [CardID] = @CardID and [LocationID] = @LocationID and [ActionTypeID] = @ActionTypeID and [ProcessUID] = @UID and [IsPromotion] = @IsPromotion";
                            connection.Execute(caSql, caParameters);

                            var caparameters = new
                            {
                                OurCompanyID = card.OurCompanyID,
                                CardID = card.ID,
                                LocationID = cardReader.LocationID,
                                CustomerID = 2,
                                ProcessID = load.ID,
                                ProcessUID = load.UID,
                                ActionTypeID = 3,
                                ActionTypeName = "Kredi Yükleme",
                                ActionDate = DateTime.UtcNow.AddHours(3),
                                CreditCharge = load.MasterCredit,
                                CreditSpend = 0,
                                Currency = "TRL",
                                IsPromotion = false,
                                CardNumber = load.CardNumber,
                                CardReaderID = cardReader.ID
                            };

                            var casql = "  INSERT INTO [dbo].[CardActions] ([OurCompanyID],[CardID],[LocationID],[CustomerID],[ProcessID],[ProcessUID],[ActionTypeID],[ActionTypeName],[ActionDate],[CreditCharge],[CreditSpend],[Currency],[IsPromotion],[CardNumber],[CardReaderID]) VALUES(@OurCompanyID,@CardID,@LocationID,@CustomerID,@ProcessID,@ProcessUID,@ActionTypeID, @ActionTypeName, @ActionDate, @CreditCharge, @CreditSpend , @Currency, @IsPromotion,@CardNumber,@CardReaderID)";
                            connection.Execute(casql, caparameters);

                        }

                        // PromoCredit kart hereketi eklenir
                        if (load.PromoCredit != null && load.PromoCredit > 0)
                        {

                            var caParameters = new { OurCompanyID = 2, CardID = card.ID, LocationID = cardReader.LocationID, ActionTypeID = 3, UID = load.UID, IsPromotion = true };
                            var caSql = "Delete FROM [dbo].[CardActions] Where [OurCompanyID] = @OurCompanyID and [CardID] = @CardID and [LocationID] = @LocationID and [ActionTypeID] = @ActionTypeID and [ProcessUID] = @UID and [IsPromotion] = @IsPromotion";
                            connection.Execute(caSql, caParameters);

                            var caparameters = new
                            {
                                OurCompanyID = card.OurCompanyID,
                                CardID = card.ID,
                                LocationID = cardReader.LocationID,
                                CustomerID = 2,
                                ProcessID = load.ID,
                                ProcessUID = load.UID,
                                ActionTypeID = 3,
                                ActionTypeName = "Kredi Yükleme",
                                ActionDate = DateTime.UtcNow.AddHours(3),
                                CreditCharge = load.PromoCredit,
                                CreditSpend = 0,
                                Currency = "TRL",
                                IsPromotion = true,
                                CardNumber = load.CardNumber,
                                CardReaderID = cardReader.ID
                            };

                            var casql = "  INSERT INTO [dbo].[CardActions] ([OurCompanyID],[CardID],[LocationID],[CustomerID],[ProcessID],[ProcessUID],[ActionTypeID],[ActionTypeName],[ActionDate],[CreditCharge],[CreditSpend],[Currency],[IsPromotion],[CardNumber],[CardReaderID]) VALUES(@OurCompanyID,@CardID,@LocationID,@CustomerID,@ProcessID,@ProcessUID,@ActionTypeID, @ActionTypeName, @ActionDate, @CreditCharge, @CreditSpend , @Currency, @IsPromotion,@CardNumber,@CardReaderID)";
                            connection.Execute(casql, caparameters);

                        }

                    }

                    // kartın bakiyesi güncellenir.
                    SetCardBalance(card.CardNumber, load.FinalCredit);

                    //var cuparameters = new { CardNumber = card.CardNumber, Credit = load.FinalCredit };
                    //var cusql = "Exec [dbo].[AddEditCard] @CardNumber,  @Credit";
                    //connection.Query(cusql, cuparameters);

                    // yüklenme bölümü onaylanır.

                    var lcparameters = new { ID = load.ID, UID = load.UID, IsSuccess = true, CompleteDate = DateTime.UtcNow.AddHours(3) };
                    var lcsql = "Update [dbo].[TicketSaleCreditLoad] Set [IsSuccess] = @IsSuccess, [CompleteDate] = @CompleteDate  Where [ID] = @ID and [UID] = @UID";
                    connection.Execute(lcsql, lcparameters);

                }

                result.IsSuccess = true;
                result.Message = "OK";
                }

            }
            catch (Exception ex)
            {
                result.Message = "ERR";
            }

            return result;
        }

        public void SetCardBalance(string CardNumber, double? Credit)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                var cuparameters = new { CardNumber, Credit };
                var cusql = "Exec [dbo].[AddEditCard] @CardNumber,  @Credit";
                connection.Query(cusql, cuparameters);
            }
        }
    }
}