using System.Text.Json.Serialization;

namespace PAN.API.Application.DTOs.Request;

public class PanRequest
{
    private string? _idNumber;

    [JsonPropertyName("id_number")]
    public string? IdNumber
    {
        get => _idNumber;
        
        set => _idNumber = string.IsNullOrWhiteSpace(value) ? null : value.ToUpper().Trim();
    }
}