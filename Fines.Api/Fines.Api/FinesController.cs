using Fines.Core.Dtos;
using Fines.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;


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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FinesResponse>>> GetFines()
    {
        //Approach below is not recommended, better approach here would be to define model of filter parameters, and bind model parameters using [FromQuery] functionality

        //Ended up with below as struggled to get binding with [FromQuery] working in conjuction with unit testing approach adopted on the controller.  Have left as is as insufficient time to refactor and 
        //produce working solution

        //The standard approach would be to used model and validation attributes on that mode to validate the model state - raising appropriate response when model state wasn't valid
        //this would have also meant API would have presented better through swagger

        //With approach adopted wanted to ensure parameters were validated prior to passing through to service
        


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
        string? qStringRegFilter = HttpContext.Request.Query["vehicleregno"].ToString();

        string? fineRegFilter = null;
        if (!string.IsNullOrEmpty(qStringRegFilter))
        {
            //UrlDecode perhaps not nececesary here looks like framework takes care of it
            fineRegFilter = WebUtility.UrlDecode(qStringRegFilter);
        }

        var fines = await _finesService.GetFinesAsync(typeFilter: fineTypeFilter, dateFilter: fineDateFilter, registrationFilter: fineRegFilter);
        
        return Ok(fines);
    }

}
