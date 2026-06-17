using Fines.Api;
using Fines.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Diagnostics.Eventing.Reader;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;


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
            finesServiceMock.Setup(c => c.GetFinesAsync(It.IsAny<FineType?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>()))
                    .Callback<FineType?, DateOnly?, string?>((val, val2, val3) => passedFilter = val);

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


        [Theory]
        [InlineData("finedate=2021-01-02", "2021-01-02")]
        [InlineData("", null)]
        public async Task FinesController_CorrectDateFilter_passedToService(string queryStringParameters, string expectedDateStr)
        {

            //Arrange
            DateOnly? expectedDate = null;

            if (expectedDateStr != null)
            {
                string[] dateFormat = { "yyyy-MM-dd" };
                expectedDate = DateOnly.ParseExact(expectedDateStr, dateFormat);
            }
               
            var finesServiceMock = new Mock<IFinesService>();

            DateOnly? passedFilter = null;
            finesServiceMock.Setup(c => c.GetFinesAsync(It.IsAny<FineType?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>()))
                    .Callback<FineType?, DateOnly?, string?>((val, val2, val3) => passedFilter = val2);

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
            Assert.Equal(expectedDate, passedFilter);
        }


        [Theory]
        [InlineData("finedate=2020-02-01&finedate=2020-02-02", "Only one valid finedate filter is allowed in the query string.")]
        [InlineData("finedate=01/02/2020", "Invalid value specified for finedate must a valid date in form 'yyyy-MM-dd'")]
        [InlineData("finedate=2020-1-1", "Invalid value specified for finedate must a valid date in form 'yyyy-MM-dd'")]
        [InlineData("finedate=2020-30-10", "Invalid value specified for finedate must a valid date in form 'yyyy-MM-dd'")]
        public async Task FinesController_BadRequest_dateFormatIssue(string queryStringParameters, string expectedErrorMsg)
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
            Assert.Equal(expectedErrorMsg, ((BadRequestObjectResult)result.Result).Value);

        }


    }
}

