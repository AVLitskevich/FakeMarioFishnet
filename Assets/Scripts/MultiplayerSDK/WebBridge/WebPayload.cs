using Newtonsoft.Json;

namespace MultiplayerSDK.WebBridge
{
    [JsonObject]
    public class WebPayload
    {
        [JsonProperty("matchId")]
        public string MatchId;
        
        [JsonProperty("nickname")]
        public string Nickname;
        
        [JsonProperty("playerId")]
        public string PlayerId;
    }
}