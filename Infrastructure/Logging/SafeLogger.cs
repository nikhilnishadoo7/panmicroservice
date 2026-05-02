using PAN.API.Infrastructure.Logging;
using Serilog;
using System.Runtime.CompilerServices;

using Serilog;

namespace PAN.API.Infrastructure.Logging;

public static class SafeLogger
{
    public static void App(string eventName, object? data = null)
    {
        Log.ForContext("LogType", "APP", destructureObjects: false)
           .Information("{event_name} {@Data}", eventName, data);
    }

    public static void Request(object data)
    {
        Log.ForContext("LogType", "REQUEST", false)
           .Information("REQUEST {@Data}", data);
    }

    public static void Response(object data)
    {
        Log.ForContext("LogType", "RESPONSE", false)
           .Information("RESPONSE {@Data}", data);
    }

    public static void Error(Exception ex, string eventName, object? data = null)
    {
        Log.ForContext("LogType", "ERROR", false)
           .Error(ex, "{event_name} {@Data}", eventName, data);
    }
}