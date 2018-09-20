using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSub.WebHooks.Receivers.Subscriber;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Services.EntityFrameworkCore
{
    /// <summary>
    /// Base class for the Entity Framework database context used for WebSub.
    /// </summary>
    public class WebSubDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="WebSubSubscription"/>.
        /// </summary>
        public DbSet<WebSubSubscription> Subscriptions { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public WebSubDbContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected WebSubDbContext() { }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<WebSubSubscription> subscriptionEntityTypeBuilder = modelBuilder.Entity<WebSubSubscription>();

            subscriptionEntityTypeBuilder.HasKey(e => e.Id);
            subscriptionEntityTypeBuilder.Property(e => e.Id).ValueGeneratedNever();

            subscriptionEntityTypeBuilder.HasIndex(e => e.CallbackUrl).IsUnique();
        }
    }
}
