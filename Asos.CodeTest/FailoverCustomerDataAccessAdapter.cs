using System.Threading.Tasks;

namespace Asos.CodeTest;

public class FailoverCustomerDataAccessAdapter : IFailoverCustomerDataAccess
{
    public async Task<CustomerResponse> GetCustomerById(int id)
    {
        return await FailoverCustomerDataAccess.GetCustomerById(id);
    }
}
