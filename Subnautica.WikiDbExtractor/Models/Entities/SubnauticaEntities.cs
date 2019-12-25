using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models.Entities
{
    public class SubnauticaEntities : DbContext
    {

        public DbSet<Language> Languages { get; set; }
        public DbSet<LanguageKey> LanguageKeys { get; set; }
        public DbSet<LanguageText> LanguageTexts { get; set; }


        public SubnauticaEntities(DbContextOptions options) : base(options)
        {
        }

        public SubnauticaEntities()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=data.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LanguageKey>(entity =>
            {
                entity.HasIndex(q => q.Key);
            });

            builder.Entity<LanguageText>(entity =>
            {
                entity.HasIndex(q => new { q.LanguageId, q.LanguageKeyId, }).IsUnique();
            });


        }

    }
}
