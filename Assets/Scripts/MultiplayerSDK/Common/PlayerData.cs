namespace MultiplayerSDK.Common
{
    public struct PlayerData
    {
        public int PlayerId;
        public string Nickname;
        public int Ping;
        public bool IsReady;
        public bool InGame;

        public PlayerData WithNickname(string nickname)
        {
            Nickname = nickname;
            return this;
        }

        public PlayerData WithPing(int ping)
        {
            Ping = ping;
            return this;
        }

        public PlayerData WithIsReady(bool isReady)
        {
            IsReady = isReady;
            return this;
        }

        public PlayerData WithInGame(bool inGame)
        {
            InGame = inGame;
            return this;
        }

        public override string ToString()
        {
            return $"[{Nickname} - {PlayerId}] ping: {Ping}ms, isReady: {IsReady}";
        }
    }
}