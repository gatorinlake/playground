namespace BlackJackTraining
{
    using System;
    using System.Collections.Generic;
    using BlackJackTraining.DataAccess;

    public class GameContext
    {
        private int[] dealingOrder;
        
        private int maxDealingPos;
                
        private int dealingPos;

        private Queue<int> peekedQueue; 

        public CardPool GameCards { get; protected set; }

        public int MaxSplitTimes { get; set; }

        public decimal BlackJackPayRate { get; set; }

        public bool DealerHitsOnSoft17 { get; set; }

        public bool TreatAllTensEqual { get; set; }

        public bool DoubleAllowed { get; set; }

        public bool SurrenderAllowed { get; set; }

        public GameContext()
        {
            this.GameCards = new CardPool(8);
            this.MaxSplitTimes = 2;
            this.BlackJackPayRate = 1.5m;
            this.DealerHitsOnSoft17 = false;
            this.TreatAllTensEqual = false;
            this.DoubleAllowed = true;
            this.SurrenderAllowed = true;
        }

        /// <summary>
        /// Return a card. If the card pool is out, it will reshuffle automatically
        /// </summary>
        /// <returns>The card value</returns>
        public int GetCardWithReshuffle()
        {
            if(this.peekedQueue != null && this.peekedQueue.Count > 0)
            {
                return this.peekedQueue.Dequeue();
            }

            return this.GetCardFromPool();
        }

        /// <summary>
        /// Peek several cards without really dealing them
        /// </summary>
        /// <param name="cardCount">the card count to peek</param>
        /// <returns></returns>
        public IEnumerable<int> PeekCards(int cardCount)
        {
            if (this.peekedQueue == null)
            {
                this.peekedQueue = new Queue<int>();
            }

            List<int> result = new List<int>();
            int i = 0;
            Queue<int>.Enumerator peekedCard = this.peekedQueue.GetEnumerator();
            while (peekedCard.MoveNext() && i < cardCount)
            {
                result.Add(peekedCard.Current);
                i++;
            }

            for (; i < cardCount; i++)
            {
                int card = this.GetCardFromPool();
                result.Add(card);
                this.peekedQueue.Enqueue(card);
            }

            return result;
        }

        private int GetCardFromPool()
        {
            Random rand = new Random();
            if (this.dealingOrder == null || this.dealingPos > this.maxDealingPos)
            {
                this.dealingOrder = this.GameCards.Shuffle();
                this.maxDealingPos = this.dealingOrder.Length - 1;
                this.maxDealingPos = rand.Next((int)(this.maxDealingPos * 0.55), (int)(this.maxDealingPos * 0.95)); // simulate player cutting cards
                this.dealingPos = 0;
            }

            return this.GameCards.DealCard(this.dealingOrder[this.dealingPos++]);
        }
    }
}
