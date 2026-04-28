namespace PAN.API.Domain.Entities;

public class providerpanmaster
{
    public long Id { get; set; }  

    public string ProviderName { get; set; }
    public string BaseUrl { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }

    public int Priority { get; set; }
    public bool IsActive { get; set; }
}