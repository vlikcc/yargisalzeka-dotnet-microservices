using Microsoft.EntityFrameworkCore;

namespace SubscriptionService
{
    public class SubscriptionDbContext : DbContext
    {
        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options) { }
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    }
}
