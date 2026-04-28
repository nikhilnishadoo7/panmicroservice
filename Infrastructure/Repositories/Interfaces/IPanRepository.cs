using PAN.API.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PAN.API.Infrastructure.Repositories.Interfaces;

public interface IPanRepository
{
    Task<PanVerification?> GetByHash(string hash);
    Task<Guid> Insert(PanVerification entity);
}