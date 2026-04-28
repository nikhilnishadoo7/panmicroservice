using PAN.API.Infrastructure.Logging;
using Serilog;
using System.Runtime.CompilerServices;

public static class SafeLogger
{
    
    public static void Request(string message, string correlationId = "")
    {
        Log.ForContext("LogType", "REQUEST")
                           .Information($@"
                ----- REQUEST START -----
                CorrelationId : {correlationId}

                {MaskingHelper.MaskSensitiveData(message)}

                ----- REQUEST END -----
                ");
     }

    
    public static void Response(string message, string correlationId = "")
    {
        Log.ForContext("LogType", "RESPONSE")
                           .Information($@"
                ----- RESPONSE START -----
                CorrelationId : {correlationId}

                {MaskingHelper.MaskSensitiveData(message)}

                ----- RESPONSE END -----
                ");
    }

    
    public static void Error(
        Exception ex,
        string message,
        HttpContext? context = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        var endpoint = context?.Request?.Path;
        var method = context?.Request?.Method;

        Log.ForContext("LogType", "ERROR")
                               .Error($@"
                    ===== ERROR START =====

                    Message   : {MaskingHelper.MaskSensitiveData(message)}
                    Endpoint  : {endpoint}
                    Method    : {method}

                    File      : {Path.GetFileName(file)}
                    Line      : {line}

                    Exception : {ex.Message}

                    StackTrace:
                    {ex.StackTrace}

                    ===== ERROR END =====
                    ");
                        }

    
    public static void App(
        string message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Log.ForContext("LogType", "APP")
                               .Information($@"
                                ===== [APP FLOW] =====
                
                    File : {Path.GetFileName(file)}
                    Line : {line}
                    Message : {MaskingHelper.MaskSensitiveData(message)}
                    ");
    }
}