namespace BackJackTraining.UnitTest
{
    using System;
    using System.Linq;
    using BlackJackTraining;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CardPoolTester
    {
        [TestMethod]
        public void TestCardPoolConstructor()
        {
            CardPool newCardPool = new CardPool();
            Assert.AreEqual(1, newCardPool.DeckCount, "DeckCount should be 1 by default.");
            Assert.AreEqual(52, newCardPool.CardCount, "CardCount should be 52 with 1 deck as default.");

            newCardPool = new CardPool(8);
            Assert.AreEqual(8, newCardPool.DeckCount, "DeckCount should be 8 with override.");
            Assert.AreEqual(52 * 8, newCardPool.CardCount, "CardCount should be 52 * 8 with 8 decks.");

            try 
            {
                newCardPool = new CardPool(-4);
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }
        }

        [TestMethod]
        public void TestGetProbability()
        {
            CardPool defaultCardPool = new CardPool();

            for (int i = 0; i < 13; i++)
            {
                Assert.AreEqual(1m/13m, defaultCardPool.GetValueRangeProbability(i, i));
            }

            Assert.AreEqual(2m / 13, defaultCardPool.GetValueRangeProbability(1, 2));
            Assert.AreEqual(8m / 13, defaultCardPool.GetValueRangeProbability(12, 5));
            Assert.AreEqual(1, defaultCardPool.GetValueRangeProbability(0, 12));
            Assert.AreEqual(6m / 13, defaultCardPool.GetValueRangeProbability(4, 9));

            try
            {
                defaultCardPool.GetValueRangeProbability(-4, 9);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }

            try
            {
                defaultCardPool.GetValueRangeProbability(20, 9);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }

            try
            {
                defaultCardPool.GetValueRangeProbability(-3, -4);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }

            try
            {
                defaultCardPool.GetValueRangeProbability(15, 13);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }
        }

        [TestMethod]
        public void TestDealingCardsWithEightDeck()
        {
            CardPool eightDeckCardPool = new CardPool(8);
            DealCardsAndCheckProbability(eightDeckCardPool, 30);
        }

        [TestMethod]
        public void TestDealingAllCards()
        {
            CardPool defaultCardPool = new CardPool();
            DealCardsAndCheckProbability(defaultCardPool, 60);

            Console.WriteLine("Shuffle Cards.");
            int[] dealingOrder = defaultCardPool.Shuffle();

            Assert.AreEqual(defaultCardPool.DeckCount * 13 * 4, dealingOrder.Length);
            DealCardsAndCheckProbability(defaultCardPool, dealingOrder.Length, dealingOrder, 0);

            Assert.AreEqual(0, defaultCardPool.CardCount);
        }

        [TestMethod]
        public void TestUnDealCards()
        {
            CardPool fourDeckCardPool = new CardPool(4);

            Console.WriteLine("Shuffle Cards.");
            int[] dealingOrder = fourDeckCardPool.Shuffle();
            int curPos = DealCardsAndCheckProbability(fourDeckCardPool, 600, dealingOrder);
            curPos = UnDealCardsAndCheckProbability(fourDeckCardPool, dealingOrder.Length, dealingOrder, curPos);
            Assert.AreEqual(0, curPos, "Dealing position is not updated correctly.");
            Assert.IsFalse(fourDeckCardPool.UnDealCard(2)); // Try to undeal any card will fail since the card deck is currently full.
        }

        [TestMethod]
        public void TestDealingCardsWithShuffles()
        {
            CardPool sixDeckCardPool = new CardPool(6);
            
            DealCardsAndCheckProbability(sixDeckCardPool, 20);
            
            Console.WriteLine("Shuffle Cards.");
            int []dealingOrder = sixDeckCardPool.Shuffle();
            int curPos = DealCardsAndCheckProbability(sixDeckCardPool, 10, dealingOrder);
            Assert.AreEqual(10, curPos, "Dealing position is not updated correctly.");

            curPos = UnDealCardsAndCheckProbability(sixDeckCardPool, 8, dealingOrder, curPos);
            Assert.AreEqual(10 - 8, curPos, "Dealing position is not updated correctly.");
            
            curPos = DealCardsAndCheckProbability(sixDeckCardPool, 32, dealingOrder, curPos);
            Assert.AreEqual(10 - 8 + 32, curPos, "Dealing position is not updated correctly.");

            Console.WriteLine("Shuffle Cards.");
            dealingOrder = sixDeckCardPool.Shuffle();
            curPos = 0;

            curPos = DealCardsAndCheckProbability(sixDeckCardPool, 22, dealingOrder, curPos);
            Assert.AreEqual(22, curPos, "Dealing position is not updated correctly.");

            // Dealing all cards left
            int cardsToDeal = dealingOrder.Length - curPos;
            curPos = DealCardsAndCheckProbability(sixDeckCardPool, cardsToDeal + 1, dealingOrder, curPos);
            Assert.AreEqual(dealingOrder.Length, curPos, "Dealing position is not updated correctly.");
        }

        [TestMethod]
        public void TestCardPoolClone()
        {
            CardPool origCardPool = new CardPool(4);
            DealCardsAndCheckProbability(origCardPool, 28);

            CardPool cloneCardPool = (CardPool)origCardPool.Clone();
            Random rand = new Random();

            for (int i = 0; i < 10; i++)
            {
                int[] availableValues = origCardPool.GetAvailableCardValues().ToArray();
                int valIndex = rand.Next(0, availableValues.Length - 1);

                int dealCard = origCardPool.DealCard(availableValues[valIndex]);
                Assert.AreEqual(dealCard, cloneCardPool.DealCard(dealCard));
                Assert.AreEqual(origCardPool.GetValueRangeProbability(dealCard, dealCard), cloneCardPool.GetValueRangeProbability(dealCard, dealCard));
            }
        }

        private static int DealCardsAndCheckProbability(CardPool cardPool, int cardsToDeal, int[] dealingOrder = null, int dealingPosition = 0)
        {
            int[] cardsByValueLocal = new int[13];
            int cardCountLocal = cardPool.CardCount;
            Random rand = new Random();

            Assert.IsTrue(cardCountLocal > 0, "Empty card pool is used.");

            // Save card pool state to local
            for (int i = 0; i < cardsByValueLocal.Length; i++)
            {
                cardsByValueLocal[i] = (int)Math.Round(cardCountLocal * cardPool.GetValueRangeProbability(i, i), 0);
            }

            // Deal card and update local state
            for (int i = 0; i < cardsToDeal; i++)
            {
                int cardValue;
                if (dealingOrder != null)
                {
                    if (dealingPosition >= dealingOrder.Length)
                    {
                        cardValue = -1;
                    }
                    else
                    {
                        cardValue = cardPool.DealCard(dealingOrder[dealingPosition++]);
                    }
                }
                else
                {
                    int[] availableValues = cardPool.GetAvailableCardValues().ToArray();
                    if (availableValues.Length == 0)
                    {
                        cardValue = -1;
                    }
                    else
                    {
                        int valIndex = rand.Next(0, availableValues.Length - 1);
                        cardValue = cardPool.DealCard(availableValues[valIndex]);
                    }
                }

                Console.WriteLine("Deal Card {0}.", cardValue);

                if(cardValue < 0)
                {
                    break;
                }

                cardCountLocal--;
                cardsByValueLocal[cardValue]--;
            }

            // Check local against the card pool
            Assert.AreEqual(cardPool.CardCount, cardCountLocal, "Local card count is incorrect.");
            if (cardCountLocal > 0)
            {
                for (int i = 0; i < cardsByValueLocal.Length; i++)
                {
                    decimal probabilityLocal = (decimal)cardsByValueLocal[i] / cardCountLocal;
                    Assert.AreEqual(cardPool.GetValueRangeProbability(i, i), probabilityLocal, string.Format("Local probability is incorrect for card value {0}", i));
                }
            }

            return dealingPosition;
        }

        private static int UnDealCardsAndCheckProbability(CardPool cardPool, int cardsToUnDeal, int[] dealingOrder, int dealingPosition)
        {
            int[] cardsByValueLocal = new int[13];
            int cardCountLocal = cardPool.CardCount;
            Random rand = new Random();

            Assert.IsTrue(dealingPosition >= cardsToUnDeal, "Doesn't have enough cards to undeal.");

            // Save card pool state to local
            for (int i = 0; i < cardsByValueLocal.Length; i++)
            {
                cardsByValueLocal[i] = (int)Math.Round(cardCountLocal * cardPool.GetValueRangeProbability(i, i), 0);
            }

            // Deal card and update local state
            for (int i = 0; i < cardsToUnDeal; i++)
            {
                int cardValue = dealingOrder[--dealingPosition];
                Assert.IsTrue(cardPool.UnDealCard(cardValue));

                Console.WriteLine("UnDeal Card {0}.", cardValue);

                cardCountLocal++;
                cardsByValueLocal[cardValue]++;
            }

            // Check local against the card pool
            Assert.AreEqual(cardPool.CardCount, cardCountLocal, "Local card count is incorrect.");
            if (cardCountLocal > 0)
            {
                for (int i = 0; i < cardsByValueLocal.Length; i++)
                {
                    decimal probabilityLocal = (decimal)cardsByValueLocal[i] / cardCountLocal;
                    Assert.AreEqual(cardPool.GetValueRangeProbability(i, i), probabilityLocal, string.Format("Local probability is incorrect for card value {0}", i));
                }
            }

            return dealingPosition;
        }
    }
}
