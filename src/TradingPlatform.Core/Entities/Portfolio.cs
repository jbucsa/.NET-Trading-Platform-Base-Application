public class Portfolio
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public virtual ICollection<Position> Positions { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}