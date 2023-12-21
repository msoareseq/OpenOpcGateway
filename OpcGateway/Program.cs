
using OpcUaApi.Drivers;
using OpcUaApi.Services;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace OpcGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string? baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(@"C:\BioCal\logs\opc\log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information("Starting up OPCGateway Application...");

            if (args.Length > 0)
            {
                baseDir = args[0];
            }
            
            Log.Information("Base Directory: {0}", baseDir);

            if (baseDir == null || !File.Exists(Path.Combine(baseDir,"appsettings.json")))
            {
                throw new Exception("Base Directory is not set or appsettings.json is missing.");
            }

            var options = new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
            };

            if (WindowsServiceHelpers.IsWindowsService())
            {
                Log.Information("Running as a Windows Service");
            }
            else
            {
                Log.Information("Running as a Console Application");
            }

            Log.Information("Base dir: {0}", baseDir);
            Log.Information("Content Root Path: {0}", options.ContentRootPath);
            Log.Information("Arguments: {0}", string.Join(", ", options.Args));
            
            try
            {
                Log.Information("Starting up the web application...");

                var builder = WebApplication.CreateBuilder(options);
                builder.Host.UseSerilog();
                builder.Host.UseWindowsService();

                // Add services to the container.
                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddSingleton<IOpcUaClient, OpcUaClientService>();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseAuthorization();


                app.MapControllers();

                app.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal Error - Application terminated.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}