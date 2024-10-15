using System.Threading.Tasks;

namespace Asos.CodeTest
{
    public interface ICustomerDataAccess
    {
        Task<CustomerResponse> LoadCustomerAsync(int customerId);
    }
}