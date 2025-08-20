using Microsoft.EntityFrameworkCore;
using SearchService.Entities;

namespace SearchService.DbContexts;

public class SearchDbContext : DbContext
{
	public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
	{
	}

	public DbSet<Decision> Decisions => Set<Decision>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Decision>(entity =>
		{
			entity.HasKey(d => d.Id);
			entity.ToTable("kararlar");
			entity.Property(d => d.Id).HasColumnName("id");
			entity.Property(d => d.YargitayDairesi).HasColumnName("yargitay_dairesi").HasMaxLength(200);
			entity.Property(d => d.EsasNo).HasColumnName("esas_no").HasMaxLength(100);
			entity.Property(d => d.KararNo).HasColumnName("karar_no").HasMaxLength(100);
			entity.Property(d => d.KararTarihi).HasColumnName("karar_tarihi");
			entity.Property(d => d.KararMetni).HasColumnName("karar_metni").IsRequired();
		});
	}
}


