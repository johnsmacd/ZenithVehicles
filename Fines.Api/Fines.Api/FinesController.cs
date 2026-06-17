using Fines.Core.Dtos;
using Fines.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

namespace Fines.Api;

[Route("api/[controller]")]
[ApiController]
public class FinesController : ControllerBase
{
    private readonly IFinesService _finesService;

    public FinesController(IFinesService finesService)
    {
        _finesService = finesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FinesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FinesResponse>>> GetFines()
    {
        //Not advised online, but not way to clearly unit bind using FromQuery and Unit test
        string? qStringFineTypeFilter = HttpContext.Request.Query["finetype"].ToString();

        FineType? fineTypeFilter = null;

        if(!string.IsNullOrEmpty(qStringFineTypeFilter))
        {
            //Only one valid filter is allowed in query string otherwies return BadRequest
            var validFineTypeFilterCount = 0;
            foreach (var value in qStringFineTypeFilter.Split(','))
            {
                if (Enum.TryParse<FineType>(qStringFineTypeFilter, true, out var parsedFineType)) 
                { 
                    fineTypeFilter = parsedFineType;
                    validFineTypeFilterCount++;
                }
            }

            if (validFineTypeFilterCount > 1)
                return BadRequest("Only one valid finetype filter is allowed in the query string.");

        }



        string? qStringDateFilter = HttpContext.Request.Query["finedate"].ToString();

        DateOnly? fineDateFilter = null;

        if (!string.IsNullOrEmpty(qStringDateFilter))
        {
            string[] dateFormat = { "yyyy-MM-dd" };
            if (!string.IsNullOrEmpty(qStringDateFilter))
            {
                if (qStringDateFilter.Split(',').Length > 1)
                {
                    return BadRequest("Only one valid finedate filter is allowed in the query string.");
                }

                if (DateOnly.TryParseExact(qStringDateFilter, dateFormat, out var dateValue))
                {
                    fineDateFilter = dateValue;
                } 
                else {
                    return BadRequest("Invalid value specified for finedate must a valid date in form 'yyyy-MM-dd'");
                }
            }
        }

        var fines = await _finesService.GetFinesAsync(typeFilter: fineTypeFilter, dateFilter: fineDateFilter, registrationFilter: null);
        
        return Ok(fines);
    }

}
