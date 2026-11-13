using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.DTOs.Catalog;
using Microsoft.EntityFrameworkCore;

namespace clinica_dental_api_cloud.Services;

public class CatalogService(ClinicaDbContext db) : ICatalogService
{
    public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken)
    {
        return await db.Services
            .AsNoTracking()
            .Where(s => s.Status == RecordStatus.Active)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceResponse(s.IdService, s.Name, s.Description, s.DurationMinutes, s.Price))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DentistResponse>> GetDentistsAsync(long? serviceId,
        CancellationToken cancellationToken)
    {
        var query = db.Dentists
            .AsNoTracking()
            .Where(d => d.Status == RecordStatus.Active && d.IdUserNavigation.Status == RecordStatus.Active);

        if (serviceId is not null)
            query = query.Where(d => d.DentistServices.Any(ds => ds.IdService == serviceId));

        return await query
            .OrderBy(d => d.IdUserNavigation.FullName)
            .Select(d => new DentistResponse(
                d.IdDentist,
                d.IdUserNavigation.FullName,
                d.IdSpecialtyNavigation.Name,
                d.LicenseNumber))
            .ToListAsync(cancellationToken);
    }
}
