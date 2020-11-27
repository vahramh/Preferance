using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Preferance.Data;
using Preferance.Migrations;
using Preferance.Models;

namespace Preferance.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        ErrorViewModel errorView = null;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Play(string id, string move = "", string cardSuit = "", string cardValue = "")
        {
            Card playedCard = new Card();
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


            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games).Include(o => o.LastHand)
                .ThenInclude(h => h.Cards)
              .FirstOrDefault(m => m.Id == id);
            await _context.Entry(match).ReloadAsync();

            match.LastHand.Cards = _context.Card.Where(c => c.HandId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.Player1Hand).Include(o => o.Player2Hand).Include(o => o.Player3Hand).Include(o => o.Player4Hand)
                .Include(o => o.HandInPlay)
                .Include(o => o.Player1HandResult).ThenInclude(p => p.hands).ThenInclude(q => q.Cards)
                .Include(o => o.Player2HandResult).ThenInclude(p => p.hands).ThenInclude(q => q.Cards)
                .Include(o => o.Player3HandResult).ThenInclude(p => p.hands).ThenInclude(q => q.Cards)
                .Include(o => o.Player4HandResult).ThenInclude(p => p.hands).ThenInclude(q => q.Cards)
                .Include(o => o.Talon).Include(o => o.HighCard)
                .ToList()[0];
            if ((game.Status == "Dealing") || (game.Status == "Bidding"))
            {
                Response.Redirect("/../../Matches/Play/" + id);
                errorView = new ErrorViewModel();
                return View("Error", errorView);
            }
            await _context.Entry(game).ReloadAsync();

            game.Player1Hand.Cards = _context.Card.Where(c => c.HandId == game.Player1Hand.Id).OrderBy(c => c.Seniority).ToList();
            game.Player2Hand.Cards = _context.Card.Where(c => c.HandId == game.Player2Hand.Id).OrderBy(c => c.Seniority).ToList();
            game.Player3Hand.Cards = _context.Card.Where(c => c.HandId == game.Player3Hand.Id).OrderBy(c => c.Seniority).ToList();
            game.Player4Hand.Cards = _context.Card.Where(c => c.HandId == game.Player4Hand.Id).OrderBy(c => c.Seniority).ToList();
            game.Talon.Cards = _context.Card.Where(c => c.HandId == game.Talon.Id).ToList();

            if (game.HandInPlay == null)
            {
                game.HandInPlay = new Hand();
                game.HandInPlay.Id = Guid.NewGuid().ToString();
                game.HandInPlay.Cards = new List<Card>();
            }
            else
            {
                game.HandInPlay.Cards = _context.Card.Where(c => c.HandId == game.HandInPlay.Id).OrderBy(c => c.Sequence).ToList();
                await _context.Entry(game).ReloadAsync();
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
                if ((game.Type == "All-Pass") & (game.Talon.Cards.Count > 0))
                {
                    game.NextPlayer = game.Dealer;
                }

                _context.SaveChanges();

                
                if (game.Player1Hand.Cards.Count + game.Player2Hand.Cards.Count == 0)
                {
                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            if (move == "Move")
            {
                if (game.NextPlayer.Id == game.Dealer.Id)
                {
                    // Playing All-Pass, dealer's move

                    playedCard = game.Talon.Cards[0];
                    if (game.HandInPlay.Cards.Count == 0)
                    {
                        playedCard.Sequence = 1;
                    }
                    else
                    {
                        playedCard.Sequence = game.HandInPlay.Cards.Max(m => m.Sequence) + 1;
                    }
                    game.HandInPlay.Cards.Add(playedCard);
                    game.Talon.Cards.Remove(playedCard);
                    game.HighCardPlayer = game.NextPlayer;
                    game.HighCard = playedCard;

                }

                if (game.NextPlayer.Id == game.Player1.Id)
                {
                    foreach (Card card in game.Player1Hand.Cards)
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
                    game.Player1Hand.Cards.Remove(playedCard);

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
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player1;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.Player1;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player1;
                                }
                            }
                        }
                    }

                }

                if (game.NextPlayer.Id == game.Player2.Id)
                {
                    foreach (Card card in game.Player2Hand.Cards)
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
                    game.Player2Hand.Cards.Remove(playedCard);

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
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player2;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.Player2;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player2;
                                }
                            }
                        }
                    }
                }

                if (game.NextPlayer.Id == game.Player3.Id)
                {
                    foreach (Card card in game.Player3Hand.Cards)
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
                    game.Player3Hand.Cards.Remove(playedCard);

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
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player3;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.Player3;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player3;
                                }
                            }
                        }
                    }
                }

                if (game.NextPlayer.Id == game.Player4.Id)
                {
                    foreach (Card card in game.Player4Hand.Cards)
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
                    game.Player4Hand.Cards.Remove(playedCard);

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
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player4;
                                }
                            }
                            else
                            {
                                game.HighCard = playedCard;
                                game.HighCardPlayer = game.Player4;
                            }
                        }
                        else
                        {
                            if (game.HighCard.Colour == cardSuit)
                            {
                                if (CardValueGreaterThan(cardValue, game.HighCard.Value))
                                {
                                    game.HighCard = playedCard;
                                    game.HighCardPlayer = game.Player4;
                                }
                            }
                        }
                    }
                }

                if (game.HandInPlay.Cards.Count == 4)
                {
                    game.NextPlayer = game.HighCardPlayer;
                    game = Collect(game);
                    if ((game.Type == "All-Pass") & (game.Talon.Cards.Count > 0))
                    {
                        game.NextPlayer = game.Dealer;
                    }
                }
                else
                {
                    if (game.HandInPlay.Cards.Count == 3)
                    {
                        if ((game.Type == "All-Pass") & (game.DealerPlaying))
                        {
                            if (game.NextPlayer.Id == game.Player1.Id)
                            {
                                game.NextPlayer = game.Player2;
                            }
                            else
                            {
                                if (game.NextPlayer.Id == game.Player2.Id)
                                {
                                    game.NextPlayer = game.Player3;
                                }
                                else
                                {
                                    if (game.NextPlayer.Id == game.Player3.Id)
                                    {
                                        game.NextPlayer = game.Player4;
                                    }
                                    else
                                    {
                                        if (game.NextPlayer.Id == game.Player4.Id)
                                        {
                                            game.NextPlayer = game.Player1;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            game = Collect(game);
                            if (game.Player1Hand.Cards.Count + game.Player2Hand.Cards.Count == 0)
                            {
                                _context.SaveChanges();
                                Response.Redirect("/../../Matches/CompleteGame/" + game.MatchId);
                                return View("GameProgress", match);
                            }
                            game.NextPlayer = game.HighCardPlayer;
                        }
                    }
                    else
                    {
                        if (game.HandInPlay.Cards.Count < 3)
                        {
                            if (game.NextPlayer.Id == game.Player1.Id)
                            {
                                game.NextPlayer = game.Player2;
                                if (game.NextPlayer.Id == game.Dealer.Id)
                                {
                                    game.NextPlayer = game.Player3;
                                }
                            }
                            else
                            {
                                if (game.NextPlayer.Id == game.Player2.Id)
                                {
                                    game.NextPlayer = game.Player3;
                                    if (game.NextPlayer.Id == game.Dealer.Id)
                                    {
                                        game.NextPlayer = game.Player4;
                                    }
                                }
                                else
                                {
                                    if (game.NextPlayer.Id == game.Player3.Id)
                                    {
                                        game.NextPlayer = game.Player4;
                                        if (game.NextPlayer.Id == game.Dealer.Id)
                                        {
                                            game.NextPlayer = game.Player1;
                                        }
                                    }
                                    else
                                    {
                                        if (game.NextPlayer.Id == game.Player4.Id)
                                        {
                                            game.NextPlayer = game.Player1;
                                            if (game.NextPlayer.Id == game.Dealer.Id)
                                            {
                                                game.NextPlayer = game.Player2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _context.SaveChanges();
                Response.Redirect("/../../Games/Play/" + id);
            }

            ViewBag.Player = UUID;
            ViewBag.Dealer = game.Dealer.Id;

            return View("GameProgress", match);
        }

        public Game Collect(Game game, string move = "")
        {
            var match = _context.Match
                .FirstOrDefault(m => m.Id == game.MatchId);
            if (true)
//                if (move == "collect")
                {
                    if (match.LastHand == null)
                {
                    match.LastHand = new Hand();
                    match.LastHand.Id = match.Id;
                    match.LastHand.Cards = new List<Card>();
                }
                else
                {
                    match.LastHand.Cards.Clear();
                }
                foreach (var card in game.HandInPlay.Cards)
                {
                    var _card = new Card();
                    _card.Id = Guid.NewGuid().ToString();
                    _card.Colour = card.Colour;
                    _card.Value = card.Value;
                    _card.Sequence = card.Sequence;
                    _card.Seniority = card.Seniority;
                    match.LastHand.Cards.Add(_card);
                }
                game.Status = "Playing";
                if (game.HighCardPlayer.Id == game.Player1.Id)
                {
                    if (game.Player1HandResult == null)
                    {
                        game.Player1HandResult = new SetOfHands();
                        game.Player1HandResult.Id = Guid.NewGuid().ToString();
                        game.Player1HandResult.hands = new List<Hand>();
                    }
                    Hand collectedHand = new Hand();
                    collectedHand.Id = Guid.NewGuid().ToString();
                    collectedHand.Cards = new List<Card>();
                    foreach (Card card in game.HandInPlay.Cards)
                    {
                        collectedHand.Cards.Add(card);
                    }
                    game.Player1HandResult.hands.Add(collectedHand);
                    game.HandInPlay.Cards.Clear();
                }
                else
                {
                    if (game.HighCardPlayer.Id == game.Player2.Id)
                    {
                        if (game.Player2HandResult == null)
                        {
                            game.Player2HandResult = new SetOfHands();
                            game.Player2HandResult.Id = Guid.NewGuid().ToString();
                            game.Player2HandResult.hands = new List<Hand>();
                        }
                        Hand collectedHand = new Hand();
                        collectedHand.Id = Guid.NewGuid().ToString();
                        collectedHand.Cards = new List<Card>();
                        foreach (Card card in game.HandInPlay.Cards)
                        {
                            collectedHand.Cards.Add(card);
                        }
                        game.Player2HandResult.hands.Add(collectedHand);
                        game.HandInPlay.Cards.Clear();
                    }
                    else
                    {
                        if (game.HighCardPlayer.Id == game.Player3.Id)
                        {
                            if (game.Player3HandResult == null)
                            {
                                game.Player3HandResult = new SetOfHands();
                                game.Player3HandResult.Id = Guid.NewGuid().ToString();
                                game.Player3HandResult.hands = new List<Hand>();
                            }
                            Hand collectedHand = new Hand();
                            collectedHand.Id = Guid.NewGuid().ToString();
                            collectedHand.Cards = new List<Card>();
                            foreach (Card card in game.HandInPlay.Cards)
                            {
                                collectedHand.Cards.Add(card);
                            }
                            game.Player3HandResult.hands.Add(collectedHand);
                            game.HandInPlay.Cards.Clear();
                        }
                        else
                        {
                            if (game.HighCardPlayer.Id == game.Player4.Id)
                            {
                                if (game.Player4HandResult == null)
                                {
                                    game.Player4HandResult = new SetOfHands();
                                    game.Player4HandResult.Id = Guid.NewGuid().ToString();
                                    game.Player4HandResult.hands = new List<Hand>();
                                }
                                Hand collectedHand = new Hand();
                                collectedHand.Id = Guid.NewGuid().ToString();
                                collectedHand.Cards = new List<Card>();
                                foreach (Card card in game.HandInPlay.Cards)
                                {
                                    collectedHand.Cards.Add(card);
                                }
                                game.Player4HandResult.hands.Add(collectedHand);
                                game.HandInPlay.Cards.Clear();
                            }
                        }

                    }
                }
                if (game.Talon.Cards.Count > 0)
                {
                    game.NextPlayer = game.Dealer;
                }
                else
                {
                    if ((game.DealerPlaying) & (game.Type == "All-Pass"))
                    {
                        game.DealerPlaying = false;
                        if (game.Dealer.Id == game.Player1.Id)
                        { game.NextPlayer = game.Player2; }
                        if (game.Dealer.Id == game.Player2.Id)
                        { game.NextPlayer = game.Player3; }
                        if (game.Dealer.Id == game.Player3.Id)
                        { game.NextPlayer = game.Player4; }
                        if (game.Dealer.Id == game.Player4.Id)
                        { game.NextPlayer = game.Player1; }
                    }
                }
            }
            else
            {
                game.Status = "Collecting";

            }
            return game;
        }

        public async Task<IActionResult> OutcomeOffer(string id, string Offer, int offerHands)
        {
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
                .FirstOrDefault(m => m.Id == id);

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .ToList()[0];

            if (Offer == "offer")
            {
                game.OutcomeOffer = offerHands;
                game.Status = "Offer";
                _context.SaveChanges();
                Response.Redirect("/../../Games/Play/" + id);
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
                        Response.Redirect("/../../Matches/CloseIncompleteMisere/?id=" + id + "&activeHands=" + game.OutcomeOffer.ToString());
                    }
                    else
                    {
                        Response.Redirect("/../../Matches/CloseIncompletePointGame/?id=" + id + "&activeHands=" + game.OutcomeOffer.ToString());
                    }
                }
            }

            if (Offer == "reject")
            {
                game.OutcomeOffer = -1;
                game.Status = "Playing";
                _context.SaveChanges();
                Response.Redirect("/../../Games/Play/" + id);
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
                case "10":
                    if ((b == "7") || (b == "8") || (b == "9"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Jack":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "10"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "Queen":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "10") || (b == "Jack"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "King":
                    if ((b == "7") || (b == "8") || (b == "9") || (b == "10") || (b == "Jack") || (b == "Queen"))
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

        [HttpGet]
        public async Task ShootEvent(string id)
        {
            HttpResponse response = Response;
            response.Headers.Add("Content-Type", "text/event-stream");

            var tWait = new Random();
            await Task.Delay(tWait.Next(3000));

            for (var i = 0; true; ++i)
            {
                Game game = _context.Game.AsNoTracking().Where(m => m.MatchId == id).OrderByDescending(g => g.Id).Take(1)
                    .Include(g => g.HandInPlay).ThenInclude(h => h.Cards)
                    .ToList()[0];

                if (game.Status == "Playing")
                {
                    if (game.HandInPlay == null)
                    {
                        await response
                            .WriteAsync($"data:0\n\n");
                    }
                    else
                    {
                        await response
                            .WriteAsync($"data:" + game.HandInPlay.Cards.Count().ToString() + "\n\n");
                    }
                }
                else
                {
                    await response
                        .WriteAsync($"data:" + game.Status + "\n\n");
                }

                await Task.Delay(3000);
            }
            response.Body.Flush();
        }
    }
}
