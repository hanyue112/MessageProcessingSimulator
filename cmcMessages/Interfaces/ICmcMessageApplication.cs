using System.Threading.Tasks;

namespace cmcMessages.Interfaces
{
    public interface ICmcMessageApplication
    {
        void Start();
        void Stop();
        void NotifyExiting();
        void NotifyTerminate();
        bool IsComplete();
    }
}