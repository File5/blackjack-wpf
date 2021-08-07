using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCS
{
    public class Blackjack
    {
        private const int DEFAULT_MONEY = 1000;
        private const int DEFAULT_BET = 50;

        private List<BlackjackCard> myCards = new List<BlackjackCard>();
        private List<BlackjackCard> dealerCards = new List<BlackjackCard>();
        public int money { get; set; }
        public int bet { get; set; }
        public int MyScore
        {
            get { return CountScore(myCards); }
        }
        public int DealerScore
        {
            get { return CountScore(dealerCards); }
        }
        public GameStatus GameStatus { get; private set; }

        public Blackjack()
        {
            money = DEFAULT_MONEY;
        }
        public void Bet()
        {
            bet = DEFAULT_BET;
            money -= bet;

            myCards.Clear();
            dealerCards.Clear();

            GameStatus = GameStatus.PRE_GAME;
        }
        public void CurrentGotCard(BlackjackCard card)
        {
            if (GameStatus == GameStatus.PLAYER) {
                myCards.Add(card);
            } else
            {
                dealerCards.Add(card);
            }
            SetGameStatus();
        }
        public void Stand()
        {
            switch (GameStatus)
            {
                case GameStatus.PRE_GAME:
                    GameStatus = GameStatus.PLAYER;
                    break;
                case GameStatus.PLAYER:
                    GameStatus = GameStatus.DEALER;
                    break;
                case GameStatus.DEALER:
                    GameStatus = GameStatus.DEALER_STAND;
                    SetGameStatus();
                    break;
                case GameStatus.DEALER_STAND:
                    SetGameStatus();
                    break;
            }
        }
        public bool IsGameEnded()
        {
            return GameStatus == GameStatus.WIN || GameStatus == GameStatus.LOSE ||
                GameStatus == GameStatus.PUSH;
        }

        private void SetGameStatus()
        {
            switch (GameStatus)
            {
                case GameStatus.PLAYER:
                    if (MyScore > 21)
                    {
                        GameStatus = GameStatus.LOSE;
                    }
                    break;
                case GameStatus.DEALER:
                    if (DealerScore > 21)
                    {
                        GameStatus = GameStatus.WIN;
                    } else if (!ShouldDealerHit())
                    {
                        GameStatus = GameStatus.DEALER_STAND;
                    }
                    break;
                case GameStatus.DEALER_STAND:
                    if (DealerScore > 21)
                    {
                        GameStatus = GameStatus.WIN;
                    } else
                    {
                        if (MyScore > DealerScore)
                        {
                            GameStatus = GameStatus.WIN;
                        } else if (MyScore < DealerScore)
                        {
                            GameStatus = GameStatus.LOSE;
                        } else
                        {
                            GameStatus = GameStatus.PUSH;
                        }
                    }
                    break;
            }
            switch (GameStatus)
            {
                case GameStatus.WIN:
                    money += 2 * bet;
                    break;
                case GameStatus.PUSH:
                    money += bet;
                    break;
            }
        }
        private bool ShouldDealerHit()
        {
            return DealerScore < 17;
        }
        private int CountScore(List<BlackjackCard> cards)
        {
            var result = 0;
            var possibleTens = 0;
            foreach (var card in cards)
            {
                var rankInt = (int)card.rank;
                if (2 <= rankInt && rankInt <= 10)
                {
                    result += rankInt;
                } else
                {
                    if (card.rank == CardRank.Ace)
                    {
                        result += 1;
                        possibleTens += 1;
                    } else
                    {
                        result += 10;
                    }
                }
            }
            while (result + 10 <= 21 && possibleTens > 0)
            {
                result += 10;
                possibleTens--;
            }
            return result;
        }
    }
    public enum GameStatus
    {
        PRE_GAME,
        PLAYER,
        DEALER,
        DEALER_STAND,
        WIN,
        LOSE,
        PUSH
    }
    public class BlackjackCard
    {
        public CardRank rank { get; set; }
        public CardSuit suit { get; set; }

        public BlackjackCard(CardRank rank, CardSuit suit)
        {
            this.rank = rank;
            this.suit = suit;
        }
        public BlackjackCard(int card)
        {
            if (card < 2) return;

            var suit = CardSuit.DIAMONDS;

            if (card < 28)
            {
                if (card > 14)
                {
                    suit = CardSuit.HEARTS;
                }
            }
            else
            {
                if (card > 40)
                {
                    suit = CardSuit.SPADES;
                }
                else
                {
                    suit = CardSuit.CLUBS;
                }
            }

            var rankInt = (card - 2) % 13 + 2;
            if (rankInt == 14 || rankInt == 1)
            {
                rankInt = 0;
            }
            rank = (CardRank)rankInt;
            this.suit = suit;
        }

    }
    public enum CardSuit
    {
        DIAMONDS, // Бубны
        HEARTS,   // Червы
        SPADES,   // Пики
        CLUBS     // Трефы
    }
    public enum CardRank
    {
        Ace = 0,
        Jack = 11,
        Queen = 12,
        King = 13
    }
}
