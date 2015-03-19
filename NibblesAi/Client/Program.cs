using System;
using Client.Properties;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var converter = new StreamToMessageConverter();
            var communicator = new Communicator(Settings.Default.ServerAddress, Settings.Default.ServerPort, converter);
            var gameClient = new GameClient(communicator, Settings.Default.AiName);

            try
            {
                communicator.Connect();
                gameClient.StartGame();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.Read();
                communicator.Dispose();
            }
        }
    }
}
