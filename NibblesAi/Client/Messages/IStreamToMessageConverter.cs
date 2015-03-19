using System;
using System.IO;

namespace Client.Messages
{
    public interface IStreamToMessageConverter
    {
        void HandleStream(Stream stream, Action<BaseMessage> messageReceived);
    }
}
