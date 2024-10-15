using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asos.CodeTest
{
    public interface IFailoverRepository
    {
        Task<List<FailoverEntry>> GetFailOverEntries();
    }
}