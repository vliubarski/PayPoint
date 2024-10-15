using System.Threading.Tasks;
using Asos.CodeTest.Models2;

namespace Asos.CodeTest.Interfaces;

public interface ICustomerDataAccess
{
    Task<CustomerResponse> LoadCustomerAsync(int customerId);
}