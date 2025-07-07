public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Price).HasColumnType("decimal(18,8)");
        builder.Property(o => o.Quantity).HasColumnType("decimal(18,8)");
        builder.HasIndex(o => o.OrderId).IsUnique();
        builder.HasIndex(o => o.InstrumentSymbol);
    }
}