namespace PAN.API.Application.DTOs.Response;

public class ApiResponse<T>
{
    public string Status { get; set; }       
    public string Code { get; set; }         
    public string Message { get; set; }
    public string? CorrelationId { get; set; }
    public string? ProviderUsed { get; set; }
    public MetaInfo? Meta { get; set; }
    public T? Data { get; set; }
}

public class MetaInfo
{
    public bool FallbackUsed { get; set; }
    public string? PrimaryProvider { get; set; }
    public bool ProviderCacheHit { get; set; }
}