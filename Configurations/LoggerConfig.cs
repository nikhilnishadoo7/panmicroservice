using Serilog;
using Serilog.Events;

namespace PAN.API.Configurations;

public static class LoggerConfig
{
    public static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()

           
            .WriteTo.Console()

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.ContainsKey("LogType") &&
                    e.Properties["LogType"].ToString().Trim('"') == "APP")
                .WriteTo.File(LogPathHelper.GetPath("application"),
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Message}{NewLine}")
            )

           
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.ContainsKey("LogType") &&
                    e.Properties["LogType"].ToString().Trim('"') == "REQUEST")
                .WriteTo.File(LogPathHelper.GetPath("request"),
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Message}{NewLine}")
            )

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.ContainsKey("LogType") &&
                    e.Properties["LogType"].ToString().Trim('"') == "RESPONSE")
                .WriteTo.File(LogPathHelper.GetPath("response"),
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Message}{NewLine}")
            )

           
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => 
                    e.Properties.ContainsKey("LogType") &&
                    e.Properties["LogType"].ToString().Trim('"') == "ERROR")
                .WriteTo.File(LogPathHelper.GetPath("error"),
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Message}{NewLine}{Exception}")
            )

            .CreateLogger();
    }
}