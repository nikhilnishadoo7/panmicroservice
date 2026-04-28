public class PanResponseJson
{
    public long Id { get; set; }

    public string CorrelationId { get; set; }

    public Guid PanVerificationId { get; set; }

    public string RequestId { get; set; }                  

    public string EncryptedRawResponseJson { get; set; }

    public DateTime CreatedAt { get; set; }
}