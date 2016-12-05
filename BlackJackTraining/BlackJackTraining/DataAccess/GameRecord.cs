namespace BlackJackTraining.DataAccess
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class GameRecord
    {
        private const string GameStartedMessageTemplate = "Game started. Initial Bet: {0}";
        private const string GenericActionMessageTemplate = "{0} {1}. Dealer: {2} Player: {3} Bet: {4}";
        private const string OpenHandMessageTemplate = "Hand {0} opened. Dealer: {1} Player: {2} Bet: {3}";
        private const string CloseHandMessageTemplate = "Hand {0} closed. Dealer: {1} Player: {2} {3}: {4}";
        private const string DealerActionMessageTemplate = "Dealer hits: {0} -> {1} Value: {2}";
        private const string GameCompletedMessageTemplate = "Game ended. {0}: {1}";

        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("ResultingAmount")]
        public decimal ResultingAmount { get; set; }

        [JsonProperty("Status")]
        public GameStatus Status { get; set; }

        [JsonProperty("GameEvents")]
        public List<GameEvent> GameEvents { get; set; }

        public GameRecord()
        {
            this.Status = GameStatus.NotComplete;
            this.GameEvents = new List<GameEvent>();
        }

        public GameRecord(long gameId) :
            this()
        {
            this.Id = gameId;
        }

        public void AddGameStartedEvent(decimal betAmount)
        {
            this.GameEvents.Add(
                new GameEvent(this.GameEvents.Count + 1, GameEventType.GameStarted, string.Format(GameStartedMessageTemplate, betAmount)));
        }

        public void AddGameCompletedEvent(decimal winAmount, decimal totalAmount)
        {
            this.GameEvents.Add(
                new GameEvent(
                    this.GameEvents.Count + 1,
                    GameEventType.GameCompleted,
                    string.Format(GameCompletedMessageTemplate, GetResultByAmount(winAmount), winAmount)));

            this.ResultingAmount = totalAmount;
            this.Status = (winAmount == 0 ? GameStatus.DrawGame : (winAmount > 0 ? GameStatus.PlayerWins : GameStatus.DealerWins));
        }

        public void AddOpenHandEvent(int playerId, HandCards dealerCards, PlayerHandCards playerCards)
        {
            this.GameEvents.Add(
                new GameEvent(
                    this.GameEvents.Count + 1,
                    GameEventType.HandOpened,
                    string.Format(
                            OpenHandMessageTemplate,
                            "Player" + playerId,
                            dealerCards,
                            playerCards,
                            playerCards.BetAmount)));
        }

        public void AddCloseHandEvent(
            int playerId,
            HandCards dealerCards,
            PlayerHandCards playerCards,
            decimal winAmount,
            string overrideResult = null)
        {
            this.GameEvents.Add(
                new GameEvent(
                    this.GameEvents.Count + 1,
                    GameEventType.HandClosed,
                    string.Format(
                            CloseHandMessageTemplate,
                            "Player" + playerId,
                            dealerCards,
                            playerCards,
                            string.IsNullOrEmpty(overrideResult) ? GetResultByAmount(winAmount) : overrideResult,
                            winAmount)));
        }

        public void AddEventByPlayerAction(
            PlayerAction playerAction,
            int playerId,
            HandCards dealerCards,
            PlayerHandCards playerCards,
            GameContext gameContext)
        {
            this.GameEvents.Add(
                new GameEvent(
                    this.GameEvents.Count + 1,
                    GameEventType.PlayerAction,
                    string.Format(
                            GenericActionMessageTemplate,
                            "Player" + playerId,
                            playerAction.ToString().ToLower(),
                            dealerCards,
                            playerCards,
                            playerCards.BetAmount)));
        }

        public void AddDealerEvent(
            string initialCards,
            string finalCards,
            int finalHandValue)
        {
            this.GameEvents.Add(
                new GameEvent(
                    this.GameEvents.Count + 1,
                    GameEventType.DealerAction,
                    string.Format(
                            DealerActionMessageTemplate,
                            initialCards,
                            finalCards,
                            finalHandValue)));
        }

        protected static string GetResultByAmount(decimal amount)
        {
            string gameResult = "Draw";
            if (amount > 0)
            {
                gameResult = "Win";
            }
            else if (amount < 0)
            {
                gameResult = "Lose";
            }

            return gameResult;
        }
    }
}
