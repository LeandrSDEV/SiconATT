using Microsoft.EntityFrameworkCore;
using Servidor.Models;

namespace Servidor.Data
{
    public class BancoContext : DbContext
    {
        public BancoContext(DbContextOptions<BancoContext> options) : base(options) { }

        public DbSet<ContrachequeModel> Contracheque { get; set; }

        public DbSet<AdministrativoModel> Administrativo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContrachequeModel>().ToTable("Contracheque");
            modelBuilder.Entity<AdministrativoModel>().ToTable("Administrativo");
 
        }
    }
}
