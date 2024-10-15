using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Asos.CodeTest;
public class CustomerService
{
    private readonly IAppSettings _settings;
    private readonly IFailoverRepository _failoverRepository;
    private readonly ICustomerDataAccess _customerDataAccess;
    private readonly IFailoverCustomerDataAccess _failoverCustomerDataAccess;
    private readonly IArchivedDataService _archivedDataService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IAppSettings settings, IFailoverRepository failoverRepository,
        ICustomerDataAccess customerDataAccess, IFailoverCustomerDataAccess failoverCustomerDataAccess,
        IArchivedDataService archivedDataService, ILogger<CustomerService> logger)
    {
        _settings = Guard.ThrowIfNull(settings);
        _failoverRepository = Guard.ThrowIfNull(failoverRepository);
        _customerDataAccess = Guard.ThrowIfNull(customerDataAccess);
        _failoverCustomerDataAccess = Guard.ThrowIfNull(failoverCustomerDataAccess);
        _archivedDataService = Guard.ThrowIfNull(archivedDataService);
        _logger = Guard.ThrowIfNull(logger);
    }

    public async Task<Customer> GetCustomer(int customerId, bool isCustomerArchived)
    {
        try
        {
            return isCustomerArchived
                ? await GetArchivedCustomer(customerId)
                : await GetCustomerFromFailoverOrCustomerRepo(customerId);
        }
        catch (CustomException ce)
        {
            _logger.LogError(ce, "Custom error occurred in GetCustomer for ID: {CustomerId}, IsArchived: {IsArchived}", customerId, isCustomerArchived);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred in GetCustomer for ID: {CustomerId}, IsArchived: {IsArchived}", customerId, isCustomerArchived);
            throw new CustomException("An unexpected error occurred while retrieving customer data.", ex);
        }
    }

    private async Task<Customer> GetCustomerFromFailoverOrCustomerRepo(int customerId)
    {
        var failoverEntries = await _failoverRepository.GetFailOverEntries();
        var numberOfFailedRequests = failoverEntries.Count(EntriesInLastXMinutes());
        var customerResponse = await GetResponseFromFailoverOrCustomerApi(customerId, numberOfFailedRequests);

        return customerResponse.IsArchived
            ? await _archivedDataService.GetArchivedCustomer(customerId)
            : customerResponse.Customer;
    }

    /// <summary>
    /// Condition for requests created in last X minutes,
    /// where X is a number of minutes read from app config
    /// </summary>
    private Func<FailoverEntry, bool> EntriesInLastXMinutes()
    {
        var thresholdTime = DateTime.UtcNow.AddMinutes(-_settings.FailedRequestsAging);
        return x => x.DateTime > thresholdTime;
    }

    /// <summary>
    /// Get customer data from http call to Failover Or Customer Api
    /// </summary>
    private async Task<CustomerResponse> GetResponseFromFailoverOrCustomerApi(int customerId, int numberOfFailedRequests)
    {
        var customerResponse = numberOfFailedRequests > _settings.FailedRequestsThreshold && _settings.IsFailoverModeEnabled
            ? await _failoverCustomerDataAccess.GetCustomerById(customerId) 
            : await _customerDataAccess.LoadCustomerAsync(customerId);

        return customerResponse ?? throw new CustomException(
            $"Customer response for ID {customerId} is null, number of failed requests is {numberOfFailedRequests}.");
    }

    private async Task<Customer> GetArchivedCustomer(int customerId)
    {
        Customer archivedCustomer = await _archivedDataService.GetArchivedCustomer(customerId);

        if(archivedCustomer != null)
        {
            return archivedCustomer;
        }
        _logger.LogError("Custom error occurred in GetArchivedCustomer for ID: {customerId}", customerId);
        throw new CustomException($"Archived customer with ID {customerId} not found.");
    }
}
