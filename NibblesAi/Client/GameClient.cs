﻿using System;
using Client.Messages;

namespace Client
{
    /// <summary>
    /// This class handles gameplay
    /// </summary>
    public class GameClient
    {
        private readonly Communicator _communicator;
        private readonly string _aiName;

        private int _areaWidth;
        private int _areaHeight;
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
            dynamic dynamicData = message.data;
            string msg = message.msg.ToString();
            switch (msg)
            {
                case "created":
                    break;
                case "start":
                    _areaWidth = dynamicData.level.width;
                    _areaHeight = dynamicData.level.height;
                    _playerNo = GetPlayerIndex(dynamicData);
                    break;
                case "positions":
                    var snake = GetSnake(dynamicData);
                    var direction = (Direction)snake.direction;
                    var x = snake.body[0][0];
                    var y = snake.body[0][1];

                    if (!AppleIsInPlayfield())
                        break;

                    DecideDirection(direction, x.Value, y.Value);

                    break;
                case "apple":
                    _apple.X = dynamicData[0];
                    _apple.Y = dynamicData[1];
                    break;
            }
        }

        private void DecideDirection(Direction snakeDirection, Int64 x, Int64 y)
        {
            if (SnakeIsOnLeftSideOfApple(x))
            {
                if (snakeDirection == Direction.Left)
                    _communicator.Send(new ControlMessage(Direction.Down));
                else
                    _communicator.Send(new ControlMessage(Direction.Right));
            }
            else if (SnakeIsOnRightSideOfApple(x))
            {
                if (snakeDirection == Direction.Right)
                    _communicator.Send(new ControlMessage(Direction.Down));
                else
                    _communicator.Send(new ControlMessage(Direction.Left));
            }
            else if (SnakesIsOnBottomSideOfApple(y))
            {
                if (snakeDirection == Direction.Down)
                    _communicator.Send(new ControlMessage(Direction.Left));
                else
                    _communicator.Send(new ControlMessage(Direction.Up));
            }
            else if (SnakesIsOnTopSideOfApple(y))
            {
                if (snakeDirection == Direction.Up)
                    _communicator.Send(new ControlMessage(Direction.Left));
                else
                    _communicator.Send(new ControlMessage(Direction.Down));
            }
        }

        private bool SnakeIsOnLeftSideOfApple(Int64 x)
        {
            return x < _apple.X;
        }

        private bool SnakeIsOnRightSideOfApple(Int64 x)
        {
            return x > _apple.X;
        }

        private bool SnakesIsOnBottomSideOfApple(Int64 y)
        {
            return y < _apple.Y;
        }

        private bool SnakesIsOnTopSideOfApple(Int64 y)
        {
            return y > _apple.Y;
        }

        private object GetSnake(dynamic data)
        {
            return data[_playerNo];
        }

        private int GetPlayerIndex(dynamic data)
        {
            for (var i = 0; i < data.players.Count; i++)
            {
                if (data.players[i].name == _aiName)
                    return i;
            }
            return -1;
        }

        private bool AppleIsInPlayfield() 
        {
            return _apple.X > -1 && _apple.Y > -1;
        }
    }
}