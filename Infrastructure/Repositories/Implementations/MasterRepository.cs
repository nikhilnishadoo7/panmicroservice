using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Repositories.Implementations;

public class MasterRepository : IMasterRepository
{
    private readonly DapperContext _context;

    public MasterRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<providerpanmaster>> GetAllActiveProviders()
    {
        SafeLogger.App("Fetching active providers from DB");

        using var db = _context.CreateConnection();

        // ✅ Uses function instead of raw query
        var result = (await db.QueryAsync<providerpanmaster>(
            "SELECT * FROM get_active_providers()"
        )).ToList();

        SafeLogger.App($"Providers fetched: {result.Count}");

        return result;
    }
}