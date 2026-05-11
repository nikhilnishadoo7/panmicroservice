using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace PAN.API.Infrastructure.Repositories.Implementations;

[ExcludeFromCodeCoverage]
public class PanRepository : IPanRepository
{
    private readonly DapperContext _context;

    public PanRepository(DapperContext context)
    {
        _context = context;
    }

    protected virtual string GetByHashProc() => "sp_get_pan_with_provider";
    protected virtual string InsertProc() => "sp_insert_pan_verification";

    // ----------------------------------------------------------------
    // GET BY HASH
    // ----------------------------------------------------------------
    public async Task<PanVerification?> GetByHash(string hash)
    {
        SafeLogger.App($"DB FETCH START | Hash: {hash}");

        using var db = (DbConnection)_context.CreateConnection(); // ✅ cast to DbConnection
        await db.OpenAsync();

        using var transaction = await db.BeginTransactionAsync();

        try
        {
            const string cursorName = "pan_provider_cur";

            await db.ExecuteAsync(
                $"CALL {GetByHashProc()}(@hash, '{cursorName}')",
                new { hash },
                transaction: transaction
            );

            var result = await db.QueryFirstOrDefaultAsync<PanVerification>(
                $"FETCH ALL FROM {cursorName}",
                transaction: transaction
            );

            await transaction.CommitAsync();

            SafeLogger.App(
                result != null && !string.IsNullOrEmpty(result.PanHash)
                    ? "DB FETCH HIT"
                    : "DB FETCH MISS");

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<long> Insert(PanVerification e)
    {
        SafeLogger.App($"DB INSERT START | Hash: {e.PanHash}");

        using var db = (DbConnection)_context.CreateConnection(); // ✅ cast to DbConnection
        await db.OpenAsync();

        try
        {
            var p = new DynamicParameters();
            p.Add("p_masterid", e.MasterId);
            p.Add("p_providerrequestid", e.ProviderRequestId);
            p.Add("p_panhash", e.PanHash);
            p.Add("p_encryptedpan", e.EncryptedPan);
            p.Add("p_panstatus", e.PanStatus);
            p.Add("p_panlookupstatus", e.PanLookUpStatus);
            p.Add("p_encryptedfullname", e.EncryptedFullName);
            p.Add("p_pancardtype", e.PanCardType);
            p.Add("p_ispanaadhaarliked", e.IsPanAadhaarLinked);
            p.Add("p_callerip", e.CallerIp);
            p.Add("p_createdat", e.CreatedAt);
            p.Add("p_out_id",
                  value: null,
                  dbType: DbType.Int64,
                  direction: ParameterDirection.InputOutput);

            await db.ExecuteAsync(
                $"CALL {InsertProc()}(" +
                "@p_masterid, @p_providerrequestid, @p_panhash, @p_encryptedpan, " +
                "@p_panstatus, @p_panlookupstatus, @p_encryptedfullname, @p_pancardtype, " +
                "@p_ispanaadhaarliked, @p_callerip, @p_createdat, @p_out_id)",
                p
            );

            var actualId = p.Get<long>("p_out_id");
            SafeLogger.App($"DB INSERT SUCCESS | ActualId: {actualId}");
            return actualId;
        }
        catch (Exception ex)
        {
            SafeLogger.App($"DB INSERT FAILED: {ex.Message}");
            throw;
        }
    }
}