using Microsoft.Extensions.Configuration;
using Npgsql;
using PAN.API.Infrastructure.Logging;
using System.Data;

namespace PAN.API.Infrastructure.Dapper;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new Exception(
                "Connection string 'Default' is missing");

        SafeLogger.App("Initializing DB Connection");
    }

    public virtual IDbConnection CreateConnection()
    {
        SafeLogger.App("Opening DB Connection");

        return new NpgsqlConnection(_connectionString);
    }
}