using System.Threading.Tasks;

namespace PAN.API.Infrastructure.Repositories.Interfaces;

public interface IHealthRepository
{
    Task<bool> IsDatabaseHealthyAsync();
}
