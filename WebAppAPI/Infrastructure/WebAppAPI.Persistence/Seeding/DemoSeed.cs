using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text.Json;
using WebAppAPI.Application.Consts;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Entities.Common;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Domain.Enums;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Seeding
{
    public static class DemoSeed
    {
        public static async Task CheckAndSeedAsync(IServiceProvider services, UserManager<AppUser> userManager)
        {
            try
            {
                if (await userManager.Users.AnyAsync()) return;
            }
            catch
            {
                Console.WriteLine("Database schema is not ready. Applying migrations and seeding demo data...");
            }

            await SeedAllAsync(services);
        }

        public static async Task SeedAllAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var db = sp.GetRequiredService<WebAppAPIDbContext>();
            var roleManager = sp.GetRequiredService<RoleManager<AppRole>>();
            var userManager = sp.GetRequiredService<UserManager<AppUser>>();

            // 1. Migration
            await db.Database.MigrateAsync();

            // 2. ROLES
            await roleManager.CreateAsync(new AppRole
            {
                Id = SystemBootstrapConstants.SystemAdministratorRoleId,
                Name = SystemBootstrapConstants.SystemAdministratorRoleName,
                IsAdmin = true,
                DateCreated = DemoDate(3, 10, 9, 0)
            });
            await roleManager.CreateAsync(new AppRole
            {
                Id = "6745d85c-c3dd-41fd-8137-83471d3c7fd6",
                Name = "StoreManager",
                IsAdmin = true,
                DateCreated = DemoDate(3, 10, 9, 7)
            });
            await roleManager.CreateAsync(new AppRole
            {
                Id = "46006b7d-b6be-4365-92f3-8959d1a52fdb",
                Name = "Customer",
                IsAdmin = false,
                DateCreated = DemoDate(3, 10, 9, 14)
            });

            // 3. USERS
            var systemAdministrator = new AppUser
            {
                Id = SystemBootstrapConstants.SystemAdministratorUserId,
                FirstName = "Eric",
                LastName = "Cartman",
                FullName = "Eric Cartman",
                UserName = "ericadmin",
                Email = "eric.cartman@demo.local",
                PhoneNumber = "+15550101001",
                DateCreated = DemoDate(3, 10, 9, 35)
            };
            await userManager.CreateAsync(systemAdministrator, "123");
            await userManager.AddToRoleAsync(systemAdministrator, SystemBootstrapConstants.SystemAdministratorRoleName);

            var storeManager = new AppUser
            {
                Id = "7139af9e-f258-4fcf-939c-c7b43eaf3852",
                FirstName = "James",
                LastName = "Hetfield",
                FullName = "James Hetfield",
                UserName = "jameshet",
                PhoneNumber = "+15550101002",
                Email = "sandman@demo.local",
                DateCreated = DemoDate(3, 10, 9, 42)
            };
            await userManager.CreateAsync(storeManager, "123");
            await userManager.AddToRoleAsync(storeManager, "StoreManager");

            var customer = new AppUser
            {
                Id = "8bb8acee-0529-434c-a7b3-022dc5466860",
                FirstName = "Butters",
                LastName = "Stotch",
                FullName = "Butters Stotch",
                UserName = "butters",
                PhoneNumber = "+15550101003",
                Email = "lululu@demo.local",
                DateCreated = DemoDate(3, 10, 9, 49)
            };
            await userManager.CreateAsync(customer, "123");
            await userManager.AddToRoleAsync(customer, "Customer");

            await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""AspNetUsers""
                SET ""DateUpdated"" = CASE ""Id""
                    WHEN {systemAdministrator.Id} THEN {systemAdministrator.DateCreated!.Value.AddMinutes(1)}
                    WHEN {storeManager.Id} THEN {storeManager.DateCreated!.Value.AddMinutes(1)}
                    WHEN {customer.Id} THEN {customer.DateCreated!.Value.AddMinutes(1)}
                END
                WHERE ""Id"" IN ({systemAdministrator.Id}, {storeManager.Id}, {customer.Id})");

            await db.SaveChangesAsync();

            // 4. PRODUCTS (read from json)
            var productsJsonPath = Path.GetFullPath(Path.Combine(
                                       Directory.GetCurrentDirectory(),
                                       "..", "..",
                                       "Infrastructure", "WebAppAPI.Persistence", "Seeding", "SeedData", "products.json"
                                   ));

            var jsonText = await System.IO.File.ReadAllTextAsync(productsJsonPath);

            var raw = JsonSerializer.Deserialize<List<ProductJson>>(
                jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Product seed data could not be deserialized.");

            var products = raw.Select((j, index) =>
            {
                var productId = Guid.Parse(j.id);
                var dateCreated = GetDemoProductDateCreated(productId, index);

                return new Product
                {
                    Id = productId,
                    Name = j.name,
                    Title = j.title,
                    Description = j.description,
                    Price = j.price,
                    Stock = j.stock,
                    Rating = j.rating,
                    DateCreated = dateCreated,
                    DateUpdated = dateCreated
                };
            }).ToList();

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();

            // 5. price DESC top 10
            var topProducts = products.Where(p => p.Stock > 0).OrderByDescending(p => p.Price).Take(10).ToList();

            // 6. ORDERS + Basket + BasketItems + StatusHistory
            var now = DemoDate(5, 1, 10, 30);

            // 6.1 Cancelled (1 item)  [Pending -> Cancelled]
            var o1 = Guid.Parse("c060467e-5452-49ea-8620-1054992a0d09");
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o1, UserId = customer.Id }, now.AddMinutes(-66), now.AddMinutes(-62)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o1, Address = "Istanbul", Description = "Order #1 Cancelled", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Cancelled }, now.AddMinutes(-61), now.AddMinutes(-55)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("415628d3-91b1-4c46-a473-77e8951db845"), BasketId = o1, ProductId = topProducts[0].Id, Quantity = 2 }, now.AddMinutes(-64)),
                WithCreated(new BasketItem { Id = Guid.Parse("b590beea-1627-4260-85bb-9aca21573663"), BasketId = o1, ProductId = topProducts[1].Id, Quantity = 1 }, now.AddMinutes(-63)),
                WithCreated(new BasketItem { Id = Guid.Parse("77d89c03-3ac0-4c3b-af0a-4bc84df236cd"), BasketId = o1, ProductId = topProducts[2].Id, Quantity = 1 }, now.AddMinutes(-62))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("a38afbc4-79c1-401b-934d-5758303e91a4"), OrderId=o1, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,   ChangedDate=now.AddMinutes(-61) }, now.AddMinutes(-61)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("ba7bd2be-15c7-4940-aa82-ecdabc9f54dc"), OrderId=o1, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Cancelled, ChangedDate=now.AddMinutes(-55) }, now.AddMinutes(-55))
            });
            await db.SaveChangesAsync();

            // 6.2 Pending (2 items) [Pending]
            var o2 = Guid.Parse("ccdd7347-aa69-4191-a6e6-758f2cc81f59");
            now = DemoDate(5, 3, 12, 30);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o2, UserId = customer.Id }, now.AddMinutes(-54), now.AddMinutes(-50)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreated(new Order { Id = o2, Address = "Istanbul", Description = "Order #2 Pending", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Pending }, now.AddMinutes(-46)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("e2222e09-b5ac-4da3-b32d-42988d3cf212"), BasketId = o2, ProductId = topProducts[3].Id, Quantity = 3 }, now.AddMinutes(-52)),
                WithCreated(new BasketItem { Id = Guid.Parse("c44a1d79-d530-48fa-ba4a-b03ed3617519"), BasketId = o2, ProductId = topProducts[4].Id, Quantity = 2 }, now.AddMinutes(-51)),
                WithCreated(new BasketItem { Id = Guid.Parse("7c394347-0781-455c-a33e-60d502e90114"), BasketId = o2, ProductId = topProducts[5].Id, Quantity = 1 }, now.AddMinutes(-50))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddAsync(WithCreated(new OrderStatusHistory { Id = Guid.Parse("6995f7d8-e556-45c1-ae08-79a6ae63c049"), OrderId = o2, PreviousStatusId = (int)OrderStatusEnum.Pending, NewStatusId = (int)OrderStatusEnum.Pending, ChangedDate = now.AddMinutes(-46) }, now.AddMinutes(-46)));
            await db.SaveChangesAsync();

            var o3 = Guid.Parse("d52123cb-bd6c-414e-bdf4-4e21d54e2c68");
            now = DemoDate(5, 5, 14, 15);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o3, UserId = customer.Id }, now.AddMinutes(-49), now.AddMinutes(-45)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreated(new Order { Id = o3, Address = "Istanbul", Description = "Order #3 Pending", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Pending }, now.AddMinutes(-41)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("c662b5a4-308d-453b-a493-38be26104ba5"), BasketId = o3, ProductId = topProducts[5].Id, Quantity = 2 }, now.AddMinutes(-47)),
                WithCreated(new BasketItem { Id = Guid.Parse("543ab905-42a0-41b4-9f91-122b69920a6e"), BasketId = o3, ProductId = topProducts[2].Id, Quantity = 2 }, now.AddMinutes(-46)),
                WithCreated(new BasketItem { Id = Guid.Parse("a8835259-1b42-440c-8268-4dfba74082eb"), BasketId = o3, ProductId = topProducts[1].Id, Quantity = 1 }, now.AddMinutes(-45))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddAsync(WithCreated(new OrderStatusHistory { Id = Guid.Parse("61e2628b-b568-4ad8-a2e7-974a2ce600eb"), OrderId = o3, PreviousStatusId = (int)OrderStatusEnum.Pending, NewStatusId = (int)OrderStatusEnum.Pending, ChangedDate = now.AddMinutes(-41) }, now.AddMinutes(-41)));
            await db.SaveChangesAsync();

            // 6.3 Approved (3 items) [Pending -> Approved]
            var o4 = Guid.Parse("f4658de0-f744-4734-90f0-3be73b852329");
            now = DemoDate(5, 8, 10, 5);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o4, UserId = customer.Id }, now.AddMinutes(-44), now.AddMinutes(-40)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o4, Address = "Istanbul", Description = "Order #4 Approved", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Approved }, now.AddMinutes(-36), now.AddMinutes(-29)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("34b804d5-3df0-48bc-90cb-1c703d01cb61"), BasketId = o4, ProductId = topProducts[4].Id, Quantity = 1 }, now.AddMinutes(-42)),
                WithCreated(new BasketItem { Id = Guid.Parse("3f15d554-0a57-4eda-bb22-fa40c7e38963"), BasketId = o4, ProductId = topProducts[0].Id, Quantity = 1 }, now.AddMinutes(-41)),
                WithCreated(new BasketItem { Id = Guid.Parse("7086d865-1d2a-4336-8abc-44d442cd46ca"), BasketId = o4, ProductId = topProducts[1].Id, Quantity = 2 }, now.AddMinutes(-40))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("4032646a-ca55-49cb-9ae2-9b22873fa542"), OrderId=o4, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending, ChangedDate=now.AddMinutes(-36) }, now.AddMinutes(-36)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("e2361a4a-aa34-4c43-98f2-9822017a7fac"), OrderId=o4, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-29) }, now.AddMinutes(-29))
            });
            await db.SaveChangesAsync();

            var o5 = Guid.Parse("bd6b6893-32be-4566-9211-a52919824a03");
            now = DemoDate(5, 10, 16, 20);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o5, UserId = customer.Id }, now.AddMinutes(-34), now.AddMinutes(-30)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o5, Address = "Istanbul", Description = "Order #5 Approved", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Approved }, now.AddMinutes(-26), now.AddMinutes(-19)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("1ba30527-3daf-40ec-b70b-114d957741ee"), BasketId = o5, ProductId = topProducts[2].Id, Quantity = 1 }, now.AddMinutes(-32)),
                WithCreated(new BasketItem { Id = Guid.Parse("33e13025-58ee-444f-931d-f5f0677a09be"), BasketId = o5, ProductId = topProducts[3].Id, Quantity = 1 }, now.AddMinutes(-31)),
                WithCreated(new BasketItem { Id = Guid.Parse("1c0f32cf-fa5f-44ae-baa6-57cdffb4f5b2"), BasketId = o5, ProductId = topProducts[4].Id, Quantity = 2 }, now.AddMinutes(-30))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("67f9affb-1abd-4a31-9a24-cd102ab3dc2e"), OrderId=o5, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending, ChangedDate=now.AddMinutes(-26) }, now.AddMinutes(-26)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("8ff8d3c4-2965-4e57-8245-96021313526f"), OrderId=o5, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-19) }, now.AddMinutes(-19))
            });
            await db.SaveChangesAsync();

            var o6 = Guid.Parse("abad5f2b-6954-48bd-8cfd-25986ddec6a4");
            now = DemoDate(5, 12, 9, 45);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o6, UserId = customer.Id }, now.AddMinutes(-28), now.AddMinutes(-24)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o6, Address = "Istanbul", Description = "Order #6 Approved", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Approved }, now.AddMinutes(-20), now.AddMinutes(-13)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("fabed2fb-2f95-44ca-a051-17cd86cbfba0"), BasketId = o6, ProductId = topProducts[5].Id, Quantity = 2 }, now.AddMinutes(-26)),
                WithCreated(new BasketItem { Id = Guid.Parse("a17c1a5a-6886-4253-bb22-7809b5250c90"), BasketId = o6, ProductId = topProducts[6].Id, Quantity = 1 }, now.AddMinutes(-25)),
                WithCreated(new BasketItem { Id = Guid.Parse("3c2c0be2-5c39-4e87-87d8-4a618242da65"), BasketId = o6, ProductId = topProducts[7].Id, Quantity = 1 }, now.AddMinutes(-24))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("ce99456c-714f-4d39-89c0-485c591cb744"), OrderId=o6, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending, ChangedDate=now.AddMinutes(-20) }, now.AddMinutes(-20)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("67ad55f2-8f22-43d5-b81e-30e885cc8b8d"), OrderId=o6, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-13) }, now.AddMinutes(-13))
            });
            await db.SaveChangesAsync();

            // 6.4 Shipping (2 items) [Pending -> Approved -> Shipping]
            var o7 = Guid.Parse("4e24f327-e217-4271-940b-d6a8b825e147");
            now = DemoDate(5, 15, 11, 30);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o7, UserId = customer.Id }, now.AddMinutes(-22), now.AddMinutes(-18)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o7, Address = "Istanbul", Description = "Order #7 Shipping", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Shipping }, now.AddMinutes(-14), now.AddMinutes(-3)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("9a07f124-aa4c-446f-b990-9b769a2aae55"), BasketId = o7, ProductId = topProducts[8].Id, Quantity = 2 }, now.AddMinutes(-20)),
                WithCreated(new BasketItem { Id = Guid.Parse("d7a5719d-17c9-42ea-8e70-c801bc9a0d52"), BasketId = o7, ProductId = topProducts[9].Id, Quantity = 2 }, now.AddMinutes(-19)),
                WithCreated(new BasketItem { Id = Guid.Parse("3a360769-d99f-4859-9b55-3149905bd7a4"), BasketId = o7, ProductId = topProducts[0].Id, Quantity = 1 }, now.AddMinutes(-18))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("9c2d1bb2-8e10-433f-85b6-f09f7ed29a5c"), OrderId=o7, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,  ChangedDate=now.AddMinutes(-14) }, now.AddMinutes(-14)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("ab649981-0735-4cb2-bd11-62886affba91"), OrderId=o7, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-6) }, now.AddMinutes(-6)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("6df965a9-c1de-4d38-9dcc-be2d5366d307"), OrderId=o7, PreviousStatusId=(int)OrderStatusEnum.Approved, NewStatusId=(int)OrderStatusEnum.Shipping, ChangedDate=now.AddMinutes(-3) }, now.AddMinutes(-3))
            });
            await db.SaveChangesAsync();

            var o8 = Guid.Parse("54140d91-aefb-4e89-b3ec-d85eeb74aa2b");
            now = DemoDate(5, 17, 15, 5);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o8, UserId = customer.Id }, now.AddMinutes(-11), now.AddMinutes(-7)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o8, Address = "Istanbul", Description = "Order #8 Shipping", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Shipping }, now.AddMinutes(-3), now.AddMinutes(8)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("e2af8eeb-633a-473a-971c-acad34f22a10"), BasketId = o8, ProductId = topProducts[1].Id, Quantity = 1 }, now.AddMinutes(-9)),
                WithCreated(new BasketItem { Id = Guid.Parse("3b16864a-2d98-484c-99e6-87be5e2319a8"), BasketId = o8, ProductId = topProducts[2].Id, Quantity = 2 }, now.AddMinutes(-8)),
                WithCreated(new BasketItem { Id = Guid.Parse("997d874a-7e86-4d1c-ab00-7555aca239d6"), BasketId = o8, ProductId = topProducts[3].Id, Quantity = 2 }, now.AddMinutes(-7))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("4c181213-35f3-4a5d-a2ef-2762d6bebf1d"), OrderId=o8, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,  ChangedDate=now.AddMinutes(-3) }, now.AddMinutes(-3)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("69f8c1ea-cf18-4269-8e5d-7f0b8672ab71"), OrderId=o8, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(4) }, now.AddMinutes(4)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("b80c2bf4-f097-4b6f-a6de-e9176ceba0ba"), OrderId=o8, PreviousStatusId=(int)OrderStatusEnum.Approved, NewStatusId=(int)OrderStatusEnum.Shipping, ChangedDate=now.AddMinutes(8) }, now.AddMinutes(8))
            });
            await db.SaveChangesAsync();

            // 6.5 Delivered (3 items) [Pending -> Approved -> Shipping -> Delivered]
            var o9 = Guid.Parse("cdd910d0-fdf6-44ff-8aa2-4933f52b72bc");
            now = DemoDate(5, 20, 10, 45);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o9, UserId = customer.Id }, now.AddMinutes(-20), now.AddMinutes(-16)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o9, Address = "Istanbul", Description = "Order #9 Delivered", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Delivered }, now.AddMinutes(-12), now.AddMinutes(3)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("d5f517a0-92b5-4f8c-bbc6-6b5be969a81d"), BasketId = o9, ProductId = topProducts[4].Id, Quantity = 2 }, now.AddMinutes(-18)),
                WithCreated(new BasketItem { Id = Guid.Parse("7b826a7d-310b-4650-ba05-aa90ddb2f492"), BasketId = o9, ProductId = topProducts[5].Id, Quantity = 2 }, now.AddMinutes(-17)),
                WithCreated(new BasketItem { Id = Guid.Parse("889b1e03-6705-4873-8d27-f143f34a24a5"), BasketId = o9, ProductId = topProducts[6].Id, Quantity = 1 }, now.AddMinutes(-16))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("260ea9bd-335e-4498-a053-1e947ac5b784"), OrderId=o9, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,   ChangedDate=now.AddMinutes(-12) }, now.AddMinutes(-12)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("6950e8a8-8996-424f-9bd5-ca38726c8160"), OrderId=o9, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-4) }, now.AddMinutes(-4)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("0c3420e9-0896-4afd-bd3d-38f4c06b5cad"), OrderId=o9, PreviousStatusId=(int)OrderStatusEnum.Approved, NewStatusId=(int)OrderStatusEnum.Shipping, ChangedDate=now.AddMinutes(0) }, now.AddMinutes(0)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("db0c904a-e6d4-480c-a74d-e6cc8783f388"), OrderId=o9, PreviousStatusId=(int)OrderStatusEnum.Shipping, NewStatusId=(int)OrderStatusEnum.Delivered, ChangedDate=now.AddMinutes(3) }, now.AddMinutes(3))
            });
            await db.SaveChangesAsync();

            var o10 = Guid.Parse("08fbee0f-4c5c-415a-8ae1-b9474fc62e74");
            now = DemoDate(5, 23, 13, 25);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o10, UserId = customer.Id }, now.AddMinutes(-18), now.AddMinutes(-14)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o10, Address = "Istanbul", Description = "Order #10 Delivered", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Delivered }, now.AddMinutes(-10), now.AddMinutes(6)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("ed7b8985-f583-4816-8710-e1cc8d0ed738"), BasketId = o10, ProductId = topProducts[7].Id, Quantity = 1 }, now.AddMinutes(-16)),
                WithCreated(new BasketItem { Id = Guid.Parse("6b8c1ac0-1960-4ef9-9eb4-22a2dc9f7031"), BasketId = o10, ProductId = topProducts[8].Id, Quantity = 2 }, now.AddMinutes(-15)),
                WithCreated(new BasketItem { Id = Guid.Parse("5fb81743-a972-4cc1-bc38-63506a2a805e"), BasketId = o10, ProductId = topProducts[9].Id, Quantity = 2 }, now.AddMinutes(-14))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("ae295c81-1745-4ac3-b636-37fe83a13fdd"), OrderId=o10, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,   ChangedDate=now.AddMinutes(-10) }, now.AddMinutes(-10)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("b917f231-3332-4b0c-91db-3f5d4d434409"), OrderId=o10, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(-2) }, now.AddMinutes(-2)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("8d65685b-9ddb-4901-be0f-26ad1819f505"), OrderId=o10, PreviousStatusId=(int)OrderStatusEnum.Approved, NewStatusId=(int)OrderStatusEnum.Shipping, ChangedDate=now.AddMinutes(2) }, now.AddMinutes(2)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("0996b268-61f7-41e3-b776-81ba311b7fcb"), OrderId=o10, PreviousStatusId=(int)OrderStatusEnum.Shipping, NewStatusId=(int)OrderStatusEnum.Delivered, ChangedDate=now.AddMinutes(6) }, now.AddMinutes(6))
            });
            await db.SaveChangesAsync();

            var o11 = Guid.Parse("13038336-46fc-4c94-9ada-7ee5490bf0b3");
            now = DemoDate(5, 27, 17, 10);
            await db.Baskets.AddAsync(WithCreatedAndUpdated(new Basket { Id = o11, UserId = customer.Id }, now.AddMinutes(-16), now.AddMinutes(-12)));
            await db.SaveChangesAsync();
            await db.Orders.AddAsync(WithCreatedAndUpdated(new Order { Id = o11, Address = "Istanbul", Description = "Order #11 Delivered", OrderCode = GenerateOrderCode(), StatusId = (int)OrderStatusEnum.Delivered }, now.AddMinutes(-8), now.AddMinutes(8)));
            await db.SaveChangesAsync();
            await db.BasketItems.AddRangeAsync(new[]
            {
                WithCreated(new BasketItem { Id = Guid.Parse("dd8e0dd3-c51f-42f1-b013-7fad27e77faa"), BasketId = o11, ProductId = topProducts[0].Id, Quantity = 1 }, now.AddMinutes(-14)),
                WithCreated(new BasketItem { Id = Guid.Parse("520ae9e8-33ec-4ae7-af35-cbd244181115"), BasketId = o11, ProductId = topProducts[2].Id, Quantity = 2 }, now.AddMinutes(-13)),
                WithCreated(new BasketItem { Id = Guid.Parse("a1a14858-bab7-43f1-8387-6cd25540218f"), BasketId = o11, ProductId = topProducts[4].Id, Quantity = 2 }, now.AddMinutes(-12))
            });
            await db.SaveChangesAsync();
            await db.OrderStatusHistories.AddRangeAsync(new[]
            {
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("a3e78eaf-4e2b-4a95-b3fe-8a53742ab958"), OrderId=o11, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Pending,   ChangedDate=now.AddMinutes(-8) }, now.AddMinutes(-8)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("896234d2-2060-49a1-966e-29a250bd5750"), OrderId=o11, PreviousStatusId=(int)OrderStatusEnum.Pending, NewStatusId=(int)OrderStatusEnum.Approved, ChangedDate=now.AddMinutes(0) }, now.AddMinutes(0)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("34736908-aa15-4b24-b95f-6251b703d989"), OrderId=o11, PreviousStatusId=(int)OrderStatusEnum.Approved, NewStatusId=(int)OrderStatusEnum.Shipping, ChangedDate=now.AddMinutes(4) }, now.AddMinutes(4)),
                WithCreated(new OrderStatusHistory{ Id=Guid.Parse("f04b241b-d489-46a0-90ee-2ac8a6afd64a"), OrderId=o11, PreviousStatusId=(int)OrderStatusEnum.Shipping, NewStatusId=(int)OrderStatusEnum.Delivered, ChangedDate=now.AddMinutes(8) }, now.AddMinutes(8))
            });
            await db.SaveChangesAsync();

            // 7. MENUS (manuel seed)
            await db.Menus.AddRangeAsync(new[]
            {
                new Menu { Id = Guid.Parse("d3aedd30-d187-4498-8d53-26128680eb9a"), Name = "ApplicationServices" },
                new Menu { Id = Guid.Parse("99cf43a4-c3e4-4ff7-bc08-f004c19fd030"), Name = "Baskets" },
                new Menu { Id = Guid.Parse("a771feb9-1524-4ba0-adc7-7b34c24149cb"), Name = "Endpoints" },
                new Menu { Id = Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434"), Name = "Orders" },
                new Menu { Id = Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71"), Name = "Products" },
                new Menu { Id = Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b"), Name = "Roles" },
                new Menu { Id = Guid.Parse("569c46f7-8af4-40f5-bf08-0ab17a7a3a3e"), Name = "Users" }
            }.Select((menu, index) => WithCreated(menu, DemoDate(3, 11, 9, 5).AddHours(index * 6 + index))));
            await db.SaveChangesAsync();

            // 8. ENDPOINTS (manuel seed; connect Menu with navigation)
            var menusById = await db.Menus.ToDictionaryAsync(m => m.Id);

            await db.Endpoints.AddRangeAsync(new[]
            {
                new Endpoint { Id=Guid.Parse("b8a8512b-2191-488b-a34a-76afb7d90006"), ActionType="Read",   HttpType="GET",    Definition="Get Authorize Definition Endpoints", Code="GET.Read.GetAuthorizeDefinitionEndpoints", AdminOnly=true,  Menu = menusById[Guid.Parse("d3aedd30-d187-4498-8d53-26128680eb9a")] },
                new Endpoint { Id=Guid.Parse("5b29dc75-e16c-4fb3-b522-d14b5e28ad33"), ActionType="Read",   HttpType="GET",    Definition="Get All Basket Items",             Code="GET.Read.GetAllBasketItems",               AdminOnly=false, Menu = menusById[Guid.Parse("99cf43a4-c3e4-4ff7-bc08-f004c19fd030")] },
                new Endpoint { Id=Guid.Parse("5476c1d3-94d4-4f0f-8dba-25b1c51217fb"), ActionType="Write",  HttpType="POST",   Definition="Add Item to Basket",               Code="POST.Write.AddItemtoBasket",               AdminOnly=false, Menu = menusById[Guid.Parse("99cf43a4-c3e4-4ff7-bc08-f004c19fd030")] },
                new Endpoint { Id=Guid.Parse("3da24ddd-984c-470c-8cc6-9db518f68ca2"), ActionType="Update", HttpType="PUT",    Definition="Update Basket Item Quantity",      Code="PUT.Update.UpdateBasketItemQuantity",      AdminOnly=false, Menu = menusById[Guid.Parse("99cf43a4-c3e4-4ff7-bc08-f004c19fd030")] },
                new Endpoint { Id=Guid.Parse("42508967-c22b-4e14-91d0-964524f19c00"), ActionType="Delete", HttpType="DELETE", Definition="Remove Basket Item",               Code="DELETE.Delete.RemoveBasketItem",           AdminOnly=false, Menu = menusById[Guid.Parse("99cf43a4-c3e4-4ff7-bc08-f004c19fd030")] },
                new Endpoint { Id=Guid.Parse("8a0c1d16-acb3-49d9-81eb-cbe667771457"), ActionType="Read",   HttpType="GET",    Definition = "Get Roles and Endpoints",        Code = "GET.Read.GetRolesandEndpoints",          AdminOnly = true, Menu = menusById[Guid.Parse("a771feb9-1524-4ba0-adc7-7b34c24149cb")]},
                new Endpoint { Id=Guid.Parse("46487cb5-8535-4614-8c6f-38c9700911bf"), ActionType="Write",  HttpType="POST",   Definition="Assign Roles to Endpoints",        Code="POST.Write.AssignRolestoEndpoints",        AdminOnly=true,  Menu = menusById[Guid.Parse("a771feb9-1524-4ba0-adc7-7b34c24149cb")] },
                new Endpoint { Id=Guid.Parse("22fc12e2-48f2-4efd-beae-ce9584e6979e"), ActionType="Read",   HttpType="GET",    Definition="Get All Orders",                   Code="GET.Read.GetAllOrders",                    AdminOnly=true,  Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("94cc08dc-d762-4a3a-9160-fbf61e0209d6"), ActionType="Read",   HttpType="GET",    Definition="Get Order by Id",                  Code="GET.Read.GetOrderbyId",                    AdminOnly=false, Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("1cd9083d-07ba-402b-a4a0-a7528913b31d"), ActionType="Write",  HttpType="POST",   Definition="Create Order",                     Code="POST.Write.CreateOrder",                   AdminOnly=false, Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("bedd1a37-43c5-4f20-b973-73e6f0b034a1"), ActionType="Delete", HttpType="DELETE", Definition="Delete Order",                     Code="DELETE.Delete.DeleteOrder",                AdminOnly=true,  Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("49c9b622-82bb-4ff0-bdea-777158180c17"), ActionType="Delete", HttpType="POST",   Definition="Delete Range of Order",            Code="POST.Delete.DeleteRangeofOrder",           AdminOnly=true,  Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("5638d377-c52e-4ea6-aa15-52f7ed3361d8"), ActionType="Update", HttpType="PUT",    Definition="Update Order Status",              Code="PUT.Update.UpdateOrderStatus",             AdminOnly=true,  Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("a9c8f7a4-a0aa-4564-b03d-1941838fdabb"), ActionType="Read",   HttpType="GET",    Definition="Get Order Status History by Id",   Code="GET.Read.GetOrderStatusHistorybyId",       AdminOnly=false, Menu = menusById[Guid.Parse("5029aaa1-f6ac-4161-aa8e-64da8cddd434")] },
                new Endpoint { Id=Guid.Parse("9093e984-c237-411f-bd39-6c644736bd56"), ActionType="Write",  HttpType="POST",   Definition="Create Product",                   Code="POST.Write.CreateProduct",                 AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("3a7de741-7b65-45c2-84af-37eb15900f4b"), ActionType="Update", HttpType="PUT",    Definition="Update Product",                   Code="PUT.Update.UpdateProduct",                 AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("d2e8ddf2-cffa-47b9-b45b-519e18dcc6be"), ActionType="Delete", HttpType="DELETE", Definition="Delete Product",                   Code="DELETE.Delete.DeleteProduct",              AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("9cc73426-76dd-4aa2-9230-99c0bb2e82f8"), ActionType="Delete", HttpType="POST",   Definition="Delete Range of Product",          Code="POST.Delete.DeleteRangeofProduct",         AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("78f67f3c-a604-451c-a936-9ce1e3eb148e"), ActionType="Write",  HttpType="POST",   Definition="Upload Files",                     Code="POST.Write.UploadFiles",                   AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("6241db49-af96-4737-b76a-3bc00cbd7310"), ActionType="Delete", HttpType="DELETE", Definition="Delete Product Image",             Code="DELETE.Delete.DeleteProductImage",         AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("0606c85e-b5c6-4110-8f9c-98a72b1ff772"), ActionType="Update", HttpType="PUT",    Definition="Change Cover Image",               Code="PUT.Update.ChangeCoverImage",              AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("06f155f2-2f34-45e0-9c51-d9a973c58c3f"), ActionType="Read",   HttpType="GET",    Definition="Get Product QR Code",              Code="GET.Read.GetProductQRCode",                AdminOnly=true,  Menu = menusById[Guid.Parse("20b1cee6-d359-4358-a82e-46f602856e71")] },
                new Endpoint { Id=Guid.Parse("d983cdf3-8e10-401e-88b0-722beb78b168"), ActionType="Read",   HttpType="GET",    Definition="Get Roles",                        Code="GET.Read.GetRoles",                        AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("09b85d6b-51d1-46ec-8f5e-76f6303ac1fb"), ActionType="Read",   HttpType="GET",    Definition="Get Role By Id",                   Code="GET.Read.GetRoleById",                     AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("32907798-0b30-4e00-b006-00e2a3c4c4c3"), ActionType="Write",  HttpType="POST",   Definition="Create Role",                      Code="POST.Write.CreateRole",                    AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("eb9b72d0-72d4-4a3f-a267-73e118247cfa"), ActionType="Update", HttpType="PATCH",  Definition="Update Role",                      Code="PATCH.Update.UpdateRole",                    AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("c3a10d8c-dc06-4ee9-812c-59f84dae661e"), ActionType="Delete", HttpType="DELETE", Definition="Delete Role",                      Code="DELETE.Delete.DeleteRole",                 AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("e7c4a0e0-747b-48ed-983c-4b65fbad074a"), ActionType="Delete", HttpType="POST",   Definition="Delete Range of Role",             Code="POST.Delete.DeleteRangeofRole",            AdminOnly=true,  Menu = menusById[Guid.Parse("230f3782-7927-4bef-be44-5d19ab8e4c7b")] },
                new Endpoint { Id=Guid.Parse("793b43f2-a533-4f5a-abd4-96c12788925b"), ActionType="Read",   HttpType="GET",    Definition="Get All Users",                    Code="GET.Read.GetAllUsers",                     AdminOnly=true,  Menu = menusById[Guid.Parse("569c46f7-8af4-40f5-bf08-0ab17a7a3a3e")] },
                new Endpoint { Id=Guid.Parse("b62126cf-c3ae-43a9-b17f-c36404e0021b"), ActionType="Write",  HttpType="POST",   Definition="Assign Role To User",              Code="POST.Write.AssignRoleToUser",              AdminOnly=true,  Menu = menusById[Guid.Parse("569c46f7-8af4-40f5-bf08-0ab17a7a3a3e")] }
            }.Select((endpoint, index) => WithCreated(endpoint, DemoDate(3, 19, 8, 20).AddHours(index * 3 + index % 5))));
            await db.SaveChangesAsync();


            // 9. ROLE <-> ENDPOINT relation (Code -> Roles)
            var map = new Dictionary<string, string[]>
            {
                // System Administrator
                ["GET.Read.GetAuthorizeDefinitionEndpoints"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetRoles"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetRolesandEndpoints"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.AssignRolestoEndpoints"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetAllUsers"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.AssignRolestoEndpoints"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["DELETE.Delete.DeleteOrder"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Delete.DeleteRangeofOrder"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.CreateRole"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetRoleById"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["DELETE.Delete.DeleteRole"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["PATCH.Update.UpdateRole"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Delete.DeleteRangeofRole"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["DELETE.Delete.DeleteProduct"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Delete.DeleteRangeofProduct"] = [SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.AssignRoleToUser"] = [SystemBootstrapConstants.SystemAdministratorRoleName],

                // Store Manager + System Administrator
                ["GET.Read.GetAllOrders"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetOrderbyId"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.CreateOrder"] = ["Customer", "StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["PUT.Update.UpdateOrderStatus"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetOrderStatusHistorybyId"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.CreateProduct"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["PUT.Update.UpdateProduct"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["DELETE.Delete.DeleteProductImage"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["PUT.Update.ChangeCoverImage"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["GET.Read.GetProductQRCode"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.UploadFiles"] = ["StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],

                // Customer + Store Manager + System Administrator
                ["GET.Read.GetAllBasketItems"] = ["Customer", "StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["POST.Write.AddItemtoBasket"] = ["Customer", "StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["PUT.Update.UpdateBasketItemQuantity"] = ["Customer", "StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
                ["DELETE.Delete.RemoveBasketItem"] = ["Customer", "StoreManager", SystemBootstrapConstants.SystemAdministratorRoleName],
            };

            // "Customer","StoreManager","SystemAdministrator"
            var rolesByName = await db.Roles
                .Where(role => role.Name != null)
                .ToDictionaryAsync(role => role.Name!);

            foreach (var kv in map)
            {
                var code = kv.Key;
                var roleNames = kv.Value;

                var endpoint = await db.Endpoints.FirstAsync(e => e.Code == code);

                foreach (var roleName in roleNames)
                {
                    var role = rolesByName[roleName];
                    // Load the navigation collection.
                    await db.Entry(role).Collection(r => r.Endpoints).LoadAsync();

                    if (!role.Endpoints.Any(x => x.Id == endpoint.Id))
                        role.Endpoints.Add(endpoint);
                }
            }
            await db.SaveChangesAsync();

            // 10. PRODUCT IMAGES + RELATIONS (add through navigation)
            var fileMeta = new Dictionary<Guid, (string FileName, string Path, string Storage, bool Cover)>
            {
                #region Azure Storage
                //// fb7ef7b8... (Galaxy Z Fold 7 seti)
                //[Guid.Parse("3ee62975-9603-4c57-8d08-b4762a401938")] = ("galaxy-z-fold7-2.jpg", "images/galaxy-z-fold7-2.jpg", "AzureStorage", false),
                //[Guid.Parse("bafae95c-3a46-4814-9ef8-93d2dcb5d9e8")] = ("galaxy-z-fold7-4.jpg", "images/galaxy-z-fold7-4.jpg", "AzureStorage", false),
                //[Guid.Parse("eaa1173a-5c12-48a8-9f61-02d46c3cd693")] = ("galaxy-z-fold7-5.jpg", "images/galaxy-z-fold7-5.jpg", "AzureStorage", false),
                //[Guid.Parse("c6636140-72c7-4aaf-a41d-7ebab27934ff")] = ("galaxy-z-fold7-3.jpg", "images/galaxy-z-fold7-3.jpg", "AzureStorage", true),
                //[Guid.Parse("e1cbde9f-c164-4f9f-8220-6f5e9306060e")] = ("galaxy-z-fold7.jpg", "images/galaxy-z-fold7.jpg", "AzureStorage", false),

                //// 39a3b77f... (Galaxy S25 seti)
                //[Guid.Parse("035be0d7-5d1e-4bf2-a76c-9a9c7dde60f7")] = ("galaxy-s25-2.jpg", "images/galaxy-s25-2.jpg", "AzureStorage", false),
                //[Guid.Parse("20bad5d5-d1fd-4e14-8930-59e177eef2b7")] = ("galaxy-s25-4.jpg", "images/galaxy-s25-4.jpg", "AzureStorage", false),
                //[Guid.Parse("96df53ca-542e-4e9e-9c59-8210eb3912a9")] = ("galaxy-s25.jpg", "images/galaxy-s25.jpg", "AzureStorage", true),
                //[Guid.Parse("a6f6f2b6-b145-46aa-8146-173565a87b30")] = ("galaxy-s25-3.jpg", "images/galaxy-s25-3.jpg", "AzureStorage", false),

                //// e38194b3... (Galaxy Z Flip 7 seti)
                //[Guid.Parse("125b89b6-9662-4156-90e0-1b604af95683")] = ("galaxy-zflip7-2.jpg", "images/galaxy-zflip7-2.jpg", "AzureStorage", false),
                //[Guid.Parse("7056667a-48aa-468d-b748-269480488e41")] = ("galaxy-zflip7-4.jpg", "images/galaxy-zflip7-4.jpg", "AzureStorage", false),
                //[Guid.Parse("72d44712-446e-4d36-9ec7-ce1e8cd9b176")] = ("galaxy-zflip7-3.jpg", "images/galaxy-zflip7-3.jpg", "AzureStorage", false),
                //[Guid.Parse("86387ae5-a15c-43e7-b618-5ebf055d2219")] = ("galaxy-zflip7.jpg", "images/galaxy-zflip7.jpg", "AzureStorage", true),
                //[Guid.Parse("b0148456-e534-42c6-ab9c-c0802772996d")] = ("galaxy-zflip7-5.jpg", "images/galaxy-zflip7-5.jpg", "AzureStorage", false),

                //// a0e7e2e1... (iPhone 17 Pro Deep Blue seti)
                //[Guid.Parse("4499b376-111e-41e9-82e9-fc48bedf1892")] = ("iphone-17-pro-deepblue.jpg", "images/iphone-17-pro-deepblue.jpg", "AzureStorage", true),
                //[Guid.Parse("74ff1581-f5a6-44a6-8999-50aa8a966663")] = ("iphone-17-pro-deepblue-3.jpg", "images/iphone-17-pro-deepblue-3.jpg", "AzureStorage", false),
                //[Guid.Parse("e6ce807c-878b-4a2c-8040-750b30b28a07")] = ("iphone-17-pro-deepblue-2.jpg", "images/iphone-17-pro-deepblue-2.jpg", "AzureStorage", false),

                //// ee871416... (iPhone 16 seti)
                //[Guid.Parse("1b34cbb7-5aa7-48cd-91e5-bfa12b512009")] = ("iphone-16.jpg", "images/iphone-16.jpg", "AzureStorage", true),
                //[Guid.Parse("7ccba00d-fd06-46f0-be31-ab589ca0c865")] = ("iphone-16-2.jpg", "images/iphone-16-2.jpg", "AzureStorage", false),
                //[Guid.Parse("b3ebd6d6-1c25-482a-91b9-d74a3d8c21aa")] = ("iphone-16-3.jpg", "images/iphone-16-3.jpg", "AzureStorage", false),

                //// de912c56... (Pink Phone seti)
                //[Guid.Parse("6bb3709f-7d84-4d41-afc4-45fe2cde5ff9")] = ("pink-phone-1.jpg", "images/pink-phone-1.jpg", "AzureStorage", true),
                //[Guid.Parse("3b749096-08c1-441d-8eb4-79b252f7752c")] = ("pink-phone-2.jpg", "images/pink-phone-2.jpg", "AzureStorage", false),
                //[Guid.Parse("c868a86b-bd79-46c6-9f6f-9f2df3a878cc")] = ("pink-phone-3.jpg", "images/pink-phone-3.jpg", "AzureStorage", false),
                #endregion
                #region Local Storage
                // fb7ef7b8... (Galaxy Z Fold 7 seti)
                [Guid.Parse("3ee62975-9603-4c57-8d08-b4762a401938")] = ("galaxy-z-fold7-2.jpg", "images\\galaxy-z-fold7-2.jpg", "LocalStorage", false),
                [Guid.Parse("bafae95c-3a46-4814-9ef8-93d2dcb5d9e8")] = ("galaxy-z-fold7-4.jpg", "images\\galaxy-z-fold7-4.jpg", "LocalStorage", false),
                [Guid.Parse("eaa1173a-5c12-48a8-9f61-02d46c3cd693")] = ("galaxy-z-fold7-5.jpg", "images\\galaxy-z-fold7-5.jpg", "LocalStorage", false),
                [Guid.Parse("c6636140-72c7-4aaf-a41d-7ebab27934ff")] = ("galaxy-z-fold7-3.jpg", "images\\galaxy-z-fold7-3.jpg", "LocalStorage", true),
                [Guid.Parse("e1cbde9f-c164-4f9f-8220-6f5e9306060e")] = ("galaxy-z-fold7.jpg", "images\\galaxy-z-fold7.jpg", "LocalStorage", false),

                // 39a3b77f... (Galaxy S25 seti)
                [Guid.Parse("035be0d7-5d1e-4bf2-a76c-9a9c7dde60f7")] = ("galaxy-s25-2.jpg", "images\\galaxy-s25-2.jpg", "LocalStorage", false),
                [Guid.Parse("20bad5d5-d1fd-4e14-8930-59e177eef2b7")] = ("galaxy-s25-4.jpg", "images\\galaxy-s25-4.jpg", "LocalStorage", false),
                [Guid.Parse("96df53ca-542e-4e9e-9c59-8210eb3912a9")] = ("galaxy-s25.jpg", "images\\galaxy-s25.jpg", "LocalStorage", true),
                [Guid.Parse("a6f6f2b6-b145-46aa-8146-173565a87b30")] = ("galaxy-s25-3.jpg", "images\\galaxy-s25-3.jpg", "LocalStorage", false),

                // e38194b3... (Galaxy Z Flip 7 seti)
                [Guid.Parse("125b89b6-9662-4156-90e0-1b604af95683")] = ("galaxy-zflip7-2.jpg", "images\\galaxy-zflip7-2.jpg", "LocalStorage", false),
                [Guid.Parse("7056667a-48aa-468d-b748-269480488e41")] = ("galaxy-zflip7-4.jpg", "images\\galaxy-zflip7-4.jpg", "LocalStorage", false),
                [Guid.Parse("72d44712-446e-4d36-9ec7-ce1e8cd9b176")] = ("galaxy-zflip7-3.jpg", "images\\galaxy-zflip7-3.jpg", "LocalStorage", false),
                [Guid.Parse("86387ae5-a15c-43e7-b618-5ebf055d2219")] = ("galaxy-zflip7.jpg", "images\\galaxy-zflip7.jpg", "LocalStorage", true),
                [Guid.Parse("b0148456-e534-42c6-ab9c-c0802772996d")] = ("galaxy-zflip7-5.jpg", "images\\galaxy-zflip7-5.jpg", "LocalStorage", false),

                // a0e7e2e1... (iPhone 17 Pro Deep Blue seti)
                [Guid.Parse("4499b376-111e-41e9-82e9-fc48bedf1892")] = ("iphone-17-pro-deepblue.jpg", "images\\iphone-17-pro-deepblue.jpg", "LocalStorage", true),
                [Guid.Parse("74ff1581-f5a6-44a6-8999-50aa8a966663")] = ("iphone-17-pro-deepblue-3.jpg", "images\\iphone-17-pro-deepblue-3.jpg", "LocalStorage", false),
                [Guid.Parse("e6ce807c-878b-4a2c-8040-750b30b28a07")] = ("iphone-17-pro-deepblue-2.jpg", "images\\iphone-17-pro-deepblue-2.jpg", "LocalStorage", false),

                // ee871416... (iPhone 16 seti)
                [Guid.Parse("1b34cbb7-5aa7-48cd-91e5-bfa12b512009")] = ("iphone-16.jpg", "images\\iphone-16.jpg", "LocalStorage", true),
                [Guid.Parse("7ccba00d-fd06-46f0-be31-ab589ca0c865")] = ("iphone-16-2.jpg", "images\\iphone-16-2.jpg", "LocalStorage", false),
                [Guid.Parse("b3ebd6d6-1c25-482a-91b9-d74a3d8c21aa")] = ("iphone-16-3.jpg", "images\\iphone-16-3.jpg", "LocalStorage", false),

                // de912c56... (Pink Phone seti)
                [Guid.Parse("6bb3709f-7d84-4d41-afc4-45fe2cde5ff9")] = ("pink-phone-1.jpg", "images\\pink-phone-1.jpg", "LocalStorage", true),
                [Guid.Parse("3b749096-08c1-441d-8eb4-79b252f7752c")] = ("pink-phone-2.jpg", "images\\pink-phone-2.jpg", "LocalStorage", false),
                [Guid.Parse("c868a86b-bd79-46c6-9f6f-9f2df3a878cc")] = ("pink-phone-3.jpg", "images\\pink-phone-3.jpg", "LocalStorage", false),
                #endregion
            };

            var rels = new (Guid ProductId, Guid ImageId)[]
            {
                // fb7ef7b8-eb0d-4566-bd03-958b4ff448cd
                (Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd"), Guid.Parse("3ee62975-9603-4c57-8d08-b4762a401938")),
                (Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd"), Guid.Parse("bafae95c-3a46-4814-9ef8-93d2dcb5d9e8")),
                (Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd"), Guid.Parse("c6636140-72c7-4aaf-a41d-7ebab27934ff")),
                (Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd"), Guid.Parse("e1cbde9f-c164-4f9f-8220-6f5e9306060e")),
                (Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd"), Guid.Parse("eaa1173a-5c12-48a8-9f61-02d46c3cd693")),

                // 39a3b77f-dd8a-460c-ae5d-a650ad3b80ca
                (Guid.Parse("39a3b77f-dd8a-460c-ae5d-a650ad3b80ca"), Guid.Parse("035be0d7-5d1e-4bf2-a76c-9a9c7dde60f7")),
                (Guid.Parse("39a3b77f-dd8a-460c-ae5d-a650ad3b80ca"), Guid.Parse("20bad5d5-d1fd-4e14-8930-59e177eef2b7")),
                (Guid.Parse("39a3b77f-dd8a-460c-ae5d-a650ad3b80ca"), Guid.Parse("96df53ca-542e-4e9e-9c59-8210eb3912a9")),
                (Guid.Parse("39a3b77f-dd8a-460c-ae5d-a650ad3b80ca"), Guid.Parse("a6f6f2b6-b145-46aa-8146-173565a87b30")),

                // e38194b3-680a-422b-a2d8-da4ac4636c3b
                (Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b"), Guid.Parse("125b89b6-9662-4156-90e0-1b604af95683")),
                (Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b"), Guid.Parse("7056667a-48aa-468d-b748-269480488e41")),
                (Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b"), Guid.Parse("72d44712-446e-4d36-9ec7-ce1e8cd9b176")),
                (Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b"), Guid.Parse("86387ae5-a15c-43e7-b618-5ebf055d2219")),
                (Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b"), Guid.Parse("b0148456-e534-42c6-ab9c-c0802772996d")),

                // a0e7e2e1-93b3-4876-9391-2d0acd6cd33a
                (Guid.Parse("a0e7e2e1-93b3-4876-9391-2d0acd6cd33a"), Guid.Parse("4499b376-111e-41e9-82e9-fc48bedf1892")),
                (Guid.Parse("a0e7e2e1-93b3-4876-9391-2d0acd6cd33a"), Guid.Parse("74ff1581-f5a6-44a6-8999-50aa8a966663")),
                (Guid.Parse("a0e7e2e1-93b3-4876-9391-2d0acd6cd33a"), Guid.Parse("e6ce807c-878b-4a2c-8040-750b30b28a07")),

                // ee871416-03c1-4522-a040-9811bf99b6ea
                (Guid.Parse("ee871416-03c1-4522-a040-9811bf99b6ea"), Guid.Parse("1b34cbb7-5aa7-48cd-91e5-bfa12b512009")),
                (Guid.Parse("ee871416-03c1-4522-a040-9811bf99b6ea"), Guid.Parse("7ccba00d-fd06-46f0-be31-ab589ca0c865")),
                (Guid.Parse("ee871416-03c1-4522-a040-9811bf99b6ea"), Guid.Parse("b3ebd6d6-1c25-482a-91b9-d74a3d8c21aa")),

                // de912c56-502d-4d65-a809-9b4fdab3410c
                (Guid.Parse("de912c56-502d-4d65-a809-9b4fdab3410c"), Guid.Parse("6bb3709f-7d84-4d41-afc4-45fe2cde5ff9")),
                (Guid.Parse("de912c56-502d-4d65-a809-9b4fdab3410c"), Guid.Parse("3b749096-08c1-441d-8eb4-79b252f7752c")),
                (Guid.Parse("de912c56-502d-4d65-a809-9b4fdab3410c"), Guid.Parse("c868a86b-bd79-46c6-9f6f-9f2df3a878cc")),
            };

            var imageIndex = 0;
            foreach (var (productId, imageId) in rels)
            {
                var p = await db.Products.FirstAsync(x => x.Id == productId);
                var m = fileMeta[imageId];
                var imageDateCreated = DemoDate(5, 29, 8, 15).AddMinutes(imageIndex * 17 + imageIndex % 5);

                var img = WithCreated(new ProductImageFile
                {
                    Id = imageId,
                    FileName = m.FileName,
                    Path = m.Path,
                    Storage = m.Storage,
                    CoverImage = m.Cover,
                    Product = new List<Product> { p }
                }, imageDateCreated);

                await db.ProductImageFiles.AddAsync(img);
                imageIndex++;
            }

            await db.SaveChangesAsync();
        }

        private static string GenerateOrderCode()
        {
            Span<byte> buffer = stackalloc byte[8];
            RandomNumberGenerator.Fill(buffer);
            long randomNumber = BitConverter.ToInt64(buffer);
            long positive10Digit = Math.Abs(randomNumber % 9_000_000_000L) + 1_000_000_000L;
            string timestamp = new DateTime(2025, 5, 14, 16, 25, 0, DateTimeKind.Utc).ToString("yyyyMMddHHmm");

            return string.Create(
                4 + 10 + 1 + 12,
                (positive10Digit, timestamp),
                static (span, state) =>
                {
                    var (number, ts) = state;
                    "ORD_".AsSpan().CopyTo(span.Slice(0, 4));
                    number.TryFormat(span.Slice(4, 10), out _);
                    span[14] = '_';
                    ts.AsSpan().CopyTo(span.Slice(15));
                }
            );
        }

        private static T WithCreated<T>(T entity, DateTime dateCreated) where T : BaseEntity
        {
            entity.DateCreated = dateCreated;

            return entity;
        }

        private static T WithCreatedAndUpdated<T>(T entity, DateTime dateCreated, DateTime dateUpdated) where T : BaseEntity
        {
            entity.DateCreated = dateCreated;
            entity.DateUpdated = dateUpdated;

            return entity;
        }

        private static DateTime DemoDate(int month, int day, int hour, int minute)
            => new(2025, month, day, hour, minute, 0, DateTimeKind.Utc);

        private static DateTime GetDemoProductDateCreated(Guid productId, int fallbackIndex)
        {
            if (ProductCreatedDates.TryGetValue(productId, out var dateCreated))
                return dateCreated;

            var day = 3 + fallbackIndex % 24;
            var hour = 8 + fallbackIndex * 5 % 11;
            var minute = 7 + fallbackIndex * 13 % 47;

            return new DateTime(2025, 3, day, hour, minute, 0, DateTimeKind.Utc);
        }

        private static readonly Dictionary<Guid, DateTime> ProductCreatedDates = new()
        {
            [Guid.Parse("ee871416-03c1-4522-a040-9811bf99b6ea")] = new DateTime(2025, 5, 28, 16, 45, 0, DateTimeKind.Utc),
            [Guid.Parse("a0e7e2e1-93b3-4876-9391-2d0acd6cd33a")] = new DateTime(2025, 5, 23, 10, 15, 0, DateTimeKind.Utc),
            [Guid.Parse("de912c56-502d-4d65-a809-9b4fdab3410c")] = new DateTime(2025, 5, 17, 13, 30, 0, DateTimeKind.Utc),
            [Guid.Parse("fb7ef7b8-eb0d-4566-bd03-958b4ff448cd")] = new DateTime(2025, 5, 9, 9, 55, 0, DateTimeKind.Utc),
            [Guid.Parse("e38194b3-680a-422b-a2d8-da4ac4636c3b")] = new DateTime(2025, 4, 29, 15, 20, 0, DateTimeKind.Utc),
            [Guid.Parse("39a3b77f-dd8a-460c-ae5d-a650ad3b80ca")] = new DateTime(2025, 4, 18, 11, 5, 0, DateTimeKind.Utc)
        };

        private sealed class ProductJson
        {
            public required string id { get; init; }
            public required string name { get; init; }
            public required string title { get; init; }
            public required string description { get; init; }
            public float price { get; init; }
            public int stock { get; init; }
            public float? rating { get; init; }
        }
    }
}
