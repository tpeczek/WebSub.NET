using Microsoft.EntityFrameworkCore;

namespace WebSub.AspNetCore.Services.EntityFrameworkCore
{
    /// <summary>
    /// Base class for the Entity Framework database context used for WebSub.
    /// </summary>
    public class WebSubDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public WebSubDbContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected WebSubDbContext() { }
    }
}
