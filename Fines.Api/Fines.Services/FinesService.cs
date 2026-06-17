using Fines.Core.Dtos;
using Fines.Core.Enums;
using Fines.Data.Models;

namespace Fines.Services;

public class FinesService : IFinesService
{
    private readonly IFinesRepository _finesRepository;

    public FinesService(IFinesRepository finesRepository)
    {
        _finesRepository = finesRepository;
    }

    public async Task<IEnumerable<FinesResponse>> GetFinesAsync(FineType? typeFilter = null, DateOnly? dateFilter = null)
    {
        var fines = await _finesRepository.GetAllFinesAsync();

        //Do filter C# side as it is more readily scaleable
        if (typeFilter != null)
        {
            fines = fines.Where(f => f.FineType == typeFilter.Value);   
        }

        return fines.Select(MapToResponse);
    }

    private static FinesResponse MapToResponse(FinesEntity fine)
    {
        return new FinesResponse
        {
            Id = fine.Id,
            FineNo = fine.FineNo,
            FineDate = fine.FineDate,
            FineType = fine.FineType,
            VehicleRegNo = fine.Vehicle.RegistrationNumber,
            VehicleDriverName = fine.VehicleDriverName,
            CustomerName = fine.Customer.CompanyName
        };
    }
}
