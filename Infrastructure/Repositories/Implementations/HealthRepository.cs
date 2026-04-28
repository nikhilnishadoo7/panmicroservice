using Dapper;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;

namespace PAN.API.Infrastructure.Repositories.Implementations;

public class HealthRepository : IHealthRepository
{
    private readonly DapperContext _context;

    public HealthRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<bool> IsDatabaseHealthyAsync()
    {
        try
        {
            using var connection = _context.CreateConnection();

            await connection.ExecuteAsync("SELECT 1"); // opens & closes internally

            return true;

 
        }
        catch (Exception ex)
        {
            SafeLogger.Error(ex, "PostgreSQL health check failed");
            return false;
        }
    }
}