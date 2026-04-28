namespace PAN.API.Application.DTOs.Response;

public class HealthReadyResponse
{
    public string Status { get; set; }
    public object Checks { get; set; }
    public string Service { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}