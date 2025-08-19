namespace SubscriptionService
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RemainingCredits { get; set; }
    }
}
