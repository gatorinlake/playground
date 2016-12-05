namespace BlackJackTraining.DataAccess
{
    using Newtonsoft.Json;

    public class GameEventData
    {
        [JsonProperty("PlayerBustedRate")]
        public decimal PlayerBustedRate { get; set; }

        [JsonProperty("DealerBustedRate")]
        public decimal DealerBustedRate { get; set; }

        [JsonProperty("Counter", NullValueHandling = NullValueHandling.Ignore)]
        public int? Counter { get; set; }

        [JsonProperty("WillPlayerBusted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WillPlayerBusted { get; set; }

        [JsonProperty("WillDealerBusted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WillDealerBusted { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
