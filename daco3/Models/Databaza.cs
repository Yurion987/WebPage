namespace daco3.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Databaza : DbContext
    {
        public DbSet<Rola> Role { get; set; }
        public DbSet<Uzivatel> Uzivatelia { get; set; }
        public DbSet<Zaznam> Zaznami { get; set; }
        public DbSet<Log> Logy { get; set; }
        
        public Databaza()
            : base("name=bakalarkaConnStr")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Uzivatel>()
              .HasIndex(x => x.Username)
              .IsUnique();
            base.OnModelCreating(modelBuilder);
        }
    }
}
