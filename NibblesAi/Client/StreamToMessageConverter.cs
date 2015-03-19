using System;
using System.IO;
using System.Text;
using Client.Messages;
using Newtonsoft.Json;

namespace Client
{
    public class StreamToMessageConverter : IStreamToMessageConverter
    {
        private const int ReadBufferSize = 2014;

        public void HandleStream(Stream stream, Action<BaseMessage> messageReceived)
        {
            using (var sr = new StreamReader(stream, Encoding.ASCII, false, 2014, true))
            {
                var readBuffer = new char[ReadBufferSize];
                var builder = new StringBuilder(ReadBufferSize * 6);
                var notEndedBrackets = 0;
                var read = sr.Read(readBuffer, 0, readBuffer.Length);
                while (read > 0)
                {
                    for (var i = 0; i < read; i++)
                    {
                        notEndedBrackets += BracketStatusDelta(readBuffer[i]);

                        builder.Append(readBuffer[i]);
                        if (notEndedBrackets == 0)
                        {
                            var message = JsonConvert.DeserializeObject<BaseMessage>(builder.ToString());
                            messageReceived(message);
                            builder = new StringBuilder(ReadBufferSize * 6);
                        }
                    }
                    read = sr.Read(readBuffer, 0, readBuffer.Length);
                }
            }
        }

        private static int BracketStatusDelta(char c)
        {
            switch (c)
            {
                case '{':
                    return 1;
                case '}':
                    return -1;
                default:
                    return 0;
            }
        }
    }
}
