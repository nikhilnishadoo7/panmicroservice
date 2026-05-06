namespace PAN.API.Domain.Entities;

public class PanResponseJson
{
    public long Id { get; set; }

    public long PanVerificationId { get; set; }

    public string RequestId { get; set; }                  

    public string EncryptedRawResponseJson { get; set; }

    public DateTime CreatedAt { get; set; }
}