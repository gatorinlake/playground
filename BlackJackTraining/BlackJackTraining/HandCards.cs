namespace BlackJackTraining
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class HandCards
    {
        protected readonly Stack<int> HoldingCards = new Stack<int>();

        public bool IsBusted
        {
            get
            {
                bool isSoftValue;
                return this.GetHandValue(out isSoftValue) > 21;
            }
        }

        public bool IsBlackJack
        {
            get
            {
                bool isSoftValue;
                if (this.GetHandValue(out isSoftValue) == 21)
                {
                    return isSoftValue;
                }

                return false;
            }
        }

        public int CardsInHand
        {
            get
            {
                return this.HoldingCards.Count;
            }
        }

        public HandCards()
        {
        }

        public HandCards(int val)
        {
            // For dealer, one face-up card on hand
            this.AddCard(val);
        }

        public HandCards(int val1, int val2)
        {
            // For player, two face-up cards on hand
            this.AddCard(val1);
            this.AddCard(val2);
        }

        public void AddCard(int val)
        {
            if(val < 0 || val > 12)
            {
                throw new ArgumentOutOfRangeException("val", "Card value is between 0 and 12.");
            }

            this.HoldingCards.Push(val);
        }

        public int ReverseAdd()
        {
            if (this.HoldingCards.Count > 0)
            {
                return this.HoldingCards.Pop();
            }

            return -1;
        }

        public int GetHandValue(out bool isSoftValue)
        {
            isSoftValue = false;
            int sum = 0;

            foreach (int card in this.HoldingCards)
            {
                if(card == 0)
                {
                    isSoftValue = true;
                }

                if(card > 9)
                {
                    sum += 10;
                }
                else
                {
                    sum += card + 1;
                }
            }

            if (isSoftValue)
            {
                if (sum > 11)
                {
                    isSoftValue = false;
                }
                else
                {
                    sum += 10;
                }
            }

            return sum;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<int> sortedCards = this.HoldingCards.ToArray().Reverse();
            
            // Array.Sort(sortedCards);
            foreach (int card in sortedCards)
            {
                sb.Append(GetCardNameByValue(card));
            }

            return sb.ToString();
        }

        public static char GetCardNameByValue(int card)
        {
            switch (card)
            {
                case 0:
                    return 'A';

                case 9:
                    return 'T';

                case 10:
                    return 'J';

                case 11:
                    return 'Q';

                case 12:
                    return 'K';

                default:
                    return (char)('1' + card);
            }
        }
    }
}
