namespace NHS111.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class NHS111DbContextFactory : IDesignTimeDbContextFactory<NHS111DbContext>
{
    public NHS111DbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NHS111DbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=nhs111;Username=postgres;Password=1234")
                      .UseSnakeCaseNamingConvention();

        return new NHS111DbContext(optionsBuilder.Options);
    }
}
