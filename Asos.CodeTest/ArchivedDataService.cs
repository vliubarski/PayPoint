using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Asos.CodeTest
{
    public class ArchivedDataService : IArchivedDataService
    {
        private readonly string _connectionString;

        public ArchivedDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Archive.Database.Connection");

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ConfigurationErrorsException("Connection string 'Archive.Database.Connection' not found in configuration.");
            }
        }

        public async Task<Customer> GetArchivedCustomer(int customerId)
        {
            try
            {
                using var sqlConnection = new SqlConnection(_connectionString);
                await sqlConnection.OpenAsync();

                var command = new SqlCommand("SELECT Id, Name FROM Customer WHERE CustomerId = @customerId", sqlConnection) { CommandType = CommandType.Text };
                command.Parameters.Add("@customerId", SqlDbType.Int).Value = customerId;


                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Customer { Id = reader.GetInt32(0), Name = reader.GetString(1) };
                }

                throw new CustomException($"Archived customer with ID {customerId} not found.");
            }
            catch (System.Exception ex)
            {
                throw new CustomException("An unexpected error occurred.", ex);
            }
        }
    }
}