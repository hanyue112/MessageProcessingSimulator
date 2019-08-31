using cmcMessages.Classes;

namespace cmcMessages.Interfaces
{
    public interface IMessageLoggers
    {
        void OnMessageDelivered(CmcMessagePayload p);
        void Start();
        void Terminate();
    }
}