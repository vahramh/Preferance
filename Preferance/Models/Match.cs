using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Preferance.Models
{
    public class Match
    {
        public string Id { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Player Player3 { get; set; }
        public Player Player4 { get; set; }
        public DateTime MatchDate { get; set; } = DateTime.Now;
        public IList<Game> Games { get; set; }
        public IList<Player> AllPlayers { get; set; }
        public Hand LastHand { get; set; }
        public string Player1Pool { get; set; } = "";
        public string Player2Pool { get; set; } = "";
        public string Player3Pool { get; set; } = "";
        public string Player4Pool { get; set; } = "";
        public string Player1Dump { get; set; } = "";
        public string Player2Dump { get; set; } = "";
        public string Player3Dump { get; set; } = "";
        public string Player4Dump { get; set; } = "";
        public string Player12Whist { get; set; } = "";
        public string Player13Whist { get; set; } = "";
        public string Player14Whist { get; set; } = "";
        public string Player21Whist { get; set; } = "";
        public string Player23Whist { get; set; } = "";
        public string Player24Whist { get; set; } = "";
        public string Player31Whist { get; set; } = "";
        public string Player32Whist { get; set; } = "";
        public string Player34Whist { get; set; } = "";
        public string Player41Whist { get; set; } = "";
        public string Player42Whist { get; set; } = "";
        public string Player43Whist { get; set; } = "";
        public int Player1CurrentPool { get; set; } = 0;
        public int Player2CurrentPool { get; set; } = 0;
        public int Player3CurrentPool { get; set; } = 0;
        public int Player4CurrentPool { get; set; } = 0;
        public int Player1CurrentDump { get; set; } = 0;
        public int Player2CurrentDump { get; set; } = 0;
        public int Player3CurrentDump { get; set; } = 0;
        public int Player4CurrentDump { get; set; } = 0;
        public int Player12CurrentWhist { get; set; } = 0;
        public int Player13CurrentWhist { get; set; } = 0;
        public int Player14CurrentWhist { get; set; } = 0;
        public int Player21CurrentWhist { get; set; } = 0;
        public int Player23CurrentWhist { get; set; } = 0;
        public int Player24CurrentWhist { get; set; } = 0;
        public int Player31CurrentWhist { get; set; } = 0;
        public int Player32CurrentWhist { get; set; } = 0;
        public int Player34CurrentWhist { get; set; } = 0;
        public int Player41CurrentWhist { get; set; } = 0;
        public int Player42CurrentWhist { get; set; } = 0;
        public int Player43CurrentWhist { get; set; } = 0;
        public float Player1CurrentScore { get; set; } = 0;
        public float Player2CurrentScore { get; set; } = 0;
        public float Player3CurrentScore { get; set; } = 0;
        public float Player4CurrentScore { get; set; } = 0;
        public string Status = "Active";
    }

    public class Game
    {
        public int Id { get; set; }
        public Player? Dealer { get; set; }
        public bool DealerPlaying { get; set; } = true;
        public Player? ActivePlayer { get; set; }
        public Player? NextPlayer { get; set; }
        public Player? Player1 { get; set; }
        public Player? Player2 { get; set; }
        public Player? Player3 { get; set; }
        public Player? Player4 { get; set; }
        public Player? HighCardPlayer { get; set; }
        public Card? HighCard { get; set; }
        public int Player1Whisting { get; set; } = 0;
        public int Player2Whisting { get; set; } = 0;
        public int Player3Whisting { get; set; } = 0;
        public int Player4Whisting { get; set; } = 0;
        public bool OpenWhist { get; set; } = false;
        public string Type { get; set; }
        public int Value { get; set; }
        public bool Player1Bidding { get; set; } = true;
        public bool Player2Bidding { get; set; } = true;
        public bool Player3Bidding { get; set; } = true;
        public bool Player4Bidding { get; set; } = true;
        public bool HerePossible { get; set; } = false;
        public bool MiserePossible { get; set; } = true;
        public string MisereOffered { get; set; } = "";
        public bool MisereSharable { get; set; } = true;
        public bool MisereShared { get; set; } = false;
        public string Status { get; set; }
        public Hand Player1Hand { get; set; }
        public Hand Player2Hand { get; set; }
        public Hand Player3Hand { get; set; }
        public Hand Player4Hand { get; set; }
        public Hand HandInPlay { get; set; }
        public Hand Talon { get; set; }
        public Hand Discarded { get; set; }
        public SetOfHands Player1HandResult { get; set; }
        public SetOfHands Player2HandResult { get; set; }
        public SetOfHands Player3HandResult { get; set; }
        public SetOfHands Player4HandResult { get; set; }
        public int OutcomeOffer { get; set; } = -1;
        public string MatchId { get; set; }
    }

    public class Hand
    {
        public string Id { get; set; }
        public IList<Card> Cards { get; set; }
    }

    public class SetOfHands
    {
        public string Id { get; set; }
        public IList<Hand> hands { get; set; }
    }

    public class Move
    {
        public int Id { get; set; }
        public Player? Player { get; set; }
        public Card? Card { get; set; }
    }
}
