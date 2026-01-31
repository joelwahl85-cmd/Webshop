using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Webshop.Data;
using Webshop.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.IO.Compression;

namespace Webshop
{
    internal class Program
    {
        static List<CartItem> Cart = new();
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var connectionString = config.GetConnectionString("WebshopDb");

            var optionsBuilder = new DbContextOptionsBuilder<WebshopContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new WebshopContext(optionsBuilder.Options);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== JoelShop ===");
                Console.WriteLine("1. Visa utvalda produkter");
                Console.WriteLine("2. Gå till shoppen");
                Console.WriteLine("3. Admin");
                Console.WriteLine("4. Statistik");
                Console.WriteLine("5. Avsluta");
                Console.Write("Val: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowFeaturedProducts(context);
                        break;

                    case "2":
                        ShopMenu(context);
                        break;

                    case "3":
                        AdminMenu(context, connectionString);
                        break;

                    case "4":
                        ShowStatistics(context, connectionString);
                        break ;
                    case "5":
                        return;

                    default:
                        Console.WriteLine("Ogiltigt val");
                        Console.ReadKey();
                        break;
                }
            }
        }

       
        
        
        static void AdminMenu(WebshopContext context, string connectionString)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Admin ===");
                Console.WriteLine("1. Lista produkter");
                Console.WriteLine("2. Lägg till produkt");
                Console.WriteLine("3. Ta bort produkt");
                Console.WriteLine("4. Tillbaka");
                Console.WriteLine("5. Markera produkt som utvald");
                Console.WriteLine("6. Avmarkera produkt");
                Console.WriteLine("7. Lägg till kategori");
                Console.WriteLine("8. Statistik");

                Console.Write("Val: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ListProducts(context);
                        Console.ReadKey();
                        break;

                    case "2":
                        AddProduct(context);
                        Console.ReadKey();
                        break;

                    case "3":
                        DeleteProduct(context);
                        Console.ReadKey();
                        break;

                    case "4":
                        return;

                    case "5":
                        SetFeatured(context, true);
                        Console.ReadKey();
                        break;

                    case "6":
                        SetFeatured(context, false);
                        Console.ReadKey();
                        break;
                    case "7":
                        AddCategory(context);
                        Console.ReadKey();
                        break;
                    case "8":
                        ShowStatistics(context, connectionString);
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void ListProducts(WebshopContext context)
        {
            var products = context.Products.ToList();

            Console.WriteLine("=== Produkter ===");
            foreach (var p in products)
            {
                Console.WriteLine($"{p.ProductID}: {p.Name} - {p.Price} kr");
            }
        }

       

        static void DeleteProduct(WebshopContext context)
        {
            Console.Write("ID att ta bort: ");
            var id = int.Parse(Console.ReadLine());

            var product = context.Products.Find(id);

            if (product == null)
            {
                Console.WriteLine("Produkten finns inte.");
                return;
            }

            context.Products.Remove(product);
            context.SaveChanges();

            Console.WriteLine("Produkt borttagen!");
        }
        static void SetFeatured(WebshopContext context, bool value)
        {
            Console.Write("Produkt-ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ogiltigt ID.");
                return;
            }

            var product = context.Products.Find(id);
            if (product == null)
            {
                Console.WriteLine("Produkten finns inte.");
                return;
            }

            product.IsFeatured = value;
            context.SaveChanges();

            Console.WriteLine(value
                ? "Produkten är nu utvald!"
                : "Produkten är inte längre utvald.");
        }
        static void ShopMenu(WebshopContext context)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Shop ===");
                Console.WriteLine("1. Visa kategorier");
                Console.WriteLine("2. Sök produkt");
                Console.WriteLine("3. Visa kundkorg");
                Console.WriteLine("4. Tillbaka");
                Console.Write("Val: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowCategories(context);
                        break;

                    case "2":
                        SearchProducts(context);
                        break;

                    case "3":
                        ShowCart(context);
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("Ogiltigt val");
                        Console.ReadKey();
                        break;
                }
            }
        }
        static void ShowCategories(WebshopContext context)
        {
            Console.Clear();
            Console.WriteLine("=== Kategorier ===");

            var categories = context.Categories.ToList();

            foreach (var c in categories)
                Console.WriteLine($"{c.CategoryID}. {c.Name}");

            Console.Write("\nVälj kategori (ID): ");

            if (int.TryParse(Console.ReadLine(), out int id))
                ShowProductsInCategory(context, id);
            else
            {
                Console.WriteLine("Ogiltigt val.");
                Console.ReadKey();
            }
        }
        static void AddCategory(WebshopContext context)
        {
            Console.Write("Kategorinamn: ");
            var name = Console.ReadLine();

            var category = new Category { Name = name };
            context.Categories.Add(category);
            context.SaveChanges();

            Console.WriteLine("Kategori tillagd!");
        }
        static void AddProduct(WebshopContext context)
        {
            Console.Write("Produktnamn: ");
            var name = Console.ReadLine();

            Console.Write("Pris: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price))
            {
                Console.WriteLine("Ogiltigt prisformat.");
                return;
            }

            Console.Write("Beskrivning: ");
            var description = Console.ReadLine();

            Console.Write("Kategori-ID: ");
            if (!int.TryParse(Console.ReadLine(), out var categoryId))
            {
                Console.WriteLine("Ogiltigt kategori-ID.");
                return;
            }

            var product = new Product
            {
                Name = name,
                Price = price,
                Description = description,
                CategoryID = categoryId
            };

            context.Products.Add(product);
            context.SaveChanges();

            Console.WriteLine("Produkt tillagd!");
        }

        static void ShowProductsInCategory(WebshopContext context, int categoryId)
        {
            Console.Clear();

            var category = context.Categories.Find(categoryId);
            if (category == null)
            {
                Console.WriteLine("Kategorin finns inte.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"=== {category.Name} ===");

            var products = context.Products
                .Where(p => p.CategoryID == categoryId)
                .ToList();

            if (!products.Any())
            {
                Console.WriteLine("Inga produkter i denna kategori.");
                Console.ReadKey();
                return;
            }

            foreach (var p in products)
                Console.WriteLine($"{p.ProductID}. {p.Name} - {p.Price} kr");

            Console.Write("\nVälj produkt (ID): ");

            if (int.TryParse(Console.ReadLine(), out int id))
                ShowProductDetails(context, id);
            else
            {
                Console.WriteLine("Ogiltigt val.");
                Console.ReadKey();
            }
        }
        static void SearchProducts(WebshopContext context)
        {
            Console.Clear();
            Console.WriteLine("=== Sök produkt ===");
            Console.Write("Sök: ");
            var query = Console.ReadLine();

            var results = context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToList();

            if (!results.Any())
            {
                Console.WriteLine("Inga produkter matchade sökningen.");
                Console.ReadKey();
                return;
            }

            foreach (var p in results)
                Console.WriteLine($"{p.ProductID}. {p.Name} - {p.Price} kr");

            Console.Write("\nVälj produkt (ID): ");

            if (int.TryParse(Console.ReadLine(), out int id))
                ShowProductDetails(context, id);
        }
        static void ShowProductDetails(WebshopContext context, int productId)
        {
            Console.Clear();

            var product = context.Products.Find(productId);
            if (product == null)
            {
                Console.WriteLine("Produkten finns inte.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"=== {product.Name} ===");
            Console.WriteLine($"Pris: {product.Price} kr");
            Console.WriteLine($"Beskrivning: {product.Description}");
            Console.WriteLine();
            Console.WriteLine("1. Köp");
            Console.WriteLine("2. Tillbaka");
            Console.Write("Val: ");

            var choice = Console.ReadLine();

            if (choice == "1")
            {
                AddToCart(context, productId);
                Console.WriteLine("Produkten lades i kundkorgen!");
                Console.ReadKey();
            }
        }
        static void ShowFeaturedProducts(WebshopContext context)
        {
            Console.Clear();
            Console.WriteLine("=== Utvalda produkter ===");

            var featured = context.Products
                .Where(p => p.IsFeatured)
                .Take(3)
                .ToList();

            if (!featured.Any())
            {
                Console.WriteLine("Inga utvalda produkter ännu.");
            }
            else
            {
                foreach (var p in featured)
                    Console.WriteLine($"{p.Name} - {p.Price} kr");
            }

            Console.WriteLine("\nTryck valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }
        static void AddToCart(WebshopContext context, int productId)
        {
            // Hämta produkten från databasen
            var product = context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                Console.WriteLine("Produkten finns inte.");
                return;
            }

            // Finns den redan i varukorgen?
            var item = Cart.FirstOrDefault(c => c.Product.ProductID == productId);

            if (item == null)
            {
                Cart.Add(new CartItem
                {
                    Product = product,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            Console.WriteLine($"{product.Name} lades till i varukorgen.");
        }

        static void ShowCart(WebshopContext context)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kundkorg ===");

                if (!Cart.Any())
                {
                    Console.WriteLine("Kundkorgen är tom.");
                    Console.ReadKey();
                    return;
                }

                int index = 1;
                foreach (var item in Cart)
                {
                    Console.WriteLine($"{index}. {item.Product.Name} - {item.Quantity} st - {item.Product.Price} kr");
                    index++;
                }

                Console.WriteLine("\n1. Ändra antal");
                Console.WriteLine("2. Ta bort produkt");
                Console.WriteLine("3. Gå till frakt");
                Console.WriteLine("4. Tillbaka");

                Console.Write("Val: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ChangeQuantity(); break;
                    case "2": RemoveFromCart(); break;
                    case "3": ShippingView(context); break;
                    case "4": return;
                }
            }
        }
        static void ChangeQuantity()
        {
            Console.Write("Vilken produkt vill du ändra? (nummer): ");
            if (!int.TryParse(Console.ReadLine(), out int index)) return;

            index--;

            if (index < 0 || index >= Cart.Count) return;

            Console.Write("Nytt antal: ");
            if (!int.TryParse(Console.ReadLine(), out int qty)) return;

            if (qty <= 0) Cart.RemoveAt(index);
            else Cart[index].Quantity = qty;
        }
        static void RemoveFromCart()
        {
            Console.Write("Vilken produkt vill du ta bort? (nummer): ");
            if (!int.TryParse(Console.ReadLine(), out int index)) return;

            index--;

            if (index < 0 || index >= Cart.Count) return;

            Cart.RemoveAt(index);
        }
        static void ShippingView(WebshopContext context)
        {
            Console.Clear();
            Console.WriteLine("=== Frakt ===");

            Console.Write("Namn: ");
            string name = Console.ReadLine();

            Console.Write("Adress: ");
            string address = Console.ReadLine();

            Console.Write("Stad: ");
            string city = Console.ReadLine();

            Console.Write("Postnummer: ");
            string zip = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Telefonnummer: ");
            string phone = Console.ReadLine();

            Console.WriteLine("\nVälj frakt:");
            Console.WriteLine("1. Standard (49 kr)");
            Console.WriteLine("2. Express (99 kr)");

            Console.Write("Val: ");
            var choice = Console.ReadLine();

            decimal shipping = choice == "2" ? 99 : 49;

            PaymentView(context, name, address, city, zip, email, phone, shipping);
        }
        static void PaymentView(WebshopContext context, string name, string address, string city, string email, string phone, string zip,  decimal shipping)
        {
            Console.Clear();
            Console.WriteLine("=== Betalning ===");

            decimal subtotal = Cart.Sum(i => i.Product.Price * i.Quantity);
            decimal moms = subtotal * 0.25m;
            decimal total = subtotal + moms + shipping;

            Console.WriteLine($"Namn: {name}");
            Console.WriteLine($"Adress: {address}\n");
            Console.WriteLine($"Stad: {city}");



            Console.WriteLine("Produkter:");
            foreach (var item in Cart)
                Console.WriteLine($"{item.Product.Name} - {item.Quantity} st - {item.Product.Price} kr");

            Console.WriteLine($"\nDelsumma: {subtotal} kr");
            Console.WriteLine($"Moms (25%): {moms} kr");
            Console.WriteLine($"Frakt: {shipping} kr");
            Console.WriteLine($"Totalt att betala: {total} kr");

            Console.WriteLine("\nVälj betalningsmetod:");
            Console.WriteLine("1. Kort");
            Console.WriteLine("2. Swish");

            Console.Write("Val: ");
            Console.ReadLine();

            // ⭐ Skapa kund
            var customer = new Customer
            {
                Name = name,
                Address = address,
                City = city,
                ZipCode = zip,
                Email = email,
                Phone = phone
            };

            context.Customers.Add(customer);
            context.SaveChanges();
            // ⭐ Skapa order
            var order = new Order
            {
                CustomerID = customer.CustomerID, // ingen kundinloggning
                Date = DateTime.Now,
                ShippingType = shipping == 99 ? "Express" : "Standard",
                ShippingPrice = shipping,
                VAT = moms,
                TotalPrice = total,
                OrderRows = new List<OrderRow>()
            };

            // ⭐ Skapa orderrader
            foreach (var item in Cart)
            {
                order.OrderRows.Add(new OrderRow
                {
                    ProductID = item.Product.ProductID,
                    Quantity = item.Quantity,
                    PriceEach = item.Product.Price
                });
            }

            // ⭐ Spara i databasen
            try
            {
                context.Orders.Add(order);
                context.SaveChanges();

                Console.WriteLine("\nBetalning genomförd!");
                Cart.Clear();
                Console.WriteLine("Varukorgen är nu tömd.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nEtt fel uppstod vid betalningen.");
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();

        }
        static void ShowStatistics(WebshopContext context, string connectionString)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Statistik ===");
                Console.WriteLine("1. Bäst säljande produkter");
                Console.WriteLine("2. Totala intäkter (EF)");
                Console.WriteLine("3. Mest aktiva kunder");
                Console.WriteLine("4. Lågt lagersaldo");
                Console.WriteLine("5. Visar ordrar (async demo)");
                Console.WriteLine("6. Totala intäkter (Dapper)");
                Console.WriteLine("7. Tillbaka");

                Console.Write("Val: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": BestSellingProducts(context); break;
                    case "2": TotalRevenue(context); break;
                    case "3": MostActiveCustomers(context); break;
                    case "4": LowStock(context); break;
                    case "5": ShowOrdersAsync(context).Wait(); Console.ReadKey(); break;

                    //  Dapper här
                    case "6": TotalRevenueDapper(connectionString); break;

                    case "7": return;
                }
            }
        }
        static void BestSellingProducts(WebshopContext context)
        {
            var result = context.OrderRows
                .GroupBy(r => r.Product.Name)
                .Select(g => new { Product = g.Key, Sold = g.Sum(r => r.Quantity) })
                .OrderByDescending(x => x.Sold)
                .ToList();

            Console.Clear();
            Console.WriteLine("=== Bäst säljande produkter ===");

            foreach (var item in result)
                Console.WriteLine($"{item.Product}: {item.Sold} st");

            Console.ReadKey();
        }
        static void TotalRevenue(WebshopContext context)
        {
            var revenue = context.Orders.Sum(o => o.TotalPrice);

            Console.Clear();
            Console.WriteLine("=== Totala intäkter ===");
            Console.WriteLine($"{revenue} kr");
            Console.ReadKey();
        }
        static void MostActiveCustomers(WebshopContext context)
        {
            Console.Clear();
            Console.WriteLine("=== Mest aktiva kunder ===");

            var stats = context.Orders
                .Include(o => o.Customer)
                .GroupBy(o => o.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(x => x.OrderCount)
                .ToList();

            foreach (var s in stats)
            {
                Console.WriteLine($"{s.CustomerName}: {s.OrderCount} ordrar, {s.TotalSpent} kr");
            }

            Console.ReadKey();
        }
        static void LowStock(WebshopContext context)
        {
            var result = context.Products
                .Where(p => p.Stock < 5)
                .ToList();

            Console.Clear();
            Console.WriteLine("=== Lågt lagersaldo (<5) ===");

            foreach (var p in result)
                Console.WriteLine($"{p.Name}: {p.Stock} kvar");

            Console.ReadKey();
        }
        static async Task ShowOrdersAsync(WebshopContext context)
        {
            var sw = Stopwatch.StartNew();

            var orders = await context.Orders
                .Include(o => o.OrderRows)
                .ToListAsync();

            sw.Stop();

            Console.WriteLine($"Hämtade {orders.Count} ordrar på {sw.ElapsedMilliseconds} ms\n");

            foreach (var o in orders)
                Console.WriteLine($"Order {o.OrderID} - {o.TotalPrice} kr - {o.Date}");
        }
        static void TotalRevenueDapper(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);

                var sql = "SELECT SUM(TotalPrice) FROM Orders";

                var revenue = connection.ExecuteScalar<decimal?>(sql) ?? 0;

                Console.WriteLine($"\nTotala intäkter (SQL/Dapper): {revenue} kr");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kunde inte hämta data via Dapper.");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\nTryck valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
        

    }
}
