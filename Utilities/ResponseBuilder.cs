using PAN.API.Application.DTOs.Common;
using PAN.API.Application.DTOs.Response;

namespace PAN.API.Utilities;

public static class ResponseBuilder
{
    // ─── SUCCESS ────────────────────────────────────────────
    public static ApiResponse<object> PanVerified(
    PanCommonResponseDto res,
    string correlationId)
    {
        return new ApiResponse<object>
        {
            Status = "SUCCESS",
            Code = "PAN_VERIFIED",
            Message = "PAN is valid",
            CorrelationId = correlationId,

            ProviderUsed = res.ProviderName,

            Meta = new MetaInfo
            {
                FallbackUsed = res.FallbackUsed,
                PrimaryProvider = res.PrimaryProvider,
                ProviderCacheHit = res.ProviderCacheHit
            },

            Data = new
            {
                pan = res.Pan,
                fullName = res.FullName,
                panStatus = res.PanStatus,
                aadhaarLinked = res.AadhaarLinked,
                category = res.Category,
                source = res.Source // 🔥 REQUIRED
            }
        };
    }

    public static ApiResponse<object> PanInvalid(
        PanCommonResponseDto res,
        string correlationId)
    {
        return new ApiResponse<object>
        {
            Status = "FAILED",
            Code = "PAN_INVALID",
            Message = "PAN is invalid or not found",
            CorrelationId = correlationId,

            ProviderUsed = GetProvider(res),

            Meta = new MetaInfo
            {
                FallbackUsed = res.FallbackUsed,
                PrimaryProvider = res.PrimaryProvider?.ToLower()
            },

            Data = new
            {
                pan = res.Pan,
                panStatus = res.PanStatus,
                source = res.Source
            }
        };
    }

    // 🔥 CENTRALIZED FIX
    private static string GetProvider(PanCommonResponseDto res)
    {
        if (!string.IsNullOrEmpty(res.ProviderName))
            return res.ProviderName.ToLower();

        if (!string.IsNullOrEmpty(res.PrimaryProvider))
            return res.PrimaryProvider.ToLower();

        return "unknown";
    }

    // ─── ERRORS ─────────────────────────────────────────────

    public static ApiResponse<object> InvalidRequest(
        string message,
        string? correlationId = null)
    {
        return Error("INVALID_REQUEST", message, correlationId);
    }

    public static ApiResponse<object> InvalidPanFormat(
        string? correlationId = null)
    {
        return Error("INVALID_PAN_FORMAT",
            "PAN format is invalid. Expected: ABCDE1234F",
            correlationId);
    }

    public static ApiResponse<object> AllProvidersFailed(
        string? correlationId = null)
    {
        return Error("PROVIDER_FAILURE",
            "All PAN providers failed. Try again later.",
            correlationId);
    }

    public static ApiResponse<object> ServerError(
        string? correlationId = null)
    {
        return Error("INTERNAL_SERVER_ERROR",
            "Something went wrong. Please try again.",
            correlationId,
            httpStatus: 500);
    }

    public static ApiResponse<object> Error(
        string code,
        string message,
        string? correlationId = null,
        int httpStatus = 400)
    {
        return new ApiResponse<object>
        {
            Status = "FAILED",
            Code = code,
            Message = message,
            CorrelationId = correlationId,
            Data = null,
            Meta = null
        };
    }
}