// Application/DTOs/Provider/SprintVerifyResponseDto.cs
namespace PAN.API.Application.DTOs.Provider;

public class SprintVerifyResponseDto
{
    public string status { get; set; }
    public Data data { get; set; }

    public class Data
    {
        public string? Pan { get; set; }
        public string idStatus { get; set; }
        public string panStatus { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string firstName { get; set; }
        public string fullName { get; set; }
        public string idHolderTitle { get; set; }
        public string idLastUpdated { get; set; }
        public string aadhaarSeedingStatus { get; set; }
    }
}