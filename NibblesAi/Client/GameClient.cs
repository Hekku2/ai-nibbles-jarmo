using System;
using Client.Messages;
using Functional.Maybe;

namespace Client
{
    /// <summary>
    /// This class handles gameplay
    /// </summary>
    public class GameClient
    {
        private readonly Communicator _communicator;
        private readonly string _aiName;
        private Pathfinder _pathfinder;

        private Direction? _lastSent;
        private int _playerNo;

        readonly Location _apple = new Location(-1, -1);

        public GameClient(Communicator communicator, string aiName)
        {
            _communicator = communicator;
            _aiName = aiName;
            _communicator.MessageEvent = HandleMessage;
        }

        public void StartGame()
        {
            _communicator.Send(new JoinMessage(_aiName));
        }

        public void HandleMessage(dynamic message)
        {
            if (message.msg != "positions")
                Console.WriteLine(message.msg);
            dynamic dynamicData = message.data;
            string msg = message.msg.ToString();
            switch (msg)
            {
                case "created":
                    break;
                case "start":
                    _pathfinder = new Pathfinder(dynamicData.level.width.Value, dynamicData.level.height.Value);
                    _playerNo = JsonFormatHelper.GetPlayerIndex(dynamicData, _aiName);
                    break;
                case "positions":
                    Snake snake = JsonFormatHelper.GetSnake(dynamicData, _playerNo);

                    if (!AppleIsInPlayfield() || snake.HeadPosition.X < 0 || snake.HeadPosition.Y < 0)
                    {
                        Console.WriteLine("No apple or snake is not in play");
                        break;
                    }

                    bool[,] blockedLocations = JsonFormatHelper.GetBlockedLocations(dynamicData, _pathfinder.MapWidth, _pathfinder.MapHeight);
                    Maybe<Direction> target = _pathfinder.FindPath(snake.HeadPosition, _apple, blockedLocations);
                    if (!target.HasValue)
                        break;

                    if (!_lastSent.HasValue || _lastSent.Value != target.Value)
                    {
                        Console.WriteLine(target.Value);
                        _lastSent = target.Value;
                        _communicator.Send(new ControlMessage(target.Value));
                    }

                    break;
                case "apple":
                    _apple.X = dynamicData[0];
                    _apple.Y = dynamicData[1];
                    break;
            }
        }

        private bool AppleIsInPlayfield() 
        {
            return _apple.X > -1 && _apple.Y > -1;
        }
    }
}
