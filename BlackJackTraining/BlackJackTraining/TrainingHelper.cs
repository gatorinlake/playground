namespace BlackJackTraining
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataAccess;

    public class TrainingHelper
    {
        /// <summary>
        /// Simulate playing games
        /// </summary>
        /// <param name="gamesToPlay">Total games to play</param>
        /// <param name="gameContext">Game context with card pool and rules</param>
        /// <param name="getInitialBet">Get the initial bet for each game</param>
        /// <param name="getPlayerAction">Get the player decision</param>
        /// <param name="outputGameRecord">Output the game record</param>
        /// <returns>Total amount won by the player</returns>
        public static decimal SimulateGames(
            int gamesToPlay,
            GameContext gameContext, 
            Func<GameContext, decimal> getInitialBet,
            Func<HandCards, PlayerHandCards, int, GameContext, PlayerAction> getPlayerAction, 
            Action<GameRecord> outputGameRecord)
        {
            decimal currentWin = 0;

            for (int gameCount = 1; gameCount <= gamesToPlay; gameCount++)
            {
                decimal currentBet = getInitialBet(gameContext);
                GameRecord currentGame = PlayOneGame(gameCount, currentBet, currentWin, gameContext, getPlayerAction);

                if(currentGame.Status == GameStatus.NotComplete)
                {
                    break;
                }

                currentWin = currentGame.ResultingAmount;
                outputGameRecord(currentGame);
            }

            return currentWin;
        }

        /// <summary>
        /// Get the next move suggestion by Wizard's Strategy: http://wizardofodds.com/games/blackjack/basics/
        /// </summary>
        /// <param name="dealerHandCards">Dealer's Hand</param>
        /// <param name="playerHandCards">Player's Hand</param>
        /// <param name="playerHands">Current hand count the player has</param>
        /// <param name="gameContext">Game Context</param>
        /// <returns></returns>
        public static PlayerAction GetActionByWizardStrategy(HandCards dealerHandCards, PlayerHandCards playerHandCards, int playerHands, GameContext gameContext)
        {
            bool isSoftValue;
            int dealerValue = dealerHandCards.GetHandValue(out isSoftValue);
            int playerValue = playerHandCards.GetHandValue(out isSoftValue);

            if (playerValue == 16 && dealerValue == 10 && playerHandCards.EligibleToSurrender(gameContext))
            {
                return PlayerAction.Surrender;
            }

            if (dealerValue >= 2 && dealerValue <= 6)
            {
                if (playerHandCards.EligibleToSplit(playerHands, gameContext))
                {
                    if(playerValue != 8 && playerValue != 10 && playerValue != 20)
                    {
                        return PlayerAction.Split;
                    }
                }

                if (isSoftValue)
                {
                    if (playerValue >= 12 && playerValue <= 15)
                    {
                        return PlayerAction.Hit;
                    }
                    else if (playerValue >= 16 && playerValue <= 18)
                    {
                        return playerHandCards.EligibleToDouble(gameContext) ? PlayerAction.Double : (playerValue == 18 ? PlayerAction.Stand : PlayerAction.Hit);
                    }
                    else if (playerValue >= 19 && playerValue <= 21)
                    {
                        return PlayerAction.Stand;
                    }
                }
                else
                {
                    if (playerValue >= 4 && playerValue <= 8)
                    {
                        return PlayerAction.Hit;
                    }
                    else if (playerValue >= 9 && playerValue <= 11)
                    {
                        return playerHandCards.EligibleToDouble(gameContext) ? PlayerAction.Double : PlayerAction.Hit;
                    }
                    else if (playerValue >= 12 && playerValue <= 21)
                    {
                        return PlayerAction.Stand;
                    }
                }
            }
            else if (dealerValue >= 7 && dealerValue <= 11)
            {
                if (playerHandCards.EligibleToSplit(playerHands, gameContext))
                {
                    if (playerValue == 16 || playerHandCards.ToString().Equals("AA"))
                    {
                        return PlayerAction.Split;
                    }
                }

                if (isSoftValue)
                {
                    if (playerValue >= 12 && playerValue <= 18)
                    {
                        return PlayerAction.Hit;
                    }
                    else if (playerValue >= 19 && playerValue <= 21)
                    {
                        return PlayerAction.Stand;
                    }
                }
                else
                {
                    if (playerValue >= 4 && playerValue <= 16)
                    {
                        if (playerValue == 10 || playerValue == 11)
                        {
                            if (playerValue > dealerValue)
                            {
                                return playerHandCards.EligibleToDouble(gameContext) ? PlayerAction.Double : PlayerAction.Hit;
                            }
                        }

                        return PlayerAction.Hit;
                    }
                    else if (playerValue >= 12 && playerValue <= 21)
                    {
                        return PlayerAction.Stand;
                    }
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Dealer value is invalid: {0}", dealerValue), "dealerHandCards");
            }

            throw new ArgumentException(string.Format("Player value is invalid: {0} IsSoftValue: {1}", playerValue, isSoftValue), "playerHandCards");
        }

        private static GameRecord PlayOneGame(
            long gameId,
            decimal initalBet,
            decimal initalWinAmount,
            GameContext gameContext, 
            Func<HandCards, PlayerHandCards, int, GameContext, PlayerAction> getPlayerAction)
        {
            GameRecord newGame = new GameRecord(gameId);

            // Player place initial bet, game starts
            decimal totalWinThisGame = 0;
            newGame.AddGameStartedEvent(initalBet);

            // Initial round of card dealing
            PlayerHandCards playerCards = new PlayerHandCards(initalBet);
            playerCards.AddCard(gameContext.GetCardWithReshuffle()); // player first card
            int dealerFaceDownCard = gameContext.GetCardWithReshuffle(); // dealer first card, face-down
            playerCards.AddCard(gameContext.GetCardWithReshuffle()); // player second card
            HandCards dealerCards = new HandCards(gameContext.GetCardWithReshuffle()); // dealer second card, face-up

            LinkedList<PlayerHandCards> playerHands = new LinkedList<PlayerHandCards>();
            playerHands.AddFirst(playerCards);
            newGame.AddOpenHandEvent(playerHands.Count, dealerCards, playerHands.First());

            // Handle player hands until all of them either stand or be busted.
            int playerId = 1;
            for (LinkedListNode<PlayerHandCards> node = playerHands.First; node != null; )
            {
                PlayerHandCards currentHand = node.Value;
                PlayerAction currentAction = getPlayerAction(dealerCards, currentHand, playerHands.Count, gameContext);
                PlayerHandCards newHand = null;

                switch (currentAction)
                {
                    case PlayerAction.Double:
                        currentHand.BetAmount *= 2;
                        currentHand.AddCard(gameContext.GetCardWithReshuffle());
                        break;

                    case PlayerAction.Hit:
                        currentHand.AddCard(gameContext.GetCardWithReshuffle());
                        break;

                    case PlayerAction.Split:
                        newHand = currentHand.SplitAndGetNewHandCards(
                                gameContext.GetCardWithReshuffle(),
                                gameContext.GetCardWithReshuffle(),
                                gameContext);
                        break;

                    case PlayerAction.Surrender:
                        currentHand.HasSurrendered = true;
                        break;
                }

                newGame.AddEventByPlayerAction(currentAction, playerId, dealerCards, currentHand, gameContext);

                if (newHand != null)
                {
                    playerHands.AddLast(newHand);
                    newGame.AddOpenHandEvent(playerHands.Count, dealerCards, newHand);
                }

                // Conditions to move to next hand
                LinkedListNode<PlayerHandCards> nodeToRemove;
                if (currentHand.HasSurrendered)
                {
                    decimal lostAmount = currentHand.BetAmount / 2;
                    totalWinThisGame -= lostAmount;
                    newGame.AddCloseHandEvent(playerId, dealerCards, currentHand, lostAmount, "Surrender");
                    nodeToRemove = node;
                    playerHands.Remove(nodeToRemove);

                    node = node.Next;
                    playerId++;
                }
                else if (currentHand.IsBusted)
                {
                    totalWinThisGame -= currentHand.BetAmount;
                    newGame.AddCloseHandEvent(playerId, dealerCards, currentHand, -currentHand.BetAmount, "Busted");
                    nodeToRemove = node;
                    playerHands.Remove(nodeToRemove);

                    node = node.Next;
                    playerId++;
                }
                else if (currentAction == PlayerAction.Stand)
                {
                    node = node.Next;
                    playerId++;
                }
            }

            // Dealer dealing for himself until he is busted, or over 17
            if (playerHands.Count > 0)
            {
                bool isSoftValue;

                dealerCards.AddCard(dealerFaceDownCard);
                string initialCards = dealerCards.ToString();
                int currentValue = dealerCards.GetHandValue(out isSoftValue);

                while (!dealerCards.IsBusted && !dealerCards.IsBlackJack)
                {
                    if (currentValue >= 17)
                    {
                        if (!isSoftValue || !gameContext.DealerHitsOnSoft17)
                        {
                            break;
                        }
                    }

                    int newCard = gameContext.GetCardWithReshuffle();
                    dealerCards.AddCard(newCard);
                    currentValue = dealerCards.GetHandValue(out isSoftValue);
                }

                newGame.AddDealerEvent(initialCards, dealerCards.ToString(), currentValue);

                // Check game result by comparing dealer with each player hand
                playerId = 1;
                for (LinkedListNode<PlayerHandCards> node = playerHands.First; node != null; node = node.Next, playerId++)
                {
                    if (!node.Value.HasSurrendered && !node.Value.IsBusted)
                    {
                        decimal winAmount;
                        if (CheckGameStatus(dealerCards, node.Value, gameContext, out winAmount) != GameStatus.NotComplete)
                        {
                            totalWinThisGame += winAmount;
                            newGame.AddCloseHandEvent(playerId, dealerCards, node.Value, winAmount);
                        }
                    }
                }
            }

            newGame.AddGameCompletedEvent(totalWinThisGame, initalWinAmount + totalWinThisGame);
            return newGame;
        }

        private static GameStatus CheckGameStatus(HandCards dealerHandCards, PlayerHandCards playerHandCards, GameContext gameContext, out decimal winAmount)
        {
            bool isSoftValue;
            int playerValue = playerHandCards.GetHandValue(out isSoftValue);
            int dealerValue = dealerHandCards.GetHandValue(out isSoftValue);
            GameStatus gameStatusResult = GameStatus.NotComplete;

            if (playerValue > 21 || playerHandCards.HasSurrendered)
            {
                gameStatusResult = GameStatus.DealerWins; // player is busted or has surrendered
            }
            else if (dealerValue >= 17)
            {
                if (!isSoftValue || !gameContext.DealerHitsOnSoft17)
                {
                    if (dealerValue > 21)
                    {
                        gameStatusResult = GameStatus.PlayerWins; // dealer is busted
                    }
                    else if (dealerValue == 21 && playerValue == 21)
                    {
                        if (dealerHandCards.IsBlackJack)
                        {
                            gameStatusResult = playerHandCards.IsBlackJack ? GameStatus.DrawGame : GameStatus.DealerWins;
                        }
                        else
                        {
                            gameStatusResult = playerHandCards.IsBlackJack ? GameStatus.PlayerWins : GameStatus.DrawGame;
                        }
                    }
                    else if (dealerValue == playerValue)
                    {
                        gameStatusResult = GameStatus.DrawGame;
                    }
                    else
                    {
                        gameStatusResult = playerValue > dealerValue ? GameStatus.PlayerWins : GameStatus.DealerWins;
                    }
                }
            }

            switch (gameStatusResult)
            {
                case GameStatus.DealerWins:
                    winAmount = -playerHandCards.BetAmount;
                    break;

                case GameStatus.PlayerWins:
                    winAmount = playerHandCards.BetAmount * (playerHandCards.IsBlackJack ? gameContext.BlackJackPayRate : 1);
                    break;

                default:
                    winAmount = 0m;
                    break;
            }

            return gameStatusResult;
        }
    }
}
