using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
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

        public void CalculatePointGameResult(int gameValue, int playerHands, int w1Order, int w2Order, int w1Hands, int w2Hands, ref int playerPool, ref int playerDump, ref int w1, ref int w1Dump, ref int w2Dump, ref int w2, ref int wd)
        {
            int poolPoints = 0;
            int whistPoints = 0;
            int ownWhists = 0;
            int minWhists = 0;
            bool responsiblePlayer = true;

            switch (gameValue)
            {
                case 6:
                    {
                        poolPoints = 2;
                        whistPoints = 2;
                        minWhists = 4;
                        ownWhists = 2;
                        break;
                    }
                case 7:
                    {
                        poolPoints = 4;
                        whistPoints = 4;
                        minWhists = 2;
                        ownWhists = 1;
                        break;
                    }
                case 8:
                    {
                        poolPoints = 6;
                        whistPoints = 6;
                        minWhists = 1;
                        ownWhists = 1;
                        break;
                    }
                case 9:
                    {
                        poolPoints = 8;
                        whistPoints = 8;
                        minWhists = 1;
                        ownWhists = 1;
                        break;
                    }
                case 10:
                    {
                        poolPoints = 10;
                        whistPoints = 10;
                        minWhists = 1;
                        ownWhists = 1;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (w1Order + w2Order == 0)
            {
                // two passes
                playerPool = playerPool + poolPoints;
                return;
            }

            if ((w1Order == 2) || (w2Order == 2))
            {
                // someone took own
                playerPool = playerPool + poolPoints;
                if (w1Order == 2)
                {
                    w1 = w1 + ownWhists * whistPoints;
                }
                else
                {
                    w2 = w2 + ownWhists * whistPoints;
                }
                return;
            }

            // played

            // write player's pool or dump

            if (playerHands >= gameValue)
            {
                playerPool = playerPool + poolPoints;
            }
            else
            {
                playerDump = playerDump + poolPoints * (gameValue - playerHands);
            }

            // calculate whists

            if ((w1Order == 1) & (w2Order == 1))
            // two whists
            {
                if (playerHands >= gameValue)

                // played
                {
                    w1 = w1 + whistPoints * w1Hands;
                    w2 = w2 + whistPoints * w2Hands;
                    if (w1Hands + w2Hands < minWhists)
                    {
                        // not enough whists

                        if ((w1Hands < ownWhists) & (gameValue <= 7))
                        {
                            if (ownWhists - w1Hands < minWhists - w1Hands - w2Hands)
                            {
                                w1Dump = w1Dump + (ownWhists - w1Hands) * whistPoints;
                            }
                            else
                            {
                                w1Dump = w1Dump + (minWhists - w1Hands - w2Hands) * whistPoints;
                            }
                        }
                        if (w2Hands < ownWhists)
                        {
                            if (ownWhists - w2Hands < minWhists - w1Hands - w2Hands)
                            {
                                w2Dump = w2Dump + (ownWhists - w2Hands) * whistPoints;
                            }
                            else
                            {
                                w2Dump = w2Dump + (minWhists - w1Hands - w2Hands) * whistPoints;
                            }
                        }
                    }
                    return;
                }
                else
                {
                    // player failed

                    w1 = w1 + (w1Hands + gameValue - playerHands) * whistPoints;
                    w2 = w2 + (w2Hands + gameValue - playerHands) * whistPoints;
                    wd = wd + (gameValue - playerHands) * whistPoints;
                    return;
                }
            }
            else
            {
                // one whist

                if (playerHands >= gameValue)

                // played
                {
                    if (w1Order == 1)
                    {
                        w1 = w1 + (w1Hands + w2Hands) * whistPoints;

                        if (w1Hands + w2Hands < minWhists)
                        {
                            w1Dump = w1Dump + (minWhists - w1Hands - w2Hands) * whistPoints;
                        }
                    }
                    else
                    {
                        w2 = w2 + (w1Hands + w2Hands) * whistPoints;

                        if (w1Hands + w2Hands < minWhists)
                        {
                            w2Dump = w2Dump + (minWhists - w1Hands - w2Hands) * whistPoints;
                        }
                    }
                    return;
                }
                else
                {
                    w1 = w1 + (w1Hands + w2Hands + 2 * (gameValue - playerHands)) * whistPoints / 2;
                    w2 = w2 + (w1Hands + w2Hands + 2 * (gameValue - playerHands)) * whistPoints / 2;
                    wd = wd + (gameValue - playerHands) * whistPoints;
                }
            }

            return;
        }

        public async Task<IActionResult> CompleteGame(string id)
        {
            var match = _context.MatchB.Include(o => o.Games).Include(o => o.North).Include(o => o.South).Include(o => o.East).Include(o => o.West).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == id);

            GameB game = _context.GameB.Where(m => m.MatchBId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.NorthHand).Include(o => o.SouthHand).Include(o => o.EastHand).Include(o => o.WestHand)
                .Include(o => o.HandInPlay)
                .Include(o => o.NorthSouthHandResult)
                .Include(o => o.EastWestHandResult)
                .ToList()[0];

            game.Status = "Completed";

            GameB newGame = new GameB();
            match.Games.Add(newGame);
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

        public async Task<IActionResult> CloseIncompletePointGame(string id, int activeHands)
        {
            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == id);

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .ToList()[0];

            if (game.Player1Whisting + game.Player2Whisting + game.Player3Whisting + game.Player4Whisting == 1)
            {
                int currentPool = 0;
                int currentDump = 0;
                int w1 = 0;
                int w1Dump = 0;
                int w2Dump = 0;
                int w2 = 0;
                int wd = 0;

                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    currentPool = match.Player1CurrentPool;
                    currentDump = match.Player1CurrentDump;
                    if (game.Dealer.Id == game.Player2.Id)
                    {
                        w1 = match.Player31CurrentWhist;
                        w1Dump = match.Player3CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player41CurrentWhist;
                        wd = match.Player21CurrentWhist;

                        CalculatePointGameResult(game.Value, activeHands, game.Player3Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);

                        if (!(match.Player1CurrentPool == currentPool))
                        {
                            match.Player1CurrentPool = currentPool;
                            match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == currentDump))
                        {
                            match.Player1CurrentDump = currentDump;
                            match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player31CurrentWhist == w1))
                        {
                            match.Player31CurrentWhist = w1;
                            match.Player31Whist = match.Player31Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w1Dump))
                        {
                            match.Player3CurrentDump = w1Dump;
                            match.Player3Dump = match.Player3Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player41CurrentWhist == w2))
                        {
                            match.Player41CurrentWhist = w2;
                            match.Player41Whist = match.Player41Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player21CurrentWhist == wd))
                        {
                            match.Player21CurrentWhist = wd;
                            match.Player21Whist = match.Player21Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        w1 = match.Player21CurrentWhist;
                        w1Dump = match.Player2CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player41CurrentWhist;
                        wd = match.Player31CurrentWhist;

                        CalculatePointGameResult(game.Value, activeHands, game.Player2Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player1CurrentPool == currentPool))
                        {
                            match.Player1CurrentPool = currentPool;
                            match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == currentDump))
                        {
                            match.Player1CurrentDump = currentDump;
                            match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player21CurrentWhist == w1))
                        {
                            match.Player21CurrentWhist = w1;
                            match.Player21Whist = match.Player21Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w1Dump))
                        {
                            match.Player2CurrentDump = w1Dump;
                            match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player41CurrentWhist == w2))
                        {
                            match.Player41CurrentWhist = w2;
                            match.Player41Whist = match.Player41Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player31CurrentWhist == wd))
                        {
                            match.Player31CurrentWhist = wd;
                            match.Player31Whist = match.Player31Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player4.Id)
                    {
                        w1 = match.Player21CurrentWhist;
                        w1Dump = match.Player2CurrentDump;
                        w2Dump = match.Player3CurrentDump;
                        w2 = match.Player31CurrentWhist;
                        wd = match.Player41CurrentWhist;

                        CalculatePointGameResult(game.Value, activeHands, game.Player2Whisting, game.Player3Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player1CurrentPool == currentPool))
                        {
                            match.Player1CurrentPool = currentPool;
                            match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == currentDump))
                        {
                            match.Player1CurrentDump = currentDump;
                            match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player21CurrentWhist == w1))
                        {
                            match.Player21CurrentWhist = w1;
                            match.Player21Whist = match.Player21Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w1Dump))
                        {
                            match.Player2CurrentDump = w1Dump;
                            match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w2Dump))
                        {
                            match.Player3CurrentDump = w2Dump;
                            match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player31CurrentWhist == w2))
                        {
                            match.Player31CurrentWhist = w2;
                            match.Player31Whist = match.Player31Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player41CurrentWhist == wd))
                        {
                            match.Player41CurrentWhist = wd;
                            match.Player41Whist = match.Player41Whist + wd.ToString() + ". ";
                        }
                    }
                }

                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    if (game.Dealer.Id == game.Player1.Id)
                    {
                        currentPool = match.Player2CurrentPool;
                        currentDump = match.Player2CurrentDump;
                        w1 = match.Player32CurrentWhist;
                        w1Dump = match.Player3CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player42CurrentWhist;
                        wd = match.Player12CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player3Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player2CurrentPool == currentPool))
                        {
                            match.Player2CurrentPool = currentPool;
                            match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == currentDump))
                        {
                            match.Player2CurrentDump = currentDump;
                            match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player32CurrentWhist == w1))
                        {
                            match.Player32CurrentWhist = w1;
                            match.Player32Whist = match.Player32Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w1Dump))
                        {
                            match.Player3CurrentDump = w1Dump;
                            match.Player3Dump = match.Player3Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player42CurrentWhist == w2))
                        {
                            match.Player42CurrentWhist = w2;
                            match.Player42Whist = match.Player42Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player12CurrentWhist == wd))
                        {
                            match.Player12CurrentWhist = wd;
                            match.Player12Whist = match.Player12Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        currentPool = match.Player2CurrentPool;
                        currentDump = match.Player2CurrentDump;
                        w1 = match.Player12CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player42CurrentWhist;
                        wd = match.Player32CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player2CurrentPool == currentPool))
                        {
                            match.Player2CurrentPool = currentPool;
                            match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == currentDump))
                        {
                            match.Player2CurrentDump = currentDump;
                            match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player12CurrentWhist == w1))
                        {
                            match.Player12CurrentWhist = w1;
                            match.Player12Whist = match.Player12Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player42CurrentWhist == w2))
                        {
                            match.Player42CurrentWhist = w2;
                            match.Player42Whist = match.Player42Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player32CurrentWhist == wd))
                        {
                            match.Player32CurrentWhist = wd;
                            match.Player32Whist = match.Player32Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player4.Id)
                    {
                        currentPool = match.Player2CurrentPool;
                        currentDump = match.Player2CurrentDump;
                        w1 = match.Player12CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player3CurrentDump;
                        w2 = match.Player32CurrentWhist;
                        wd = match.Player42CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player3Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player2CurrentPool == currentPool))
                        {
                            match.Player2CurrentPool = currentPool;
                            match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == currentDump))
                        {
                            match.Player2CurrentDump = currentDump;
                            match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player12CurrentWhist == w1))
                        {
                            match.Player12CurrentWhist = w1;
                            match.Player12Whist = match.Player12Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w2Dump))
                        {
                            match.Player3CurrentDump = w2Dump;
                            match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player32CurrentWhist == w2))
                        {
                            match.Player32CurrentWhist = w2;
                            match.Player32Whist = match.Player32Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player42CurrentWhist == wd))
                        {
                            match.Player42CurrentWhist = wd;
                            match.Player42Whist = match.Player42Whist + wd.ToString() + ". ";
                        }
                    }
                }

                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    if (game.Dealer.Id == game.Player1.Id)
                    {
                        currentPool = match.Player3CurrentPool;
                        currentDump = match.Player3CurrentDump;
                        w1 = match.Player23CurrentWhist;
                        w1Dump = match.Player2CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player43CurrentWhist;
                        wd = match.Player13CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player2Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player3CurrentPool == currentPool))
                        {
                            match.Player3CurrentPool = currentPool;
                            match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == currentDump))
                        {
                            match.Player3CurrentDump = currentDump;
                            match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player23CurrentWhist == w1))
                        {
                            match.Player23CurrentWhist = w1;
                            match.Player23Whist = match.Player23Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w1Dump))
                        {
                            match.Player2CurrentDump = w1Dump;
                            match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player43CurrentWhist == w2))
                        {
                            match.Player43CurrentWhist = w2;
                            match.Player43Whist = match.Player43Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player13CurrentWhist == wd))
                        {
                            match.Player13CurrentWhist = wd;
                            match.Player13Whist = match.Player13Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player2.Id)
                    {
                        currentPool = match.Player3CurrentPool;
                        currentDump = match.Player3CurrentDump;
                        w1 = match.Player13CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player4CurrentDump;
                        w2 = match.Player43CurrentWhist;
                        wd = match.Player23CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player4Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player3CurrentPool == currentPool))
                        {
                            match.Player3CurrentPool = currentPool;
                            match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == currentDump))
                        {
                            match.Player3CurrentDump = currentDump;
                            match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player13CurrentWhist == w1))
                        {
                            match.Player13CurrentWhist = w1;
                            match.Player13Whist = match.Player13Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == w2Dump))
                        {
                            match.Player4CurrentDump = w2Dump;
                            match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player43CurrentWhist == w2))
                        {
                            match.Player43CurrentWhist = w2;
                            match.Player43Whist = match.Player43Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player23CurrentWhist == wd))
                        {
                            match.Player23CurrentWhist = wd;
                            match.Player23Whist = match.Player23Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player4.Id)
                    {
                        currentPool = match.Player3CurrentPool;
                        currentDump = match.Player3CurrentDump;
                        w1 = match.Player13CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player2CurrentDump;
                        w2 = match.Player23CurrentWhist;
                        wd = match.Player43CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player2Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player3CurrentPool == currentPool))
                        {
                            match.Player3CurrentPool = currentPool;
                            match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == currentDump))
                        {
                            match.Player3CurrentDump = currentDump;
                            match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player13CurrentWhist == w1))
                        {
                            match.Player13CurrentWhist = w1;
                            match.Player13Whist = match.Player13Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w2Dump))
                        {
                            match.Player2CurrentDump = w2Dump;
                            match.Player2Dump = match.Player2Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player23CurrentWhist == w2))
                        {
                            match.Player23CurrentWhist = w2;
                            match.Player23Whist = match.Player23Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player43CurrentWhist == wd))
                        {
                            match.Player43CurrentWhist = wd;
                            match.Player43Whist = match.Player43Whist + wd.ToString() + ". ";
                        }
                    }
                }

                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    if (game.Dealer.Id == game.Player1.Id)
                    {
                        currentPool = match.Player4CurrentPool;
                        currentDump = match.Player4CurrentDump;
                        w1 = match.Player24CurrentWhist;
                        w1Dump = match.Player2CurrentDump;
                        w2Dump = match.Player3CurrentDump;
                        w2 = match.Player34CurrentWhist;
                        wd = match.Player14CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player2Whisting, game.Player3Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player4CurrentPool == currentPool))
                        {
                            match.Player4CurrentPool = currentPool;
                            match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == currentDump))
                        {
                            match.Player4CurrentDump = currentDump;
                            match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player24CurrentWhist == w1))
                        {
                            match.Player24CurrentWhist = w1;
                            match.Player24Whist = match.Player24Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w1Dump))
                        {
                            match.Player2CurrentDump = w1Dump;
                            match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w2Dump))
                        {
                            match.Player3CurrentDump = w2Dump;
                            match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player34CurrentWhist == w2))
                        {
                            match.Player34CurrentWhist = w2;
                            match.Player34Whist = match.Player34Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player14CurrentWhist == wd))
                        {
                            match.Player14CurrentWhist = wd;
                            match.Player14Whist = match.Player14Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player2.Id)
                    {
                        currentPool = match.Player4CurrentPool;
                        currentDump = match.Player4CurrentDump;
                        w1 = match.Player14CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player3CurrentDump;
                        w2 = match.Player34CurrentWhist;
                        wd = match.Player24CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player3Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player4CurrentPool == currentPool))
                        {
                            match.Player4CurrentPool = currentPool;
                            match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == currentDump))
                        {
                            match.Player4CurrentDump = currentDump;
                            match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player14CurrentWhist == w1))
                        {
                            match.Player14CurrentWhist = w1;
                            match.Player14Whist = match.Player14Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player3CurrentDump == w2Dump))
                        {
                            match.Player3CurrentDump = w2Dump;
                            match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player34CurrentWhist == w2))
                        {
                            match.Player34CurrentWhist = w2;
                            match.Player34Whist = match.Player34Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player24CurrentWhist == wd))
                        {
                            match.Player24CurrentWhist = wd;
                            match.Player24Whist = match.Player24Whist + wd.ToString() + ". ";
                        }
                    }
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        currentPool = match.Player4CurrentPool;
                        currentDump = match.Player4CurrentDump;
                        w1 = match.Player14CurrentWhist;
                        w1Dump = match.Player1CurrentDump;
                        w2Dump = match.Player2CurrentDump;
                        w2 = match.Player24CurrentWhist;
                        wd = match.Player34CurrentWhist;
                        CalculatePointGameResult(game.Value, activeHands, game.Player1Whisting, game.Player2Whisting, 10 - activeHands, 0, ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                        if (!(match.Player4CurrentPool == currentPool))
                        {
                            match.Player4CurrentPool = currentPool;
                            match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                        }
                        if (!(match.Player4CurrentDump == currentDump))
                        {
                            match.Player4CurrentDump = currentDump;
                            match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                        }
                        if (!(match.Player14CurrentWhist == w1))
                        {
                            match.Player14CurrentWhist = w1;
                            match.Player14Whist = match.Player14Whist + w1.ToString() + ". ";
                        }
                        if (!(match.Player1CurrentDump == w1Dump))
                        {
                            match.Player1CurrentDump = w1Dump;
                            match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                        }
                        if (!(match.Player2CurrentDump == w2Dump))
                        {
                            match.Player2CurrentDump = w2Dump;
                            match.Player2Dump = match.Player2Dump + w2Dump.ToString() + ". ";
                        }
                        if (!(match.Player24CurrentWhist == w2))
                        {
                            match.Player24CurrentWhist = w2;
                            match.Player24Whist = match.Player24Whist + w2.ToString() + ". ";
                        }
                        if (!(match.Player34CurrentWhist == wd))
                        {
                            match.Player34CurrentWhist = wd;
                            match.Player34Whist = match.Player34Whist + wd.ToString() + ". ";
                        }
                    }
                }
            }

            _context.SaveChanges();

            game.Status = "Completed";

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

            Game newGame = new Game();
            match.Games.Add(newGame);
            _context.SaveChanges();

            if (game.Dealer.Id == game.Player1.Id)
            {
                newGame.Dealer = game.Player2;
            }
            else
            {
                if (game.Dealer.Id == game.Player2.Id)
                {
                    newGame.Dealer = game.Player3;
                }
                else
                {
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        newGame.Dealer = game.Player4;
                    }
                    else
                    {
                        newGame.Dealer = game.Player1;
                    }
                }
            }
            newGame.Player1 = match.Player1;
            newGame.Player2 = match.Player2;
            newGame.Player3 = match.Player3;
            newGame.Player4 = match.Player4;
            newGame.MisereShared = false;
            if (newGame.Dealer.Id == newGame.Player1.Id)
            {
                newGame.NextPlayer = newGame.Player1;
                newGame.Player1Bidding = false;
            }
            else
            {
                if (newGame.Dealer.Id == newGame.Player2.Id)
                {
                    newGame.NextPlayer = newGame.Player2;
                    newGame.Player2Bidding = false;
                }
                else
                {
                    if (newGame.Dealer.Id == newGame.Player3.Id)
                    {
                        newGame.NextPlayer = newGame.Player3;
                        newGame.Player3Bidding = false;
                    }
                    else
                    {
                        newGame.NextPlayer = newGame.Player4;
                        newGame.Player4Bidding = false;
                    }
                }
            }
            newGame.ActivePlayer = newGame.NextPlayer;
            newGame.Type = "";
            newGame.Value = 0;
            newGame.Status = "Dealing";
            _context.SaveChanges();
            Response.Redirect("/../../Matches/Play?id=" + id + "&deal=true");

            errorView = new ErrorViewModel();
            return View("Error", errorView);
        }

        public async Task<IActionResult> CloseIncompleteMisere(string id, int activeHands)
        {
            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == id);

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .ToList()[0];

            int handResult = activeHands;

            if (game.MisereShared)
            {
                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 5;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (5 * handResult);
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 5;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (5 * handResult);
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 5;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (5 * handResult);
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 5;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (5 * handResult);
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 5;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (5 * handResult);
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 5;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (5 * handResult);
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 5;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (5 * handResult);
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 5;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (5 * handResult);
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
            }
            else
            {
                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 10;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (10 * handResult);
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 10;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (10 * handResult);
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 10;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (10 * handResult);
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 10;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (10 * handResult);
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
            }

            _context.SaveChanges();

            game.Status = "Completed";

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

            Game newGame = new Game();
            match.Games.Add(newGame);
            _context.SaveChanges();

            if (game.Dealer.Id == game.Player1.Id)
            {
                newGame.Dealer = game.Player2;
            }
            else
            {
                if (game.Dealer.Id == game.Player2.Id)
                {
                    newGame.Dealer = game.Player3;
                }
                else
                {
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        newGame.Dealer = game.Player4;
                    }
                    else
                    {
                        newGame.Dealer = game.Player1;
                    }
                }
            }
            newGame.Player1 = match.Player1;
            newGame.Player2 = match.Player2;
            newGame.Player3 = match.Player3;
            newGame.Player4 = match.Player4;
            newGame.MisereShared = false;
            if (newGame.Dealer.Id == newGame.Player1.Id)
            {
                newGame.NextPlayer = newGame.Player1;
                newGame.Player1Bidding = false;
            }
            else
            {
                if (newGame.Dealer.Id == newGame.Player2.Id)
                {
                    newGame.NextPlayer = newGame.Player2;
                    newGame.Player2Bidding = false;
                }
                else
                {
                    if (newGame.Dealer.Id == newGame.Player3.Id)
                    {
                        newGame.NextPlayer = newGame.Player3;
                        newGame.Player3Bidding = false;
                    }
                    else
                    {
                        newGame.NextPlayer = newGame.Player4;
                        newGame.Player4Bidding = false;
                    }
                }
            }
            newGame.ActivePlayer = newGame.NextPlayer;
            newGame.Type = "";
            newGame.Value = 0;
            newGame.Status = "Dealing";
            _context.SaveChanges();
            Response.Redirect("/../../Matches/Play?id=" + id + "&deal=true");

            errorView = new ErrorViewModel();
            return View("Error", errorView);
        }

        public Game CompleteMisere(Game game)
        {
            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == game.MatchId);

            int handResult = 0;

            if (game.ActivePlayer.Id == game.Player1.Id)
            {
                if (!(game.Player1HandResult == null))
                {
                    handResult = game.Player1HandResult.hands.Count();
                }
            }

            if (game.ActivePlayer.Id == game.Player2.Id)
            {
                if (!(game.Player2HandResult == null))
                {
                    handResult = game.Player2HandResult.hands.Count();
                }
            }

            if (game.ActivePlayer.Id == game.Player3.Id)
            {
                if (!(game.Player3HandResult == null))
                {
                    handResult = game.Player3HandResult.hands.Count();
                }
            }

            if (game.ActivePlayer.Id == game.Player4.Id)
            {
                if (!(game.Player4HandResult == null))
                {
                    handResult = game.Player4HandResult.hands.Count();
                }
            }

            if (game.MisereShared)
            {
                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 5;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (5 * handResult);
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 5;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (5 * handResult);
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 5;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (5 * handResult);
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 5;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (5 * handResult);
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 5;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (5 * handResult);
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 5;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (5 * handResult);
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 5;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (5 * handResult);
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.Dealer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 5;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (5 * handResult);
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
            }
            else
            {
                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player1CurrentPool = match.Player1CurrentPool + 10;
                        match.Player1Pool = match.Player1Pool + match.Player1CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player1CurrentDump = match.Player1CurrentDump + (10 * game.Player1HandResult.hands.Count());
                        match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player2CurrentPool = match.Player2CurrentPool + 10;
                        match.Player2Pool = match.Player2Pool + match.Player2CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player2CurrentDump = match.Player2CurrentDump + (10 * game.Player2HandResult.hands.Count());
                        match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player3CurrentPool = match.Player3CurrentPool + 10;
                        match.Player3Pool = match.Player3Pool + match.Player3CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player3CurrentDump = match.Player3CurrentDump + (10 * game.Player3HandResult.hands.Count());
                        match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                    }
                }

                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    if (handResult == 0)
                    {
                        match.Player4CurrentPool = match.Player4CurrentPool + 10;
                        match.Player4Pool = match.Player4Pool + match.Player4CurrentPool + ". ";
                    }
                    else
                    {
                        match.Player4CurrentDump = match.Player4CurrentDump + (10 * game.Player4HandResult.hands.Count());
                        match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                    }
                }
            }
            _context.SaveChanges();

            return (game);
        }

        public Game CompleteAllPass(Game game)
        {
            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == game.MatchId);

            if (!(game.Player1HandResult == null))
            {
                match.Player1CurrentDump = match.Player1CurrentDump + game.Player1HandResult.hands.Count();
                if (game.Player1HandResult.hands.Count() > 0)
                {
                    match.Player1Dump = match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                }
                else
                {
                    match.Player1CurrentPool = match.Player1CurrentPool + 1;
                    match.Player1Pool = match.Player1Pool + match.Player1CurrentPool.ToString() + ".";
                }
            }
            if (!(game.Player2HandResult == null))
            {
                match.Player2CurrentDump = match.Player2CurrentDump + game.Player2HandResult.hands.Count();
                if (game.Player2HandResult.hands.Count() > 0)
                {
                    match.Player2Dump = match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                }
                else
                {
                    match.Player2CurrentPool = match.Player2CurrentPool + 1;
                    match.Player2Pool = match.Player2Pool + match.Player2CurrentPool.ToString() + ".";
                }
            }
            if (!(game.Player3HandResult == null))
            {
                match.Player3CurrentDump = match.Player3CurrentDump + game.Player3HandResult.hands.Count();
                if (game.Player3HandResult.hands.Count() > 0)
                {
                    match.Player3Dump = match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                }
                else
                {
                    match.Player3CurrentPool = match.Player3CurrentPool + 1;
                    match.Player3Pool = match.Player3Pool + match.Player3CurrentPool.ToString() + ".";
                }
            }
            if (!(game.Player4HandResult == null))
            {
                match.Player4CurrentDump = match.Player4CurrentDump + game.Player4HandResult.hands.Count();
                if (game.Player4HandResult.hands.Count() > 0)
                {
                    match.Player4Dump = match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                }
                else
                {
                    match.Player4CurrentPool = match.Player4CurrentPool + 1;
                    match.Player4Pool = match.Player4Pool + match.Player4CurrentPool.ToString() + ".";
                }
            }
            _context.SaveChanges();
            return (game);
        }

        public Game CompletePointGame(Game game)
        {
            int currentPool = 0;
            int currentDump = 0;
            int w1 = 0;
            int w1Dump = 0;
            int w2Dump = 0;
            int w2 = 0;
            int wd = 0;

            var match = _context.Match.Include(o => o.Games).Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
              .FirstOrDefault(m => m.Id == game.MatchId);

            if (game.ActivePlayer.Id == game.Player1.Id)
            {
                if (game.Dealer.Id == game.Player2.Id)
                {
                    currentPool = match.Player1CurrentPool;
                    currentDump = match.Player1CurrentDump;
                    w1 = match.Player31CurrentWhist;
                    w1Dump = match.Player3CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player41CurrentWhist;
                    wd = match.Player21CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player1HandResult.hands.Count(), game.Player3Whisting, game.Player4Whisting, game.Player3HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player1CurrentPool == currentPool))
                    {
                        match.Player1CurrentPool = currentPool;
                        match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == currentDump))
                    {
                        match.Player1CurrentDump = currentDump;
                        match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player31CurrentWhist == w1))
                    {
                        match.Player31CurrentWhist = w1;
                        match.Player31Whist = match.Player31Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w1Dump))
                    {
                        match.Player3CurrentDump = w1Dump;
                        match.Player3Dump = match.Player3Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player41CurrentWhist == w2))
                    {
                        match.Player41CurrentWhist = w2;
                        match.Player41Whist = match.Player41Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player21CurrentWhist == wd))
                    {
                        match.Player21CurrentWhist = wd;
                        match.Player21Whist = match.Player21Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player3.Id)
                {
                    currentPool = match.Player1CurrentPool;
                    currentDump = match.Player1CurrentDump;
                    w1 = match.Player21CurrentWhist;
                    w1Dump = match.Player2CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player41CurrentWhist;
                    wd = match.Player31CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player1HandResult.hands.Count(), game.Player2Whisting, game.Player4Whisting, game.Player2HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player1CurrentPool == currentPool))
                    {
                        match.Player1CurrentPool = currentPool;
                        match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == currentDump))
                    {
                        match.Player1CurrentDump = currentDump;
                        match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player21CurrentWhist == w1))
                    {
                        match.Player21CurrentWhist = w1;
                        match.Player21Whist = match.Player21Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w1Dump))
                    {
                        match.Player2CurrentDump = w1Dump;
                        match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player41CurrentWhist == w2))
                    {
                        match.Player41CurrentWhist = w2;
                        match.Player41Whist = match.Player41Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player31CurrentWhist == wd))
                    {
                        match.Player31CurrentWhist = wd;
                        match.Player31Whist = match.Player31Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player4.Id)
                {
                    currentPool = match.Player1CurrentPool;
                    currentDump = match.Player1CurrentDump;
                    w1 = match.Player21CurrentWhist;
                    w1Dump = match.Player2CurrentDump;
                    w2Dump = match.Player3CurrentDump;
                    w2 = match.Player31CurrentWhist;
                    wd = match.Player41CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player1HandResult.hands.Count(), game.Player2Whisting, game.Player3Whisting, game.Player2HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player1CurrentPool == currentPool))
                    {
                        match.Player1CurrentPool = currentPool;
                        match.Player1Pool = match.Player1Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == currentDump))
                    {
                        match.Player1CurrentDump = currentDump;
                        match.Player1Dump = match.Player1Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player21CurrentWhist == w1))
                    {
                        match.Player21CurrentWhist = w1;
                        match.Player21Whist = match.Player21Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w1Dump))
                    {
                        match.Player2CurrentDump = w1Dump;
                        match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w2Dump))
                    {
                        match.Player3CurrentDump = w2Dump;
                        match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player31CurrentWhist == w2))
                    {
                        match.Player31CurrentWhist = w2;
                        match.Player31Whist = match.Player31Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player41CurrentWhist == wd))
                    {
                        match.Player41CurrentWhist = wd;
                        match.Player41Whist = match.Player41Whist + wd.ToString() + ". ";
                    }
                }
            }

            if (game.ActivePlayer.Id == game.Player2.Id)
            {
                if (game.Dealer.Id == game.Player1.Id)
                {
                    currentPool = match.Player2CurrentPool;
                    currentDump = match.Player2CurrentDump;
                    w1 = match.Player32CurrentWhist;
                    w1Dump = match.Player3CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player42CurrentWhist;
                    wd = match.Player12CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player2HandResult.hands.Count(), game.Player3Whisting, game.Player4Whisting, game.Player3HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player2CurrentPool == currentPool))
                    {
                        match.Player2CurrentPool = currentPool;
                        match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == currentDump))
                    {
                        match.Player2CurrentDump = currentDump;
                        match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player32CurrentWhist == w1))
                    {
                        match.Player32CurrentWhist = w1;
                        match.Player32Whist = match.Player32Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w1Dump))
                    {
                        match.Player3CurrentDump = w1Dump;
                        match.Player3Dump = match.Player3Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player42CurrentWhist == w2))
                    {
                        match.Player42CurrentWhist = w2;
                        match.Player42Whist = match.Player42Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player12CurrentWhist == wd))
                    {
                        match.Player12CurrentWhist = wd;
                        match.Player12Whist = match.Player12Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player3.Id)
                {
                    currentPool = match.Player2CurrentPool;
                    currentDump = match.Player2CurrentDump;
                    w1 = match.Player12CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player42CurrentWhist;
                    wd = match.Player32CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player2HandResult.hands.Count(), game.Player1Whisting, game.Player4Whisting, game.Player1HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player2CurrentPool == currentPool))
                    {
                        match.Player2CurrentPool = currentPool;
                        match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == currentDump))
                    {
                        match.Player2CurrentDump = currentDump;
                        match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player12CurrentWhist == w1))
                    {
                        match.Player12CurrentWhist = w1;
                        match.Player12Whist = match.Player12Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player42CurrentWhist == w2))
                    {
                        match.Player42CurrentWhist = w2;
                        match.Player42Whist = match.Player42Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player32CurrentWhist == wd))
                    {
                        match.Player32CurrentWhist = wd;
                        match.Player32Whist = match.Player32Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player4.Id)
                {
                    currentPool = match.Player2CurrentPool;
                    currentDump = match.Player2CurrentDump;
                    w1 = match.Player12CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player3CurrentDump;
                    w2 = match.Player32CurrentWhist;
                    wd = match.Player42CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player2HandResult.hands.Count(), game.Player1Whisting, game.Player3Whisting, game.Player1HandResult.hands.Count(), game.Player3HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player2CurrentPool == currentPool))
                    {
                        match.Player2CurrentPool = currentPool;
                        match.Player2Pool = match.Player2Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == currentDump))
                    {
                        match.Player2CurrentDump = currentDump;
                        match.Player2Dump = match.Player2Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player12CurrentWhist == w1))
                    {
                        match.Player12CurrentWhist = w1;
                        match.Player12Whist = match.Player12Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w2Dump))
                    {
                        match.Player3CurrentDump = w2Dump;
                        match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player32CurrentWhist == w2))
                    {
                        match.Player32CurrentWhist = w2;
                        match.Player32Whist = match.Player32Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player42CurrentWhist == wd))
                    {
                        match.Player42CurrentWhist = wd;
                        match.Player42Whist = match.Player42Whist + wd.ToString() + ". ";
                    }
                }
            }

            if (game.ActivePlayer.Id == game.Player3.Id)
            {
                if (game.Dealer.Id == game.Player1.Id)
                {
                    currentPool = match.Player3CurrentPool;
                    currentDump = match.Player3CurrentDump;
                    w1 = match.Player23CurrentWhist;
                    w1Dump = match.Player2CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player43CurrentWhist;
                    wd = match.Player13CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player3HandResult.hands.Count(), game.Player2Whisting, game.Player4Whisting, game.Player2HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player3CurrentPool == currentPool))
                    {
                        match.Player3CurrentPool = currentPool;
                        match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == currentDump))
                    {
                        match.Player3CurrentDump = currentDump;
                        match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player23CurrentWhist == w1))
                    {
                        match.Player23CurrentWhist = w1;
                        match.Player23Whist = match.Player23Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w1Dump))
                    {
                        match.Player2CurrentDump = w1Dump;
                        match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player43CurrentWhist == w2))
                    {
                        match.Player43CurrentWhist = w2;
                        match.Player43Whist = match.Player43Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player13CurrentWhist == wd))
                    {
                        match.Player13CurrentWhist = wd;
                        match.Player13Whist = match.Player13Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player2.Id)
                {
                    currentPool = match.Player3CurrentPool;
                    currentDump = match.Player3CurrentDump;
                    w1 = match.Player13CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player4CurrentDump;
                    w2 = match.Player43CurrentWhist;
                    wd = match.Player23CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player3HandResult.hands.Count(), game.Player1Whisting, game.Player4Whisting, game.Player1HandResult.hands.Count(), game.Player4HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player3CurrentPool == currentPool))
                    {
                        match.Player3CurrentPool = currentPool;
                        match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == currentDump))
                    {
                        match.Player3CurrentDump = currentDump;
                        match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player13CurrentWhist == w1))
                    {
                        match.Player13CurrentWhist = w1;
                        match.Player13Whist = match.Player13Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == w2Dump))
                    {
                        match.Player4CurrentDump = w2Dump;
                        match.Player4Dump = match.Player4Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player43CurrentWhist == w2))
                    {
                        match.Player43CurrentWhist = w2;
                        match.Player43Whist = match.Player43Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player23CurrentWhist == wd))
                    {
                        match.Player23CurrentWhist = wd;
                        match.Player23Whist = match.Player23Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player4.Id)
                {
                    currentPool = match.Player3CurrentPool;
                    currentDump = match.Player3CurrentDump;
                    w1 = match.Player13CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player2CurrentDump;
                    w2 = match.Player23CurrentWhist;
                    wd = match.Player43CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player3HandResult.hands.Count(), game.Player1Whisting, game.Player2Whisting, game.Player1HandResult.hands.Count(), game.Player2HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player3CurrentPool == currentPool))
                    {
                        match.Player3CurrentPool = currentPool;
                        match.Player3Pool = match.Player3Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == currentDump))
                    {
                        match.Player3CurrentDump = currentDump;
                        match.Player3Dump = match.Player3Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player13CurrentWhist == w1))
                    {
                        match.Player13CurrentWhist = w1;
                        match.Player13Whist = match.Player13Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w2Dump))
                    {
                        match.Player2CurrentDump = w2Dump;
                        match.Player2Dump = match.Player2Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player23CurrentWhist == w2))
                    {
                        match.Player23CurrentWhist = w2;
                        match.Player23Whist = match.Player23Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player43CurrentWhist == wd))
                    {
                        match.Player43CurrentWhist = wd;
                        match.Player43Whist = match.Player43Whist + wd.ToString() + ". ";
                    }
                }
            }

            if (game.ActivePlayer.Id == game.Player4.Id)
            {
                if (game.Dealer.Id == game.Player1.Id)
                {
                    currentPool = match.Player4CurrentPool;
                    currentDump = match.Player4CurrentDump;
                    w1 = match.Player24CurrentWhist;
                    w1Dump = match.Player2CurrentDump;
                    w2Dump = match.Player3CurrentDump;
                    w2 = match.Player34CurrentWhist;
                    wd = match.Player14CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player4HandResult.hands.Count(), game.Player2Whisting, game.Player3Whisting, game.Player2HandResult.hands.Count(), game.Player3HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player4CurrentPool == currentPool))
                    {
                        match.Player4CurrentPool = currentPool;
                        match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == currentDump))
                    {
                        match.Player4CurrentDump = currentDump;
                        match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player24CurrentWhist == w1))
                    {
                        match.Player24CurrentWhist = w1;
                        match.Player24Whist = match.Player24Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w1Dump))
                    {
                        match.Player2CurrentDump = w1Dump;
                        match.Player2Dump = match.Player2Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w2Dump))
                    {
                        match.Player3CurrentDump = w2Dump;
                        match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player34CurrentWhist == w2))
                    {
                        match.Player34CurrentWhist = w2;
                        match.Player34Whist = match.Player34Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player14CurrentWhist == wd))
                    {
                        match.Player14CurrentWhist = wd;
                        match.Player14Whist = match.Player14Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player2.Id)
                {
                    currentPool = match.Player4CurrentPool;
                    currentDump = match.Player4CurrentDump;
                    w1 = match.Player14CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player3CurrentDump;
                    w2 = match.Player34CurrentWhist;
                    wd = match.Player24CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player4HandResult.hands.Count(), game.Player1Whisting, game.Player3Whisting, game.Player1HandResult.hands.Count(), game.Player3HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player4CurrentPool == currentPool))
                    {
                        match.Player4CurrentPool = currentPool;
                        match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == currentDump))
                    {
                        match.Player4CurrentDump = currentDump;
                        match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player14CurrentWhist == w1))
                    {
                        match.Player14CurrentWhist = w1;
                        match.Player14Whist = match.Player14Whist + wd.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player3CurrentDump == w2Dump))
                    {
                        match.Player3CurrentDump = w2Dump;
                        match.Player3Dump = match.Player3Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player34CurrentWhist == w2))
                    {
                        match.Player34CurrentWhist = w2;
                        match.Player34Whist = match.Player34Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player24CurrentWhist == wd))
                    {
                        match.Player24CurrentWhist = wd;
                        match.Player24Whist = match.Player24Whist + wd.ToString() + ". ";
                    }
                }
                if (game.Dealer.Id == game.Player3.Id)
                {
                    currentPool = match.Player4CurrentPool;
                    currentDump = match.Player4CurrentDump;
                    w1 = match.Player14CurrentWhist;
                    w1Dump = match.Player1CurrentDump;
                    w2Dump = match.Player2CurrentDump;
                    w2 = match.Player24CurrentWhist;
                    wd = match.Player34CurrentWhist;
                    CalculatePointGameResult(game.Value, game.Player4HandResult.hands.Count(), game.Player1Whisting, game.Player2Whisting, game.Player1HandResult.hands.Count(), game.Player2HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
                    if (!(match.Player4CurrentPool == currentPool))
                    {
                        match.Player4CurrentPool = currentPool;
                        match.Player4Pool = match.Player4Pool + currentPool.ToString() + ". ";
                    }
                    if (!(match.Player4CurrentDump == currentDump))
                    {
                        match.Player4CurrentDump = currentDump;
                        match.Player4Dump = match.Player4Dump + currentDump.ToString() + ". ";
                    }
                    if (!(match.Player14CurrentWhist == w1))
                    {
                        match.Player14CurrentWhist = w1;
                        match.Player14Whist = match.Player14Whist + w1.ToString() + ". ";
                    }
                    if (!(match.Player1CurrentDump == w1Dump))
                    {
                        match.Player1CurrentDump = w1Dump;
                        match.Player1Dump = match.Player1Dump + w1Dump.ToString() + ". ";
                    }
                    if (!(match.Player2CurrentDump == w2Dump))
                    {
                        match.Player2CurrentDump = w2Dump;
                        match.Player2Dump = match.Player2Dump + w2Dump.ToString() + ". ";
                    }
                    if (!(match.Player24CurrentWhist == w2))
                    {
                        match.Player24CurrentWhist = w2;
                        match.Player24Whist = match.Player24Whist + w2.ToString() + ". ";
                    }
                    if (!(match.Player34CurrentWhist == wd))
                    {
                        match.Player34CurrentWhist = wd;
                        match.Player34Whist = match.Player34Whist + wd.ToString() + ". ";
                    }
                }
            }

            _context.SaveChanges();

            return (game);
        }

        public Game DetermineFirstPlayer(Game _game)
        {
            if (_game.Dealer.Id == _game.Player1.Id)
            {
                _game.NextPlayer = _game.Player2;
            }
            else
            {
                if (_game.Dealer.Id == _game.Player2.Id)
                {
                    _game.NextPlayer = _game.Player3;
                }
                else
                {
                    if (_game.Dealer.Id == _game.Player3.Id)
                    {
                        _game.NextPlayer = _game.Player4;
                    }
                    else
                    {
                        _game.NextPlayer = _game.Player1;
                    }
                }
            }
            return _game;
        }

        public Game Bid(Game _game, string UUID, string bid)
        {
            if (bid == "bid")
            {
                if (_game.Type == "All-Pass")
                {
                    _game.Type = "Spades";
                    _game.Value = 6;
                }
                else
                {
                    if (_game.Type == "Spades")
                    {
                        _game.Type = "Clubs";
                    }
                    else
                    {
                        if (_game.Type == "Clubs")
                        {
                            _game.Type = "Diamonds";
                        }
                        else
                        {
                            if (_game.Type == "Diamonds")
                            {
                                _game.Type = "Hearts";
                            }
                            else
                            {
                                if (_game.Type == "Hearts")
                                {
                                    _game.Type = "No Trump";
                                }
                                else
                                {
                                    if (_game.Type == "No Trump")
                                    {
                                        _game.Type = "Spades";
                                        _game.Value += 1;
                                    }
                                    else
                                    {
                                        if (_game.Type == "Misere")
                                        {
                                            _game.Type = "Spades";
                                            _game.Value = 9;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (bid == "here")
            {
                bid = "bid";
            }

            if ((_game.Player1.Id == UUID) & (!(_game.Type == "All-Pass")) & (!(_game.Type == "Misere")))
            {
                if ((_game.Player4.Id == _game.Dealer.Id) || ((_game.Player3.Id == _game.Dealer.Id) & (!(_game.Player4Bidding))))
                {
                    _game.HerePossible = true;
                }
                else
                {
                    _game.HerePossible = false;
                }
            }

            if ((_game.Player2.Id == UUID) & (!(_game.Type == "All-Pass")) & (!(_game.Type == "Misere")))
            {
                if ((_game.Player1.Id == _game.Dealer.Id) || ((_game.Player4.Id == _game.Dealer.Id) & (!(_game.Player1Bidding))))
                {
                    _game.HerePossible = true;
                }
                else
                {
                    _game.HerePossible = false;
                }
            }

            if ((_game.Player3.Id == UUID) & (!(_game.Type == "All-Pass")) & (!(_game.Type == "Misere")))
            {
                if ((_game.Player2.Id == _game.Dealer.Id) || ((_game.Player1.Id == _game.Dealer.Id) & (!(_game.Player2Bidding))))
                {
                    _game.HerePossible = true;
                }
                else
                {
                    _game.HerePossible = false;
                }
            }

            if ((_game.Player4.Id == UUID) & (!(_game.Type == "All-Pass")) & (!(_game.Type == "Misere")))
            {
                if ((_game.Player3.Id == _game.Dealer.Id) || ((_game.Player2.Id == _game.Dealer.Id) & (!(_game.Player3Bidding))))
                {
                    _game.HerePossible = true;
                }
                else
                {
                    _game.HerePossible = false;
                }
            }

            if (bid == "Misere")
            {
                _game.Type = "Misere";
                _game.MiserePossible = false;
            }

            else
            {
                if (bid == "offer")
                {
                    _game.MisereOffered = UUID;
                    _game.MisereSharable = false;
                }
                else
                {
                    if (bid == "reject")
                    {
                        _game.MisereOffered = "";
                        _game.MisereSharable = false;
                    }
                    else
                    {
                        if (bid == "accept")
                        {
                            _game.MisereShared = true;
                            _game.Type = "Misere";
                            _game.MiserePossible = false;
                            UUID = _game.MisereOffered;
                            _game.MisereOffered = "";
                            bid = "Misere";
                        }
                    }
                }
            }

            if (_game.Player1.Id == UUID)
            {
                if ((_game.Player2.Id == _game.Dealer.Id) & (!(bid == "")))
                {
                    _game.MiserePossible = false;
                }

                if (bid == "pass")
                {
                    _game.Player1Bidding = false;
                    if (_game.Player2Bidding)
                    {
                        _game.NextPlayer = _game.Player2;
                        if ((!(_game.Player3Bidding || _game.Player4Bidding || _game.Player1Bidding)) & (!(_game.Type == "All-Pass")))
                        {
                            _game = StartGame(2, _game);
                        }
                    }
                    else
                    {
                        if (_game.Player3Bidding)
                        {
                            _game.NextPlayer = _game.Player3;
                            if ((!(_game.Player4Bidding || _game.Player1Bidding || _game.Player2Bidding)) & (!(_game.Type == "All-Pass")))
                            {
                                _game = StartGame(3, _game);
                            }
                        }
                        else
                        {
                            if (_game.Player4Bidding)
                            {
                                _game.NextPlayer = _game.Player4;
                                if ((!(_game.Player1Bidding || _game.Player2Bidding || _game.Player3Bidding)) & (!(_game.Type == "All-Pass")))
                                {
                                    _game = StartGame(4, _game);
                                }
                            }
                            else
                            {
                                _game.ActivePlayer = _game.Dealer;
                                _game.NextPlayer = _game.Dealer;
                                if (_game.Type == "All-Pass")
                                {
                                    _game.Status = "Playing";
                                }
                                else
                                {
                                    _game.Status = "Ordering";
                                }
                            }
                        }
                    }
                }
                else
                {
                    if ((bid == "bid") || (bid == "Misere"))
                    {
                        _game.Player1Bidding = true;
                        _game.ActivePlayer = _game.Player1;

                        if (_game.Player2Bidding)
                        {
                            _game.NextPlayer = _game.Player2;
                        }
                        else
                        {
                            if (_game.Player3Bidding)
                            {
                                _game.NextPlayer = _game.Player3;
                            }
                            else
                            {
                                if (_game.Player4Bidding)
                                {
                                    _game.NextPlayer = _game.Player4;
                                }
                                else
                                {
                                    _game.ActivePlayer = _game.Player1;
                                    if (_game.Type == "All-Pass")
                                    {
                                        _game.Status = "Playing";
                                        if (_game.Dealer.Id == _game.Player1.Id)
                                        {
                                            _game.NextPlayer = _game.Player2;
                                        }
                                        else
                                        {
                                            if (_game.Dealer.Id == _game.Player2.Id)
                                            {
                                                _game.NextPlayer = _game.Player3;
                                            }
                                            else
                                            {
                                                if (_game.Dealer.Id == _game.Player3.Id)
                                                {
                                                    _game.NextPlayer = _game.Player4;
                                                }
                                                else
                                                {
                                                    _game.NextPlayer = _game.Player1;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _game.Status = "Ordering";
                                        _game.NextPlayer = _game.ActivePlayer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_game.Player2.Id == UUID)
                {
                    if ((_game.Player3.Id == _game.Dealer.Id) & (!(bid == "")))
                    {
                        _game.MiserePossible = false;
                    }

                    if (bid == "pass")
                    {
                        _game.Player2Bidding = false;
                        if (_game.Player3Bidding)
                        {
                            _game.NextPlayer = _game.Player3;
                            if ((!(_game.Player4Bidding || _game.Player1Bidding || _game.Player2Bidding)) & (!(_game.Type == "All-Pass")))
                            {
                                _game = StartGame(3, _game);
                            }
                        }
                        else
                        {
                            if ((_game.Player4Bidding) & (!(_game.Player4.Id == _game.Dealer.Id)))
                            {
                                _game.NextPlayer = _game.Player4;
                                if ((!(_game.Player1Bidding || _game.Player2Bidding || _game.Player3Bidding)) & (!(_game.Type == "All-Pass")))
                                {
                                    _game = StartGame(4, _game);
                                }
                            }
                            else
                            {
                                if ((_game.Player1Bidding) & (!(_game.Player1.Id == _game.Dealer.Id)))
                                {
                                    _game.NextPlayer = _game.Player1;
                                    if ((!(_game.Player2Bidding || _game.Player3Bidding || _game.Player4Bidding)) & (!(_game.Type == "All-Pass")))
                                    {
                                        _game = StartGame(1, _game);
                                    }
                                }
                                else
                                {
                                    _game.ActivePlayer = _game.Dealer;
                                    _game.NextPlayer = _game.Dealer;
                                    if (_game.Type == "All-Pass")
                                    {
                                        _game.Status = "Playing";
                                    }
                                    else
                                    {
                                        _game.Status = "Ordering";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((bid == "bid") || (bid == "Misere"))
                        {
                            _game.Player2Bidding = true;
                            _game.ActivePlayer = _game.Player2;

                            if (_game.Player3Bidding)
                            {
                                _game.NextPlayer = _game.Player3;
                            }
                            else
                            {
                                if (_game.Player4Bidding)
                                {
                                    _game.NextPlayer = _game.Player4;
                                }
                                else
                                {
                                    if (_game.Player1Bidding)
                                    {
                                        _game.NextPlayer = _game.Player1;
                                    }
                                    else
                                    {
                                        _game.ActivePlayer = _game.Player2;
                                        if (_game.Type == "All-Pass")
                                        {
                                            _game.Status = "Playing";
                                            if (_game.Dealer.Id == _game.Player1.Id)
                                            {
                                                _game.NextPlayer = _game.Player2;
                                            }
                                            else
                                            {
                                                if (_game.Dealer.Id == _game.Player2.Id)
                                                {
                                                    _game.NextPlayer = _game.Player3;
                                                }
                                                else
                                                {
                                                    if (_game.Dealer.Id == _game.Player3.Id)
                                                    {
                                                        _game.NextPlayer = _game.Player4;
                                                    }
                                                    else
                                                    {
                                                        _game.NextPlayer = _game.Player1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _game.Status = "Ordering";
                                            _game.NextPlayer = _game.ActivePlayer;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_game.Player3.Id == UUID)
                    {
                        if ((_game.Player4.Id == _game.Dealer.Id) & (!(bid == "")))
                        {
                            _game.MiserePossible = false;
                        }

                        if (bid == "pass")
                        {
                            _game.Player3Bidding = false;
                            if (_game.Player4Bidding)
                            {
                                _game.NextPlayer = _game.Player4;
                                if ((!(_game.Player1Bidding || _game.Player2Bidding || _game.Player3Bidding)) & (!(_game.Type == "All-Pass")))
                                {
                                    _game = StartGame(4, _game);
                                }
                            }
                            else
                            {
                                if (_game.Player1Bidding)
                                {
                                    _game.NextPlayer = _game.Player1;
                                    if ((!(_game.Player2Bidding || _game.Player3Bidding || _game.Player4Bidding)) & (!(_game.Type == "All-Pass")))
                                    {
                                        _game = StartGame(1, _game);
                                    }
                                }
                                else
                                {
                                    if (_game.Player2Bidding)
                                    {
                                        _game.NextPlayer = _game.Player2;
                                        if ((!(_game.Player3Bidding || _game.Player4Bidding || _game.Player1Bidding)) & (!(_game.Type == "All-Pass")))
                                        {
                                            _game = StartGame(2, _game);
                                        }
                                    }
                                    else
                                    {
                                        _game.ActivePlayer = _game.Dealer;
                                        _game.NextPlayer = _game.Dealer;
                                        if (_game.Type == "All-Pass")
                                        {
                                            _game.Status = "Playing";
                                        }
                                        else
                                        {
                                            _game.Status = "Ordering";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((bid == "bid") || (bid == "Misere"))
                            {
                                _game.Player3Bidding = true;
                                _game.ActivePlayer = _game.Player3;

                                if (_game.Player4Bidding)
                                {
                                    _game.NextPlayer = _game.Player4;
                                }
                                else
                                {
                                    if (_game.Player1Bidding)
                                    {
                                        _game.NextPlayer = _game.Player1;
                                    }
                                    else
                                    {
                                        if (_game.Player2Bidding)
                                        {
                                            _game.NextPlayer = _game.Player2;
                                        }
                                        else
                                        {
                                            _game.ActivePlayer = _game.Player3;
                                            if (_game.Type == "All-Pass")
                                            {
                                                _game.Status = "Playing";
                                                if (_game.Dealer.Id == _game.Player1.Id)
                                                {
                                                    _game.NextPlayer = _game.Player2;
                                                }
                                                else
                                                {
                                                    if (_game.Dealer.Id == _game.Player2.Id)
                                                    {
                                                        _game.NextPlayer = _game.Player3;
                                                    }
                                                    else
                                                    {
                                                        if (_game.Dealer.Id == _game.Player3.Id)
                                                        {
                                                            _game.NextPlayer = _game.Player4;
                                                        }
                                                        else
                                                        {
                                                            _game.NextPlayer = _game.Player1;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _game.Status = "Ordering";
                                                _game.NextPlayer = _game.ActivePlayer;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_game.Player4.Id == UUID)
                        {
                            if ((_game.Player1.Id == _game.Dealer.Id) & (!(bid == "")))
                            {
                                _game.MiserePossible = false;
                            }

                            if (bid == "pass")
                            {
                                _game.Player4Bidding = false;
                                if (_game.Player1Bidding)
                                {
                                    _game.NextPlayer = _game.Player1;
                                    if ((!(_game.Player2Bidding || _game.Player3Bidding || _game.Player4Bidding)) & (!(_game.Type == "All-Pass")))
                                    {
                                        _game = StartGame(1, _game);
                                    }
                                }
                                else
                                {
                                    if (_game.Player2Bidding)
                                    {
                                        _game.NextPlayer = _game.Player2;
                                        if ((!(_game.Player3Bidding || _game.Player4Bidding || _game.Player1Bidding)) & (!(_game.Type == "All-Pass")))
                                        {
                                            _game = StartGame(2, _game);
                                        }
                                    }
                                    else
                                    {
                                        if (_game.Player3Bidding)
                                        {
                                            _game.NextPlayer = _game.Player3;
                                            if ((!(_game.Player4Bidding || _game.Player1Bidding || _game.Player2Bidding)) & (!(_game.Type == "All-Pass")))
                                            {
                                                _game = StartGame(3, _game);
                                            }
                                        }
                                        else
                                        {
                                            _game.ActivePlayer = _game.Dealer;
                                            _game.NextPlayer = _game.Dealer;
                                            if (_game.Type == "All-Pass")
                                            {
                                                _game.Status = "Playing";
                                            }
                                            else
                                            {
                                                _game.Status = "Ordering";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((bid == "bid") || (bid == "Misere"))
                                {
                                    _game.Player4Bidding = true;
                                    _game.ActivePlayer = _game.Player4;

                                    if (_game.Player1Bidding)
                                    {
                                        _game.NextPlayer = _game.Player1;
                                    }
                                    else
                                    {
                                        if (_game.Player2Bidding)
                                        {
                                            _game.NextPlayer = _game.Player2;
                                        }
                                        else
                                        {
                                            if (_game.Player3Bidding)
                                            {
                                                _game.NextPlayer = _game.Player3;
                                            }
                                            else
                                            {
                                                _game.ActivePlayer = _game.Player4;
                                                if (_game.Type == "All-Pass")
                                                {
                                                    _game.Status = "Playing";
                                                    if (_game.Dealer.Id == _game.Player1.Id)
                                                    {
                                                        _game.NextPlayer = _game.Player2;
                                                    }
                                                    else
                                                    {
                                                        if (_game.Dealer.Id == _game.Player2.Id)
                                                        {
                                                            _game.NextPlayer = _game.Player3;
                                                        }
                                                        else
                                                        {
                                                            if (_game.Dealer.Id == _game.Player3.Id)
                                                            {
                                                                _game.NextPlayer = _game.Player4;
                                                            }
                                                            else
                                                            {
                                                                _game.NextPlayer = _game.Player1;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    _game.Status = "Ordering";
                                                    _game.NextPlayer = _game.ActivePlayer;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _game;
        }

        public Game StartGame(int i, Game _game)
        {
            if (_game.Type == "All-Pass")
            {
                _game.Status = "Playing";
            }
            else
            {
                _game.Status = "Ordering";
            }

            if (!(_game.Status == "Ordering"))
            {
                if (_game.Dealer.Id == _game.Player1.Id)
                {
                    _game.NextPlayer = _game.Player2;
                }
                if (_game.Dealer.Id == _game.Player2.Id)
                {
                    _game.NextPlayer = _game.Player3;
                }
                if (_game.Dealer.Id == _game.Player3.Id)
                {
                    _game.NextPlayer = _game.Player4;
                }
                if (_game.Dealer.Id == _game.Player4.Id)
                {
                    _game.NextPlayer = _game.Player1;
                }
            }

            switch (i)
            {
                case 1:
                    _game.ActivePlayer = _game.Player1;
                    break;
                case 2:
                    _game.ActivePlayer = _game.Player2;
                    break;
                case 3:
                    _game.ActivePlayer = _game.Player3;
                    break;
                case 4:
                    _game.ActivePlayer = _game.Player4;
                    break;
            }
            return _game;
        }

        public Game Deal(Game game)
        {
            IList<Card> deck = new List<Card>();
            Card x;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    x = new Card();
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
                    deck.Add(x);
                }
            }

            deck = deck.OrderBy(a => Guid.NewGuid()).ToList();

            Game _game = game;
            int n = 0;
            int q = 0;

            if (_game.Player1.Id == game.Dealer.Id)
            { q = 0; }
            else
            { q = 10; }

            _game.Player1Hand = new Hand();
            _game.Player1Hand.Id = Guid.NewGuid().ToString();
            _game.Player1Hand.Cards = new List<Card>();
            for (int j = 0; j < q; j++)
            {
                _game.Player1Hand.Cards.Add(deck[n]);
                n++;
            }
            if (_game.Player2.Id == game.Dealer.Id)
            { q = 0; }
            else
            { q = 10; }

            _game.Player1HandResult = new SetOfHands();
            _game.Player1HandResult.Id = Guid.NewGuid().ToString();
            _game.Player1HandResult.hands = new List<Hand>();

            _game.Player2Hand = new Hand();
            _game.Player2Hand.Id = Guid.NewGuid().ToString();
            _game.Player2Hand.Cards = new List<Card>();
            for (int j = 0; j < q; j++)
            {
                _game.Player2Hand.Cards.Add(deck[n]);
                n++;
            }
            if (_game.Player3.Id == game.Dealer.Id)
            { q = 0; }
            else
            { q = 10; }

            _game.Player2HandResult = new SetOfHands();
            _game.Player2HandResult.Id = Guid.NewGuid().ToString();
            _game.Player2HandResult.hands = new List<Hand>();

            _game.Player3Hand = new Hand();
            _game.Player3Hand.Id = Guid.NewGuid().ToString();
            _game.Player3Hand.Cards = new List<Card>();
            for (int j = 0; j < q; j++)
            {
                _game.Player3Hand.Cards.Add(deck[n]);
                n++;
            }
            if (_game.Player4.Id == game.Dealer.Id)
            { q = 0; }
            else
            { q = 10; }

            _game.Player3HandResult = new SetOfHands();
            _game.Player3HandResult.Id = Guid.NewGuid().ToString();
            _game.Player3HandResult.hands = new List<Hand>();

            _game.Player4Hand = new Hand();
            _game.Player4Hand.Id = Guid.NewGuid().ToString();
            _game.Player4Hand.Cards = new List<Card>();
            for (int j = 0; j < q; j++)
            {
                _game.Player4Hand.Cards.Add(deck[n]);
                n++;
            }

            _game.Player4HandResult = new SetOfHands();
            _game.Player4HandResult.Id = Guid.NewGuid().ToString();
            _game.Player4HandResult.hands = new List<Hand>();

            _game.Talon = new Hand();
            _game.Talon.Id = Guid.NewGuid().ToString();
            _game.Talon.Cards = new List<Card>();
            _game.Talon.Cards.Add(deck[30]);
            _game.Talon.Cards.Add(deck[31]);
            return _game;
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
    }
}
