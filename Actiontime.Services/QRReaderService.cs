using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class QRReaderService: IQRReaderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        private readonly ISyncService _syncservice;


        public QRReaderService(ApplicationDbContext db, ApplicationCloudDbContext cdb, ISyncService syncervice)
        {
            _db = db;
            _cdb = cdb;
            _syncservice = syncervice;

        }

        public async Task<ReaderResult> AddReader(string message)
        {
            ReaderResult result = new ReaderResult();

            var location = _db.OurLocations.FirstOrDefault();

            var device = JsonConvert.DeserializeObject<DeviceInfo?>(message);
            result.SerialNumber = device?.SerialNumber ?? "NoSerial";
            result.Process = 1002;

            if (device != null)
            {
                var isexists = _db.Qrreaders.FirstOrDefault(x => x.SerialNumber == device.SerialNumber);

                if (isexists == null)
                {
                    Data.Entities.Qrreader newreader = new();

                    newreader.LocationPartId = null;
                    newreader.OurCompanyId = location?.OurCompanyId;
                    newreader.LocationId = location?.Id;
                    newreader.LocationTypeId = location?.LocationTypeId;
                    newreader.QrreaderTypeId = 2;
                    newreader.LocationPartId = 0;
                    newreader.StartDate = DateTime.UtcNow;
                    newreader.Ipaddress = device?.IP;
                    newreader.Version = device?.Ver.ToString();
                    newreader.SerialNumber = device?.SerialNumber;
                    newreader.TriggerCount = 1;
                    newreader.DurationTime = device?.DurationTime;
                    newreader.TriggerTime = device?.TriggerTime;
                    newreader.Uid = Guid.NewGuid();
                    newreader.IsActive = true;

                    _db.Qrreaders.Add(newreader);
                    _db.SaveChanges();

                    isexists = newreader;
                }
                else
                {
                    isexists.Ipaddress = device?.IP;
                    isexists.Version = device?.Ver.ToString();
                    isexists.TriggerCount = 1;
                    isexists.DurationTime = device?.DurationTime;
                    isexists.TriggerTime = device?.TriggerTime;
                    isexists.UpdateDate = DateTime.UtcNow;

                    _db.SaveChanges(true);
                }

                var parameter = _db.QrreaderParameters.FirstOrDefault(x => x.LocationId == 0 && x.QrreaderTypeId == 2);

                if (parameter != null)
                {
                    result.DurationTime = parameter.DurationTime ?? 180;
                    result.TriggerTime = parameter.TriggerTime ?? 1;
                }
            }

            return result;
        }

        public async Task<ConfirmResult> ConfirmQR(string message)
        {
            ConfirmResult result = new ConfirmResult();
            result.Process = 2002;

            var qr = JsonConvert.DeserializeObject<QRInfo?>(message);

            if (qr != null && qr.Process == 2001)
            {
                result.SerialNumber = qr.SerialNumber;
                result.QRCode = qr.QRCode;

                var qrParts = qr.QRCode.Split("-");
                if (qrParts != null && qrParts.Length >= 3)
                {
                    long orderRowId = 0;
                    int locationId = 0;

                    Int64.TryParse(qrParts[0], out orderRowId);
                    Int32.TryParse(qrParts[1], out locationId);


                    var ticketNumber = qr.QRCode.Substring(qrParts[0].Length + 1 + qrParts[1].Length + 1);

                    var orderRow = _db.OrderRows.FirstOrDefault(x => x.TicketNumber == ticketNumber && x.RowStatusId == 2 && x.Id == orderRowId && x.LocationId == locationId);
                    var qrReader = _db.Qrreaders.FirstOrDefault(x => x.SerialNumber == qr.SerialNumber);
                    var confirm = _db.TripConfirms.FirstOrDefault(x => x.TicketNumber == qr.QRCode);
                    var location = _db.OurLocations.FirstOrDefault();

                    if (orderRow != null)
                    {
                        if (qrReader != null && qrReader.IsActive == true)
                        {
                            if (orderRow.RowStatusId == 2)
                            {
                                if (confirm == null)
                                {
                                    confirm = new();

                                    confirm.ConfirmNumber = Guid.NewGuid();
                                    confirm.SaleOrderId = orderRow.OrderId;
                                    confirm.SaleOrderRowId = orderRow.Id;
                                    confirm.LocationId = qrReader.LocationId;
                                    confirm.LocationPartId = qrReader.LocationPartId;
                                    confirm.ConfirmTime = location?.LocalDateTime ?? DateTime.Now;
                                    confirm.TicketNumber = qr.QRCode;
                                    confirm.UnitDuration = orderRow.Duration * 60;
                                    confirm.RecordDate = location?.LocalDateTime ?? DateTime.Now;
                                    confirm.IsApproved = true;
                                    confirm.ReaderSerialNumber = qrReader.SerialNumber;

                                    _db.TripConfirms.Add(confirm);
                                    _db.SaveChanges();
                                }
                                else
                                {
                                    var trip = _db.Trips.FirstOrDefault(x => x.ConfirmId == confirm.Id && x.TicketNumber == confirm.TicketNumber);

                                    if (trip != null)
                                    {
                                        if (trip != null && trip.TripEnd != null)
                                        {
                                            result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                            result.Success = 0;
                                            result.Message = $"Used Before";
                                            result.Title = "Warning";
                                            result.LEDTemplate = 4;

                                            return result;
                                        }
                                    }
                                    else
                                    {
                                        confirm.ReaderSerialNumber = qrReader.SerialNumber;
                                        _db.SaveChanges();
                                    }
                                }

                                result.Success = 1;
                                result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                result.Message = "Confirmed";
                                result.Title = "OK";
                                result.LEDTemplate = 1;

                                return result;
                            }
                            else
                            {
                                result.Success = 0;
                                result.ConfirmNumber = string.Empty;
                                result.Message = "No Available";
                                result.Title = "Warning";
                                result.LEDTemplate = 4;

                                return result;
                            }
                        }
                        else
                        {
                            result.Success = 0;
                            result.ConfirmNumber = string.Empty;
                            result.Message = "No Reader";
                            result.Title = "Warning";
                            result.LEDTemplate = 4;

                            return result;
                        }
                    }
                    else
                    {
                        result.Success = 0;
                        result.ConfirmNumber = string.Empty;
                        result.Message = "Not Compatiple";
                        result.Title = "Alert";
                        result.LEDTemplate = 4;

                        return result;
                    }
                }
                else
                {
                    result.Success = 0;
                    result.ConfirmNumber = string.Empty;
                    result.Message = "No QR";
                    result.Title = "Error";
                    result.LEDTemplate = 4;

                    return result;
                }
            }
            else
            {
                result.Success = 0;
                result.ConfirmNumber = string.Empty;
                result.Message = "QR Not Correct";
                result.Title = "Error";
                result.LEDTemplate = 4;

                return result;
            }
        }

        public async Task<StartResult> StartQR(string message)
        {
            StartResult result = new StartResult();
            result.Process = 2004;

            var qr = JsonConvert.DeserializeObject<ConfirmInfo?>(message);

            if (qr != null && qr.Process == 2003)
            {
                result.SerialNumber = qr.SerialNumber;

                var qrParts = qr.QRCode.Split("-");

                if (qrParts != null && qrParts.Length >= 2)
                {
                    string employeeUID = qr.QRCode.Substring(qrParts[0].Length + 1);

                    var confirm = _db.TripConfirms.FirstOrDefault(x => x.ConfirmNumber.ToString() == qr.ConfirmNumber);

                    var employee = _db.Employees.FirstOrDefault(x => x.EmployeeUid.ToString() == employeeUID);
                    var location = _db.OurLocations.FirstOrDefault();

                    if (confirm != null)
                    {
                        if (qr.SerialNumber == confirm.ReaderSerialNumber)
                        {
                            var qrReader = _db.Qrreaders.FirstOrDefault(x => x.SerialNumber == qr.SerialNumber);

                            var part = _db.LocationPartials.FirstOrDefault(x => x.Id == confirm.LocationPartId);

                            if (qrReader != null && qrReader.IsActive == true)
                            {
                                if (employee != null)
                                {
                                    var trip = _db.Trips.FirstOrDefault(x => x.ConfirmId == confirm.Id && x.TicketNumber == confirm.TicketNumber);

                                    if (trip == null)
                                    {
                                        confirm.EmployeeId = employee.Id;
                                        _db.SaveChanges();

                                        trip = new Trip();

                                        trip.ConfirmId = confirm.Id;
                                        trip.LocationId = confirm.LocationId;
                                        trip.EmployeeId = employee.Id;
                                        trip.TicketNumber = confirm.TicketNumber;
                                        trip.ReaderSerialNumber = confirm.ReaderSerialNumber;
                                        trip.PartId = confirm.LocationPartId;
                                        trip.TripDate = location.LocalDate;
                                        trip.TripStart = location.LocalDateTime;
                                        trip.RecordDate = DateTime.Now;
                                        trip.RecordEmployeeId = employee.Id;
                                        trip.Uid = Guid.NewGuid();
                                        trip.UnitDuration = confirm.UnitDuration;

                                        _db.Trips.Add(trip);
                                        _db.SaveChanges();

                                        result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                        result.Success = 1;
                                        result.Message = $"{confirm.UnitDuration / 60} Minutes";
                                        result.Title = "Started";
                                        result.LEDTemplate = 2;
                                        result.Duration = confirm.UnitDuration ?? 180;

                                        return result;
                                    }
                                    else if (trip != null && trip.TripStart != null && trip.TripEnd == null)
                                    {
                                        location = _db.OurLocations.FirstOrDefault();
                                        var _tripDuration = (trip.TripStart - location.LocalDateTime).Value.TotalSeconds;

                                        result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                        result.Success = 1;
                                        result.Message = $"{confirm.UnitDuration / 60} Minutes";
                                        result.Title = "Started";
                                        result.LEDTemplate = 2;
                                        result.Duration = (int)_tripDuration;

                                        return result;
                                    }
                                    else
                                    {
                                        location = _db.OurLocations.FirstOrDefault();
                                        var _tripDuration = (trip.TripStart - location.LocalDateTime).Value.TotalSeconds;

                                        result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                        result.Success = 0;
                                        result.Message = $"Used Before";
                                        result.Title = "Warning";
                                        result.LEDTemplate = 4;
                                        result.Duration = (int)_tripDuration;

                                        return result;
                                    }
                                }
                                else
                                {
                                    result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                    result.Success = 0;
                                    result.Message = "No Employee";
                                    result.Title = "Warning";
                                    result.LEDTemplate = 4;
                                    result.Duration = confirm.UnitDuration ?? 180;

                                    return result;
                                }
                            }
                            else
                            {
                                result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                                result.Success = 0;
                                result.Message = "No Reader";
                                result.Title = "Warning";
                                result.LEDTemplate = 4;
                                result.Duration = confirm.UnitDuration ?? 180;

                                return result;
                            }
                        }
                        else
                        {
                            result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                            result.Success = 0;
                            result.Message = "Another Device";
                            result.Title = "Warning";
                            result.LEDTemplate = 4;
                            result.Duration = confirm.UnitDuration ?? 180;

                            return result;
                        }
                    }
                    else
                    {
                        result.ConfirmNumber = confirm.ConfirmNumber.ToString();
                        result.Success = 0;
                        result.Message = "No Confirm";
                        result.Title = "Warning";
                        result.LEDTemplate = 4;
                        result.Duration = 0;

                        return result;
                    }
                }
                else
                {
                    result.ConfirmNumber = qr.ConfirmNumber.ToString();
                    result.Success = 0;
                    result.Message = "No Correct";
                    result.Title = "Warning";
                    result.LEDTemplate = 4;
                    result.Duration = 0;

                    return result;
                }
            }
            else
            {
                result.ConfirmNumber = string.Empty;
                result.Success = 0;
                result.Message = "No QR";
                result.Title = "Warning";
                result.LEDTemplate = 4;
                result.Duration = 0;

                return result;
            }
        }

        public async Task<CompleteResult> CompleteQR(string message)
        {
            CompleteResult result = new CompleteResult();
            result.Process = 2006;

            var qr = JsonConvert.DeserializeObject<ConplateInfo?>(message);

            if (qr != null && qr.Process == 2005)
            {
                result.SerialNumber = qr.SerialNumber;
                result.ConfirmNumber = qr.ConfirmNumber;

                var qrParts = qr.QRCode.Split("-");

                if (qrParts != null && qrParts.Length >= 2)
                {
                    string employeeUID = qr.QRCode.Substring(qrParts[0].Length + 1);

                    var confirm = _db.TripConfirms.FirstOrDefault(x => x.ConfirmNumber.ToString() == qr.ConfirmNumber);
                    var qrReader = _db.Qrreaders.FirstOrDefault(x => x.SerialNumber == qr.SerialNumber);
                    var employee = _db.Employees.FirstOrDefault(x => x.EmployeeUid.ToString() == employeeUID);
                    var location = _db.OurLocations.FirstOrDefault();

                    if (confirm != null)
                    {
                        var trip = _db.Trips.FirstOrDefault(x => x.ConfirmId == confirm.Id && x.TicketNumber == confirm.TicketNumber);

                        if (qrReader != null && qrReader.IsActive == true)
                        {
                            if (employee != null)
                            {
                                var orderRow = _db.OrderRows.FirstOrDefault(x => x.Id == confirm.SaleOrderRowId && x.OrderId == confirm.SaleOrderId);

                                if (trip != null && trip.TripEnd == null)
                                {
                                    if (qr.Duration >= 5)
                                    {
                                        trip.TripEnd = trip.TripStart.Value.AddSeconds(qr.Duration);
                                        trip.UpdateDate = DateTime.Now;
                                        trip.UpdateEmployeeId = employee.Id;

                                        _db.SaveChanges();

                                        if (orderRow != null)
                                        {
                                            orderRow.RowStatusId = 3;
                                            orderRow.TicketTripId = trip.Id;
                                            orderRow.QrreaderId = qrReader.Id;
                                            orderRow.DeviceId = qrReader.SerialNumber;
                                            orderRow.UpdateDate = DateTime.Now;
                                            orderRow.UpdateEmployeeId = employee.Id;
                                            orderRow.SyncDate = DateTime.Now;
                                            _db.SaveChanges();

                                            // create new contexts for SyncService to avoid using disposed contexts
                                            _ = Task.Run(() =>
                                            {
                                                _syncservice.AddQuee("OrderRow", 2, orderRow.Id, orderRow.Uid);
                                            });
                                        }

                                        result.Success = 1;
                                        result.Message = "Thank You";
                                        result.Title = "Completed";
                                        result.LEDTemplate = 3;

                                        return result;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else if (trip != null && trip.TripEnd != null)
                                {
                                    result.Success = 1;
                                    result.Message = "Thank You";
                                    result.Title = "Completed";
                                    result.LEDTemplate = 3;

                                    return result;
                                }
                                else
                                {
                                    result.Success = 0;
                                    result.Message = "No Trip";
                                    result.Title = "Warning";
                                    result.LEDTemplate = 4;

                                    return result;
                                }
                            }
                            else
                            {
                                result.Success = 0;
                                result.Message = "No Employee";
                                result.Title = "Warning";
                                result.LEDTemplate = 4;

                                return result;
                            }
                        }
                        else
                        {
                            result.Success = 0;
                            result.Message = "No Reader";
                            result.Title = "Warning";
                            result.LEDTemplate = 4;

                            return result;
                        }
                    }
                    else
                    {
                        result.Success = 0;
                        result.Message = "No Confirm";
                        result.Title = "Warning";
                        result.LEDTemplate = 4;

                        return result;
                    }
                }
                else
                {
                    result.Success = 0;
                    result.Message = "No Correct";
                    result.Title = "Warning";
                    result.LEDTemplate = 4;

                    return result;
                }
            }
            else
            {
                result.Success = 0;
                result.Message = "No QR";
                result.Title = "Warning";
                result.LEDTemplate = 4;

                return result;
            }
        }

        public async Task<WebSocketResult> CloudRideStartStop(string confirmNumber)
        {
            WebSocketResult result = new WebSocketResult();

            var location = _db.OurLocations.FirstOrDefault();

            result.LocationId = location.Id;
            result.ProcessTime = location.LocalDateTime.ToString();

            if (!string.IsNullOrEmpty(confirmNumber))
            {
                var confirm = _db.VtripConfirms.FirstOrDefault(x => x.ConfirmNumber.ToString() == confirmNumber);

                if (confirm != null)
                {
                    try
                    {
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

                        result.Success = 1;
                        result.Message = confirm.TripEnd != null ? $"Minder {confirm.LocationPartId.Value} de Kullanım Tamamlandı" : $"Minder {confirm.LocationPartId.Value} de Kullanım Başladı";
                        result.ConfirmNumber = confirmNumber;
                    }
                    catch (Exception ex)
                    {
                        result.Success = 0;
                        result.Message = ex.Message;
                        result.ConfirmNumber = confirmNumber;
                    }
                }
                else
                {
                    result.Success = 0;
                    result.Message = "Confirm Not Correct";
                    result.ConfirmNumber = confirmNumber;
                }
            }
            else
            {
                result.Success = 0;
                result.Message = "No Confirm";
                result.ConfirmNumber = "";
            }

            return result;
        }

        public async Task<DrawerResult> AddDrawer(string message)
        {
            DrawerResult result = new DrawerResult();

            var device = JsonConvert.DeserializeObject<DrawerDeviceInfo?>(message);
            result.SerialNumber = device?.SerialNumber ?? "NoSerial";
            result.Process = 1002;

            var location = _db.OurLocations.FirstOrDefault();

            if (device != null)
            {
                var isexists = _db.DrawerDevices.FirstOrDefault(x => x.SerialNumber == device.SerialNumber);

                if (isexists != null)
                {
                    _db.DrawerDevices.Remove(isexists);
                }

                Data.Entities.DrawerDevice newdrawer = new();

                newdrawer.OurCompanyId = location?.OurCompanyId ?? 1;
                newdrawer.LocationId = location?.Id ?? 0;
                newdrawer.Ipaddress = device?.IP;
                newdrawer.Version = device?.Ver.ToString();
                newdrawer.SerialNumber = device?.SerialNumber;
                newdrawer.Uid = Guid.NewGuid();
                newdrawer.IsActive = true;
                newdrawer.PartName = "Cash Drawer";
                newdrawer.DateRecord = DateTime.Now;

                _db.DrawerDevices.Add(newdrawer);
                _db.SaveChanges(true);
            }

            return result;
        }
    }
}