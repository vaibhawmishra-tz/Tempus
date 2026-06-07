using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Tempus.EFCore.Extensions;

namespace Tempus.Integration.Tests;

/// <summary>
/// EF Core + SQL Server round-trip tests. Require Docker and a running
/// SQL Server container via Testcontainers. Skipped automatically when
/// the SKIP_INTEGRATION_TESTS environment variable is set.
/// </summary>
[Trait("Category", "Integration")]
public class EFCoreSqlServerIntegrationTests : IAsyncLifetime
{
    private MsSqlContainer _container = null!;
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("SKIP_INTEGRATION_TESTS") is not null)
            return;

        try
        {
            _container = new MsSqlBuilder().Build();
            await _container.StartAsync().ConfigureAwait(false);
            _connectionString = _container.GetConnectionString();
        }
        catch (ArgumentException)
        {
            // Docker not available — tests will be silently skipped
            _container = null!;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync().ConfigureAwait(false);
    }

    // ── shared entity / context ───────────────────────────────────────────────

    private sealed class TemporalEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateOnly BirthDate { get; set; }
        public TimeOnly MeetingTime { get; set; }
    }

    private sealed class SqlServerDbContext : DbContext
    {
        private readonly string _cs;
        public DbSet<TemporalEntity> Entities { get; set; } = null!;

        public SqlServerDbContext(string connectionString) => _cs = connectionString;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseTemporalConventions();
            modelBuilder.Entity<TemporalEntity>().HasKey(e => e.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_cs);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EnforceUtcDateTimes_DateTime_StoredAsUtc()
    {
        if (_container is null) return; // skipped

        await using var ctx = new SqlServerDbContext(_connectionString);
        await ctx.Database.EnsureCreatedAsync();

        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = new DateTime(2026, 5, 27, 9, 0, 0, DateTimeKind.Local).ToUniversalTime(),
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = new DateOnly(2000, 1, 1),
            MeetingTime = new TimeOnly(9, 0)
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task UseTemporalConventions_DateOnly_RoundTrips_SqlServer()
    {
        if (_container is null) return; // skipped

        await using var ctx = new SqlServerDbContext(_connectionString);
        await ctx.Database.EnsureCreatedAsync();

        var original = new DateOnly(2026, 5, 27);
        ctx.Entities.Add(new TemporalEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            BirthDate = original,
            MeetingTime = new TimeOnly(10, 30)
        });
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var entity = await ctx.Entities.FirstAsync();
        entity.BirthDate.Should().Be(original);
    }

    [Fact]
    public async Task UseTemporalConventions_DateTimeOffset_StoredAsUtc_SqlServer()
    {
        if (_container is null) return; // skipped

        await using var ctx = new SqlServerDbContext(_connectionString);
        await ctx.Database.EnsureCreatedAsync();

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
}
