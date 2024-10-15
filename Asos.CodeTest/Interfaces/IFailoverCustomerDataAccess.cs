using System.Threading.Tasks;
using Asos.CodeTest.Models2;

namespace Asos.CodeTest.Interfaces;

public interface IFailoverCustomerDataAccess
{
    Task<CustomerResponse> GetCustomerById(int id);
}