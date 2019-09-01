using cmcMessages.Interfaces;
using System;
using System.Diagnostics;

namespace cmcMessages.Classes
{
    public class MessageFactory : IMessageFactory
    {
        private readonly int _generatorId;
        private ulong _seqNum = 0;

        public MessageFactory(int generatorId)
        {
            _generatorId = generatorId;
            Trace.WriteLine($"Initializing and allocating memory for generator {Number2String(_generatorId, true)}");
        }

        public string GetName()
        {
            return Number2String(_generatorId, true).ToString();
        }

        public CmcMessagePayload Next()
        {
            return new CmcMessagePayload(++_seqNum, Number2String(_generatorId, true));
        }

        public CmcMessagePayload FullStop()
        {
            return new CmcMessagePayload(0, Number2String(_generatorId, true));
        }

        private char Number2String(int number, bool isCaps)
        {
            return (char)((isCaps ? 65 : 97) + (number - 1));
        }
    }

    public class CmcMessagePayload
    {
        public ulong SeqNum { get; }
        public char MessageType { get; }
        public DateTime DatetimeCreated { get; }

        public CmcMessagePayload(ulong seqNum, char messageType)
        {
            SeqNum = seqNum;
            MessageType = messageType;
            DatetimeCreated = DateTime.UtcNow;
        }
    }
}