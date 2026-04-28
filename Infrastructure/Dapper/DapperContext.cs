using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Dapper;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
                   ?? throw new Exception("Connection string 'Default' is missing");

        SafeLogger.App("Initializing DB Connection");

        if (string.IsNullOrEmpty(_connectionString))
        {
            SafeLogger.App("Connection string is NULL");
            throw new Exception("Connection string is NULL");
        }
    }

    public IDbConnection CreateConnection()
    {
        SafeLogger.App("Opening DB Connection");
        return new NpgsqlConnection(_connectionString);
    }
}