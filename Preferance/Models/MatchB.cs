using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Preferance.Models
{
    public class MatchB
    {
        public string Id { get; set; }
        public Player North { get; set; }
        public Player South { get; set; }
        public Player East { get; set; }
        public Player West { get; set; }
        public DateTime MatchDate { get; set; } = DateTime.Now;
        public IList<GameB> Games { get; set; }
        public IList<Player> AllPlayers { get; set; }
        public HandB LastHand { get; set; }
        public int NorthSouthScore { get; set; } = 0;
        public int EastWestScore { get; set; } = 0;
        public string Status = "Active";
    }

    public class GameB
    {
        public int Id { get; set; }
        public Player? Dealer { get; set; }
        public bool ActiveTeamNS { get; set; } = true;
        public Player? NextPlayer { get; set; }
        public Player? North { get; set; }
        public Player? South { get; set; }
        public Player? East { get; set; }
        public Player? West { get; set; }
        public Player? HighCardPlayer { get; set; }
        public CardB? HighCard { get; set; }
        public string Type { get; set; }
        public int Value { get; set; }
        public bool Challenge { get; set; } = false;
        public bool Contra { get; set; } = false;
        public bool Kaput { get; set; } = false;
        public string Status { get; set; }
        public HandB NorthHand { get; set; }
        public HandB SouthHand { get; set; }
        public HandB EastHand { get; set; }
        public HandB WestHand { get; set; }
        public HandB HandInPlay { get; set; }
        public HandB OpenCards { get; set; }
        public HandB NorthSouthHandResult { get; set; }
        public HandB EastWestHandResult { get; set; }
        public int NorthSouthPoints { get; set; } = 0;
        public int EastWestPoints { get; set; } = 0;
        public int NorthSouthExtras { get; set; } = 0;
        public int EastWestExtras { get; set; } = 0;
        public int NorthSouthScore { get; set; } = 0;
        public int EastWestScore { get; set; } = 0;
        public string MatchBId { get; set; }
    }

    public class HandB
    {
        public string Id { get; set; }
        public IList<CardB> Cards { get; set; }
    }

    public class SetOfHandsB
    {
        public string Id { get; set; }
        public IList<HandB> hands { get; set; }
    }

    public class MoveB
    {
        public int Id { get; set; }
        public Player? Player { get; set; }
        public Card? CardB { get; set; }
    }
}
