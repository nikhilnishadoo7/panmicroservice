namespace PAN.API.Domain.Entities;

public class PanVerification
{
    public Guid Id { get; set; }

    public string CorrelationId { get; set; }

    public long MasterId { get; set; }   

    public string? ProviderRequestId { get; set; }

    public string PanHash { get; set; }
    public string EncryptedPan { get; set; }

    public string PanStatus { get; set; }
    public string PanLookUpStatus { get; set; }

    public string? ProviderName { get; set; }
    public string EncryptedFullName { get; set; }
    public string PanCardType { get; set; }

    public bool? IsPanAadhaarLinked { get; set; }

    public string CallerIp { get; set; }

    public DateTime CreatedAt { get; set; }
}