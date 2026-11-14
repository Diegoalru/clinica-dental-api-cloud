using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.DTOs.Patients;
using Microsoft.EntityFrameworkCore;

namespace clinica_dental_api_cloud.Services;

public class PatientService(ClinicaDbContext db) : IPatientService
{
    private const int MaxResults = 20;

    public async Task<IReadOnlyList<PatientResponse>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        var query = db.Patients
            .AsNoTracking()
            .Where(p => p.Status == RecordStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // % y _ son comodines de LIKE: se escapan para buscarlos como texto
            var escaped = search.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
            var namePattern = $"%{escaped}%";
            // La cédula se compara sin guiones a los dos lados: 900010001, 90001 y 9-0001-0001 encuentran a la
            // misma persona aunque se guarde como X-XXXX-XXXX (ver IdentificationFormat)
            var identificationPattern = $"%{escaped.Replace("-", "")}%";
            query = query.Where(p => EF.Functions.ILike(p.FullName, namePattern, @"\")
                                     || EF.Functions.ILike(p.Identification.Replace("-", ""),
                                         identificationPattern, @"\"));
        }

        return await query
            .OrderBy(p => p.FullName)
            .Take(MaxResults)
            .Select(p => new PatientResponse(
                p.IdPatient, p.FullName, p.Identification, p.Phone,
                p.Email ?? p.IdUserNavigation!.Email, p.IdUser))
            .ToListAsync(cancellationToken);
    }
}
