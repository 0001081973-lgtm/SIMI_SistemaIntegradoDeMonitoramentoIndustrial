using Microsoft.EntityFrameworkCore;
using Shared;

namespace ApiProcessamento.Data
{
    /// <summary>
    /// Contexto do banco de dados SQLite para persistência dos dados dos sensores.
    /// </summary>
    public class SensorDbContext : DbContext
    {
        public SensorDbContext(DbContextOptions<SensorDbContext> options) : base(options) { }

        /// <summary>Tabela de registros de sensores.</summary>
        public DbSet<SensorData> Sensores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Temperatura).IsRequired();
                entity.Property(e => e.Pressao).IsRequired();
                entity.Property(e => e.Umidade).IsRequired();
                entity.Property(e => e.Vibracao).IsRequired();
                entity.Property(e => e.Origem).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });
        }
    }
}
