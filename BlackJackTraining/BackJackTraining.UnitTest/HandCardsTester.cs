namespace BackJackTraining.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using BlackJackTraining;

    [TestClass]
    public class HandCardsTester
    {
        [TestMethod]
        public void TestHandCardsConstructor()
        {
            HandCards dealerHandCards = new HandCards(4);
            Assert.AreEqual("5", dealerHandCards.ToString());

            HandCards playerHandCards = new HandCards(10, 0);
            Assert.AreEqual("JA", playerHandCards.ToString());

            playerHandCards = new HandCards(9, 12);
            Assert.AreEqual("TK", playerHandCards.ToString());

            playerHandCards = new HandCards(11, 2);
            Assert.AreEqual("Q3", playerHandCards.ToString());

            try
            {
                dealerHandCards = new HandCards(-2);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }

            try
            {
                playerHandCards = new HandCards(6, 16);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentOutOfRangeException);
            }
        }

        [TestMethod]
        public void TestPlayerHandCardsConstructor()
        {
            PlayerHandCards playerHandCards = new PlayerHandCards(12);
            Assert.AreEqual(12, playerHandCards.BetAmount);
            Assert.IsFalse(playerHandCards.HasSurrendered);
        }

        [TestMethod]
        public void TestAddAndRemoveHandCards()
        {
            HandCards dealerHandCards = new HandCards(0);
            bool isSoftValue;

            Assert.AreEqual(11, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsTrue(isSoftValue);

            dealerHandCards = new HandCards(8);
            Assert.AreEqual("9", dealerHandCards.ToString());
            Assert.AreEqual(9, dealerHandCards.GetHandValue(out isSoftValue));
            dealerHandCards.AddCard(3);
            Assert.AreEqual("94", dealerHandCards.ToString());
            Assert.AreEqual(13, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsFalse(isSoftValue);

            Assert.AreEqual(3, dealerHandCards.ReverseAdd());
            Assert.AreEqual(8, dealerHandCards.ReverseAdd());
            Assert.AreEqual(-1, dealerHandCards.ReverseAdd()); // no card on hand
            Assert.AreEqual(string.Empty, dealerHandCards.ToString());

            // Soft 17
            dealerHandCards.AddCard(5);
            dealerHandCards.AddCard(0);
            Assert.AreEqual("6A", dealerHandCards.ToString());
            Assert.AreEqual(17, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsTrue(isSoftValue);

            // Hard 17
            dealerHandCards.AddCard(12);
            Assert.AreEqual("6AK", dealerHandCards.ToString());
            Assert.AreEqual(17, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsFalse(isSoftValue);

            // Black Jack
            dealerHandCards = new HandCards(10, 0);
            Assert.AreEqual("JA", dealerHandCards.ToString());
            Assert.AreEqual(21, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsTrue(isSoftValue);
            Assert.IsTrue(dealerHandCards.IsBlackJack);

            // Hard 21
            dealerHandCards = new HandCards(11, 9);
            dealerHandCards.AddCard(0);
            Assert.AreEqual("QTA", dealerHandCards.ToString());
            Assert.AreEqual(21, dealerHandCards.GetHandValue(out isSoftValue));
            Assert.IsFalse(isSoftValue);
            Assert.IsFalse(dealerHandCards.IsBlackJack);
        }

        [TestMethod]
        public void TestSplitCards()
        {
            PlayerHandCards playerHandCards = new PlayerHandCards(10);
            GameContext gameContext = new GameContext();
            playerHandCards.AddCard(7);
            playerHandCards.AddCard(7);
            Assert.IsTrue(playerHandCards.EligibleToSplit(1, gameContext));
            playerHandCards.AddCard(7);
            Assert.IsFalse(playerHandCards.EligibleToSplit(1, gameContext)); // more than 2 cards

            playerHandCards = new PlayerHandCards(10);
            playerHandCards.AddCard(10);
            playerHandCards.AddCard(12);
            Assert.IsFalse(playerHandCards.EligibleToSplit(1, gameContext));
            Assert.IsNull(playerHandCards.SplitAndGetNewHandCards(3, 6, gameContext));

            gameContext.TreatAllTensEqual = true;
            Assert.IsTrue(playerHandCards.EligibleToSplit(1, gameContext)); // when all ten values are treated the same, it can be split
            HandCards newHand = playerHandCards.SplitAndGetNewHandCards(4, 8, gameContext);
            Assert.IsNotNull(newHand);
            Assert.AreEqual("J5", playerHandCards.ToString());
            Assert.AreEqual("K9", newHand.ToString());
        }

        [TestMethod]
        public void TestEligibleToDoubleOrSurrender()
        {
            PlayerHandCards playerHandCards = new PlayerHandCards(10);
            GameContext gameContext = new GameContext();
            playerHandCards.AddCard(3);
            playerHandCards.AddCard(8);
            Assert.IsTrue(playerHandCards.EligibleToDouble(gameContext));
            playerHandCards.AddCard(7);
            Assert.IsFalse(playerHandCards.EligibleToDouble(gameContext)); // more than 2 cards

            playerHandCards = new PlayerHandCards(10);
            playerHandCards.AddCard(10);
            playerHandCards.AddCard(12);
            Assert.IsTrue(playerHandCards.EligibleToSurrender(gameContext));
            playerHandCards.AddCard(2);
            Assert.IsFalse(playerHandCards.EligibleToSurrender(gameContext)); // more than 2 cards
        }
    }
}
