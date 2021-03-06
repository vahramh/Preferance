﻿using System;
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
    public class MatchesController : Controller
    {
        ErrorViewModel errorView = null;

        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
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
                return View(await _context.Match.OrderByDescending(m => m.MatchDate).Include(j => j.Player1).Include(j => j.Player2).Include(j => j.Player3).Include(j => j.Player4).ToListAsync());
            }
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(string id)
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

        // GET: Matches/Create
        public IActionResult Create()
        {
            Match CurrentMatch = new Match
            {
                Id = Guid.NewGuid().ToString()
            };
            CurrentMatch.Player1 = new Player();
            CurrentMatch.Player2 = new Player();
            CurrentMatch.Player3 = new Player();
            CurrentMatch.Player4 = new Player();
            CurrentMatch.AllPlayers = _context.Player.OrderBy(p => p.Name).ToListAsync().Result;
            return View(CurrentMatch);
        }

        // POST: Matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Player1,Player2,Player3,Player4,MatchDate")] Match match)
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
        public async Task<IActionResult> MatchStart([Bind("Id,Player1,Player2,Player3,Player4")] Match match)
        {
            match.Player1 = _context.Player.FirstOrDefault(m => m.Id == match.Player1.Id);
            match.Player2 = _context.Player.FirstOrDefault(m => m.Id == match.Player2.Id);
            match.Player3 = _context.Player.FirstOrDefault(m => m.Id == match.Player3.Id);
            match.Player4 = _context.Player.FirstOrDefault(m => m.Id == match.Player4.Id);
            if (ModelState.IsValid)
            {
                _context.Add(match);
                _context.SaveChanges();
                /*                return RedirectToAction(nameof(Index)); */
            }
            match.Games = new List<Game>();
            return View("FirstDealer", match);
        }

        public async Task<IActionResult> SetFirstDealerTempData(string MatchId, string FirstDealer)
        {
            Match match = new Match();
            match = _context.Match.Include(p => p.Player1).Include(p => p.Player2).Include(p => p.Player3).Include(p => p.Player4).FirstOrDefault(m => m.Id == MatchId);
            match.Games = new List<Game>();
            Game game = new Game();
            game.Id = 0;
            game.Dealer = _context.Player.FirstOrDefault(p => p.Id == FirstDealer);
            game.Player1 = match.Player1;
            game.Player2 = match.Player2;
            game.Player3 = match.Player3;
            game.Player4 = match.Player4;
            game.MisereShared = false;
            if (game.Dealer.Id == game.Player1.Id)
            {
                game.NextPlayer = game.Player1;
                game.Player1Bidding = false;
            }
            else
            {
                if (game.Dealer.Id == game.Player2.Id)
                {
                    game.NextPlayer = game.Player2;
                    game.Player2Bidding = false;
                }
                else
                {
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        game.NextPlayer = game.Player3;
                        game.Player3Bidding = false;
                    }
                    else
                    {
                        game.NextPlayer = game.Player4;
                        game.Player4Bidding = false;
                    }
                }
            }
            game.ActivePlayer = game.NextPlayer;
            game.Type = "";
            game.Value = 0;
            game.Status = "Dealing";
            match = _context.Match.FirstOrDefault(m => m.Id == MatchId);
            match.Games.Add(game);
            match.LastHand = new Hand();
            match.LastHand.Id = match.Id;
            match.LastHand.Cards = new List<Card>();
            _context.SaveChanges();

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            string UUID = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewBag.Dealer = FirstDealer;
            ViewBag.Player = UUID;
            Response.Redirect("/../../Matches/Play/" + MatchId);
            return View("MatchPlay", match);
        }

        // GET: Matches/Play/5
        public async Task<IActionResult> Play(string id, bool deal = false, string bid = "", string whist = "", string type = "", string value = "0", string discard1suit = "", string discard1value = "", string discard2suit = "", string discard2value = "")
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

            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(o => o.LastHand)
                .ThenInclude(o => o.Cards)
              .FirstOrDefault(m => m.Id == id);

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.Player1Hand).Include(o => o.Player2Hand).Include(o => o.Player3Hand).Include(o => o.Player4Hand).Include(o => o.Talon)
                .ToList()[0];

            if ((game.Status == "Playing") || (game.Status == "Collecting") || (game.Status == "Offer"))
            {
                Response.Redirect("/../../Games/Play/" + id);
                errorView = new ErrorViewModel();
                return View("Error", errorView);
            }

            if (!(whist == ""))
            {
                if (whist == "open")
                {
                    game.OpenWhist = true;
                    game.Status = "Playing";
                    game = DetermineFirstPlayer(game);

                    _context.SaveChanges();
                    Response.Redirect("/../../Games/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
                else
                {
                    game.OpenWhist = false;
                    game.Status = "Playing";
                    game = DetermineFirstPlayer(game);

                    _context.SaveChanges();
                    Response.Redirect("/../../Games/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            if (bid == "order")
            {
                match.LastHand.Cards.Clear();
                if (game.Discarded == null)
                {
                    game.Discarded = new Hand();
                    game.Discarded.Id = Guid.NewGuid().ToString();
                    game.Discarded.Cards = new List<Card>();
                }
                game.Talon.Cards = _context.Card.Where(c => c.HandId == game.Talon.Id).OrderBy(c => c.Seniority).ToList();
                game.Player1Hand.Cards = _context.Card.Where(c => c.HandId == game.Player1Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player2Hand.Cards = _context.Card.Where(c => c.HandId == game.Player2Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player3Hand.Cards = _context.Card.Where(c => c.HandId == game.Player3Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player4Hand.Cards = _context.Card.Where(c => c.HandId == game.Player4Hand.Id).OrderBy(c => c.Seniority).ToList();
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
                foreach (var card in game.Talon.Cards)
                {
                    var _card = new Card();
                    _card.Id = Guid.NewGuid().ToString();
                    _card.Colour = card.Colour;
                    _card.Value = card.Value;
                    _card.Sequence = card.Sequence;
                    _card.Seniority = card.Seniority;
                    match.LastHand.Cards.Add(_card);
                }

                if (game.ActivePlayer.Id == game.Player1.Id)
                {
                    game.Player1Hand.Cards.Add(game.Talon.Cards[0]);
                    game.Player1Hand.Cards.Add(game.Talon.Cards[1]);
                    game.Talon.Cards.Clear();
                    _context.SaveChanges();
                    foreach (Card card in game.Player1Hand.Cards)
                    {
                        if (((card.Colour == discard1suit) & (card.Value == discard1value))
                            || ((card.Colour == discard2suit) & (card.Value == discard2value)))
                        {
                            game.Discarded.Cards.Add(card);
                        }
                    }
                    foreach (Card card in game.Discarded.Cards)
                    {
                        game.Player1Hand.Cards.Remove(card);
                    }
                }
                if (game.ActivePlayer.Id == game.Player2.Id)
                {
                    game.Player2Hand.Cards.Add(game.Talon.Cards[0]);
                    game.Player2Hand.Cards.Add(game.Talon.Cards[1]);
                    game.Talon.Cards.Clear();
                    _context.SaveChanges();
                    foreach (Card card in game.Player2Hand.Cards)
                    {
                        if (((card.Colour == discard1suit) & (card.Value == discard1value))
                            || ((card.Colour == discard2suit) & (card.Value == discard2value)))
                        {
                            game.Discarded.Cards.Add(card);
                        }
                    }
                    foreach (Card card in game.Discarded.Cards)
                    {
                        game.Player2Hand.Cards.Remove(card);
                    }
                }
                if (game.ActivePlayer.Id == game.Player3.Id)
                {
                    game.Player3Hand.Cards.Add(game.Talon.Cards[0]);
                    game.Player3Hand.Cards.Add(game.Talon.Cards[1]);
                    game.Talon.Cards.Clear();
                    _context.SaveChanges();
                    foreach (Card card in game.Player3Hand.Cards)
                    {
                        if (((card.Colour == discard1suit) & (card.Value == discard1value))
                            || ((card.Colour == discard2suit) & (card.Value == discard2value)))
                        {
                            game.Discarded.Cards.Add(card);
                        }
                    }
                    foreach (Card card in game.Discarded.Cards)
                    {
                        game.Player3Hand.Cards.Remove(card);
                    }
                }
                if (game.ActivePlayer.Id == game.Player4.Id)
                {
                    game.Player4Hand.Cards.Add(game.Talon.Cards[0]);
                    game.Player4Hand.Cards.Add(game.Talon.Cards[1]);
                    game.Talon.Cards.Clear();
                    _context.SaveChanges();
                    foreach (Card card in game.Player4Hand.Cards)
                    {
                        if (((card.Colour == discard1suit) & (card.Value == discard1value))
                            || ((card.Colour == discard2suit) & (card.Value == discard2value)))
                        {
                            game.Discarded.Cards.Add(card);
                        }
                    }
                    foreach (Card card in game.Discarded.Cards)
                    {
                        game.Player4Hand.Cards.Remove(card);
                    }
                }

                game.Type = type;
                game.Value = Int32.Parse(value);
                if ((game.Type == "All-Pass") || (game.Type == "Misere"))
                {
                    game.Status = "Playing";
                    if (game.Type == "Misere")
                    {
                        if (game.Dealer.Id == game.Player1.Id)
                        {
                            game.NextPlayer = game.Player2;
                        }
                        else
                        {
                            if (game.Dealer.Id == game.Player2.Id)
                            {
                                game.NextPlayer = game.Player3;
                            }
                            else
                            {
                                if (game.Dealer.Id == game.Player3.Id)
                                {
                                    game.NextPlayer = game.Player4;
                                }
                                else
                                {
                                    if (game.Dealer.Id == game.Player4.Id)
                                    {
                                        game.NextPlayer = game.Player1;
                                    }
                                }
                            }
                        }
                    }
                    _context.SaveChanges();
                    Response.Redirect("/../../Games/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
                else
                {
                    game.Status = "Whisting";
                    if (game.ActivePlayer.Id == game.Player1.Id)
                    {
                        if (game.Player2.Id == game.Dealer.Id)
                        {
                            game.NextPlayer = game.Player3;
                        }
                        else
                        {
                            game.NextPlayer = game.Player2;
                        }
                    }
                    if (game.ActivePlayer.Id == game.Player2.Id)
                    {
                        if (game.Player3.Id == game.Dealer.Id)
                        {
                            game.NextPlayer = game.Player4;
                        }
                        else
                        {
                            game.NextPlayer = game.Player3;
                        }
                    }
                    if (game.ActivePlayer.Id == game.Player3.Id)
                    {
                        if (game.Player4.Id == game.Dealer.Id)
                        {
                            game.NextPlayer = game.Player1;
                        }
                        else
                        {
                            game.NextPlayer = game.Player4;
                        }
                    }
                    if (game.ActivePlayer.Id == game.Player4.Id)
                    {
                        if (game.Player1.Id == game.Dealer.Id)
                        {
                            game.NextPlayer = game.Player2;
                        }
                        else
                        {
                            game.NextPlayer = game.Player1;
                        }
                    }
                    _context.SaveChanges();
                    Response.Redirect("/../../Matches/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            if (game.Status == "Whisting")
            {
                if (bid == "whist")
                {
                    if (UUID == game.Player1.Id)
                    {
                        game.Player1Whisting = 1;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player2.Id) & (game.ActivePlayer.Id == game.Player3.Id)) || (game.ActivePlayer.Id == game.Player2.Id))
                        {
                            // Player 1 is the second whister

                            if ((game.Dealer.Id == game.Player4.Id))

                            //First whister is Player 3

                            {
                                if (game.Player3Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            //First whister is Player 4
                            {
                                if (game.Player4Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            //Player 1 is the first whister

                            if (game.Player2.Id == game.Dealer.Id)
                            {
                                // Player 3 is the second whister

                                // Has player 3 said "Take"?

                                if (game.Player3Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player3Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player3;
                                }
                            }
                            else
                            {
                                // Player 2 is the second whister
                                if (game.Player2Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player2Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player2;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }
                    }

                    if (UUID == game.Player2.Id)
                    {
                        game.Player2Whisting = 1;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player3.Id) & (game.ActivePlayer.Id == game.Player4.Id)) || (game.ActivePlayer.Id == game.Player3.Id))
                        {
                            // Player 2 is the second whister

                            if ((game.Dealer.Id == game.Player1.Id))

                            //First whister is Player 4

                            {
                                if (game.Player4Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            //First whister is Player 1
                            {
                                if (game.Player1Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            //Player 2 is the first whister

                            if (game.Player3.Id == game.Dealer.Id)
                            {
                                // Player 4 is the second whister
                                if (game.Player4Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player4Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player4;
                                }
                            }
                            else
                            {
                                // Player 3 is the second whister
                                if (game.Player3Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player3Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player3;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }
                    }

                    if (UUID == game.Player3.Id)
                    {
                        game.Player3Whisting = 1;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player4.Id) & (game.ActivePlayer.Id == game.Player1.Id)) || (game.ActivePlayer.Id == game.Player4.Id))
                        {
                            // Player 3 is the second whister

                            if ((game.Dealer.Id == game.Player2.Id))

                            //First whister is Player 1

                            {
                                if (game.Player1Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            //First whister is Player 2
                            {
                                if (game.Player2Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            //Player 3 is the first whister

                            if (game.Player4.Id == game.Dealer.Id)
                            {
                                // Player 1 is the second whister
                                if (game.Player1Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player1Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player1;
                                }
                            }
                            else
                            {
                                // Player 4 is the second whister
                                if (game.Player4Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player4Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player4;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }
                    }

                    if (UUID == game.Player4.Id)
                    {
                        game.Player4Whisting = 1;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player1.Id) & (game.ActivePlayer.Id == game.Player2.Id)) || (game.ActivePlayer.Id == game.Player1.Id))
                        {
                            // Player 4 is the second whister

                            if ((game.Dealer.Id == game.Player3.Id))

                            //First whister is Player 2

                            {
                                if (game.Player2Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            //First whister is Player 3
                            {
                                if (game.Player3Whisting == 1)
                                {
                                    //Two whists

                                    game.Status = "Playing";

                                    // Set the first player

                                    game = DetermineFirstPlayer(game);

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Games/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // Only one whist

                                    game.Status = "Opening";

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            //Player 4 is the first whister

                            if (game.Player1.Id == game.Dealer.Id)
                            {
                                // Player 2 is the second whister
                                if (game.Player2Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player2Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player2;
                                }
                            }
                            else
                            {
                                // Player 1 is the second whister
                                if (game.Player1Whisting == 2)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.Player1Whisting = 0;
                                }
                                else
                                {
                                    game.NextPlayer = game.Player1;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }
                    }

                }

                if (bid == "take")
                {
                    // I'm definitely the second whister. Need to revert to the first whister for decision
                    if (UUID == game.Player1.Id)
                    {
                        game.Player1Whisting = 2;

                        if (game.Player4.Id == game.Dealer.Id)
                        {
                            // First whister is Player 3

                            game.NextPlayer = game.Player3;
                        }
                        else
                        {
                            // First whister is Player 4

                            game.NextPlayer = game.Player4;
                        }
                    }

                    if (UUID == game.Player2.Id)
                    {
                        game.Player2Whisting = 2;

                        if (game.Player1.Id == game.Dealer.Id)
                        {
                            // First whister is Player 4

                            game.NextPlayer = game.Player4;
                        }
                        else
                        {
                            // First whister is Player 1

                            game.NextPlayer = game.Player1;
                        }
                    }

                    if (UUID == game.Player3.Id)
                    {
                        game.Player3Whisting = 2;

                        if (game.Player2.Id == game.Dealer.Id)
                        {
                            // First whister is Player 1

                            game.NextPlayer = game.Player1;
                        }
                        else
                        {
                            // First whister is Player 2

                            game.NextPlayer = game.Player2;
                        }
                    }

                    if (UUID == game.Player4.Id)
                    {
                        game.Player4Whisting = 2;

                        if (game.Player3.Id == game.Dealer.Id)
                        {
                            // First whister is Player 2

                            game.NextPlayer = game.Player2;
                        }
                        else
                        {
                            // First whister is Player 3

                            game.NextPlayer = game.Player3;
                        }
                    }

                    _context.SaveChanges();
                    Response.Redirect("/../../Matches/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);

                }

                if (bid == "pass")
                {
                    if (UUID == game.Player1.Id)
                    {
                        game.Player1Whisting = 0;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player2.Id) & (game.ActivePlayer.Id == game.Player3.Id)) || (game.ActivePlayer.Id == game.Player2.Id))
                        {
                            // Player 1 is the second Whister.

                            // Who's the first whister?

                            if (game.Dealer.Id == game.Player4.Id)
                            {
                                // Player 3 is the first whister

                                if (game.Player3Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player3;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            {
                                // Player 4 is the first whister

                                if (game.Player4Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player4;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            // Player 1 is the first whister

                            //Who's the second whister?

                            if (game.Dealer.Id == game.Player2.Id)
                            {
                                // Player 3 is the second whister

                                // Has Player 3 said "Take"?

                                if (game.Player3Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player3;
                                }
                            }
                            else
                            {
                                // Player 2 is the second whister

                                // Has Player 2 said "Take"?

                                if (game.Player2Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player2;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }

                    }

                    if (UUID == game.Player2.Id)
                    {
                        game.Player2Whisting = 0;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player3.Id) & (game.ActivePlayer.Id == game.Player4.Id)) || (game.ActivePlayer.Id == game.Player3.Id))
                        {
                            // Player 2 is the second Whister.

                            // Who's the first whister?

                            if (game.Dealer.Id == game.Player1.Id)
                            {
                                // Player 4 is the first whister

                                if (game.Player4Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player4;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            {
                                // Player 1 is the first whister

                                if (game.Player1Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player1;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            // Player 1 is the first whister, need to go to second whister

                            //Who's the second whister?

                            if (game.Dealer.Id == game.Player3.Id)
                            {
                                // Player 4 is the second whister

                                // Has Player 4 said "Take"?

                                if (game.Player4Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player4;
                                }
                            }
                            else
                            {
                                // Player 3 is the second whister

                                // Has Player 3 said "Take"?

                                if (game.Player3Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player3;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }

                    }

                    if (UUID == game.Player3.Id)
                    {
                        game.Player3Whisting = 0;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player4.Id) & (game.ActivePlayer.Id == game.Player1.Id)) || (game.ActivePlayer.Id == game.Player4.Id))
                        {
                            // Player 2 is the second Whister.

                            // Who's the first whister?

                            if (game.Dealer.Id == game.Player2.Id)
                            {
                                // Player 1 is the first whister

                                if (game.Player1Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player1;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            {
                                // Player 2 is the first whister

                                if (game.Player2Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player2;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            // Player 2 is the first whister, need to go to second whister

                            //Who's the second whister?

                            if (game.Dealer.Id == game.Player4.Id)
                            {
                                // Player 1 is the second whister

                                // Has Player 1 said "Take"?

                                if (game.Player1Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player1;
                                }
                            }
                            else
                            {
                                // Player 4 is the second whister

                                // Has Player 4 said "Take"?

                                if (game.Player4Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player4;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }

                    }

                    if (UUID == game.Player4.Id)
                    {
                        game.Player4Whisting = 0;

                        // if I'm the second Whister

                        if (((game.Dealer.Id == game.Player1.Id) & (game.ActivePlayer.Id == game.Player2.Id)) || (game.ActivePlayer.Id == game.Player1.Id))
                        {
                            // Player 2 is the second Whister.

                            // Who's the first whister?

                            if (game.Dealer.Id == game.Player3.Id)
                            {
                                // Player 2 is the first whister

                                if (game.Player2Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player2;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                            else
                            {
                                // Player 3 is the first whister

                                if (game.Player3Whisting == 1)
                                {
                                    // Only one whist

                                    game.Status = "Opening";
                                    game.NextPlayer = game.Player3;

                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/Play/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    // This can only happen if the game is 8 or more. Two passes. Finish the game, start a new one

                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                            }
                        }
                        else
                        {
                            // Player 4 is the first whister, need to go to second whister

                            //Who's the second whister?

                            if (game.Dealer.Id == game.Player1.Id)
                            {
                                // Player 2 is the second whister

                                // Has Player 2 said "Take"?

                                if (game.Player2Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player2;
                                }
                            }
                            else
                            {
                                // Player 1 is the second whister

                                // Has Player 3 said "Take"?

                                if (game.Player1Whisting == 2)
                                {
                                    _context.SaveChanges();
                                    Response.Redirect("/../../Matches/CompleteGame/" + id);
                                    errorView = new ErrorViewModel();
                                    return View("Error", errorView);
                                }
                                else
                                {
                                    game.NextPlayer = game.Player1;
                                }
                            }
                            _context.SaveChanges();
                            Response.Redirect("/../../Matches/Play/" + id);
                            errorView = new ErrorViewModel();
                            return View("Error", errorView);
                        }

                    }
                }
            }


            if (!(game.Talon == null))
            {
                game.Talon.Cards = _context.Card.Where(c => c.HandId == game.Talon.Id).OrderBy(c => c.Seniority).ToList();
                game.Player1Hand.Cards = _context.Card.Where(c => c.HandId == game.Player1Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player2Hand.Cards = _context.Card.Where(c => c.HandId == game.Player2Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player3Hand.Cards = _context.Card.Where(c => c.HandId == game.Player3Hand.Id).OrderBy(c => c.Seniority).ToList();
                game.Player4Hand.Cards = _context.Card.Where(c => c.HandId == game.Player4Hand.Id).OrderBy(c => c.Seniority).ToList();
            }
            /*            HttpContext.Response.Headers.Add("refresh", "2"); 
            Game game = match.Games.OrderByDescending(m => m.Id).FirstOrDefault();
            */
            /* ********************************** Dealing ************************************* */

            if (deal)
            {
                if (game.Status == "Dealing")
                {
                    game = Deal(game);
                    game.Status = "Bidding";
                    game.Type = "All-Pass";
                    game.ActivePlayer = game.Dealer;
                    if (game.Dealer.Id == game.Player1.Id)
                    {
                        game.NextPlayer = game.Player2;
                    }
                    if (game.Dealer.Id == game.Player2.Id)
                    {
                        game.NextPlayer = game.Player3;
                    }
                    if (game.Dealer.Id == game.Player3.Id)
                    {
                        game.NextPlayer = game.Player4;
                    }
                    if (game.Dealer.Id == game.Player4.Id)
                    {
                        game.NextPlayer = game.Player1;
                    }
                    deal = false;
                    _context.SaveChanges();
                    Response.Redirect("/../../Matches/Play/" + id);
                    errorView = new ErrorViewModel();
                    return View("Error", errorView);
                }
            }

            /* **************************** Bidding Process *********************************** */

            if ((game.Status == "Bidding") & ((UUID == game.NextPlayer.Id) || ((UUID == game.Dealer.Id) & ((bid == "reject") || (bid == "accept")))))
            {
                game = Bid(game, UUID, bid);
                _context.SaveChanges();
            }

            if (match.Games[match.Games.Count - 1].ActivePlayer == null)
            {
                match.Games[match.Games.Count - 1].ActivePlayer = match.Games[match.Games.Count - 1].Dealer;
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
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games).Include(m => m.LastHand)
              .FirstOrDefault(m => m.Id == id);
            match.LastHand.Cards = _context.Card.Where(c => c.HandId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

            Game game = _context.Game.Where(m => m.MatchId == match.Id).OrderByDescending(g => g.Id).Take(1)
                .Include(o => o.Player1Hand).Include(o => o.Player2Hand).Include(o => o.Player3Hand).Include(o => o.Player4Hand)
                .Include(o => o.HandInPlay)
                .Include(o => o.Player1HandResult).ThenInclude(p => p.hands)
                .Include(o => o.Player2HandResult).ThenInclude(p => p.hands)
                .Include(o => o.Player3HandResult).ThenInclude(p => p.hands)
                .Include(o => o.Player4HandResult).ThenInclude(p => p.hands)
                .Include(o => o.Talon)
                .ToList()[0];

            switch (game.Type)
            {
                case "All-Pass":
                    {
                        game = CompleteAllPass(game);
                        break;
                    }
                case "Misere":
                    {
                        game = CompleteMisere(game);
                        break;
                    }
                default:

                    {
                        game = CompletePointGame(game);
                        break;
                    }
            }

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
            match.LastHand.Cards.Clear();
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
//            Response.Redirect("/../../Matches/Play/" + id);
            Response.Redirect("/../../Matches/Play?id=" + id + "&deal=true");
            errorView = new ErrorViewModel();
            return View("Error", errorView);

        }

        public async Task<IActionResult> CloseIncompletePointGame(string id, int activeHands)
        {
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games).Include(m => m.LastHand)
              .FirstOrDefault(m => m.Id == id);
            match.LastHand.Cards = _context.Card.Where(c => c.HandId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

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
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games).Include(m => m.LastHand)
              .FirstOrDefault(m => m.Id == id);
            match.LastHand.Cards = _context.Card.Where(c => c.HandId == match.LastHand.Id).OrderBy(c => c.Sequence).ToList();

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
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
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
            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
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

            var match = _context.Match.Include(o => o.Player1).Include(o => o.Player2).Include(o => o.Player3).Include(o => o.Player4).Include(m => m.Games)
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
                    CalculatePointGameResult(game.Value, game.Player1HandResult.hands.Count(), game.Player2Whisting, game.Player3Whisting, game.Player2HandResult.hands.Count(), game.Player3HandResult.hands.Count(), ref currentPool, ref currentDump, ref w1, ref w1Dump, ref w2Dump, ref w2, ref wd);
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

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = _context.Match.AsNoTracking().Include(p => p.Player1).Include(p => p.Player2).Include(p => p.Player3).Include(p => p.Player4)
                .FirstOrDefault(m => m.Id == id);

            _context.SaveChanges();

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
            var _match = _context.Match.Include(p => p.Player1).Include(p => p.Player2).Include(p => p.Player3).Include(p => p.Player4).FirstOrDefault(m => m.Id == match.Id);
            if (ModelState.IsValid)
            {
                if (!(match.Player1CurrentPool == _match.Player1CurrentPool)) 
                {
                    _match.Player1CurrentPool = match.Player1CurrentPool;
                    _match.Player1Pool = _match.Player1Pool + match.Player1CurrentPool.ToString() + ". "; 
                }
                if (!(match.Player2CurrentPool == _match.Player2CurrentPool))
                {
                    _match.Player2CurrentPool = match.Player2CurrentPool;
                    _match.Player2Pool = _match.Player2Pool + match.Player2CurrentPool.ToString() + ". ";
                }
                if (!(match.Player3CurrentPool == _match.Player3CurrentPool))
                {
                    _match.Player3CurrentPool = match.Player3CurrentPool;
                    _match.Player3Pool = _match.Player3Pool + match.Player3CurrentPool.ToString() + ". ";
                }
                if (!(match.Player4CurrentPool == _match.Player4CurrentPool))
                {
                    _match.Player4CurrentPool = match.Player4CurrentPool;
                    _match.Player4Pool = _match.Player4Pool + match.Player4CurrentPool.ToString() + ". ";
                }
                if (!(match.Player1CurrentDump == _match.Player1CurrentDump))
                {
                    _match.Player1CurrentDump = match.Player1CurrentDump;
                    _match.Player1Dump = _match.Player1Dump + match.Player1CurrentDump.ToString() + ". ";
                }
                if (!(match.Player2CurrentDump == _match.Player2CurrentDump))
                {
                    _match.Player2CurrentDump = match.Player2CurrentDump;
                    _match.Player2Dump = _match.Player2Dump + match.Player2CurrentDump.ToString() + ". ";
                }
                if (!(match.Player3CurrentDump == _match.Player3CurrentDump))
                {
                    _match.Player3CurrentDump = match.Player3CurrentDump;
                    _match.Player3Dump = _match.Player3Dump + match.Player3CurrentDump.ToString() + ". ";
                }
                if (!(match.Player4CurrentDump == _match.Player4CurrentDump))
                {
                    _match.Player4CurrentDump = match.Player4CurrentDump;
                    _match.Player4Dump = _match.Player4Dump + match.Player4CurrentDump.ToString() + ". ";
                }
                if (!(match.Player12CurrentWhist == _match.Player12CurrentWhist))
                {
                    _match.Player12CurrentWhist = match.Player12CurrentWhist;
                    _match.Player12Whist = _match.Player12Whist + match.Player12CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player13CurrentWhist == _match.Player13CurrentWhist))
                {
                    _match.Player13CurrentWhist = match.Player13CurrentWhist;
                    _match.Player13Whist = _match.Player13Whist + match.Player13CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player14CurrentWhist == _match.Player14CurrentWhist))
                {
                    _match.Player14CurrentWhist = match.Player14CurrentWhist;
                    _match.Player14Whist = _match.Player14Whist + match.Player14CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player21CurrentWhist == _match.Player21CurrentWhist))
                {
                    _match.Player21CurrentWhist = match.Player21CurrentWhist;
                    _match.Player21Whist = _match.Player21Whist + match.Player21CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player23CurrentWhist == _match.Player23CurrentWhist))
                {
                    _match.Player23CurrentWhist = match.Player23CurrentWhist;
                    _match.Player23Whist = _match.Player23Whist + match.Player23CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player24CurrentWhist == _match.Player24CurrentWhist))
                {
                    _match.Player24CurrentWhist = match.Player24CurrentWhist;
                    _match.Player24Whist = _match.Player24Whist + match.Player24CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player31CurrentWhist == _match.Player31CurrentWhist))
                {
                    _match.Player31CurrentWhist = match.Player31CurrentWhist;
                    _match.Player31Whist = _match.Player31Whist + match.Player31CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player32CurrentWhist == _match.Player32CurrentWhist))
                {
                    _match.Player32CurrentWhist = match.Player32CurrentWhist;
                    _match.Player32Whist = _match.Player32Whist + match.Player32CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player34CurrentWhist == _match.Player34CurrentWhist))
                {
                    _match.Player34CurrentWhist = match.Player34CurrentWhist;
                    _match.Player34Whist = _match.Player34Whist + match.Player34CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player41CurrentWhist == _match.Player41CurrentWhist))
                {
                    _match.Player41CurrentWhist = match.Player41CurrentWhist;
                    _match.Player41Whist = _match.Player41Whist + match.Player41CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player42CurrentWhist == _match.Player42CurrentWhist))
                {
                    _match.Player42CurrentWhist = match.Player42CurrentWhist;
                    _match.Player42Whist = _match.Player42Whist + match.Player42CurrentWhist.ToString() + ". ";
                }
                if (!(match.Player43CurrentWhist == _match.Player43CurrentWhist))
                {
                    _match.Player43CurrentWhist = match.Player43CurrentWhist;
                    _match.Player43Whist = _match.Player43Whist + match.Player43CurrentWhist.ToString() + ". ";
                }

                float p1TempDump = 0;
                float p2TempDump = 0;
                float p3TempDump = 0;
                float p4TempDump = 0;

                p1TempDump = _match.Player1CurrentDump - _match.Player1CurrentPool;
                p2TempDump = _match.Player2CurrentDump - _match.Player2CurrentPool;
                p3TempDump = _match.Player3CurrentDump - _match.Player3CurrentPool;
                p4TempDump = _match.Player4CurrentDump - _match.Player4CurrentPool;

                _match.Player1CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p1TempDump) * 10
                    + _match.Player12CurrentWhist + _match.Player13CurrentWhist + _match.Player14CurrentWhist
                    - _match.Player21CurrentWhist - _match.Player31CurrentWhist - _match.Player41CurrentWhist;

                _match.Player2CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p2TempDump) * 10
                    + _match.Player21CurrentWhist + _match.Player23CurrentWhist + _match.Player24CurrentWhist
                    - _match.Player12CurrentWhist - _match.Player32CurrentWhist - _match.Player42CurrentWhist;

                _match.Player3CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p3TempDump) * 10
                    + _match.Player31CurrentWhist + _match.Player32CurrentWhist + _match.Player34CurrentWhist
                    - _match.Player13CurrentWhist - _match.Player23CurrentWhist - _match.Player43CurrentWhist;

                _match.Player4CurrentScore = ((p1TempDump + p2TempDump + p3TempDump + p4TempDump) / 4 - p4TempDump) * 10
                    + _match.Player41CurrentWhist + _match.Player42CurrentWhist + _match.Player43CurrentWhist
                    - _match.Player14CurrentWhist - _match.Player24CurrentWhist - _match.Player34CurrentWhist;

                
                try
                {
                    _context.Update(_match);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(_match.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
//                return RedirectToAction(nameof(Index));
            }
            return View(_match);
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
