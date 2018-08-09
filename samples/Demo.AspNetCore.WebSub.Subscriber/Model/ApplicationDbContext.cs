using Microsoft.EntityFrameworkCore;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Services.EntityFrameworkCore;

namespace Demo.AspNetCore.WebSub.Subscriber.Model
{
    internal class ApplicationDbContext : WebSubDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }
    }
}
