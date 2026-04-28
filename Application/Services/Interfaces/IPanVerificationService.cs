using PAN.API.Application.DTOs.Common;
using PAN.API.Application.DTOs.Request;
using System.Threading.Tasks;

namespace PAN.API.Application.Services.Interfaces;

public interface IPanVerificationService
{
    Task<PanCommonResponseDto> PanVerifyAsync(
        PanRequest request,
        string correlationId,
        string ip);
}