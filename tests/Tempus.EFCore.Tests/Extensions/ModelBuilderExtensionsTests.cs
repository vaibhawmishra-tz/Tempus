using Microsoft.EntityFrameworkCore;
using Tempus.EFCore.Extensions;

namespace Tempus.EFCore.Tests.Extensions;

public class ModelBuilderExtensionsTests
{
    // ── test entity ────────────────────────────────────────────────────────────

    private sealed class TemporalEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateOnly BirthDate { get; set; }
        public TimeOnly MeetingTime { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateOnly? ExpiresOn { get; set; }
        public TimeOnly? CloseTime { get; set; }
    }

    // ── DbContexts ────────────────────────────────────────────────────────────

    private sealed class EnforceUtcContext : DbContext
    {
        public DbSet<TemporalEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.EnforceUtcDateTimes();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase($"EnforceUtc_{Guid.NewGuid()}");
    }

    private sealed class TemporalConventionsContext : DbContext
    {
        public DbSet<TemporalEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.UseTemporalConventions();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase($"Temporal_{Guid.NewGuid()}");
    }

    // ── EnforceUtcDateTimes ────────────────────────────────────────────────────

    [Fact]
    public async Task EnforceUtcDateTimes_StoredDateTime_IsUtcKind()
    {
        await using var ctx = new EnforceUtcContext();
        var localNow = DateTime.SpecifyKind(new DateTime(2026, 5, 27, 9, 0, 0), DateTimeKind.Local);

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = localNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(1990, 1, 1),
            MeetingTime = new TimeOnly(10, 0)
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task EnforceUtcDateTimes_NullableDateTime_RoundTrips()
    {
        await using var ctx = new EnforceUtcContext();
        var deletedAt = new DateTime(2026, 5, 27, 12, 0, 0, DateTimeKind.Utc);

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(1990, 1, 1),
            MeetingTime = new TimeOnly(10, 0),
            DeletedAt = deletedAt
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.DeletedAt.Should().NotBeNull();
        entity.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        entity.DeletedAt!.Value.Should().Be(deletedAt);
    }

    // ── UseTemporalConventions ─────────────────────────────────────────────────

    [Fact]
    public async Task UseTemporalConventions_DateOnly_RoundTrips()
    {
        await using var ctx = new TemporalConventionsContext();
        var original = new DateOnly(2026, 5, 27);

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = original,
            MeetingTime = new TimeOnly(10, 0)
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.BirthDate.Should().Be(original);
    }

    [Fact]
    public async Task UseTemporalConventions_TimeOnly_RoundTrips()
    {
        await using var ctx = new TemporalConventionsContext();
        var original = new TimeOnly(14, 30, 45);

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(2026, 1, 1),
            MeetingTime = original
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.MeetingTime.Should().Be(original);
    }

    [Fact]
    public async Task UseTemporalConventions_DateTimeOffset_StoredAsUtc()
    {
        await using var ctx = new TemporalConventionsContext();
        var eastern = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.FromHours(-4));

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = eastern,
            BirthDate = new DateOnly(2026, 1, 1),
            MeetingTime = new TimeOnly(9, 0)
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.UpdatedAt.Offset.Should().Be(TimeSpan.Zero);
        entity.UpdatedAt.UtcDateTime.Hour.Should().Be(13); // 09:00 EDT = 13:00 UTC
    }

    [Fact]
    public async Task UseTemporalConventions_NullableDateOnly_RoundTrips()
    {
        await using var ctx = new TemporalConventionsContext();
        var original = new DateOnly(2027, 12, 31);

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(2026, 1, 1),
            MeetingTime = new TimeOnly(9, 0),
            ExpiresOn = original
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.ExpiresOn.Should().Be(original);
    }

    [Fact]
    public async Task UseTemporalConventions_NullableDateOnly_Null_RoundTrips()
    {
        await using var ctx = new TemporalConventionsContext();

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(2026, 1, 1),
            MeetingTime = new TimeOnly(9, 0),
            ExpiresOn = null
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.ExpiresOn.Should().BeNull();
    }
}
