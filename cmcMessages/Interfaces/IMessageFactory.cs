using cmcMessages.Classes;

namespace cmcMessages.Interfaces
{
    public interface IMessageFactory
    {
        CmcMessagePayload Next();
        CmcMessagePayload FullStop();
        string GetName();
    }
}