public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Portfolio> Portfolios { get; set; }
}