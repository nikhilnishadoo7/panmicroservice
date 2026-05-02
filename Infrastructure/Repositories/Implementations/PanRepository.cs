using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Repositories.Implementations;

public class PanRepository : IPanRepository
{
    private readonly DapperContext _context;

    public PanRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PanVerification?> GetByHash(string hash)
    {
        SafeLogger.App($"DB FETCH START | Hash: {hash}");

        using var db = _context.CreateConnection();

        
        var result = await db.QueryFirstOrDefaultAsync<PanVerification>(
            "SELECT * FROM get_pan_with_provider(@hash)",
            new { hash }
        );

        SafeLogger.App(result != null && !string.IsNullOrEmpty(result.PanHash)
            ? "DB FETCH HIT"
            : "DB FETCH MISS");

        return result;
    }

    public async Task<Guid> Insert(PanVerification e)
    {
        SafeLogger.App($"DB INSERT START | Id: {e.Id}");

        var sql = @"SELECT insert_pan_verification(
            @Id, @CorrelationId, @MasterId, @ProviderRequestId,
            @PanHash, @EncryptedPan, @PanStatus, @PanLookUpStatus,
            @EncryptedFullName, @PanCardType, @IsPanAadhaarLinked,
            @CallerIp, @CreatedAt
        );";

        using var db = _context.CreateConnection();

        try
        {
            // ✅ Returns real DB id — new or existing
            var actualId = await db.ExecuteScalarAsync<Guid>(sql, new
            {
                e.Id,
                e.CorrelationId,
                e.MasterId,
                e.ProviderRequestId,
                e.PanHash,
                e.EncryptedPan,
                e.PanStatus,
                e.PanLookUpStatus,
                e.EncryptedFullName,
                e.PanCardType,
                e.IsPanAadhaarLinked,
                e.CallerIp,
                e.CreatedAt
            });

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