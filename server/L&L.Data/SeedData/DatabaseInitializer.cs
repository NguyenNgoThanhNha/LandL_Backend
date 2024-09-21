using L_L.Data.Entities;
using L_L.Data.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace L_L.Data.SeedData
{
    public interface IDataaseInitialiser
    {
        Task InitialiseAsync();
        Task SeedAsync();
        Task TrySeedAsync();
    }

    public class DatabaseInitialiser : IDataaseInitialiser
    {
        public readonly AppDbContext _context;

        public DatabaseInitialiser(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                // Migration Database - Create database if it does not exist
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            if (_context.UserRoles.Any() && _context.Users.Any())
            {
                return;
            }

            var adminRole = new UserRole { RoleName = "Admin" };
            var customerRole = new UserRole { RoleName = "Customer" };
            var driverRole = new UserRole { RoleName = "Driver" };
            List<UserRole> userRoles = new()
            {
                adminRole,
                customerRole,
                driverRole,
            };
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();
            // Seed Users
            var users = new List<User>();

            for (int i = 1; i <= 10; i++)
            {
                var customer = new User
                {
                    UserName = $"Customer{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Customer{i}",
                    Email = $"customer{i}@gmail.com",
                    Gender = "Male",
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"012345678{i}",
                    Status = "Active",
                    TypeLogin = "Normal",
                    UserRole = customerRole, // Ensure `customerRole` is defined
                };
                users.Add(customer);

                var driver = new User
                {
                    UserName = $"Driver{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Driver{i}",
                    Email = $"driver{i}@gmail.com",
                    Gender = "Male",
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"018765432{i}",
                    Status = "Active",
                    TypeLogin = "Normal",
                    UserRole = driverRole, // Ensure `driverRole` is defined
                };
                users.Add(driver);
            }
            var admin = new User()
            {
                UserName = "Admin",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Super Admin",
                Email = "admin@gmail.com",
                Gender = "Male",
                City = "HCM",
                Address = "HCM",
                PhoneNumber = $"0135724680",
                Status = "Active",
                TypeLogin = "Normal",
                UserRole = adminRole, // Ensure `admin` is defined
            };
            users.Add(admin);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Add vehicletypes
            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType { VehicleTypeName = "0.5 Tấn", BaseRate = 150000 },
                new VehicleType { VehicleTypeName = "1.25 Tấn", BaseRate = 200000 },
                new VehicleType { VehicleTypeName = "1.9 Tấn", BaseRate = 230000 },
                new VehicleType { VehicleTypeName = "2.4 Tấn", BaseRate = 260000 },
                new VehicleType { VehicleTypeName = "3.5 Tấn", BaseRate = 320000 },
                new VehicleType { VehicleTypeName = "5 Tấn", BaseRate = 350000 },
                new VehicleType { VehicleTypeName = "7 Tấn", BaseRate = 400000 },
                new VehicleType { VehicleTypeName = "10 Tấn", BaseRate = 500000 },
            };

            await _context.VehicleTypes.AddRangeAsync(vehicleTypes);
            await _context.SaveChangesAsync();
            
            // Seed 10 Trucks
            var vehicleTypesList = await _context.VehicleTypes.ToListAsync(); // Fetch all vehicle types from the database
            var listDriver = await _context.Users.Where(x => x.RoleID == 3).ToListAsync();
            var trucks = new List<Truck>();

            for (int i = 1; i <= 10; i++)
            {
                var randomVehicleType = vehicleTypesList[new Random().Next(vehicleTypes.Count)];

                var truck = new Truck
                {
                    TruckName = $"Truck{i}",
                    Status = "Active",
                    PlateCode = $"PLATE{i:D4}",
                    Color = i % 2 == 0 ? "White" : "Blue",
                    TotalBill = i * 1000,
                    Manufacturer = i % 2 == 0 ? "Ford" : "Toyota",
                    VehicleModel = i % 2 == 0 ? "Model X" : "Model Y",
                    FrameNumber = $"FrameNo{i:D4}",
                    EngineNumber = $"EngineNo{i:D4}",
                    LoadCapacity = $"{1000 + i * 50}",
                    DimensionsLength = 4.5m + i * 0.1m,
                    DimensionsWidth = 2.0m + i * 0.05m,
                    DimensionsHeight = 2.5m + i * 0.05m,
                    VehicleTypeId = randomVehicleType.VehicleTypeId,  // Use a valid TypeId from the list
                    UserId = listDriver[i % listDriver.Count].UserId 
                };

                trucks.Add(truck);
            }

            await _context.Trucks.AddRangeAsync(trucks);
            await _context.SaveChangesAsync();


            var shippingRates = new List<ShippingRate>
            {
                // Shipping rates for 0.5 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[0].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 150000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[0].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 16000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[0].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 15000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[0].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 12000 },

                // Shipping rates for 1.25 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[1].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 200000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[1].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 18000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[1].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 16000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[1].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 13000 },

                // Shipping rates for 1.9 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[2].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 230000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[2].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 19000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[2].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 17000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[2].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 14000 },

                // Shipping rates for 2.4 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[3].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 260000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[3].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 20000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[3].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 18000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[3].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 15000 },

                // Shipping rates for 3.5 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[4].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 320000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[4].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 21000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[4].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 19000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[4].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 16000 },

                // Shipping rates for 5 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[5].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 350000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[5].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 22000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[5].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 20000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[5].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 17000 },

                // Shipping rates for 7 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[6].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 400000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[6].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 24000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[6].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 22000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[6].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 19000 },

                // Shipping rates for 10 Tấn
                new ShippingRate { VehicleTypeId = vehicleTypes[7].VehicleTypeId, DistanceFrom = 4, DistanceTo = 4, RatePerKM = 500000 }, // 4 KM ĐẦU
                new ShippingRate { VehicleTypeId = vehicleTypes[7].VehicleTypeId, DistanceFrom = 5, DistanceTo = 15, RatePerKM = 27000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[7].VehicleTypeId, DistanceFrom = 16, DistanceTo = 100, RatePerKM = 25000 },
                new ShippingRate { VehicleTypeId = vehicleTypes[7].VehicleTypeId, DistanceFrom = 101, DistanceTo = null, RatePerKM = 22000 },
            };

            await _context.ShippingRates.AddRangeAsync(shippingRates);
            await _context.SaveChangesAsync();

            var packageTypes = new List<PackageType>
            {
                // T1: ≤ 0.5 Tấn
                new PackageType
                {
                    WeightLimit = 0.5m,
                    LengthMin = 0.1m,
                    LengthMax = 2.60m,
                    WidthMin = 0.1m,
                    WidthMax = 1.50m,
                    HeightMin = 0.1m,
                    HeightMax = 1.41m,
                    VehicleRangeMin = 500,
                    VehicleRangeMax = 799
                },

                // T2: ≤ 0.8 Tấn
                new PackageType
                {
                    WeightLimit = 0.8m,
                    LengthMin = 2.54m,
                    LengthMax = 3.20m,
                    WidthMin = 1.42m,
                    WidthMax = 1.62m,
                    HeightMin = 1.16m,
                    HeightMax = 1.58m,
                    VehicleRangeMin = 800,
                    VehicleRangeMax = 999
                },

                // T3: ≤ 1 Tấn
                new PackageType
                {
                    WeightLimit = 1.0m,
                    LengthMin = 3.05m,
                    LengthMax = 3.47m,
                    WidthMin = 1.60m,
                    WidthMax = 1.75m,
                    HeightMin = 1.15m,
                    HeightMax = 1.80m,
                    VehicleRangeMin = 1000,
                    VehicleRangeMax = 1249
                },

                // T4: ≤ 1.25 Tấn
                new PackageType
                {
                    WeightLimit = 1.25m,
                    LengthMin = 3.17m,
                    LengthMax = 4.40m,
                    WidthMin = 1.67m,
                    WidthMax = 1.92m,
                    HeightMin = 1.11m,
                    HeightMax = 1.84m,
                    VehicleRangeMin = 1250,
                    VehicleRangeMax = 1999
                },

                // T5: ≤ 2 Tấn
                new PackageType
                {
                    WeightLimit = 2.0m,
                    LengthMin = 3.48m,
                    LengthMax = 4.40m,
                    WidthMin = 1.67m,
                    WidthMax = 1.91m,
                    HeightMin = 1.49m,
                    HeightMax = 1.97m,
                    VehicleRangeMin = 2000,
                    VehicleRangeMax = 2499
                },

                // T6: ≤ 2.5 Tấn
                new PackageType
                {
                    WeightLimit = 2.5m,
                    LengthMin = 3.36m,
                    LengthMax = 4.47m,
                    WidthMin = 1.67m,
                    WidthMax = 1.83m,
                    HeightMin = 1.55m,
                    HeightMax = 1.78m,
                    VehicleRangeMin = 2000,
                    VehicleRangeMax = 2999
                },

                // T7: ≤ 3 Tấn
                new PackageType
                {
                    WeightLimit = 3.0m,
                    LengthMin = 4.25m,
                    LengthMax = 5.03m,
                    WidthMin = 1.75m,
                    WidthMax = 2.12m,
                    HeightMin = 1.77m,
                    HeightMax = 2.39m,
                    VehicleRangeMin = 3000,
                    VehicleRangeMax = 3999
                },

                // T8: ≤ 4 Tấn
                new PackageType
                {
                    WeightLimit = 4.0m,
                    LengthMin = 4.40m,
                    LengthMax = 5.79m,
                    WidthMin = 1.95m,
                    WidthMax = 2.10m,
                    HeightMin = 1.49m,
                    HeightMax = 2.42m,
                    VehicleRangeMin = 4000,
                    VehicleRangeMax = 4999
                },

                // T9: ≤ 5 Tấn
                new PackageType
                {
                    WeightLimit = 5.0m,
                    LengthMin = 4.92m,
                    LengthMax = 6.75m,
                    WidthMin = 2.03m,
                    WidthMax = 2.33m,
                    HeightMin = 1.83m,
                    HeightMax = 2.47m,
                    VehicleRangeMin = 5000,
                    VehicleRangeMax = 7999
                },

                // T10: ≤ 8 Tấn
                new PackageType
                {
                    WeightLimit = 8.0m,
                    LengthMin = 5.22m,
                    LengthMax = 7.80m,
                    WidthMin = 2.28m,
                    WidthMax = 2.36m,
                    HeightMin = 1.78m,
                    HeightMax = 2.5m,
                    VehicleRangeMin = 8000,
                    VehicleRangeMax = 9999
                },

                // T11: ≤ 10 Tấn
                new PackageType
                {
                    WeightLimit = 10.0m,
                    LengthMin = 6.33m,
                    LengthMax = 9.20m,
                    WidthMin = 2.25m,
                    WidthMax = 2.39m,
                    HeightMin = 2.07m,
                    HeightMax = 2.53m,
                    VehicleRangeMin = 10000,
                    VehicleRangeMax = 14999
                },
            };


            await _context.PackageTypes.AddRangeAsync(packageTypes);
            await _context.SaveChangesAsync();

            var vehiclePackageRelations = new List<VehiclePackageRelation>();

            // Define weight limits corresponding to each package type.
            var weightLimits = new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m, 2.4m, 2.5m, 3.5m, 5.0m, 7.0m, 10.0m };

            // Vehicle mappings with their respective weight limits and IDs.
            var vehicleMappings = new List<(int VehicleTypeId, decimal WeightLimit)>
            {
                (1, 0.5m),
                (2, 1.25m),  // VehicleTypeId = 2 can carry up to 3 packages: 0.5, 0.8, 1.0
                (3, 1.9m),
                (4, 2.4m),
                (5, 3.5m),
                (6, 5.0m),
                (7, 7.0m),
                (8, 10.0m)
            };

            // Define specific constraints for each vehicle type
            var vehicleConstraints = new Dictionary<int, List<decimal>>
            {
                { 1, new List<decimal> { 0.5m } },
                { 2, new List<decimal> { 0.5m, 0.8m, 1.0m } },
                { 3, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m } },
                { 4, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m } },
                { 5, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m, 2.4m } },
                { 6, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m, 2.4m, 2.5m } },
                { 7, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m, 2.4m, 2.5m, 3.5m } },
                { 8, new List<decimal> { 0.5m, 0.8m, 1.0m, 1.25m, 1.9m, 2.4m, 2.5m, 3.5m, 5.0m, 7.0m, 10.0m } }
            };

            // Populate vehicle-package relations based on the defined constraints
            foreach (var vehicle in vehicleMappings)
            {
                var allowedPackageWeights = vehicleConstraints[vehicle.VehicleTypeId];

                foreach (var weightLimit in weightLimits)
                {
                    if (allowedPackageWeights.Contains(weightLimit))
                    {
                        vehiclePackageRelations.Add(new VehiclePackageRelation
                        {
                            VehicleTypeId = vehicle.VehicleTypeId,
                            PackageTypeId = weightLimits.IndexOf(weightLimit) + 1
                        });
                    }
                }
            }

            // Handle the case where there are fewer vehicle-package relations than expected.
            if (vehiclePackageRelations.Count < weightLimits.Count)
            {
                Console.WriteLine($"Warning: Only {vehiclePackageRelations.Count} vehicle-package relations were created. Ensure that all package types are covered.");
            }
            
            await _context.VehiclePackageRelations.AddRangeAsync(vehiclePackageRelations);

            // Save to DB
            await _context.SaveChangesAsync();
        }
    }

    public static class DatabaseInitialiserExtension
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            // Create IServiceScope to resolve service scope
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitialiser>();

            await initializer.InitialiseAsync();

            // Try to seeding data
            await initializer.SeedAsync();
        }
    }
}
