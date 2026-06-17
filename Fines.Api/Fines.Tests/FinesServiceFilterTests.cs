
using Fines.Core.Enums;
using Fines.Data;
using Fines.Data.Models;
using Fines.Services;
using Microsoft.EntityFrameworkCore;

namespace Fines.Tests
{
    public class FinesServiceFilterTests
    {

        [Theory]
        [InlineData(FineType.Speeding, 1)]
        [InlineData(FineType.Parking, 0)]
        public async Task GetFinesAsync_FilterByType(FineType type, int expectedCount)
        {
            //Arrange
            var options = new DbContextOptionsBuilder<FinesDbContext>()
                .UseInMemoryDatabase(databaseName: "finesFilterDatabase")
                .Options;

            // Insert seed data into the database using one instance of the context
            using (var context = new FinesDbContext(options))
            {
                context.Customers.Add(new CustomerEntity { Id = 42, CompanyName = "naugthy john" });
                context.Vehicles.Add(new VehicleEntity { Id = 1, RegistrationNumber = "REG1" });
                context.Fines.Add(new FinesEntity { Id = 1, FineNo = "FN-001", FineDate = DateTime.Now, FineType = FineType.Speeding, VehicleId = 1, VehicleDriverName = "john was here", CustomerId = 42 });
                context.SaveChanges();

                IFinesRepository finesRepository = new FinesRepository(context);

                var service = new FinesService(finesRepository);

                //Act   
                var fineResponses = await service.GetFinesAsync(type);

                //Assert
                Assert.Equal(expectedCount, fineResponses.Count());

                //Clean up InMemory database
                context.Database.EnsureDeleted();

            }
        }

        [Theory]
        [InlineData("2020-12-17", 3)]
        [InlineData("2020-12-16", 0)]
        [InlineData("2020-12-18", 0)]
        public async Task GetFinesAsync_FilterByDate(string isoDateStr, int expectedCount)
        {
            //Arrange
            string[] dateFormat = { "yyyy-MM-dd" };
            var isoDate = DateOnly.ParseExact(isoDateStr, dateFormat);

            var options = new DbContextOptionsBuilder<FinesDbContext>()
                .UseInMemoryDatabase(databaseName: "finesDateFilterDatabase")
                .Options;

            // Insert seed data into the database using one instance of the context
            using (var context = new FinesDbContext(options))
            {
                context.Customers.Add(new CustomerEntity { Id = 42, CompanyName = "naugthy john" });
                context.Vehicles.Add(new VehicleEntity { Id = 1, RegistrationNumber = "REG1" });
                context.Fines.Add(new FinesEntity { Id = 1, FineNo = "FN-001", FineDate = new DateTime(2020,12,17,15,30,05), FineType = FineType.Speeding, VehicleId = 1, VehicleDriverName = "john was here", CustomerId = 42 });
                context.Fines.Add(new FinesEntity { Id = 2, FineNo = "FN-002", FineDate = new DateTime(2020,12,17,00,00,00), FineType = FineType.Speeding, VehicleId = 1, VehicleDriverName = "john was here", CustomerId = 42 });
                context.Fines.Add(new FinesEntity { Id = 3, FineNo = "FN-003", FineDate = new DateTime(2020,12,17,23,59,59), FineType = FineType.Speeding, VehicleId = 1, VehicleDriverName = "john was here", CustomerId = 42 });
                context.SaveChanges();

                IFinesRepository finesRepository = new FinesRepository(context);

                var service = new FinesService(finesRepository);

                //Act   
                var fineResponses = await service.GetFinesAsync(dateFilter: isoDate);

                //Clean up InMemory database
                context.Database.EnsureDeleted();

                //Assert
                Assert.Equal(expectedCount, fineResponses.Count());



            }
        }



    }
}
