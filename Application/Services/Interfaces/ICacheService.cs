using PAN.API.Domain.Entities;

namespace PAN.API.Application.Services.Interfaces;

public interface ICacheService
{
    List<providerpanmaster> GetProviders();
    void SetProviders(List<providerpanmaster> providers);
    void InvalidateProviders();
}