using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Configuration;

namespace Asos.CodeTest;

public class FailoverRepository : IFailoverRepository
{
    private readonly string _connectionString;

    public FailoverRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FailoverDatabase");
    }

    public async Task<List<FailoverEntry>> GetFailOverEntries()
    {
        var failoverEntries = new List<FailoverEntry>();

        try
        {
            await using var sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();

            using var command = new SqlCommand("GetFailoverEntries", sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var failoverData = new FailoverEntry
                {
                    DateTime = reader.GetDateTime(0)
                };

                failoverEntries.Add(failoverData);
            }
        }
        catch (SqlException ex)
        {
            // Log and handle exception, or rethrow as a custom exception
            throw new CustomException("An error occurred while retrieving failover entries from the database.", ex);
        }

        return failoverEntries;
    }
}
