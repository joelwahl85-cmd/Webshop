using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Webshop.Data
{
    public class WebshopContextFactory : IDesignTimeDbContextFactory<WebshopContext>
    {
        public WebshopContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebshopContext>();
            optionsBuilder.UseSqlServer("Server=tcp:joelsdb.database.windows.net,1433;Initial Catalog=JoelsDb;Persist Security Info=False;User ID=dbadmin;Password=Nyköping85;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new WebshopContext(optionsBuilder.Options);
        }
    }
}
