using Fines.Core.Enums;
using Fines.Data;
using Fines.Data.Models;
using Fines.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fines.Tests
{
    public class FinesServiceCustomerNameTests
    {

        //Another way of test service to ensure entity framework is set up well - by using an in-memory database FinesRepository, test the EF stuff as well
        //In memory database is fined for simple database work.  No good for transactions


        [Fact]
        public async Task GetFinesAsync_ReturnsCompanyName()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<FinesDbContext>()
                .UseInMemoryDatabase(databaseName: "finesDatabase")
                .Options;

            // Insert seed data into the database using one instance of the context
            using (var context = new FinesDbContext(options))
            {
                context.Customers.Add(new CustomerEntity { Id = 42, CompanyName = "naugthy john" });
                context.Vehicles.Add(new VehicleEntity { Id = 1, RegistrationNumber = "REG1" });
                context.Fines.Add(new FinesEntity  { Id = 1, FineNo = "FN-001", FineDate = DateTime.Now, FineType = FineType.Speeding, VehicleId = 1, VehicleDriverName = "john was here", CustomerId = 42 });
                context.SaveChanges();
            }


            using (var context = new FinesDbContext(options))
            {
                IFinesRepository finesRepository = new FinesRepository(context);

                var service = new FinesService(finesRepository); 

                //Act   
                var fineResponses = await service.GetFinesAsync();
                var firstFineResponse = fineResponses.First();

                //Assert
                Assert.NotEqual("", firstFineResponse.CustomerName);

                //Clean up InMemory database
                context.Database.EnsureDeleted();
            }
        }
    }
}
