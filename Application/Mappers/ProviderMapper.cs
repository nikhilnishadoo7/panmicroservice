using Newtonsoft.Json;
using PAN.API.Application.DTOs.Common;
using PAN.API.Application.DTOs.Provider;
using PAN.API.Domain.Entities;
using System.Diagnostics;

namespace PAN.API.Application.Mappers;

public static class ProviderMapper
{
    public static PanCommonResponseDto MapSurePass(string json)
    {
        var dto = JsonConvert.DeserializeObject<SurePassResponseDto>(json)
                  ?? throw new Exception("Invalid SurePass response");

        return new PanCommonResponseDto
        {
            IsSuccess = dto.success,
            Pan = dto.data?.pan_number,
            FullName = dto.data?.full_name,
            PanStatus = dto.data?.pan_status,
            Category = dto.data?.category,
            AadhaarLinked = dto.data?.aadhaar_seeding_status == "Y",
            client_id = dto.data?.client_id ?? string.Empty,
            // ✅ FIX
            ProviderName = "surepass",
            PrimaryProvider = "surepass",

            Status = dto.message_code,
            Code = dto.data?.pan_status_desc,
            Message = "Processed by PAN.API"
        };
    }
    public static PanCommonResponseDto MapSprint(string json)
    {
        var dto = JsonConvert.DeserializeObject<SprintVerifyResponseDto>(json)
                  ?? throw new Exception("Invalid Sprint response");
        return new PanCommonResponseDto
        {
            IsSuccess = dto.status == "SUCCESS",
            Pan = dto.data?.idNumber,
            FullName = dto.data?.fullName,
            PanStatus = dto.data?.panStatus,
            Category = "Individual",
            AadhaarLinked = dto.data?.aadhaarSeedingStatus == "Successful",
            ProviderName = "sprintverify",
            PrimaryProvider = "sprintverify",

            client_id = dto.requestId,
            Status = dto.status,
            Code = dto.data?.idStatus,
            Message = "Processed by PAN.API"
        };
    }

    public static PanCommonResponseDto MapFromDb(PanVerification e)
    {
        return new PanCommonResponseDto
        {
            IsSuccess = e.PanLookUpStatus == "SUCCESS",
            // 🔥 FINAL FIX
            Source = "DATABASE",

            Pan = e.EncryptedPan,
            FullName = e.EncryptedFullName,

            PanStatus = e.PanStatus,
            AadhaarLinked = e.IsPanAadhaarLinked ?? false,
            Category = e.PanCardType,

            client_id = e.ProviderRequestId,

            ProviderName = "database",
            PrimaryProvider = "database",

            Status = e.PanLookUpStatus ?? "SUCCESS",
            Code = e.PanStatus ?? "VALID",
            Message = "Fetched from Database",

            FallbackUsed = false
        };
    }
}