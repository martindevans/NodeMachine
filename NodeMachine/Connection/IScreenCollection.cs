using System.Threading.Tasks;
using NodeMachine.Model;
using System.Collections.Generic;

namespace NodeMachine.Connection
{
    public interface IScreenCollection
        : IEnumerable<Screen>
    {
        Task<Screen> Head();
    }
}
