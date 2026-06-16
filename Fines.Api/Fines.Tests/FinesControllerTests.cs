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
        [InlineData("finetype=Speeding", FineType.Speeding)]
        [InlineData("finetype=Unknown", FineType.Unknown)]
        [InlineData("", null)]
        [InlineData("FType=Speeding", null)]
        [InlineData("FineType=Seeding", null)]
        [InlineData("finetype=johnwashere", null)]
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


            //Act
            var result = await finesController.GetFines();

            //Assert
            Assert.Equal(expected, passedFilter);
        }

        [Theory]
        [InlineData("finetype=Speeding&finetype=Unknown")]
        public async Task FinesController_BadRequest_mulitpleFineTypesInQString(string queryStringParameters)
        {

            //Arrange
            var finesServiceMock = new Mock<IFinesService>();

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


            //Act
            var result = await finesController.GetFines();

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Only one valid finetype filter is allowed in the query string.", ((BadRequestObjectResult)result.Result).Value);

        }
    }
}

