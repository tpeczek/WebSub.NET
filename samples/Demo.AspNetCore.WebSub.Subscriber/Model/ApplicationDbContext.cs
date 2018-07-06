using Microsoft.EntityFrameworkCore;
using WebSub.AspNetCore.Services.EntityFrameworkCore;

namespace Demo.AspNetCore.WebSub.Subscriber.Model
{
    internal class ApplicationDbContext : WebSubDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }
    }
}
