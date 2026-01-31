using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Webshop.Data
{
    public class WebshopContextFactory : IDesignTimeDbContextFactory<WebshopContext>
    {
        public WebshopContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebshopContext>();
            optionsBuilder.UseSqlServeroptionsBuilder.UseSqlServer("UseUserSecretsHere");

            return new WebshopContext(optionsBuilder.Options);
        }
    }
}
