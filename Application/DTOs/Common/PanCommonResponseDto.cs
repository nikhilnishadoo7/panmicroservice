namespace PAN.API.Application.DTOs.Common;

public class PanCommonResponseDto
{
    public bool IsSuccess { get; set; }
    public string Source { get; set; } = "PROVIDER";
    public bool ProviderCacheHit { get; set; } = false;
    public long MasterId { get; set; }
    public string? Pan { get; set; }
    public string? FullName { get; set; }
    public string? PanStatus { get; set; }
    public bool AadhaarLinked { get; set; }
    public string? Category { get; set; }

    
    public string? client_id { get; set; }

    public string? ProviderName { get; set; } 

   
    public string Status { get; set; } = "UNKNOWN";
    public string Code { get; set; } = "NA";
    public string Message { get; set; } = "No message";

    
    public bool FallbackUsed { get; set; } = false;
    public string? PrimaryProvider { get; set; }
}