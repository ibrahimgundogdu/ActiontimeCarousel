using Actiontime.Data.Context;
using Actiontime.DataCloud.Context;
using Actiontime.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyAppServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContextler
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var cloudConnectionString = configuration.GetConnectionString("CloudConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddDbContext<ApplicationCloudDbContext>(options =>
            {
                options.UseSqlServer(cloudConnectionString);
            });

            // Servisler

            services.AddScoped<IAppAuthenticationService, AppAuthenticationService>();
            services.AddScoped<ICashService, CashService>();
            services.AddScoped<ICloudService, CloudService>();
            services.AddScoped<ISaleOrderService, SaleOrderService>();
            services.AddScoped<IConnectivityService, ConnectivityService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IQRReaderService, QRReaderService>();
            services.AddScoped<ISyncService, SyncService>();
            services.AddScoped<IWebSocketService, WebSocketService>();
            services.AddScoped<IServiceHelper, ServiceHelper>();
            




            return services;
        }
    }
}
