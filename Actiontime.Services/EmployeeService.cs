using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Employee = Actiontime.Data.Entities.Employee;


namespace Actiontime.Services
{
	public class EmployeeService
	{
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;
        private readonly CloudService _cloudService;
        private ApplicationDbContext db;
        private ApplicationCloudDbContext cdb;

        public EmployeeService(IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextFactory<ApplicationCloudDbContext> cdbFactory, CloudService cloudService)
		{

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _cdbFactory = cdbFactory ?? throw new ArgumentNullException(nameof(cdbFactory));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
        }

        public Employee? CheckEmployeeLogin(string Username, string Password)
		{
            using var _db = _dbFactory.CreateDbContext();
            //using var _cdb = _cdbFactory.CreateDbContext();

            var _password = ServiceHelper.MD5Hash(Password).ToUpper();
			return _db.Employees.FirstOrDefault(x => x.Username == Username && x.Password.ToUpper() == _password);
		}

		public Employee? GetEmployee(int employeeID)
		{
            using var _db = _dbFactory.CreateDbContext();
            //using var _cdb = _cdbFactory.CreateDbContext();
            return _db.Employees.FirstOrDefault(x => x.Id == employeeID);
		}

		public List<Employee>? GetEmployeeList()
		{
            using var _db = _dbFactory.CreateDbContext();
            //using var _cdb = _cdbFactory.CreateDbContext();
            return _db.Employees.ToList();
		}

		public EmployeeSchedule? GetEmployeeSchedule(DateTime date, int employeeId)
		{
            using var _db = _dbFactory.CreateDbContext();

            var location = _db.OurLocations.FirstOrDefault();

			return _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employeeId && x.ScheduleDate == date && x.LocationId == location.Id);
		}

		public List<EmployeeSchedule>? GetEmployeeSchedules(DateTime date, int employeeId)
		{
            using var _db = _dbFactory.CreateDbContext();

            var location = _db.OurLocations.FirstOrDefault();

			var schedule = _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employeeId && x.ScheduleDate == date && x.LocationId == location.Id);

			return _db.EmployeeSchedules.Where(x => x.EmployeeId == employeeId && x.ScheduleWeek == schedule.ScheduleWeek && x.LocationId == location.Id).ToList();
		}

		//
		public void GetSchedules()
		{
            using var _db = _dbFactory.CreateDbContext();

            var sqlemp = "EXEC GetEmployeeSchedules";
			_db.Database.ExecuteSqlRaw(sqlemp);

			var sqlloc = "EXEC GetLocationSchedules";
			_db.Database.ExecuteSqlRaw(sqlloc);
		}

		public void GetShifts()
		{
            using var _db = _dbFactory.CreateDbContext();
            using var _cdb = _cdbFactory.CreateDbContext();

            var location = _db.OurLocations.FirstOrDefault();
            
            //Localler
            var locShift = _db.LocationShifts.FirstOrDefault(x => x.ShiftDate == location.LocalDate && x.LocationId == location.Id);
			var empShifts = _db.EmployeeShifts.Where(x => x.ShiftDate == location.LocalDate && x.LocationId == location.Id).ToList();
			var empBreaks = _db.EmployeeBreaks.Where(x => x.BreakDate == location.LocalDate && x.LocationId == location.Id).ToList();

			//Cloudlar
			var clocShift = _cdb.LocationShifts.FirstOrDefault(x => x.LocationId == location.Id && x.ShiftDate == location.LocalDate);
			var cempShifts = _cdb.EmployeeShifts.Where(x => x.ShiftDate == location.LocalDate && x.IsWorkTime == true && x.LocationId == location.Id).ToList();
			var cempBreaks = _cdb.EmployeeShifts.Where(x => x.ShiftDate == location.LocalDate && x.IsBreakTime == true && x.LocationId == location.Id).ToList();


			//Lokasyon mesai
			if (clocShift != null)
			{
				if (locShift != null && clocShift.Uid == locShift.Uid)
				{
					locShift.ShiftStart = clocShift.ShiftDateStart.Value;
					locShift.ShiftFinish = clocShift.ShiftDateFinish;
					_db.SaveChanges();

				}
				else if (locShift != null && clocShift.Uid != locShift.Uid)
				{
					locShift.ShiftStart = clocShift.ShiftDateStart.Value;
					locShift.ShiftFinish = clocShift.ShiftDateFinish;
					locShift.Uid = clocShift.Uid.Value;

					_db.SaveChanges();

				}
				else if (locShift == null)
				{
					Data.Entities.LocationShift locationShift = new Data.Entities.LocationShift();

					locationShift.LocationId = clocShift.LocationId;
					locationShift.EmployeeId = clocShift.EmployeeId;
					locationShift.ShiftDate = clocShift.ShiftDate;
					locationShift.ShiftStart = clocShift.ShiftDateStart.Value;
					locationShift.ShiftFinish = clocShift.ShiftDateFinish;
					locationShift.RecordEmployeeId = clocShift.RecordEmployeeId;
					locationShift.RecordDate = clocShift.RecordDate;
					locationShift.Uid = clocShift.Uid.Value;

					_db.LocationShifts.Add(locationShift);
					_db.SaveChanges();
				}

			}

			//Çalışan Mesaileri
			if (cempShifts != null && cempShifts.Count > 0)
			{
				foreach (var item in cempShifts)
				{

					var empshift = empShifts.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);


					if (empshift != null && empshift.Uid == item.Uid)
					{
						empshift.ShiftStart = item.ShiftDateStart.Value;
						empshift.ShiftEnd = item.ShiftDateEnd;

						_db.SaveChanges();

					}
					else if (empshift != null && empshift.Uid != empshift.Uid)
					{
						empshift.ShiftStart = item.ShiftDateStart.Value;
						empshift.ShiftEnd = item.ShiftDateEnd;
						empshift.Uid = item.Uid.Value;

						_db.SaveChanges();

					}
					else if (empshift == null)
					{
						Data.Entities.EmployeeShift employeeShift = new Data.Entities.EmployeeShift();

						employeeShift.LocationId = item.LocationId.Value;
						employeeShift.EmployeeId = item.EmployeeId.Value;
						employeeShift.ShiftDate = item.ShiftDate.Value;
						employeeShift.ShiftStart = item.ShiftDateStart.Value;
						employeeShift.ShiftEnd = item.ShiftDateEnd;
						employeeShift.RecordEmployeeId = item.RecordEmployeeId.Value;
						employeeShift.RecordDate = item.RecordDate.Value;
						employeeShift.Uid = item.Uid.Value;

						_db.EmployeeShifts.Add(employeeShift);
						_db.SaveChanges();
					}
				}

			}

			//Çalışan Molaları
			if (cempBreaks != null && cempBreaks.Count > 0)
			{
				foreach (var item in cempBreaks)
				{

					var empbreak = empBreaks.FirstOrDefault(x => x.EmployeeId == item.EmployeeId && x.Uid == item.Uid);


					if (empbreak != null)
					{
						empbreak.BreakStart = item.ShiftDateStart.Value;
						empbreak.BreakEnd = item.ShiftDateEnd;

						_db.SaveChanges();

					}

					else if (empbreak == null)
					{
						Data.Entities.EmployeeBreak employeeBreak = new Data.Entities.EmployeeBreak();

						employeeBreak.LocationId = item.LocationId.Value;
						employeeBreak.EmployeeId = item.EmployeeId.Value;
						employeeBreak.BreakDate = item.ShiftDate.Value;
						employeeBreak.BreakStart = item.ShiftDateStart.Value;
						employeeBreak.BreakEnd = item.ShiftDateEnd;
						employeeBreak.RecordEmployeeId = item.RecordEmployeeId.Value;
						employeeBreak.RecordDate = item.RecordDate.Value;
						employeeBreak.Uid = item.Uid.Value;

						_db.EmployeeBreaks.Add(employeeBreak);
						_db.SaveChanges();
					}
				}

			}

		}



		public void GetLookups()
		{
            using var _db = _dbFactory.CreateDbContext();

            var sql = "EXEC GetLookups";
			_db.Database.ExecuteSqlRaw(sql);
		}

		//GetPersonInfo  GetPersonInfoList
		public PersonInfo GetPersonInfo(int employeeID)
		{
            using var _db = _dbFactory.CreateDbContext();
           

            PersonInfo info = new PersonInfo();
			info.Id = employeeID;

			var employee = _db.Employees.FirstOrDefault(x => x.Id == employeeID);
			string statusName = "Waiting";

			if (employee != null)
			{
				var location = _db.OurLocations.FirstOrDefault();
				var datelist = _db.DatePeriods.FirstOrDefault(x => x.Date == location.LocalDate);


				info.Id = employee.Id;
				info.UID = employee.EmployeeUid.ToString();
				info.FullName = employee.FullName;
				info.PhotoName = !string.IsNullOrEmpty(employee.FotoFile) ? employee.FotoFile : "user.png";
				info.StatusName = "";
				info.DateSelected = location?.LocalDateTime?.ToString("yyyy-MM-dd HH:mm");
				info.WeekSelected = datelist?.PeriodNumber;
				info.ScheduleDuration = "00:00";

				var empSchedule = _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employeeID && x.ScheduleDate == location.LocalDate && x.LocationId == location.Id);
				if (empSchedule != null)
				{
					info.ScheduleTime = $"{empSchedule.ShiftStart.ToString("HH:mm")} - {empSchedule.ShiftEnd?.ToString("HH:mm")}";
					info.ScheduleDuration = empSchedule.ShiftDuration?.ToString(@"hh\:mm");
				}
				else
				{
					statusName = "Off Day";
				}

				var empShift = _db.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeID && x.ShiftDate == location.LocalDate && x.LocationId == location.Id);
				if (empShift != null)
				{
					if (empShift.ShiftStart != null && empShift.ShiftEnd != null)
					{
						statusName = "Shift is Over";
					}
					else if (empShift.ShiftStart != null && empShift.ShiftEnd == null)
					{
						statusName = "At Work";
					}

					info.ShiftTime = $"{empShift.ShiftStart.ToString("HH:mm")} - {empShift.ShiftEnd?.ToString("HH:mm")}";
					info.ShiftDuration = empShift.ShiftDuration?.ToString(@"hh\:mm");
				}


				var empBreaks = _db.EmployeeBreaks.Where(x => x.EmployeeId == employeeID && x.BreakDate == location.LocalDate && x.LocationId == location.Id).ToList();
				if (empBreaks != null)
				{
					if (empBreaks.Any(x => x.BreakStart != null && x.BreakEnd == null))
					{
						statusName = "At Break";
					}
					info.BreakTotalTime = $"{empBreaks.Sum(x => x.DurationMinute)} Mn.";
					info.BreakCount = empBreaks.Count;
				}

			}

			info.StatusName = statusName;

			return info;
		}


		public List<PersonInfo> GetPersonInfoList()
		{
            using var _db = _dbFactory.CreateDbContext();

            List<PersonInfo> infos = new List<PersonInfo>();

			var employees = _db.Employees.ToList();
			var location = _db.OurLocations.FirstOrDefault();

			foreach (var employee in employees)
			{
				PersonInfo info = new PersonInfo();

				string statusName = "Waiting";

				var datelist = _db.DatePeriods.FirstOrDefault(x => x.Date == location.LocalDate);

				info.Id = employee.Id;
				info.UID = employee.EmployeeUid.ToString();
				info.FullName = employee.FullName;
				info.PhotoName = !string.IsNullOrEmpty(employee.FotoFile) ? employee.FotoFile : "user.png";
				info.StatusName = "";
				info.DateSelected = location?.LocalDateTime?.ToString("yyyy-MM-dd HH:mm");
				info.WeekSelected = datelist?.PeriodNumber;
				info.ScheduleDuration = "00:00";

				var empSchedule = _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employee.Id && x.ScheduleDate == location.LocalDate && x.LocationId == location.Id);
				if (empSchedule != null)
				{
					info.ScheduleTime = $"{empSchedule.ShiftStart.ToString("HH:mm")} - {empSchedule.ShiftEnd?.ToString("HH:mm")}";
					info.ScheduleDuration = empSchedule.ShiftDuration?.ToString(@"hh\:mm");
				}
				else
				{
					statusName = "Off Day";
				}

				var empShift = _db.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employee.Id && x.ShiftDate == location.LocalDate && x.LocationId == location.Id);
				if (empShift != null)
				{
					if (empShift.ShiftStart != null && empShift.ShiftEnd != null)
					{
						statusName = "Shift is Over";
					}
					else if (empShift.ShiftStart != null && empShift.ShiftEnd == null)
					{
						statusName = "At Work";
					}

					info.ShiftTime = $"{empShift.ShiftStart.ToString("HH:mm")} - {empShift.ShiftEnd?.ToString("HH:mm")}";
					info.ShiftDuration = empShift.ShiftDuration?.ToString(@"hh\:mm");
				}


				var empBreaks = _db.EmployeeBreaks.Where(x => x.EmployeeId == employee.Id && x.BreakDate == location.LocalDate && x.LocationId == location.Id).ToList();
				if (empBreaks != null)
				{
					if (empBreaks.Any(x => x.BreakStart != null && x.BreakEnd == null))
					{
						statusName = "At Break";
					}
					info.BreakTotalTime = $"{empBreaks.Sum(x => x.DurationMinute)} Mn.";
					info.BreakCount = empBreaks.Count;
				}

				info.StatusName = statusName;

				infos.Add(info);
			}

			return infos;
		}

		//CheckEmployeeShift
		public string CheckEmployeeShift(int employeeId)
		{
            using var _db = _dbFactory.CreateDbContext();
            using var _cdb = _cdbFactory.CreateDbContext();

            string result = string.Empty;
			WebSocketService webSocketService = new WebSocketService();

			var dateKey = DateTime.Now;
			var date = dateKey.Date;
			var location = _db.OurLocations.FirstOrDefault();

			dateKey = location.LocalDateTime ?? dateKey;
			date = location.LocalDate ?? date;

			if (location != null)
			{
				var locSchedule = _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == date && x.LocationId == location.Id);
				var empSchedule = _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employeeId && x.ScheduleDate == date && x.LocationId == location.Id);
				var employeeFullName = _db.Employees.FirstOrDefault(x => x.Id == employeeId)?.FullName;

				if (locSchedule != null)
				{
					if (empSchedule != null)
					{
						var startTime = empSchedule.ShiftStart.AddHours(-1);
						var endTime = empSchedule.ShiftEnd != null ? empSchedule.ShiftEnd?.AddHours(1) : dateKey.AddHours(1);


                        if (startTime <= dateKey && endTime >= dateKey)
						{
							var empShift = _db.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeId && x.ShiftDate == date && x.LocationId == location.Id);
							if (empShift == null)
							{
								empShift = new Data.Entities.EmployeeShift()
								{
									EmployeeId = employeeId,
									ShiftDate = date,
									LocationId = location.Id,
									ShiftStart = dateKey,
									ShiftEnd = null,
									RecordEmployeeId = employeeId,
									RecordDate = dateKey
								};

								_db.EmployeeShifts.Add(empShift);
								_db.SaveChanges();

								result = "Employee Shift Started";

								SyncProcess process = new SyncProcess()
								{
									DateCreate = DateTime.Now,
									Entity = "EmployeeShift",
									Process = 1,
									EntityId = empShift.Id,
									EntityUid = empShift.Uid,
								};

								_db.SyncProcesses.Add(process);
								_db.SaveChanges(true);

								
								Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

								WebSocketResult wsresult = new WebSocketResult()
								{
									ConfirmNumber = Guid.NewGuid().ToString(),
									LocationId = location.Id,
									Process = WebSocketProcess.EmployeeShiftStart.ToString(),
									ProcessTime = dateKey.ToString(),
									Success = 1,
									Message = $"{employeeFullName} Çalışanının {location.LocationName} Lokasyonunda Mesaisi Başladı."
								};

								Task task2 = Task.Run(() => webSocketService.SendWebSocketMessage(wsresult));

							}
							else
							{
								if (empShift.Duration == null && empShift.ShiftEnd == null)
								{
									empShift.ShiftEnd = dateKey;
									_db.SaveChanges();

									SyncProcess process = new SyncProcess()
									{
										DateCreate = DateTime.Now,
										Entity = "EmployeeShift",
										Process = 2,
										EntityId = empShift.Id,
										EntityUid = empShift.Uid,
									};

									_db.SyncProcesses.Add(process);
									_db.SaveChanges(true);

									Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

									
									WebSocketResult wsresult = new WebSocketResult()
									{
										ConfirmNumber = Guid.NewGuid().ToString(),
										LocationId = location.Id,
										Process = WebSocketProcess.EmployeeShiftStop.ToString(),
										ProcessTime = dateKey.ToString(),
										Success = 1,
										Message = $"{employeeFullName} Çalışanının {location.LocationName} Lokasyonunda Mesaisi Bitti."
									};

									Task task2 = Task.Run(() => webSocketService.SendWebSocketMessage(wsresult));


									var empBreak = _db.EmployeeBreaks.Where(x => x.EmployeeId == employeeId && x.BreakDate == date && x.LocationId == location.Id).ToList().OrderByDescending(x => x.BreakStart).FirstOrDefault();

									if (empBreak != null && empBreak.DurationMinute == null)
									{
										empBreak.BreakEnd = dateKey;
										_db.SaveChanges();


										SyncProcess processb = new SyncProcess()
										{
											DateCreate = DateTime.Now,
											Entity = "EmployeeBreak",
											Process = 2,
											EntityId = empBreak.Id,
											EntityUid = empBreak.Uid,
										};

										_db.SyncProcesses.Add(processb);
										_db.SaveChanges(true);

										Task taskb = Task.Run(() => _cloudService.AddCloudProcess(processb));

										WebSocketResult wsresultb = new WebSocketResult()
										{
											ConfirmNumber = Guid.NewGuid().ToString(),
											LocationId = location.Id,
											Process = WebSocketProcess.EmployeeBreakStop.ToString(),
											ProcessTime = dateKey.ToString(),
											Success = 1,
											Message = $"{employeeFullName} Çalışanının {location.LocationName} Lokasyonunda Molası Bitti."
										};

										Task task2b = Task.Run(() => webSocketService.SendWebSocketMessage(wsresultb));

									}

									result = "Employee Shift & Employee Break Finished";
								}
								else
								{
									result = "Employee Shift Already Finished";
								}
							}
						}
						else
						{
							result = "Now is outside the defined employee schedule time.";
						}
					}
					else
					{
						result = "Employee Schedule not defined";
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

		//CheckEmployeeBreak
		public string CheckEmployeeBreak(int employeeId)
		{
            using var _db = _dbFactory.CreateDbContext();
            using var _cdb = _cdbFactory.CreateDbContext();

            string result = string.Empty;
			WebSocketService webSocketService = new WebSocketService();

			var dateKey = DateTime.Now;//.AddHours(-10);
			var date = dateKey.Date;
			var location = _db.OurLocations.FirstOrDefault();
			dateKey = location.LocalDateTime ?? dateKey;
			date = location.LocalDate ?? date;

			if (location != null)
			{
				var locSchedule = _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == date && x.LocationId == location.Id);
				var empSchedule = _db.EmployeeSchedules.FirstOrDefault(x => x.EmployeeId == employeeId && x.ScheduleDate == date && x.LocationId == location.Id);
				var employeeFullName = _db.Employees.FirstOrDefault(x => x.Id == employeeId)?.FullName;

				if (locSchedule != null)
				{
					if (empSchedule != null)
					{
						if (empSchedule.ShiftStart <= dateKey && empSchedule.ShiftEnd >= dateKey)
						{
							var empShift = _db.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeId && x.ShiftDate == date && x.LocationId == location.Id);


							if (empShift != null)
							{


								if (empShift.Duration == null && empShift.ShiftEnd == null)
								{
									var empBreak = _db.EmployeeBreaks.Where(x => x.EmployeeId == employeeId && x.BreakDate == date && x.LocationId == location.Id).ToList().OrderByDescending(x => x.BreakStart).FirstOrDefault();
									if (empBreak != null && empBreak.DurationMinute == null)
									{
										empBreak.BreakEnd = dateKey;
										_db.SaveChanges();

										result = "Employee Break Finished";

										SyncProcess process = new SyncProcess()
										{
											DateCreate = DateTime.Now,
											Entity = "EmployeeBreak",
											Process = 2,
											EntityId = empBreak.Id,
											EntityUid = empBreak.Uid,
										};

										_db.SyncProcesses.Add(process);
										_db.SaveChanges(true);

										Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

										WebSocketResult wsresultb = new WebSocketResult()
										{
											ConfirmNumber = Guid.NewGuid().ToString(),
											LocationId = location.Id,
											Process = WebSocketProcess.EmployeeBreakStop.ToString(),
											ProcessTime = dateKey.ToString(),
											Success = 1,
											Message = $"{employeeFullName} Çalışanının {location.LocationName} Lokasyonunda Molası Bitti."
										};

										Task task2b = Task.Run(() => webSocketService.SendWebSocketMessage(wsresultb));


									}
									else if (empBreak == null || empBreak.DurationMinute != null)
									{
										empBreak = new EmployeeBreak()
										{
											EmployeeId = employeeId,
											BreakDate = date,
											LocationId = location.Id,
											BreakStart = dateKey,
											BreakEnd = null,
											RecordEmployeeId = employeeId,
											RecordDate = dateKey
										};

										_db.EmployeeBreaks.Add(empBreak);
										_db.SaveChanges();

										result = "Employee Break Started";

										SyncProcess process = new SyncProcess()
										{
											DateCreate = DateTime.Now,
											Entity = "EmployeeBreak",
											Process = 1,
											EntityId = empBreak.Id,
											EntityUid = empBreak.Uid,
										};

										_db.SyncProcesses.Add(process);
										_db.SaveChanges(true);

										Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

										WebSocketResult wsresultb = new WebSocketResult()
										{
											ConfirmNumber = Guid.NewGuid().ToString(),
											LocationId = location.Id,
											Process = WebSocketProcess.EmployeeBreakStart.ToString(),
											ProcessTime = dateKey.ToString(),
											Success = 1,
											Message = $"{employeeFullName} Çalışanının {location.LocationName} Lokasyonunda Molası Başladı."
										};

										Task task2b = Task.Run(() => webSocketService.SendWebSocketMessage(wsresultb));

									}

								}
								else
								{
									result = "Employee Shift Already Finished";
								}


							}
							else
							{
								result = "Employee Shift No Started Yet";
							}
						}
						else
						{
							result = "Now is out of defined Employee Schedule time";
						}
					}
					else
					{
						result = "Employee Schedule not defined";
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


	}
}
