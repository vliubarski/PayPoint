using System.Threading.Tasks;

namespace Asos.CodeTest;

public interface IFailoverCustomerDataAccess
{
    Task<CustomerResponse> GetCustomerById(int id);
}