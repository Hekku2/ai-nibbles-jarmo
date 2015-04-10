namespace Client
{
    public class JsonFormatHelper
    {
        private JsonFormatHelper(){}

        public static int GetPlayerIndex(dynamic data, string playerName)
        {
            for (var i = 0; i < data.players.Count; i++)
            {
                if (data.players[i].name == playerName)
                    return i;
            }
            return -1;
        }

        public static Snake GetSnake(dynamic data, int playerIndex)
        {
            var rawSnake = data.snakes[playerIndex];
            var positions = new Location[rawSnake.body.Count];
            for (var i = 0; i < rawSnake.body.Count; i++)
                positions[i] = new Location(rawSnake.body[i][0].Value, rawSnake.body[i][1].Value);

            var snake = new Snake
            {
                Direction = (Direction)rawSnake.direction,
                Locations = positions,
                HeadPosition = positions[0]
            };

            return snake;
        }

        public static bool[,] GetBlockedLocations(dynamic data, long width, long height)
        {
            var blockedLocations = new bool[width, height];
            foreach (var snake in data.snakes)
            {
                foreach (var bodyPart in snake.body)
                {
                    if (bodyPart[0].Value > 0 && bodyPart[0].Value < width && bodyPart[1].Value > 0 && bodyPart[1].Value < height)
                        blockedLocations[bodyPart[0].Value, bodyPart[1].Value] = true;
                }
            }

            return blockedLocations;
        }
    }
}
