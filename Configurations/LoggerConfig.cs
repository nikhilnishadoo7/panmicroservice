using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;

namespace PAN.API.Configurations;

[ExcludeFromCodeCoverage]
public static class LoggerConfig
{
    public static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()

            .WriteTo.Console()

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("LogType", out var val) &&
                    val.ToString().Trim('"') == "APP")
                .WriteTo.File(
                    path: LogPathHelper.GetPath("APP"),
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    buffered: false
                )
            )

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("LogType", out var val) &&
                    val.ToString().Trim('"') == "REQUEST")
                .WriteTo.File(
                    path: LogPathHelper.GetPath("REQUEST"),
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    buffered: false
                )
            )

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("LogType", out var val) &&
                    val.ToString().Trim('"') == "RESPONSE")
                .WriteTo.File(
                    path: LogPathHelper.GetPath("RESPONSE"),
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    buffered: false
                )
            )

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("LogType", out var val) &&
                    val.ToString().Trim('"') == "ERROR")
                .WriteTo.File(
                    path: LogPathHelper.GetPath("ERROR"),
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    buffered: false
                )
            )

            .CreateLogger();
    }
}