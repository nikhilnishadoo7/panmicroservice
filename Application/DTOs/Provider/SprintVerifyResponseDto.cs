public class SprintVerifyResponseDto
{
    public string status { get; set; }
    public string requestId { get; set; }

    public SprintData data { get; set; }
}

public class SprintData
{
    public string idNumber { get; set; }   // ✔ must match JSON
    public string fullName { get; set; }
    public string panStatus { get; set; }
    public string idStatus { get; set; }
    public string aadhaarSeedingStatus { get; set; }
}