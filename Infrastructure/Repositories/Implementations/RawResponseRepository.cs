using Dapper;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Repositories.Implementations;

public class RawResponseRepository : IRawResponseRepository
{
    private readonly DapperContext _context;

    public RawResponseRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(PanResponseJson e)
    {
        SafeLogger.App($"RAW INSERT START | VerificationId: {e.PanVerificationId}");

        var sql = @"CALL insert_pan_response(
                @CorrelationId,
                @PanVerificationId,
                @RequestId,
                @EncryptedRawResponseJson,
                @CreatedAt
            );";

        using var db = _context.CreateConnection();

        try
        {
            await db.ExecuteAsync(sql, new
            {
                e.CorrelationId,
                e.PanVerificationId,
                e.RequestId,
                e.EncryptedRawResponseJson,
                e.CreatedAt
            });

            SafeLogger.App("RAW INSERT SUCCESS");
        }
        catch (Exception ex)
        {
            SafeLogger.App($"RAW INSERT FAILED: {ex.Message}");
            throw;
        }
    }
}