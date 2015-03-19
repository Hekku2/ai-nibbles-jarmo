using System;
using System.IO;
using Client.Messages;

namespace Client
{
    public interface IStreamToMessageConverter
    {
        void HandleStream(Stream stream, Action<BaseMessage> messageReceived);
    }
}
