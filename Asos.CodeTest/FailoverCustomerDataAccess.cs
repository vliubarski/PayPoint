using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Logging;

namespace Asos.CodeTest;

public class FailoverCustomerDataAccess
{
    private readonly HttpClient _client;
    private readonly ILogger<FailoverCustomerDataAccess> _logger;

    public FailoverCustomerDataAccess(HttpClient client, ILogger<FailoverCustomerDataAccess> logger)
    {
        _client = client;
        _logger = logger;
        _client.BaseAddress = new Uri("https://failover-api/endpoint/data");
    }

    public async Task<CustomerResponse> GetCustomerById(int id)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/customers/{id}");

        try
        {
            _logger.LogInformation("Sending request to failover API for customer ID: {CustomerId}", id);
            var response = await _client.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully retrieved customer data for ID: {CustomerId}", id);

            return DataDeserializer.Deserialize<CustomerResponse>(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error occurred while making an HTTP request for customer ID: {CustomerId}", id);
            throw new CustomException("Error occurred while retrieving customer data.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize customer data for ID: {CustomerId}", id);
            throw new CustomException("Failed to deserialize customer data.", ex);
        }
    }
}
