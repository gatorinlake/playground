namespace BlackJackTraining
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CardPool : ICloneable
    {
        // 0 -> A, [1 - 9] -> [2 - 10], 10 -> J, 11 -> Q, 12 -> K
        protected readonly int []CardsByValue = new int [13];

        public CardPool():
            this(1)
        {

        }

        public CardPool(int deckCount)
        {
            if (deckCount <= 0)
            {
                throw new ArgumentOutOfRangeException("deckCount", "deckCount must be 1 or greater.");
            }

            this.DeckCount = deckCount;
            this.InitCardsByValue();
        }

        public int CardCount
        {
            get
            {
                lock (this.CardsByValue)
                {
                    return this.CardsByValue.Sum();
                }
            }
        }

        public int DeckCount { get; private set; }

        public decimal GetValueRangeProbability(int lowValue, int highValue)
        {
            if(lowValue > highValue)
            {
                return this.GetValueRangeProbability(highValue, lowValue);
            }

            if (lowValue < 0)
            {
                throw new ArgumentOutOfRangeException("lowValue", "lowValue can't be negative.");
            }

            if (highValue >= this.CardsByValue.Length)
            {
                throw new ArgumentOutOfRangeException("highValue", "highValue must be less than 13.");
            }

            lock (this.CardsByValue)
            {
                decimal totalCards = this.CardCount;
                if(totalCards <= 0)
                {
                    return 0;
                }

                decimal targetCards = 0;
                for (int i = lowValue; i <= highValue; i++)
                {
                    targetCards += this.CardsByValue[i];
                }

                return targetCards / totalCards;
            }
        }

        public virtual int DealCard(int cardValue)
        {
            if (cardValue < 0 || cardValue > 12)
            {
                throw new ArgumentOutOfRangeException("cardValue");
            }

            lock (this.CardsByValue)
            {
                if (this.CardsByValue[cardValue] <= 0)
                {
                    return -1;
                }

                this.CardsByValue[cardValue] = this.CardsByValue[cardValue] - 1;
                return cardValue;
            }
        }

        public virtual bool UnDealCard(int cardValue)
        {
            if (cardValue < 0 || cardValue > 12)
            {
                throw new ArgumentOutOfRangeException("cardValue");
            }

            lock (this.CardsByValue)
            {
                // Cards of value overflow
                if (this.CardsByValue[cardValue] >= 4 * this.DeckCount)
                {
                    return false;
                }

                this.CardsByValue[cardValue] = this.CardsByValue[cardValue] + 1;
                return true;
            }
        }

        public virtual int[] Shuffle()
        {
            int cardCountEachValue = 4 * this.DeckCount;
            int [] dealingOrder = new int[this.CardsByValue.Length * cardCountEachValue];

            lock (this.CardsByValue)
            {
                // Reset counters for each card value
                for (int i = 0; i < this.CardsByValue.Length; i++)
                {
                    this.CardsByValue[i] = cardCountEachValue;

                    int basePos = i * cardCountEachValue;
                    for (int j = 0; j < cardCountEachValue; j++)
                    {
                        dealingOrder[basePos + j] = i;
                    }
                }
            }

            // Shuffle dealing order array
            Random rand = new Random();
            for (int i = 0; i < dealingOrder.Length - 1; i++)
            {
                int posToSwap = rand.Next(i, dealingOrder.Length - 1);
                if (posToSwap != i)
                {
                    int temp = dealingOrder[posToSwap];
                    dealingOrder[posToSwap] = dealingOrder[i];
                    dealingOrder[i] = temp;
                }
            }

            return dealingOrder;
        }

        public IEnumerable<int> GetAvailableCardValues()
        {
            lock (this.CardsByValue)
            {
                var availableValues = new List<int>();
                for (int i = 0; i < this.CardsByValue.Length; i++)
                {
                    if (this.CardsByValue[i] > 0)
                    {
                        availableValues.Add(i);
                    }
                }

                return availableValues;
            }
        }

        public virtual object Clone()
        {
            CardPool newCardPool = new CardPool(this.DeckCount);
            for (int i = 0; i < this.CardsByValue.Length; i++)
            {
                newCardPool.CardsByValue[i] = this.CardsByValue[i];
            }

            return newCardPool;
        }

        protected void InitCardsByValue()
        {
            lock (this.CardsByValue)
            {
                // Reset counters for each card value
                for (int i = 0; i < this.CardsByValue.Length; i++)
                {
                    this.CardsByValue[i] = 4 * this.DeckCount;
                }
            }
        }
    }
}
