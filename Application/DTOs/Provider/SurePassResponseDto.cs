// Application/DTOs/Provider/SurePassResponseDto.cs
namespace PAN.API.Application.DTOs.Provider;

public class SurePassResponseDto
{
    public Data data { get; set; }
    public int status_code { get; set; }
    public bool success { get; set; }
    public object message { get; set; }
    public string message_code { get; set; }

    public class Data
    {
        public string client_id { get; set; }
        public string pan_number { get; set; }
        public string full_name { get; set; }
        public string pan_status { get; set; }
        public string pan_status_desc { get; set; }
        public string aadhaar_seeding_status { get; set; }
        public string aadhaar_seeding_status_desc { get; set; }
        public string category { get; set; }
    }
}