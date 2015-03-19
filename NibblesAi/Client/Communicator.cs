using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Client.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Client
{
    /// <summary>
    /// Purpose of this class is to contain communication implementation details
    /// </summary>
    public class Communicator
    {
        private const int ReadBufferSize = 2014;

        private bool _running;
        private readonly JsonSerializerSettings _settings;
        private readonly TcpClient _sender;
        private readonly IStreamToMessageConverter _streamToMessageConverter;

        public Action<BaseMessage> MessageEvent { get; set; }

        public Communicator(string address, int port, IStreamToMessageConverter streamToMessageConverter)
        {
            _streamToMessageConverter = streamToMessageConverter;
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _sender = new TcpClient(address, port) {
                ReceiveBufferSize = ReadBufferSize
            };
        }

        public void Connect()
        {
            _running = true;
            var thread = new Thread(ReadMessagesFromServer);
            thread.Start();
        }

        public void Dispose()
        { 
            _running = false;
            if (_sender != null)
            {
                _sender.Close();
            }
        }

        public void Send(BaseMessage message)
        {
            var converted = JsonConvert.SerializeObject(message, _settings);
            Console.WriteLine(converted);
            _sender.Client.Send(Encoding.ASCII.GetBytes(converted));
            //HAX: For some strange reason, sometimes the server doesn't stop reading after JSON end if message is sent too fast.
            Thread.Sleep(1);
        }

        private void ReadMessagesFromServer() 
        {
            while (_running)
            {
                var stream = _sender.GetStream();
                _streamToMessageConverter.HandleStream(stream, MessageEvent);
            }
        }
    }
}
