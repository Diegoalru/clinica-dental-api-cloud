using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.DTOs.Patients;
using Microsoft.EntityFrameworkCore;

namespace clinica_dental_api_cloud.Services;

public class PatientService(ClinicaDbContext db) : IPatientService
{
    private const string ActiveStatus = "ACTIVE";
    private const int MaxResults = 20;

    public async Task<IReadOnlyList<PatientResponse>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        var query = db.Patients
            .AsNoTracking()
            .Where(p => p.Status == ActiveStatus);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // % y _ son comodines de LIKE: se escapan para buscarlos como texto
            var escaped = search.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
            var pattern = $"%{escaped}%";
            query = query.Where(p => EF.Functions.ILike(p.FullName, pattern, @"\")
                                     || EF.Functions.ILike(p.Identification, pattern, @"\"));
        }

        return await query
            .OrderBy(p => p.FullName)
            .Take(MaxResults)
            .Select(p => new PatientResponse(p.IdPatient, p.FullName, p.Identification, p.Phone, p.Email))
            .ToListAsync(cancellationToken);
    }
}
