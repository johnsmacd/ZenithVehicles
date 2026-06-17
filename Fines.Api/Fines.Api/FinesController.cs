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
    [ProducesResponseType(typeof(IEnumerable<FinesResponse>), StatusCodes.Status200OK|StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FinesResponse>>> GetFines()
    {
        //Not advised online, though struggled in time to bind from query string to model using FromQuery and to also
        //Unit test then validation of inputs, passed through to Service. Future improvement would be to bind querystring to model and validate model state.
        //Likely required customer validators to perform some of below validation.

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

        //Pass raw registration filter to service layer - an area for future improvement wouls be some validation of entry number/letters only
        //Best place for validation perhaps on client side - though with limited experience with React in sufficient time to apply now & provide appropriate
        //Client side message to user.
        //Also need to consider the URL encoding of freetext on client into API query string, encoding on client and decoding here.
        string? qStringRegFilter = HttpContext.Request.Query["vehicleregno"].ToString();

        string? fineRegFilter = null;
        if (!string.IsNullOrEmpty(qStringRegFilter))
        {
            fineRegFilter = qStringRegFilter;
        }

        var fines = await _finesService.GetFinesAsync(typeFilter: fineTypeFilter, dateFilter: fineDateFilter, registrationFilter: fineRegFilter);
        
        return Ok(fines);
    }

}
