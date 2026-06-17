using Fines.Core.Dtos;
using Fines.Core.Enums;
using Microsoft.AspNetCore.Mvc;

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

        var fines = await _finesService.GetFinesAsync(typeFilter: fineTypeFilter, null);
        return Ok(fines);
    }

}
