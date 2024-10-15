using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using Asos.CodeTest.Models2;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Asos.CodeTest.UnitTests
{

    [TestFixture]
    public sealed class CustomerServiceTests
    {
        private CustomerService _customerService;

        private Mock<IFailoverRepository> _mockFailoverRepository;

        private Mock<IAppSettings> _mockAppSettings;

        private Mock<ICustomerDataAccess> _mockCustomerDataAccess;
        private Mock<IFailoverCustomerDataAccess> _mockFailoverCustomerDataAccess;
        private Mock<IArchivedDataService> _mockArchivedDataService;
        private Mock<ILogger<CustomerService>> _mockLogger;

        [SetUp]
        public void Init()
        {
            _mockFailoverRepository = new Mock<IFailoverRepository>();
            _mockCustomerDataAccess = new Mock<ICustomerDataAccess>();
            _mockAppSettings = new Mock<IAppSettings>();
            _mockFailoverCustomerDataAccess = new Mock<IFailoverCustomerDataAccess>();
            _mockArchivedDataService = new Mock<IArchivedDataService>();
            _mockLogger = new Mock<ILogger<CustomerService>>();

            _customerService = new CustomerService(_mockAppSettings.Object, _mockFailoverRepository.Object,
                _mockCustomerDataAccess.Object, _mockFailoverCustomerDataAccess.Object, _mockArchivedDataService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ReturnsCustomerFromMainCustomerDataStore_WHEN_NotInFailOver_AND_NotArchived()
        {
            // Arrange
            const int customerId = 12345;
            var emptyFailoverEntries = new List<FailoverEntry>();
            var expectedCustomer = new Customer { Id = customerId, Name = "Test Customer" };
            _mockAppSettings.Setup(mock => mock.IsFailoverModeEnabled).Returns(true);
            _mockFailoverRepository.Setup(mock => mock.GetFailOverEntries()).ReturnsAsync(emptyFailoverEntries);

            _mockCustomerDataAccess.Setup(mock => mock.LoadCustomerAsync(customerId))
                .ReturnsAsync(new CustomerResponse { Customer = expectedCustomer });

            // Act
            var result = await _customerService.GetCustomer(customerId, false);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCustomer));
        }

        [Test]
        public async Task GetCustomer_ReturnsArchivedCustomer_WhenCustomerIsArchived()
        {
            // Arrange
            int customerId = 1;
            bool isCustomerArchived = true;
            var archivedCustomer = new Customer { Id = customerId };

            _mockArchivedDataService
                .Setup(a => a.GetArchivedCustomer(customerId))
                .ReturnsAsync(archivedCustomer);

            // Act
            var result = await _customerService.GetCustomer(customerId, isCustomerArchived);

            // Assert
            Assert.That(result, Is.EqualTo(archivedCustomer));
        }

        [Test]
        public async Task GetCustomer_FailoverModeEnabled_ReturnsFailoverCustomer()
        {
            // Arrange
            int customerId = 2;
            bool isCustomerArchived = false;
            long FailoverThreshold = 100;
            long FailedRequestsAging = 10; // in minutes

            var failoverEntries101 = Enumerable.Range(0, 101)
                                .Select(i => new FailoverEntry { DateTime = DateTime.Now })
                                .ToList();

            _mockFailoverRepository
                .Setup(fr => fr.GetFailOverEntries())
                .ReturnsAsync(failoverEntries101);

            _mockAppSettings
                .Setup(s => s.IsFailoverModeEnabled)
                .Returns(true);

            _mockAppSettings
                .Setup(s => s.FailedRequestsThreshold)
                .Returns(FailoverThreshold);

            _mockAppSettings
                .Setup(s => s.FailedRequestsAging)
                .Returns(FailedRequestsAging);

            var failoverCustomer = new CustomerResponse { Customer = new Customer { Id = customerId }, IsArchived = false };

            _mockFailoverCustomerDataAccess
                .Setup(f => f.GetCustomerById(customerId))
                .ReturnsAsync(failoverCustomer);

            // Act
            var result = await _customerService.GetCustomer(customerId, isCustomerArchived);

            // Assert
            Assert.That(result, Is.EqualTo(failoverCustomer.Customer));
        }

        [Test]
        public async Task GetCustomer_LoadsArchivedCustomer_WhenCustomerResponseIsArchived()
        {
            // Arrange
            int customerId = 3;
            bool isCustomerArchived = false;
            var noFailoverEntries = new List<FailoverEntry>();

            _mockFailoverRepository
                .Setup(fr => fr.GetFailOverEntries())
                .ReturnsAsync(noFailoverEntries);

            _mockAppSettings
                .Setup(s => s.IsFailoverModeEnabled)
                .Returns(false);

            var customerResponse = new CustomerResponse { Customer = new Customer { Id = customerId, Name = "test name2" }, IsArchived = true };

            _mockCustomerDataAccess
                .Setup(c => c.LoadCustomerAsync(customerId))
                .ReturnsAsync(customerResponse);

            var archivedCustomer = new Customer { Id = customerId };

            _mockArchivedDataService
                .Setup(a => a.GetArchivedCustomer(customerId))
                .ReturnsAsync(archivedCustomer);

            // Act
            var result = await _customerService.GetCustomer(customerId, isCustomerArchived);

            // Assert
            Assert.That(result, Is.EqualTo(archivedCustomer));
        }

        [Test]
        public void GetCustomer_WhenCustomExceptionIsThrown_LogsErrorAndRethrows()
        {
            // Arrange
            int customerId = 1;
            bool isCustomerArchived = true;

            _mockArchivedDataService
                .Setup(x => x.GetArchivedCustomer(customerId))
                .Throws(new CustomException("Custom exception"));

            // Act
            var ex = Assert.ThrowsAsync<CustomException>(async () =>
                await _customerService.GetCustomer(customerId, isCustomerArchived));

            // Verify 
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Custom error occurred in GetCustomer")),
                    It.IsAny<CustomException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void GetCustomer_WhenGeneralExceptionIsThrown_LogsErrorAndThrowsCustomException()
        {
            // Arrange
            int customerId = 1;
            bool isCustomerArchived = false;

            _mockCustomerDataAccess
                .Setup(x => x.LoadCustomerAsync(customerId))
                .ThrowsAsync(new Exception("General exception"));

            // Act
            var ex = Assert.ThrowsAsync<CustomException>(async () =>
                await _customerService.GetCustomer(customerId, isCustomerArchived));

            // Verify 
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unexpected error occurred in GetCustomer")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.That(ex.Message, Is.EqualTo("An unexpected error occurred while retrieving customer data."));
        }

        [Test]
        public void GetCustomer_Should_LogError_And_ThrowCustomException_WhenArchivedCustomerNotFound()
        {
            // Arrange
            int customerId = 123;
            bool isCustomerArchived = true;

            // Mock the archived service to return null (simulating customer not found)
            _mockArchivedDataService.Setup(service => service.GetArchivedCustomer(customerId))
                                    .ReturnsAsync((Customer)null); // returns null

            // Act & Assert
            var exception = Assert.ThrowsAsync<CustomException>(async () =>
                await _customerService.GetCustomer(customerId, isCustomerArchived));


            // Verify the exception message
            Assert.That(exception.Message, Is.EqualTo($"Archived customer with ID {customerId} not found."));
        }
    }
}