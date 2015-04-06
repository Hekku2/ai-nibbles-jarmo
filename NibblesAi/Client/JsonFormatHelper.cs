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

        public static Location[] GetBlockedLocations(dynamic data)
        {
            var blockedLocations = new Location[data.snakes[0].body.Count + data.snakes[1].body.Count];
            var index = 0;
            foreach (var snake in data.snakes)
            {
                foreach (var bodyPart in snake.body)
                {
                    blockedLocations[index] = new Location(bodyPart[0].Value, bodyPart[1].Value);
                    index++;
                }
            }

            return blockedLocations;
        }
    }
}
