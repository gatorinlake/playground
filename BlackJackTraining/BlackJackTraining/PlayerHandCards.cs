namespace BlackJackTraining
{
    public class PlayerHandCards : HandCards
    {
        public decimal BetAmount { get; set; }

        /// <summary>
        /// A closed hand means the player stands, or is busted
        /// </summary>
        public bool HasSurrendered { get; set; }

        public PlayerHandCards(decimal betAmount)
        {
            this.BetAmount = betAmount;
            this.HasSurrendered = false;
        }

        public bool EligibleToSplit(int playerHands, GameContext gameContext)
        {
            if (this.CardsInHand == 2 && playerHands <= gameContext.MaxSplitTimes)
            {
                char[] handCardsStr = this.ToString().ToCharArray();
                if (handCardsStr[0] == handCardsStr[1])
                {
                    return true;
                }

                if (gameContext.TreatAllTensEqual)
                {
                    if (handCardsStr[0] == 'T' || handCardsStr[0] == 'J' || handCardsStr[0] == 'Q' || handCardsStr[0] == 'K')
                    {
                        if (handCardsStr[1] == 'T' || handCardsStr[1] == 'J' || handCardsStr[1] == 'Q' || handCardsStr[1] == 'K')
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool EligibleToDouble(GameContext gameContext)
        {
            return (gameContext.DoubleAllowed && this.CardsInHand == 2);
        }

        public bool EligibleToSurrender(GameContext gameContext)
        {
            return (gameContext.SurrenderAllowed && this.CardsInHand == 2);
        }

        public PlayerHandCards SplitAndGetNewHandCards(int val1, int val2, GameContext gameContext)
        {
            if (this.EligibleToSplit(1, gameContext))
            {
                PlayerHandCards newHand = new PlayerHandCards(this.BetAmount);
                newHand.AddCard(this.ReverseAdd());
                newHand.AddCard(val2);
                this.AddCard(val1);
                return newHand;
            }

            return null;
        }
    }
}
