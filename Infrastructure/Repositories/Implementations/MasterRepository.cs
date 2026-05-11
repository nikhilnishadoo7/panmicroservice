using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;
using System.Data.Common;

namespace PAN.API.Infrastructure.Repositories.Implementations;

public class MasterRepository : IMasterRepository
{
    private readonly DapperContext _context;

    public MasterRepository(DapperContext context)
    {
        _context = context;
    }

    protected virtual string GetActiveProvidersProc() => "sp_get_active_providers";

    public async Task<List<providerpanmaster>> GetAllActiveProviders()
    {
        SafeLogger.App("Fetching active providers from DB");

        using var db = (DbConnection)_context.CreateConnection(); // ✅ cast to DbConnection
        await db.OpenAsync();

        using var transaction = await db.BeginTransactionAsync();

        try
        {
            const string cursorName = "active_providers_cur";

            await db.ExecuteAsync(
                $"CALL {GetActiveProvidersProc()}('{cursorName}')",
                transaction: transaction
            );

            var result = (await db.QueryAsync<providerpanmaster>(
                $"FETCH ALL FROM {cursorName}",
                transaction: transaction
            )).ToList();

            await transaction.CommitAsync();

            SafeLogger.App($"Providers fetched: {result.Count}");
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}