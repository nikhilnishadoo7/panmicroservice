using System.Diagnostics.CodeAnalysis;
using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;

namespace PAN.API.Infrastructure.Repositories.Implementations;

[ExcludeFromCodeCoverage]
public class PanRepository : IPanRepository
{
    private readonly DapperContext _context;

    public PanRepository(DapperContext context)
    {
        _context = context;
    }

    protected virtual string GetByHashQuery()
    {
        return
            "SELECT * FROM get_pan_with_provider(@hash)";
    }

    protected virtual string InsertQuery()
    {
        return
        @"SELECT insert_pan_verification(
              @MasterId,
              @ProviderRequestId,
              @PanHash,
              @EncryptedPan,
              @PanStatus,
              @PanLookUpStatus,
              @EncryptedFullName,
              @PanCardType,
              @IsPanAadhaarLinked,
              @CallerIp,
              @CreatedAt
          );";
    }

    public async Task<PanVerification?> GetByHash(string hash)
    {
        SafeLogger.App($"DB FETCH START | Hash: {hash}");

        using var db = _context.CreateConnection();

        var result =
            await db.QueryFirstOrDefaultAsync<PanVerification>(
                GetByHashQuery(),
                new { hash });

        SafeLogger.App(
            result != null &&
            !string.IsNullOrEmpty(result.PanHash)
                ? "DB FETCH HIT"
                : "DB FETCH MISS");

        return result;
    }

    public async Task<long> Insert(PanVerification e)
    {
        SafeLogger.App($"DB INSERT START | Id: {e.Id}");

        using var db = _context.CreateConnection();

        try
        {
            var actualId =
                await db.ExecuteScalarAsync<long>(
                    InsertQuery(),
                    new
                    {
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

            SafeLogger.App(
                $"DB INSERT SUCCESS | ActualId: {actualId}");

            return actualId;
        }
        catch (Exception ex)
        {
            SafeLogger.App(
                $"DB INSERT FAILED: {ex.Message}");

            throw;
        }
    }
}