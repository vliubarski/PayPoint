using System.Threading.Tasks;
using Asos.CodeTest.Models2;

namespace Asos.CodeTest.Interfaces;

public interface IArchivedDataService
{
    Task<Customer> GetArchivedCustomer(int customerId);
}