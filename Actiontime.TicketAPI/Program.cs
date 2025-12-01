using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            //builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            var cloudConn = builder.Configuration.GetConnectionString("CloudConnection");
            //builder.Services.AddDbContext<ApplicationCloudDbContext>(opts => opts.UseSqlServer(cloudConn));
            builder.Services.AddDbContextFactory<ApplicationCloudDbContext>(opts => opts.UseSqlServer(cloudConn));

            builder.Services.AddControllers();
            builder.Services.AddHostedService<BackgroundWorkerService>();
            //builder.Services.AddTransient<BackgroundWorkerService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", build =>
                {
                    build.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseCors("AllowAll");

            app.Run();
        }
    }
}