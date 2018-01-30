using System.Threading.Tasks;

namespace TodoAPI.Core
{
    public interface IUnitOfWork
    {
         Task CompleteAsync();
    }
}