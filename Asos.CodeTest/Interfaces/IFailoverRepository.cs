using System.Collections.Generic;
using System.Threading.Tasks;
using Asos.CodeTest.Models2;

namespace Asos.CodeTest.Interfaces;

public interface IFailoverRepository
{
    Task<List<FailoverEntry>> GetFailOverEntries();
}