using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Asos.CodeTest
{
    public class CustomerDataAccess : ICustomerDataAccess
    {
        private readonly HttpClient _client;
        private readonly ILogger<CustomerDataAccess> _logger;

        public CustomerDataAccess(HttpClient client, IConfiguration configuration, ILogger<CustomerDataAccess> logger)
        {
            _client = client;
            _logger = logger;
            _client.BaseAddress = new Uri(configuration["CustomerService:ThirdParty:Http:Connection"]);
        }

        public async Task<CustomerResponse> LoadCustomerAsync(int customerId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/customers/{customerId}");

            try
            {
                _logger.LogInformation("Sending request to load customer data for ID: {CustomerId}", customerId);
                var response = await _client.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved customer data for ID: {CustomerId}", customerId);

                return DataDeserializer.Deserialize<CustomerResponse>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while loading customer data for ID: {CustomerId}", customerId);
                throw new CustomException("Error occurred while loading customer data.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize customer response for ID: {CustomerId}", customerId);
                throw new CustomException("Failed to deserialize customer response.", ex);
            }
        }
    }
}
