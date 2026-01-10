using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Inspection = Actiontime.Data.Entities.Inspection;
using InspectionRow = Actiontime.Data.Entities.InspectionRow;

namespace Actiontime.Services
{
    public class LocationService : ILocationService
    {

        private readonly ICloudService _cloudService;
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        private readonly IServiceScopeFactory _scopeFactory;
        public LocationService(ApplicationDbContext db, ApplicationCloudDbContext cdb, ICloudService cloudService, IServiceScopeFactory scopeFactory)
        {
            _db = db;
            _cdb = cdb;
            _cloudService = cloudService;
            _scopeFactory = scopeFactory;
        }

        public OurLocation? GetOurLocation()
        {

            return _db.OurLocations.FirstOrDefault();
        }

        public List<ProductPriceModel>? GetProductPrice()
        {
            var prices = _db.ProductPrices.OrderBy(x => x.Duration).ToList();

            List<ProductPriceModel> priceModels = new List<ProductPriceModel>();

            prices.ForEach(price =>
            {
                // Her ProductPrice öğesini ProductPriceModel'e dönüştürme işlemi
                ProductPriceModel priceModel = new ProductPriceModel
                {
                    Id = price.Id,
                    PriceId = price.Id,
                    CategoryId = price.CategoryId,
                    Price = price.Price,
                    CategoryName = price.CategoryName,
                    Currency = price.Currency,
                    Duration = price.Duration,
                    MasterPrice = price.MasterPrice,
                    OurCompanyId = price.OurCompanyId,
                    PriceCategoryId = price.PriceCategoryId,
                    PriceCategoryName = price.PriceCategoryName,
                    ProductId = price.ProductId,
                    ProductName = price.ProductName,
                    PromoPrice = price.PromoPrice,
                    Quantity = price.Quantity,
                    TaxRate = price.TaxRate,
                    TotalPrice = price.TotalPrice

                };

                priceModels.Add(priceModel);
            });

            return priceModels;
        }

        public LocationSchedule? GetLocationSchedule(DateTime date)
        {
            var location = _db.OurLocations.FirstOrDefault();

            return _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == DateOnly.FromDateTime(date) && x.LocationId == location.Id);
        }

        public List<LocationSchedule>? GetLocationSchedules(DateTime date)
        {
            var location = _db.OurLocations.FirstOrDefault();

            var schedule = _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == DateOnly.FromDateTime(date) && x.LocationId == location.Id);

            return _db.LocationSchedules.Where(x => x.ScheduleWeek == schedule.ScheduleWeek && x.LocationId == location.Id).ToList();
        }

        public OurLocationInfo GetOurLocationInfo()
        {
            OurLocationInfo info = new OurLocationInfo();

            var location = _db.OurLocations.FirstOrDefault();
            string statusName = "Waiting";

            if (location != null)
            {

                var datelist = _db.DatePeriods.FirstOrDefault(x => x.Date == location.LocalDate);


                info.Id = location.Id;
                info.UID = location.LocationUid?.ToString()?.ToUpper();
                info.FullName = location.LocationName;
                info.StatusName = "";
                info.DateSelected = location.LocalDateTime?.ToString("yyyy-MM-dd HH:mm");
                info.WeekSelected = datelist.PeriodNumber;
                info.ScheduleDuration = "00:00";
                info.Code = location.SortBy.Trim();

                var locSchedule = _db.LocationSchedules.FirstOrDefault(x => x.LocationId == location.Id && x.ScheduleDate == location.LocalDate);
                if (locSchedule != null)
                {
                    info.ScheduleTime = $"{locSchedule.ScheduleStart.ToString("HH:mm")} - {locSchedule.ScheduleEnd?.ToString("HH:mm")}";
                    info.ScheduleDuration = locSchedule.ScheduleDuration?.ToString(@"hh\:mm");
                }
                else
                {
                    statusName = "Off Day";
                }

                var locShift = _db.LocationShifts.FirstOrDefault(x => x.ShiftDate == location.LocalDate && x.LocationId == location.Id);
                if (locShift != null)
                {
                    if (locShift.ShiftStart != null && locShift.ShiftFinish != null)
                    {
                        statusName = "Shift is Over";
                    }
                    else if (locShift.ShiftStart != null && locShift.ShiftFinish == null)
                    {
                        statusName = "Open";
                    }

                    info.ShiftTime = $"{locShift.ShiftStart.ToString("HH:mm")} - {locShift.ShiftFinish?.ToString("HH:mm")}";
                    info.ShiftDuration = locShift.ShiftDuration?.ToString(@"hh\:mm");
                }

            }

            info.StatusName = statusName;

            return info;
        }

        //CheckLocationShift
        public string CheckLocationShift(int employeeId)
        {
            string result = string.Empty;

            var dateKey = DateTime.Now;
            var date = DateOnly.FromDateTime(dateKey.Date);
            var location = _db.OurLocations.FirstOrDefault();

            dateKey = location.LocalDateTime ?? dateKey;
            date = location.LocalDate ?? date;


            if (location != null)
            {
                var locSchedule = _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == date && x.LocationId == location.Id);



                if (locSchedule != null)
                {
                    var startTime = locSchedule.ScheduleStart.AddHours(-1);
                    var endTime = locSchedule.ScheduleEnd != null ? locSchedule.ScheduleEnd?.AddHours(1) : dateKey.AddHours(1);

                    if (startTime <= dateKey && endTime >= dateKey)
                    {
                        var locShift = _db.LocationShifts.FirstOrDefault(x => x.ShiftDate == date && x.LocationId == location.Id);

                        if (locShift == null)
                        {
                            locShift = new Data.Entities.LocationShift()
                            {

                                EmployeeId = employeeId,
                                ShiftDate = date,
                                LocationId = location.Id,
                                ShiftStart = dateKey,
                                ShiftFinish = null,
                                RecordEmployeeId = employeeId,
                                RecordDate = dateKey,

                            };

                            _db.LocationShifts.Add(locShift);
                            _db.SaveChanges();

                            result = "Location Shift Started";

                            SyncProcess process = new SyncProcess()
                            {
                                DateCreate = DateTime.Now,
                                Entity = "LocationShift",
                                Process = 1,
                                EntityId = locShift.Id,
                                EntityUid = locShift.Uid,
                            };

                            _db.SyncProcesses.Add(process);
                            _db.SaveChanges(true);

                            
                            //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

                            Task.Run(() =>
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                                // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                                var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                                if (persistedProcess != null)
                                {
                                    scopedCloud.AddCloudProcess(persistedProcess);
                                }
                            });



                        }
                        else
                        {
                            if (locShift.Duration == null && locShift.ShiftFinish == null)
                            {
                                locShift.ShiftFinish = dateKey;
                                _db.SaveChanges();

                                result = "Location Shift Finished";

                                SyncProcess process = new SyncProcess()
                                {
                                    DateCreate = DateTime.Now,
                                    Entity = "LocationShift",
                                    Process = 2,
                                    EntityId = locShift.Id,
                                    EntityUid = locShift.Uid,
                                };

                                _db.SyncProcesses.Add(process);
                                _db.SaveChanges(true);

                                //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));
                                Task.Run(() =>
                                {
                                    using var scope = _scopeFactory.CreateScope();
                                    var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                                    // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                                    var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                    var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                                    if (persistedProcess != null)
                                    {
                                        scopedCloud.AddCloudProcess(persistedProcess);
                                    }
                                });
                            }
                            else
                            {
                                result = "Location Shift Already Finished";
                            }
                        }
                    }
                    else
                    {
                        result = "Now is out of defined Location Schedule time";
                    }
                }
                else
                {
                    result = "Location Schedule not defined";
                }

            }
            else
            {
                result = "Location not defined";
            }



            return result;
        }

        public List<LocationPartModel> GetLiveParts()
        {

            var location = _db.OurLocations.FirstOrDefault();
            var parts = _db.LocationPartials.Where(x => x.IsActive == true).OrderBy(x => x.Number).ToList();
            var devices = _db.Qrreaders.Where(x => x.IsActive == true).ToList();
            var now = location.LocalDateTime.Value;



            List<LocationPartModel> partModels = new List<LocationPartModel>();

            parts.ForEach(part =>
            {
                var trip = _db.Vtrips.Where(x => x.TripDate == location.LocalDate && x.PartId == part.Id && x.LocationId == location.Id).OrderByDescending(x => x.TripStart).FirstOrDefault();



                if (trip != null)
                {
                    var deviceName = trip?.ReaderSerialNumber?.Substring((trip?.ReaderSerialNumber?.Length ?? 4) - 4);

                    var elapsed = (int)((location.LocalDateTime.Value) - trip.TripStart.Value).TotalSeconds;
                    var timeElapsed = trip.TripDurationSecond != null ? trip.TripDurationSecond.Value : elapsed;
                    var ticketnumber = !string.IsNullOrEmpty(trip.TicketNumber) ? trip.TicketNumber : "0000";
                    ticketnumber = ticketnumber.Substring(ticketnumber.Length - 4, 4);

                    LocationPartModel partModel = new LocationPartModel
                    {
                        PartId = part.Id,
                        SortNumber = (int)(part?.Number ?? 1),
                        PartName = part?.PartName ?? "Minder",
                        Device = deviceName ?? "0000",
                        Ticket = "#" + ticketnumber,
                        StartTime = trip.TripStart ?? new DateTime(),
                        Duration = trip.UnitDuration ?? 180,
                        TimeElapsed = timeElapsed,
                        TimeRemain = (trip.UnitDuration ?? 0) - timeElapsed,
                        EmployeeName = trip.FullName ?? "",
                        Status = trip.TripDurationSecond != null ? 2 : 1,
                        NowTime = DateTime.Now,
                        LocalTime = location.LocalDateTime.Value
                    };

                    partModels.Add(partModel);
                }
                else
                {
                    var startTime = new DateTime().Date;
                    var device = devices.FirstOrDefault(x => x.LocationPartId == part.Id);
                    var deviceName = device?.SerialNumber?.Substring((device?.SerialNumber?.Length ?? 4) - 4);

                    LocationPartModel partModel = new LocationPartModel
                    {
                        PartId = part.Id,
                        SortNumber = (int)(part?.Number ?? 1),
                        PartName = part?.PartName ?? "Minder",
                        Device = deviceName ?? "0000",
                        Ticket = "#0000",
                        StartTime = startTime,
                        Duration = 0,
                        TimeElapsed = 0,
                        TimeRemain = 0,
                        EmployeeName = "",
                        Status = 0
                    };

                    partModels.Add(partModel);
                }


            });

            return partModels;
        }

        public InspectionModel GetInspection(int employeeId)
        {
            var location = _db.OurLocations.FirstOrDefault();
            var employee = _db.Employees.FirstOrDefault(x => x.Id == employeeId);
            var parts = _db.LocationPartials.Where(x => x.IsActive == true).OrderBy(x => x.Number).ToList();



            InspectionModel inspectionModel = new InspectionModel();


            var inspection = _db.Inspections.FirstOrDefault(x => x.LocationId == location.Id && x.InspectionDate == location.LocalDate);

            if (inspection == null)
            {
                inspection = new Inspection();

                inspection.InspectionDate = location.LocalDate.Value;
                inspection.RecordDate = location.LocalDateTime;
                inspection.LocationId = location.Id;
                inspection.InspectionTypeId = location.LocationTypeId;
                inspection.InspectorId = employeeId;
                //inspection.DateBegin = now;
                inspection.LanguageCode = "en";
                inspection.RecordEmployeeId = employeeId;
                inspection.Uid = Guid.NewGuid();

                _db.Inspections.Add(inspection);
                _db.SaveChanges();
            }

            inspectionModel.Id = inspection.Id;
            inspectionModel.InspectionDate = location.LocalDateTime.Value; //inspection.InspectionDate;
            inspectionModel.LocationName = location.LocationName;
            inspectionModel.Inspector = employee.FullName;
            inspectionModel.StartDate = inspection.DateBegin;
            inspectionModel.FinishDate = inspection.DateEnd;
            inspectionModel.Language = inspection.LanguageCode;
            inspectionModel.Description = inspection.Description;
            inspectionModel.Status = inspection.DateEnd != null ? "Completed" : inspection.DateBegin != null ? "Going on" : "Waiting";
            inspectionModel.Closed = inspection.DateEnd != null ? true : false;


            var items = _db.InspectionItems.Where(x => x.IsActive == true).ToList();
            var rows = _db.VinspectionRows.Where(x => x.LanguageCode == inspection.LanguageCode && x.InspectionId == inspection.Id && x.InspectorId == employeeId).ToList();

            int partItemCount = items.Where(x => x.IsPart == true).Count();
            int partItemCountGeneral = items.Where(x => x.IsPart == false).Count();

            List<InspectionPartSummaryModel> ınspectionPartSummaryModels = new();
            List<InspectionAnswer> inspectionAnswers = new();

            parts.ForEach(part =>
            {
                var partRows = rows.Where(x => x.LocationPartId == part.Id).ToList();
                int partRowCount = partRows.Count();

                InspectionPartSummaryModel partSummary = new()
                {
                    InspeectionId = inspectionModel.Id,
                    PartId = part.Id,
                    PartName = part.PartName ?? $"#{part.Id}",
                    TotalItem = partItemCount,
                    VotedItem = partRowCount,
                    TotalRate = partItemCount > 0 ? (partRowCount / partItemCount) : (double)0.00
                };

                ınspectionPartSummaryModels.Add(partSummary);


                partRows.ForEach(partRow =>
                {
                    InspectionAnswer answer = new()
                    {
                        InspeectionId = inspectionModel.Id,
                        PartId = part.Id,
                        Number = partRow.InspectionItemId,
                        CategoryName = partRow.CategoryName,
                        PartName = part.PartName,
                        ItemName = partRow.InspectionItemName,
                        Answer = partRow.InspectionValue,
                        EstimateAnswer = partRow.EstimatedValue,
                        Description = partRow.Description,
                        Employee = partRow.FullName,
                    };

                    inspectionAnswers.Add(answer);

                });


            });

            //genel bölüm

            var partRows = rows.Where(x => x.LocationPartId == 0).ToList();
            int partRowCount = partRows.Count();

            InspectionPartSummaryModel partSummary = new()
            {
                PartId = 0,
                InspeectionId = inspectionModel.Id,
                PartName = "General",
                TotalItem = partItemCountGeneral,
                VotedItem = partRowCount,
                TotalRate = partItemCountGeneral > 0 ? (double)((double)((double)partRowCount / (double)partItemCountGeneral) * 100) : (double)0
            };

            ınspectionPartSummaryModels.Add(partSummary);

            partRows.ForEach(partRow =>
            {
                InspectionAnswer answer = new()
                {
                    Id = partRow.Id,
                    InspeectionId = inspectionModel.Id,
                    PartId = 0,
                    Number = partRow.InspectionItemId,
                    CategoryName = partRow.CategoryName,
                    PartName = "General",
                    ItemName = partRow.InspectionItemName,
                    Answer = partRow.InspectionValue,
                    EstimateAnswer = partRow.EstimatedValue,
                    Description = partRow.Description,
                    Employee = partRow.FullName,
                };

                inspectionAnswers.Add(answer);

            });


            inspectionModel.PartSummaryList = ınspectionPartSummaryModels;
            inspectionModel.AnswerList = inspectionAnswers;

            bool completed = true;

            foreach (var item in ınspectionPartSummaryModels)
            {
                if (item.VotedItem != item.TotalItem)
                {
                    completed = false;
                    break;
                }
            }

            inspectionModel.Completed = completed;

            return inspectionModel;
        }

        public InspectionPartModel GetPartInspection(int inspectionId, int partId, int pageId, int employeeId)
        {
            var location = _db.OurLocations.FirstOrDefault();
            var employee = _db.Employees.FirstOrDefault(x => x.Id == employeeId);
            var part = _db.LocationPartials.FirstOrDefault(x => x.Id == partId);
            if (partId == 0)
            {
                part = new LocationPartial()
                {
                    Id = 0,
                    Code = "",
                    Direction = "",
                    IsActive = true,
                    LocationId = location.Id,
                    Number = 0,
                    OurCompanyId = 1,
                    PartialId = 0,
                    PartialTypeId = 0,
                    PartName = "General"

                };
            }


            InspectionPartModel model = new InspectionPartModel();

            var inspection = _db.Inspections.FirstOrDefault(x => x.Id == inspectionId);

            if (inspection == null)
            {
                return model;
            }

            if (inspection.DateBegin == null)
            {
                inspection.DateBegin = location.LocalDateTime;
                _db.SaveChanges();
            }



            model.Id = inspection.Id;
            model.PartId = partId;
            model.InspectionDate = location.LocalDateTime.Value; //inspection.InspectionDate;
            model.LocationName = location.LocationName;
            model.Inspector = employee.FullName;
            model.StartDate = inspection.DateBegin;
            model.FinishDate = inspection.DateEnd;
            model.Language = inspection.LanguageCode;
            model.Description = inspection.Description;
            model.Status = inspection.DateEnd != null ? "Completed" : inspection.DateBegin != null ? "Going on" : "Waiting";
            model.Closed = inspection.DateEnd != null ? true : false;



            var items = _db.VinspectionItems.Where(x => x.IsActive == true).ToList();
            if (part.Id == 0)
            {
                items = items.Where(x => x.IsPart == false).ToList();
            }
            else
            {
                items = items.Where(x => x.IsPart == true).ToList();
            }

            var item = items.FirstOrDefault(x => x.Id == pageId);

            if (pageId == 1)
            {
                item = items.FirstOrDefault();
            }


            var rows = _db.VinspectionRows.Where(x => x.InspectionId == inspection.Id && x.LocationPartId == part.Id).ToList();
            var row = _db.VinspectionRows.FirstOrDefault(x => x.InspectionId == inspection.Id && x.InspectionItemId == item.Id && x.LocationPartId == part.Id);
            var images = _db.InspectionItemImages.Where(x => x.InspectionItemId == item.Id).ToList();

            model.currentPageId = item.Id;
            model.nextPageId = item.Id;
            model.prevPageId = item.Id;

            int index = items.IndexOf(item);

            if (index != -1)
            {
                var prevItem = index > 0 ? items[index - 1] : items.FirstOrDefault();
                var nextItem = index < items.Count - 1 ? items[index + 1] : items.LastOrDefault();

                model.nextPageId = nextItem.Id;
                model.prevPageId = prevItem.Id;
            }


            int partItemCount = items.Count();

            InspectionPartSummaryModel ınspectionPartSummaryModel = new();
            InspectionQuestion inspectionQuestion = new();
            InspectionAnswer inspectionAnswer = new();
            List<InspectionImage> inspectionImages = new();

            var partRows = rows.Where(x => x.LocationPartId == part.Id).ToList();
            int partRowCount = partRows.Count();

            InspectionPartSummaryModel partSummary = new()
            {
                InspeectionId = model.Id,
                PartId = part?.Id ?? 0,
                PartName = part?.PartName ?? $"General",
                TotalItem = partItemCount,
                VotedItem = partRowCount,
                TotalRate = partItemCount > 0 ? (partRowCount / partItemCount) : (double)0.00
            };

            model.PartSummary = partSummary;

            if (item != null)
            {
                InspectionQuestion question = new()
                {
                    Id = item.Id,
                    inspectionTypeId = item.InspectionTypeId ?? 1,
                    inspectionCatId = item.InspectionCatId ?? 1,
                    categoryName = item.CategoryName ?? "",
                    number = item.Number ?? "01",
                    itemName = item.ItemName ?? "",
                    itemNameTr = item.ItemNameTr ?? "",
                    answerType = item.AnswerType ?? 1,
                    estimatedTime = item.EstimatedTime ?? 0,
                    estimatedAnswer = item.EstimatedAnswer,
                    sortBy = item.SortBy ?? "01",
                    isPart = item.IsPart ?? false,
                };

                model.Question = question;

            };

            if (row != null)
            {
                InspectionAnswer answer = new()
                {
                    InspeectionId = model.Id,
                    PartId = part?.Id ?? 0,
                    Number = row.InspectionItemId,
                    CategoryName = row.CategoryName,
                    PartName = part?.PartName ?? "General",
                    ItemName = row.InspectionItemName,
                    Answer = row.InspectionValue,
                    EstimateAnswer = row.EstimatedValue,
                    Description = row.Description,
                    Employee = row.FullName,
                    InpectionDate = row.InpectionDate,
                    Id = row.Id
                };

                model.Answer = answer;
            };

            List<InspectionImage> imageList = new List<InspectionImage>();

            images.ForEach(img =>
            {
                InspectionImage image = new()
                {
                    Id = img.Id,
                    imageName = img.ImageName ?? "",
                    inspectionItemId = img.InspectionItemId ?? 0,
                    sortBy = img.SortBy ?? "01"
                };

                imageList.Add(image);
            });

            model.Images = imageList;





            return model;
        }

        public AppResult SavePartInspection(int inspectionId, int partId, int pageId, int employeeId, string answer, string? description)
        {

            AppResult result = new() { Success = false, Message = string.Empty, Description = string.Empty };

            var location = _db.OurLocations.FirstOrDefault();
            var employee = _db.Employees.FirstOrDefault(x => x.Id == employeeId);
            var part = _db.LocationPartials.FirstOrDefault(x => x.Id == partId);


            var inspection = _db.Inspections.FirstOrDefault(x => x.Id == inspectionId);

            if (inspection == null)
            {
                return result;
            }

            if (inspection.DateBegin == null)
            {
                inspection.DateBegin = location.LocalDateTime;
                _db.SaveChanges();
            }


            var item = _db.InspectionItems.FirstOrDefault(x => x.Id == pageId);

            if (item != null)
            {
                var row = _db.InspectionRows.FirstOrDefault(x => x.InspectionId == inspectionId && x.InspectionItemId == pageId && x.LocationPartId == partId);

                if (row == null)
                {
                    InspectionRow newrow = new InspectionRow();

                    newrow.InspectionId = inspectionId;
                    newrow.InspectionItemId = item.Id;
                    newrow.InspectionCategoryId = item.InspectionCatId ?? 1;
                    newrow.LocationPartId = partId;
                    newrow.LanguageCode = "en";
                    newrow.InspectionItemName = item.ItemName;
                    newrow.InspectionValue = answer;
                    newrow.EstimatedValue = item.EstimatedAnswer;
                    newrow.Description = description;
                    newrow.InspectorId = employeeId;
                    newrow.InpectionDate = location.LocalDateTime;

                    _db.InspectionRows.Add(newrow);
                    _db.SaveChanges(true);

                    row = newrow;

                    result.Description = "Item Inserted";
                }
                else
                {
                    row.InspectionValue = answer;
                    row.Description = description;
                    row.InspectorId = employeeId;
                    row.InpectionDate = location.LocalDateTime;

                    _db.SaveChanges(true);

                    result.Description = "Item Updated";
                }

                result.Success = true;
                result.Message = "Update Success";

            }

            return result;
        }


        public AppResult CloseInspection(int inspectionId, int employeeId, string? description)
        {

            AppResult result = new() { Success = false, Message = string.Empty, Description = string.Empty };

            var location = _db.OurLocations.FirstOrDefault();
            var employee = _db.Employees.FirstOrDefault(x => x.Id == employeeId);

            InspectionPartModel model = new InspectionPartModel();

            var inspection = _db.Inspections.FirstOrDefault(x => x.Id == inspectionId);

            if (inspection == null)
            {
                return result;
            }

            if (inspection.DateEnd == null)
            {
                inspection.DateEnd = location.LocalDateTime;
                inspection.UpdateEmployeeId = employeeId;
                inspection.Description = description;

                _db.SaveChanges();

                result.Description = "Inspection Completed";

            }
            else
            {
                inspection.Description = description;
                _db.SaveChanges();

                result.Description = "Inspection Updated";

            }

            SyncProcess process = new SyncProcess()
            {
                DateCreate = location.LocalDateTime.Value,
                Entity = "Inspection",
                Process = 1,
                EntityId = inspection.Id,
                EntityUid = inspection.Uid,
            };

            _db.SyncProcesses.Add(process);
            _db.SaveChanges(true);

            //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));
            Task.Run(() =>
            {
                using var scope = _scopeFactory.CreateScope();
                var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                if (persistedProcess != null)
                {
                    scopedCloud.AddCloudProcess(persistedProcess);
                }
            });

            result.Success = true;
            result.Message = "Success";

            return result;
        }

        public bool CheckInspection()
        {

            var location = _db.OurLocations.FirstOrDefault();

            var inspection = _db.Inspections.FirstOrDefault(x => x.InspectionDate == location.LocalDate && x.LocationId == location.Id);

            if (inspection != null && inspection.DateBegin != null && inspection.DateEnd != null)
            {
                return true;
            }

            return false;
        }

        public void GetSync()
        {

            var processList = _db.SyncProcesses.ToList();

            foreach (var process in processList)
            {
                //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));
                Task.Run(() =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                    // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                    var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                    if (persistedProcess != null)
                    {
                        scopedCloud.AddCloudProcess(persistedProcess);
                    }
                });

                Task.Delay(1000);
            }
        }

        public string CompletePartRide(int partId, int employeeId)
        {

            string result = string.Empty;

            var location = _db.OurLocations.FirstOrDefault();
            var part = _db.LocationPartials.FirstOrDefault(x => x.Id == partId);


            var trip = _db.Trips.Where(x => x.TripDate == location.LocalDate && x.PartId == part.Id && x.LocationId == location.Id).OrderByDescending(x => x.TripStart).FirstOrDefault();


            if (trip == null)
            {
                return "Trip Not Found";
            }

            if (trip.TripEnd == null)
            {
                trip.TripEnd = location.LocalDateTime;
                trip.UpdateDate = location.LocalDateTime;
                trip.UpdateEmployeeId = employeeId;

                _db.SaveChanges();

                using (ApplicationCloudDbContext _cdb = new ApplicationCloudDbContext())
                {

                    try
                    {
                        var confirm = _db.VtripConfirms.FirstOrDefault(x => x.ConfirmId == trip.ConfirmId);


                        var locationPartTrip = _cdb.LocationPartTrips.FirstOrDefault(x => x.ConfirmNumber == confirm.ConfirmNumber);



                        if (locationPartTrip != null)
                        {

                            locationPartTrip.ConfirmNumber = confirm.ConfirmNumber.Value;
                            locationPartTrip.TicketNumber = confirm.TicketNumber;
                            locationPartTrip.LocationId = confirm.LocationId.Value;
                            locationPartTrip.PartId = confirm.PartialId.Value;
                            locationPartTrip.PartSort = confirm.LocationPartId.Value;
                            locationPartTrip.TripStart = confirm.TripStart.Value;
                            locationPartTrip.TripEnd = confirm.TripEnd;
                            locationPartTrip.UnitDuration = (Int16)(confirm.UnitDuration ?? 180);
                            locationPartTrip.ElapsedDuration = confirm.TripDurationSecond ?? 0;
                            locationPartTrip.EmployeeName = confirm.FullName;
                            locationPartTrip.TimeZone = confirm.Timezone.Value;
                            locationPartTrip.Status = confirm.TripEnd != null ? "Cumpleted" : "Riding";
                            locationPartTrip.ConfirmNumber = confirm.ConfirmNumber.Value;

                            _cdb.SaveChanges();

                        }
                        else
                        {

                            locationPartTrip = new LocationPartTrip();

                            locationPartTrip.ConfirmNumber = confirm.ConfirmNumber.Value;
                            locationPartTrip.TicketNumber = confirm.TicketNumber;
                            locationPartTrip.LocationId = confirm.LocationId.Value;
                            locationPartTrip.PartId = confirm.PartialId.Value;
                            locationPartTrip.PartSort = confirm.LocationPartId.Value;
                            locationPartTrip.TripStart = confirm.TripStart.Value;
                            locationPartTrip.TripEnd = confirm.TripEnd;
                            locationPartTrip.UnitDuration = (Int16)(confirm.UnitDuration ?? 180);
                            locationPartTrip.ElapsedDuration = confirm.TripDurationSecond ?? 0;
                            locationPartTrip.EmployeeName = confirm.FullName;
                            locationPartTrip.TimeZone = confirm.Timezone.Value;
                            locationPartTrip.Status = confirm.TripEnd != null ? "Cumpleted" : "Riding";
                            locationPartTrip.ConfirmNumber = confirm.ConfirmNumber.Value;

                            _cdb.LocationPartTrips.Add(locationPartTrip);
                            _cdb.SaveChanges();
                        }

                    }
                    catch (Exception ex)
                    {
                        result = "Trip Not Syncronized";
                    }

                }



                result = "Trip Completed";

            }
            else
            {
                result = "Trip Already Completed Before";

            }

            return result;
        }



    }
}
