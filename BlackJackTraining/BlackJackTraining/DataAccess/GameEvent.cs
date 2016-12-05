namespace BlackJackTraining.DataAccess
{
    using Newtonsoft.Json;

    public enum GameEventType
    {
        GameStarted,

        PlayerAction,

        DealerAction,

        HandOpened,

        HandClosed,

        GameCompleted
    }

    public class GameEvent
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("EventType")]
        public GameEventType EventType { get; set; }

        [JsonProperty("EventMessage")]
        public string EventMessage { get; set; }

        [JsonProperty("EventData")]
        public GameEventData EventData { get; set; }

        internal GameEvent(int eventId, GameEventType eventType, string eventMessage)
        {
            this.Id = eventId;
            this.EventType = eventType;
            this.EventMessage = eventMessage;
        }
    }
}
