using System.Text.Json;
using gameHubBack.Entities;
using gameHubBack.Entities.BatalhaRural;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace gameHubBack.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<BatalhaRuralGame> BatalhaRuralGames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BatalhaRuralGame>()
            .HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId);

        var boardTilesComparer = new ValueComparer<string[][]>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => JsonSerializer.Deserialize<string[][]>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);

        var tokensComparer = new ValueComparer<List<GameToken>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => JsonSerializer.Deserialize<List<GameToken>>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);

        modelBuilder.Entity<BatalhaRuralPlayer>()
            .Property(p => p.BoardTiles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<string[][]>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(boardTilesComparer);

        modelBuilder.Entity<BatalhaRuralPlayer>()
            .Property(p => p.Tokens)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<GameToken>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(tokensComparer);
    }
}
