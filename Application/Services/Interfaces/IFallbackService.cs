using System.Threading.Tasks;

namespace PAN.API.Application.Services.Interfaces;

public interface IFallbackService
{
    Task<(bool success, object? response, string providerName)> ExecuteAsync(string pan, string correlationId);
}