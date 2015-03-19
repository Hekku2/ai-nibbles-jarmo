using System;
using System.IO;
using System.Text;
using Client.Messages;
using Newtonsoft.Json;

namespace Client
{
    public class StreamToMessageConverter : IStreamToMessageConverter
    {
        /// <summary>
        /// Note: Too big buffer (2014 at least) sometimes makes converter to lose next messages after big messages.
        /// </summary>
        private const int StreamBufferSize = 256;

        public void HandleStream(Stream stream, Action<BaseMessage> messageReceived)
        {
            var serializer = new JsonSerializer();

            while (true)
            {
                try
                {
                    using (var sr = new StreamReader(stream, Encoding.ASCII, false, StreamBufferSize, true))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var parsed = serializer.Deserialize<BaseMessage>(jsonTextReader);
                        messageReceived(parsed);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
