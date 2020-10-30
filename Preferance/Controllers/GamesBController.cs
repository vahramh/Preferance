using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Preferance.Data;
using Preferance.Migrations;
using Preferance.Models;

namespace Preferance.Controllers
{
    public class GamesBController : Controller
    {
        private readonly ApplicationDbContext _context;

        ErrorViewModel errorView = null;

        public GamesBController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Play(string id, string move = "", string cardSuit = "", string cardValue = "")
        {
            CardB playedCard = new CardB();
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            string UUID = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (UUID == null)
            {
                errorView = new ErrorViewModel();
                ModelState.AddModelError(string.Empty, "You are not logged in. ");
                errorView.RequestId = "0x10018000";
                errorView.RequestString = "You are not logged in. ";
                return View("Error", errorView);
            }

            var match = _context.MatchB.Include(o => o.Games).Include(o => o.North).Include(o => o.South).Include(o => o.East).Include(o => o.West).Include(o => o.LastHand)
                .ThenInclude(h => h.Cards)
              .FirstOrDefault(m => m.Id == id);

            match.Games = _context.GameB.OrderBy(o => o.Id).Where(g => g.MatchBId == match.Id).ToList();

            match.LastHand.Cards = _context.CardB.Where(c => c.HandBId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

            GameB game = _context.GameB.Where(m => m.MatchBId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.North).Include(o => o.South).Include(o => o.East).Include(o => o.West)
                .Include(o => o.NorthHand).Include(o => o.SouthHand).Include(o => o.EastHand).Include(o => o.WestHand)
                .Include(o => o.HandInPlay)
                .Include(o => o.OpenCards).ThenInclude(q => q.Cards)
                .Include(o => o.NorthSouthHandResult).ThenInclude(q => q.Cards)
                .Include(o => o.EastWestHandResult).ThenInclude(q => q.Cards)
                .ToList()[0];
            if ((game.Status == "Dealing") || (game.Status == "Bidding"))
            {
                Response.Redirect("/../../MatchesB/Play/" + id);
                errorView = new ErrorViewModel();
                return View("Error", errorView);
            }

            if (game.OpenCards == null)
            {
                game.OpenCards = new HandB();
                game.OpenCards.Id = Guid.NewGuid().ToString();
                game.OpenCards.Cards = new List<CardB>();
            }
            game.NorthHand.Cards = _context.CardB.Where(c => c.HandBId == game.NorthHand.Id).OrderBy(c => c.Seniority).ToList();
            game.SouthHand.Cards = _context.CardB.Where(c => c.HandBId == game.SouthHand.Id).OrderBy(c => c.Seniority).ToList();
            game.EastHand.Cards = _context.CardB.Where(c => c.HandBId == game.EastHand.Id).OrderBy(c => c.Seniority).ToList();
            game.WestHand.Cards = _context.CardB.Where(c => c.HandBId == game.WestHand.Id).OrderBy(c => c.Seniority).ToList();

            if (game.HandInPlay == null)
            {
                game.HandInPlay = new HandB();
                game.HandInPlay.Id = Guid.NewGuid().ToString();
                game.HandInPlay.Cards = new List<CardB>();
            }
            else
            {
                game.HandInPlay.Cards = _context.CardB.Where(c => c.HandBId == game.HandInPlay.Id).OrderBy(c => c.Sequence).ToList();
            }

            if (move == "")
            {
                ViewBag.Player = UUID;
                ViewBag.Dealer = game.Dealer.Id;

                return View("GameProgress", match);
            }

            if (move == "collect")
            {
                Collect(game, move);
                _context.SaveChanges();


                if (game.NorthHand.Cards.Count + game.SouthHand.Cards.Count + game.EastHand.Cards.Count + game.WestHand.Cards.Count == 0)
                {
                    Response.Redirect("/../../MatchesB/CompleteGame/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            if (move == "Move")
            {
                if (game.NextPlayer.Id == game.North.Id)
                {
                    foreach (CardB card in game.NorthHand.Cards)
                    {
                        if ((card.Colour == cardSuit) & (card.Value == cardValue))
                        {
                            playedCard = card;
                            if (game.HandInPlay.Cards.Count == 0)
                            {
                                playedCard.Sequence = 1;
                            }
                            else
                            {
                                playedCard.Sequence = game.HandInPlay.Cards.Max(m => m.Sequence) + 1;
                            }
                        }
                    }
                    game.HandInPlay.Cards.Add(playedCard);
                    game.NorthHand.Cards.Remove(playedCard);

                    if (game.HandInPlay.Cards.Count == 1)
                    {
                        game.HighCardPlayer = game.NextPlayer;
                        game.HighCard = playedCard;
                    }
                    else
                    {
                        if (cardSuit == game.Type)
                        {
                            if (game.HighCard.Colour == game.Type)
                            {
                                if (TrumpCardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.North;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.North;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.North;
                                }
                            }
                        }
                    }

                }

                if (game.NextPlayer.Id == game.South.Id)
                {
                    foreach (CardB card in game.SouthHand.Cards)
                    {
                        if ((card.Colour == cardSuit) & (card.Value == cardValue))
                        {
                            playedCard = card;
                            if (game.HandInPlay.Cards.Count == 0)
                            {
                                playedCard.Sequence = 1;
                            }
                            else
                            {
                                playedCard.Sequence = game.HandInPlay.Cards.Max(m => m.Sequence) + 1;
                            }
                        }
                    }
                    game.HandInPlay.Cards.Add(playedCard);
                    game.SouthHand.Cards.Remove(playedCard);

                    if (game.HandInPlay.Cards.Count == 1)
                    {
                        game.HighCardPlayer = game.NextPlayer;
                        game.HighCard = playedCard;
                    }
                    else
                    {
                        if (cardSuit == game.Type)
                        {
                            if (game.HighCard.Colour == game.Type)
                            {
                                if (TrumpCardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.South;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.South;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.South;
                                }
                            }
                        }
                    }
                }

                if (game.NextPlayer.Id == game.East.Id)
                {
                    foreach (CardB card in game.EastHand.Cards)
                    {
                        if ((card.Colour == cardSuit) & (card.Value == cardValue))
                        {
                            playedCard = card;
                            if (game.HandInPlay.Cards.Count == 0)
                            {
                                playedCard.Sequence = 1;
                            }
                            else
                            {
                                playedCard.Sequence = game.HandInPlay.Cards.Max(m => m.Sequence) + 1;
                            }
                        }
                    }
                    game.HandInPlay.Cards.Add(playedCard);
                    game.EastHand.Cards.Remove(playedCard);

                    if (game.HandInPlay.Cards.Count == 1)
                    {
                        game.HighCardPlayer = game.NextPlayer;
                        game.HighCard = playedCard;
                    }
                    else
                    {
                        if (cardSuit == game.Type)
                        {
                            if (game.HighCard.Colour == game.Type)
                            {
                                if (TrumpCardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.East;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.East;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.East;
                                }
                            }
                        }
                    }
                }

                if (game.NextPlayer.Id == game.West.Id)
                {
                    foreach (CardB card in game.WestHand.Cards)
                    {
                        if ((card.Colour == cardSuit) & (card.Value == cardValue))
                        {
                            playedCard = card;
                            if (game.HandInPlay.Cards.Count == 0)
                            {
                                playedCard.Sequence = 1;
                            }
                            else
                            {
                                playedCard.Sequence = game.HandInPlay.Cards.Max(m => m.Sequence) + 1;
                            }
                        }
                    }
                    game.HandInPlay.Cards.Add(playedCard);
                    game.WestHand.Cards.Remove(playedCard);

                    if (game.HandInPlay.Cards.Count == 1)
                    {
                        game.HighCardPlayer = game.NextPlayer;
                        game.HighCard = playedCard;
                    }
                    else
                    {
                        if (cardSuit == game.Type)
                        {
                            if (game.HighCard.Colour == game.Type)
                            {
                                if (TrumpCardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.West;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.West;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.West;
                                }
                            }
                        }
                    }
                }

                if (game.HandInPlay.Cards.Count == 4)
                {
                    game.NextPlayer = game.HighCardPlayer;
                    game = Collect(game);
                    _context.SaveChanges();
                    if (game.NorthHand.Cards.Count == 0)
                    {
                        Response.Redirect("/../../MatchesB/CompleteGame/" + id);
                        errorView = new ErrorViewModel();
                        return View("Error", errorView);
                    }
                }
                else
                {
                    if (game.NextPlayer.Id == game.North.Id)
                    {
                        game.NextPlayer = game.East;
                    }
                    else
                    {
                        if (game.NextPlayer.Id == game.East.Id)
                        {
                            game.NextPlayer = game.South;
                        }
                        else
                        {
                            if (game.NextPlayer.Id == game.South.Id)
                            {
                                game.NextPlayer = game.West;
                            }
                            else
                            {
                                if (game.NextPlayer.Id == game.West.Id)
                                {
                                    game.NextPlayer = game.North;
                                }
                            }
                        }
                    }
                }

                _context.SaveChanges();
                Response.Redirect("/../../GamesB/Play/" + id);
            }

            ViewBag.Player = UUID;
            ViewBag.Dealer = game.Dealer.Id;

            return View("GameProgress", match);
        }

        public GameB Collect(GameB game, string move = "")
        {
            var match = _context.MatchB
                .FirstOrDefault(m => m.Id == game.MatchBId);
            if (true)
            //                if (move == "collect")
            {
                if (match.LastHand == null)
                {
                    match.LastHand = new HandB();
                    match.LastHand.Id = match.Id;
                    match.LastHand.Cards = new List<CardB>();
                }
                else
                {
                    match.LastHand.Cards.Clear();
                }
                foreach (var card in game.HandInPlay.Cards)
                {
                    var _card = new CardB();
                    _card.Id = Guid.NewGuid().ToString();
                    _card.Colour = card.Colour;
                    _card.Value = card.Value;
                    _card.Sequence = card.Sequence;
                    _card.Seniority = card.Seniority;
                    match.LastHand.Cards.Add(_card);
                    if ((game.NextPlayer.Id == game.North.Id) || (game.NextPlayer.Id == game.South.Id))
                    {
                        game.NorthSouthPoints = game.NorthSouthPoints + CardValue(game.Type, _card);
                    }
                    else
                    {
                        game.EastWestPoints = game.EastWestPoints + CardValue(game.Type, _card);
                    }
                }
                if ((game.NextPlayer.Id == game.North.Id) || (game.NextPlayer.Id == game.South.Id))
                {
                    if (game.NorthHand.Cards.Count == 0)
                    {
                        game.NorthSouthPoints = game.NorthSouthPoints + 10;
                    }
                }
                else
                {
                    if (game.NorthHand.Cards.Count == 0)
                    {
                        game.EastWestPoints = game.EastWestPoints + 10;
                    }
                }
                game.Status = "Playing";
                if ((game.HighCardPlayer.Id == game.North.Id) || (game.HighCardPlayer.Id == game.South.Id))
                {
                    if (game.NorthSouthHandResult == null)
                    {
                        game.NorthSouthHandResult = new HandB();
                        game.NorthSouthHandResult.Id = Guid.NewGuid().ToString();
                        game.NorthSouthHandResult.Cards = new List<CardB>();
                    }
                    foreach (CardB card in game.HandInPlay.Cards)
                    {
                        game.NorthSouthHandResult.Cards.Add(card);
                    }
                    game.HandInPlay.Cards.Clear();
                }
                else
                {
                    if (game.EastWestHandResult == null)
                    {
                        game.EastWestHandResult = new HandB();
                        game.EastWestHandResult.Id = Guid.NewGuid().ToString();
                        game.EastWestHandResult.Cards = new List<CardB>();
                    }
                    foreach (CardB card in game.HandInPlay.Cards)
                    {
                        game.EastWestHandResult.Cards.Add(card);
                    }
                    game.HandInPlay.Cards.Clear();
                }
                if (game.NorthHand.Cards.Count == 0)
                { game.Status = "Dealing"; }
            }
            return game;
        }

        public int CardValue(string Trump, CardB card) 
        {
            if ((card.Value == "7") || (card.Value == "7"))
            { return 0; }
            if (card.Value == "Queen")
            { return 3; }
            if (card.Value == "King")
            { return 4; }
            if (card.Value == "10")
            { return 10; }
            if (card.Value == "Ace")
            {
                if (Trump == "No Trump")
                { return 19; }
                else
                { return 11; }
            }
            if (card.Value == "9")
            {
                if (Trump == card.Colour)
                { return 14; }
                else
                { return 0; }
            }
            if (card.Value == "Jack")
            {
                if (Trump == card.Colour)
                { return 20; }
                else
                { return 2; }
            }
            return 0;
        }

        public async Task<IActionResult> OutcomeOffer(string id, string Offer, int offerHands)
        {
            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
                .FirstOrDefault(m => m.Id == id);

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .ToList()[0];

            if (Offer == "offer")
            {
                game.OutcomeOffer = offerHands;
                game.Status = "Offer";
                _context.SaveChanges();
                Response.Redirect("/../../GamesB/Play/" + id);
            }

            if (Offer == "accept")
            {
                if (game.Type == "All-Pass")
                {

                }
                else
                {
                    if (game.Type == "Misere")
                    {
                        Response.Redirect("/../../MatchesB/CloseIncompleteMisere/?id=" + id + "&activeHands=" + game.OutcomeOffer.ToString());
                    }
                    else
                    {
                        Response.Redirect("/../../MatchesB/CloseIncompletePointGame/?id=" + id + "&activeHands=" + game.OutcomeOffer.ToString());
                    }
                }
            }

            if (Offer == "reject")
            {
                game.OutcomeOffer = -2;
                game.Status = "Playing";
                _context.SaveChanges();
                Response.Redirect("/../../GamesB/Play/" + id);
            }

            errorView = new ErrorViewModel();
            return View("Error", errorView);
        }

        public bool CardValueGreaterThan(string a, string b)
        {
            switch (a)
            {
                case "7":
                    return false;
                case "8":
                    if (b == "7")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "9":
                    if ((b == "7") || (b == "8"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Jack":
                    if ((b == "7") || (b == "8") || (b == "9"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Queen":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "Jack"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "King":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "Jack") || (b == "Queen"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "10":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "Jack") || (b == "Queen") || (b == "King"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        public bool TrumpCardValueGreaterThan(string a, string b)
        {
            switch (a)
            {
                case "7":
                    return false;
                case "8":
                    if (b == "7")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Queen":
                    if ((b == "7") || (b == "8"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "King":
                    if ((b == "7") || (b == "8") || (b == "Queen"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "10":
                    if ((b == "7") || (b == "8") || (b == "Queen") || (b == "King"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Ace":
                    if ((b == "7") || (b == "8") || (b == "Queen") || (b == "King") || (b == "10"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "9":
                    if ((b == "7") || (b == "8") || (b == "Queen") || (b == "King") || (b == "10") || (b == "Ace"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Jack":
                    if ((b == "7") || (b == "8") || (b == "Queen") || (b == "King") || (b == "10") || (b == "Ace") || (b == "9"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        // GET: Games
        public async Task<IActionResult> Index()
        {
            return View(await _context.Game.ToListAsync());
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Games/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Value,Player1Bidding,Player2Bidding,Player3Bidding,Player4Bidding,MiserePossible,MisereOffered,MisereSharable,MisereShared,Status,MatchId")] Game game)
        {
            if (ModelState.IsValid)
            {
                _context.Add(game);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Games/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Value,Player1Bidding,Player2Bidding,Player3Bidding,Player4Bidding,MiserePossible,MisereOffered,MisereSharable,MisereShared,Status,MatchId")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Game.FindAsync(id);
            _context.Game.Remove(game);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.Id == id);
        }
    }
}
