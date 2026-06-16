using Fines.Core.Dtos;
using Fines.Core.Enums;

public interface IFinesService
{
    Task<IEnumerable<FinesResponse>> GetFinesAsync(FineType? typeFilter);
}
