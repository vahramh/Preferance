using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Preferance.Data;
using Preferance.Models;

namespace Preferance.Controllers
{
    public class MatchesBController : Controller
    {
        ErrorViewModel errorView = null;

        private readonly ApplicationDbContext _context;

        public MatchesBController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MatchesB
        public async Task<IActionResult> Index()
        {
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
            else
            {
                return View(await _context.MatchB.OrderByDescending(m => m.MatchDate).Include(j => j.North).Include(j => j.South).Include(j => j.East).Include(j => j.West).ToListAsync());
            }
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.MatchB
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: Matches/Create
        public IActionResult Create()
        {
            MatchB CurrentMatch = new MatchB
            {
                Id = Guid.NewGuid().ToString()
            };
            CurrentMatch.North = new Player();
            CurrentMatch.South = new Player();
            CurrentMatch.East = new Player();
            CurrentMatch.West = new Player();
            CurrentMatch.AllPlayers = _context.Player.OrderBy(p => p.Name).ToListAsync().Result;
            return View(CurrentMatch);
        }

        // POST: Matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,North,South,East,West,MatchDate")] Match match)
        {
            if (ModelState.IsValid)
            {
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(match);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MatchStart([Bind("Id,North,South,East,West")] MatchB match)
        {
            match.North = _context.Player.FirstOrDefault(m => m.Id == match.North.Id);
            match.South = _context.Player.FirstOrDefault(m => m.Id == match.South.Id);
            match.East = _context.Player.FirstOrDefault(m => m.Id == match.East.Id);
            match.West = _context.Player.FirstOrDefault(m => m.Id == match.West.Id);
            if (ModelState.IsValid)
            {
                _context.Add(match);
                _context.SaveChanges();
                /*                return RedirectToAction(nameof(Index)); */
            }
            match.Games = new List<GameB>();

            return View("FirstDealer", match);
        }

        public async Task<IActionResult> SetFirstDealerTempData(string MatchId, string FirstDealer)
        {
            MatchB match = new MatchB();
            match = _context.MatchB.Include(o => o.Games).Include(p => p.North).Include(p => p.South).Include(p => p.East).Include(p => p.West).FirstOrDefault(m => m.Id == MatchId);
            match.Games = new List<GameB>();
            GameB game = new GameB();
            game.Id = 0;
            game.Dealer = _context.Player.FirstOrDefault(p => p.Id == FirstDealer);
            game.North = match.North;
            game.South = match.South;
            game.East = match.East;
            game.West = match.West;
            if (game.Dealer.Id == game.North.Id)
            {
                game.NextPlayer = game.East;
            }
            else
            {
                if (game.Dealer.Id == game.East.Id)
                {
                    game.NextPlayer = game.South;
                }
                else
                {
                    if (game.Dealer.Id == game.South.Id)
                    {
                        game.NextPlayer = game.West;
                    }
                    else
                    {
                        game.NextPlayer = game.North;
                    }
                }
            }
            game.Type = "";
            game.Value = 0;
            game.Status = "Dealing";
            match = _context.MatchB.FirstOrDefault(m => m.Id == MatchId);
            match.Games.Add(game);
            match.LastHand = new HandB();
            match.LastHand.Id = match.Id;
            match.LastHand.Cards = new List<CardB>();
            _context.SaveChanges();

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            string UUID = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewBag.Dealer = FirstDealer;
            ViewBag.Player = UUID;
            Response.Redirect("/../../MatchesB/Play/" + MatchId);
            return View("MatchPlay", match);
        }

        // GET: Matches/Play/5
        public async Task<IActionResult> Play(string id, bool deal = false, string bid = "", string type = "", int value = 0, bool challenge = false, bool contra = false, bool kaput = false)
        {
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
                .ThenInclude(o => o.Cards)
              .FirstOrDefault(m => m.Id == id);

            GameB game = _context.GameB.Where(m => m.MatchBId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.NorthHand).Include(o => o.SouthHand).Include(o => o.EastHand).Include(o => o.WestHand)
                .ToList()[0];

            if ((game.Status == "Playing") || (game.Status == "Collecting") || (game.Status == "Offer"))
            {
                Response.Redirect("/../../GamesB/Play/" + id);
                errorView = new ErrorViewModel();
                return View("Error", errorView);
            }

            if (!(game.NorthHand == null))
            {
                game.NorthHand.Cards = _context.CardB.Where(c => c.HandBId == game.NorthHand.Id).OrderBy(c => c.Seniority).ToList();
                game.SouthHand.Cards = _context.CardB.Where(c => c.HandBId == game.SouthHand.Id).OrderBy(c => c.Seniority).ToList();
                game.EastHand.Cards = _context.CardB.Where(c => c.HandBId == game.EastHand.Id).OrderBy(c => c.Seniority).ToList();
                game.WestHand.Cards = _context.CardB.Where(c => c.HandBId == game.WestHand.Id).OrderBy(c => c.Seniority).ToList();
            }

            /* ********************************** Dealing ************************************* */

            if (deal)
            {
                if (game.Status == "Dealing")
                {
                    game = DealB(game);
                    game.Status = "Bidding";
                    game.Type = "All-Pass";
                    if (game.Dealer.Id == game.North.Id)
                    {
                        game.NextPlayer = game.East;
                    }
                    if (game.Dealer.Id == game.East.Id)
                    {
                        game.NextPlayer = game.South;
                    }
                    if (game.Dealer.Id == game.South.Id)
                    {
                        game.NextPlayer = game.West;
                    }
                    if (game.Dealer.Id == game.West.Id)
                    {
                        game.NextPlayer = game.North;
                    }
                    deal = false;
                    _context.SaveChanges();
                    Response.Redirect("/../../MatchesB/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            /* **************************** Bidding Process *********************************** */

            if ((game.Status == "Bidding") & ((UUID == game.NextPlayer.Id) || ((UUID == game.Dealer.Id) & ((bid == "reject") || (bid == "accept")))))
            {
                _context.SaveChanges();
            }

            if (bid == "order")
            {
                if (!(game.Status == "Bidding"))
                {
                    ViewBag.Message = "Order has already been placed, go to game.";
                }
                else
                {
                    if (UUID == game.North.Id || UUID == game.South.Id)
                    { game.ActiveTeamNS = true; }
                    else
                    { game.ActiveTeamNS = false; }
                    game.Challenge = challenge;
                    game.Contra = contra;
                    game.Kaput = kaput;
                    game.EastWestPoints = 0;
                    game.NorthSouthPoints = 0;
                    game.Status = "Playing";
                    game.Type = type;
                    game.Value = value;
                    _context.SaveChanges();
                    Response.Redirect("/../../GamesB/Play/" + id);
                }
            }
            ViewBag.Dealer = match.Games.OrderByDescending(i => i.Id).FirstOrDefault().Dealer.Id;
            ViewBag.Player = UUID;
            return View("MatchPlay", match);
        }

        public async Task<IActionResult> CompleteGame(string id)
        {
            var match = _context.MatchB.Include(o => o.Games).Include(o => o.North).Include(o => o.South).Include(o => o.East).Include(o => o.West).Include(m => m.Games).Include(m => m.LastHand)
              .FirstOrDefault(m => m.Id == id);
            match.LastHand.Cards = _context.CardB.Where(c => c.HandBId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

            GameB game = _context.GameB.Where(m => m.MatchBId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.NorthHand).Include(o => o.SouthHand).Include(o => o.EastHand).Include(o => o.WestHand)
                .Include(o => o.HandInPlay)
                .Include(o => o.NorthSouthHandResult)
                .Include(o => o.EastWestHandResult)
                .ToList()[0];

            game.Status = "Completed";

            GameB newGame = new GameB();
            match.Games.Add(newGame);
            match.LastHand.Cards.Clear();
            _context.SaveChanges();

            if (game.Dealer.Id == game.North.Id)
            {
                newGame.Dealer = game.East;
            }
            else
            {
                if (game.Dealer.Id == game.East.Id)
                {
                    newGame.Dealer = game.South;
                }
                else
                {
                    if (game.Dealer.Id == game.South.Id)
                    {
                        newGame.Dealer = game.West;
                    }
                    else
                    {
                        newGame.Dealer = game.North;
                    }
                }
            }
            newGame.North = match.North;
            newGame.South = match.South;
            newGame.East = match.East;
            newGame.West = match.West;
            newGame.Type = "";
            newGame.Value = 0;
            newGame.Status = "Dealing";
            match = _context.MatchB.FirstOrDefault(m => m.Id == id);
            _context.SaveChanges();
//            Response.Redirect("/../../Matches/Play/" + id);
            Response.Redirect("/../../MatchesB/Play?id=" + id + "&deal=true");
            errorView = new ErrorViewModel();
            return View("Error", errorView);

        }

        public GameB DealB(GameB game)
        {
            IList<CardB> deck = new List<CardB>();
            CardB x;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    x = new CardB();
                    x.Id = Guid.NewGuid().ToString();
                    switch (i)
                    {
                        case 0:
                            x.Colour = "Spades";
                            x.Seniority = 100;
                            break;
                        case 1:
                            x.Colour = "Clubs";
                            x.Seniority = 300;
                            break;
                        case 2:
                            x.Colour = "Diamonds";
                            x.Seniority = 200;
                            break;
                        case 3:
                            x.Colour = "Hearts";
                            x.Seniority = 400;
                            break;
                    }
                    switch (j)
                    {
                        case 0:
                            x.Value = "Ace";
                            x.Seniority = x.Seniority + 14;
                            break;
                        case 1:
                            x.Value = "Jack";
                            x.Seniority = x.Seniority + 11;
                            break;
                        case 2:
                            x.Value = "Queen";
                            x.Seniority = x.Seniority + 12;
                            break;
                        case 3:
                            x.Value = "King";
                            x.Seniority = x.Seniority + 13;
                            break;
                        default:
                            x.Value = (j + 3).ToString();
                            x.Seniority = x.Seniority + j;
                            break;
                    }
                    x.SeniorityTrump = x.Seniority;
                    if (x.Value == "9")
                    {
                        x.SeniorityTrump = x.SeniorityTrump + 10;
                    }
                    if (x.Value == "Jack")
                    {
                        x.SeniorityTrump = x.SeniorityTrump + 20;
                    }
                    deck.Add(x);
                }
            }

            deck = deck.OrderBy(a => Guid.NewGuid()).ToList();

            GameB _game = game;
            int n = 0;
            int q = 8;

            _game.NorthHand = new HandB();
            _game.NorthHand.Id = Guid.NewGuid().ToString();
            _game.NorthHand.Cards = new List<CardB>();
            for (int j = 0; j < q; j++)
            {
                _game.NorthHand.Cards.Add(deck[n]);
                n++;
            }

            _game.SouthHand = new HandB();
            _game.SouthHand.Id = Guid.NewGuid().ToString();
            _game.SouthHand.Cards = new List<CardB>();
            for (int j = 0; j < q; j++)
            {
                _game.SouthHand.Cards.Add(deck[n]);
                n++;
            }

            _game.NorthSouthHandResult = new HandB();
            _game.NorthSouthHandResult.Id = Guid.NewGuid().ToString();

            _game.EastHand = new HandB();
            _game.EastHand.Id = Guid.NewGuid().ToString();
            _game.EastHand.Cards = new List<CardB>();
            for (int j = 0; j < q; j++)
            {
                _game.EastHand.Cards.Add(deck[n]);
                n++;
            }

            _game.WestHand = new HandB();
            _game.WestHand.Id = Guid.NewGuid().ToString();
            _game.WestHand.Cards = new List<CardB>();
            for (int j = 0; j < q; j++)
            {
                _game.WestHand.Cards.Add(deck[n]);
                n++;
            }

            _game.EastWestHandResult = new HandB();
            _game.EastWestHandResult.Id = Guid.NewGuid().ToString();

            return _game;
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = _context.Match.Include(o => o.Games).Include(p => p.Player1).Include(p => p.Player2).Include(p => p.Player3).Include(p => p.Player4).FirstOrDefault(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id, Player1CurrentPool, Player1CurrentDump, Player12CurrentWhist, Player13CurrentWhist, Player14CurrentWhist," +
            "Player2CurrentPool, Player2CurrentDump, Player21CurrentWhist, Player23CurrentWhist, Player24CurrentWhist," +
            "Player3CurrentPool, Player3CurrentDump, Player31CurrentWhist, Player32CurrentWhist, Player34CurrentWhist," +
            "Player4CurrentPool, Player4CurrentDump, Player41CurrentWhist, Player42CurrentWhist, Player43CurrentWhist")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            var _match = _context.Match.AsNoTracking().FirstOrDefault(m => m.Id == match.Id);
            if (ModelState.IsValid)
            {
                match.Player1Pool = _match.Player1Pool + match.Player1CurrentPool.ToString() + ". ";
                match.Player2Pool = _match.Player2Pool + match.Player2CurrentPool.ToString() + ". ";
                match.Player3Pool = _match.Player3Pool + match.Player3CurrentPool.ToString() + ". ";
                match.Player4Pool = _match.Player4Pool + match.Player4CurrentPool.ToString() + ". ";
                match.Player12Whist = _match.Player12Whist + match.Player12CurrentWhist.ToString() + ". ";
                match.Player13Whist = _match.Player13Whist + match.Player13CurrentWhist.ToString() + ". ";
                match.Player14Whist = _match.Player14Whist + match.Player14CurrentWhist.ToString() + ". ";
                match.Player21Whist = _match.Player21Whist + match.Player21CurrentWhist.ToString() + ". ";
                match.Player23Whist = _match.Player23Whist + match.Player23CurrentWhist.ToString() + ". ";
                match.Player24Whist = _match.Player24Whist + match.Player24CurrentWhist.ToString() + ". ";
                match.Player31Whist = _match.Player31Whist + match.Player31CurrentWhist.ToString() + ". ";
                match.Player32Whist = _match.Player32Whist + match.Player32CurrentWhist.ToString() + ". ";
                match.Player34Whist = _match.Player34Whist + match.Player34CurrentWhist.ToString() + ". ";
                match.Player41Whist = _match.Player41Whist + match.Player41CurrentWhist.ToString() + ". ";
                match.Player42Whist = _match.Player42Whist + match.Player42CurrentWhist.ToString() + ". ";
                match.Player43Whist = _match.Player43Whist + match.Player43CurrentWhist.ToString() + ". ";

                float p1TempDump = 0;
                float p2TempDump = 0;
                float p3TempDump = 0;
                float p4TempDump = 0;

                p1TempDump = match.Player1CurrentDump - match.Player1CurrentPool;
                p2TempDump = match.Player2CurrentDump - match.Player2CurrentPool;
                p3TempDump = match.Player3CurrentDump - match.Player3CurrentPool;
                p4TempDump = match.Player4CurrentDump - match.Player4CurrentPool;

                match.Player1CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p1TempDump) * 10
                    + match.Player12CurrentWhist + match.Player13CurrentWhist + match.Player14CurrentWhist
                    - match.Player21CurrentWhist - match.Player31CurrentWhist - match.Player41CurrentWhist;

                match.Player2CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p2TempDump) * 10
                    + match.Player21CurrentWhist + match.Player23CurrentWhist + match.Player24CurrentWhist
                    - match.Player12CurrentWhist - match.Player32CurrentWhist - match.Player42CurrentWhist;

                match.Player3CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p3TempDump) * 10
                    + match.Player31CurrentWhist + match.Player32CurrentWhist + match.Player34CurrentWhist
                    - match.Player13CurrentWhist - match.Player23CurrentWhist - match.Player43CurrentWhist;

                match.Player4CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p4TempDump) * 10
                    + match.Player41CurrentWhist + match.Player42CurrentWhist + match.Player43CurrentWhist
                    - match.Player14CurrentWhist - match.Player24CurrentWhist - match.Player34CurrentWhist;

                
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
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
            return View(match);
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Match
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var match = await _context.Match.FindAsync(id);
            _context.Match.Remove(match);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(string id)
        {
            return _context.Match.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task ShootEvent(string id)
        {
            var response = Response;
            response.Headers.Add("Content-Type", "text/event-stream");

            for (var i = 0; true; ++i)
            {
                GameB game = _context.GameB.AsNoTracking().Where(m => m.MatchBId == id).OrderByDescending(g => g.Id).Take(1)
                    .ToList()[0];

                await response
                    .WriteAsync($"data:" + game.Status + "\r\r");

                await Task.Delay(100);
            }
            response.Body.Flush();
        }
    }
}
