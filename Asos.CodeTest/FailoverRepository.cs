using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Asos.CodeTest
{
    public class FailoverRepository : IFailoverRepository
    {
        public async Task<List<FailoverEntry>> GetFailOverEntries()
        {
            var failoverEntries = new List<FailoverEntry>();

            var connectionString = ConfigurationManager.ConnectionStrings["FailoverDatabase.Connection"]
                .ConnectionString;

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                var command =
                    new SqlCommand("GetFailoverEntries", sqlConnection) {CommandType = CommandType.StoredProcedure};

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var failoverData = new FailoverEntry {DateTime = reader.GetDateTime(0)};

                        failoverEntries.Add(failoverData);
                    }
                }
            }

            return failoverEntries;
        }
    }
}