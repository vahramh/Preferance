using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Preferance.Models;

namespace Preferance.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Preferance.Models.Match> Match { get; set; }
        public DbSet<Preferance.Models.Game> Game { get; set; }
        public DbSet<Preferance.Models.Hand> Hand { get; set; }
        public DbSet<Preferance.Models.Card> Card { get; set; }
        public DbSet<Preferance.Models.MatchB> MatchB { get; set; }
        public DbSet<Preferance.Models.GameB> GameB { get; set; }
        public DbSet<Preferance.Models.HandB> HandB { get; set; }
        public DbSet<Preferance.Models.CardB> CardB { get; set; }

        public DbSet<Preferance.Models.Player> Player { get; set; }
    }
}
