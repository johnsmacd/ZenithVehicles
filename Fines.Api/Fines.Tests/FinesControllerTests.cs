using Fines.Core.Enums;
using Moq;
using Xunit;
using Fines.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;


namespace Fines.Tests
{
    public class FinesControllerTests
    {

        [Theory]
        [InlineData("FineType=Speeding", FineType.Speeding)]
        [InlineData("", null)]
        [InlineData("FType=Speeding", null)]
        [InlineData("FineType=Seeding", null)]
        public async Task FinesController_CorrectFineFilter_passedToService(string queryStringParameters, FineType? expected)
        {

            //Arrange
            var finesServiceMock = new Mock<IFinesService>();


            FineType? passedFilter = null;
            finesServiceMock.Setup(c => c.GetFinesAsync(It.IsAny<FineType?>()))
                    .Callback<FineType?>((val) => passedFilter = val);

            var httpContext = new DefaultHttpContext();
            if (!string.IsNullOrWhiteSpace(queryStringParameters))
            {
                httpContext.Request.QueryString = new QueryString("?" + queryStringParameters);
            }

            var finesController = new FinesController(finesServiceMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            ////Handle null values
            //int expectedInteger = -2;
            //if (expected == null)
            //    expectedInteger = -1;
            //else
            //    expectedInteger = (int)expected.Value;

            //Act
            var result = await finesController.GetFines();

            //Assert
            Assert.Equal(expected, passedFilter);



        }

    }
}

