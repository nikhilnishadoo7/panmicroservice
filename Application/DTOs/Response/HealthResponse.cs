namespace PAN.API.Application.DTOs.Response;

public class HealthResponse
{
    public string Status { get; set; }
    public string Service { get; set; }
    public string Version { get; set; }
    public DateTime Timestamp { get; set; }
}