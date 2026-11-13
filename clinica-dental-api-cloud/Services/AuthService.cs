using BCrypt.Net;
using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.Data.Entities;
using clinica_dental_api_cloud.DTOs.Auth;
using clinica_dental_api_cloud.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BCryptNet = BCrypt.Net.BCrypt;

namespace clinica_dental_api_cloud.Services;

public class AuthService(ClinicaDbContext db, ITokenService tokenService, ILogger<AuthService> logger) : IAuthService
{
    private const int BcryptWorkFactor = 11;

    // Se verifica cuando el correo no existe, para que ese caso tarde lo mismo que una contraseña incorrecta
    private static readonly string DummyHash = BCryptNet.HashPassword(Guid.NewGuid().ToString(), BcryptWorkFactor);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var account = await db.Users
            .AsNoTracking()
            // EF Core lo traduce a lower(email) = @email; string.Equals con StringComparison no se puede traducir
            .Where(u => u.Email.ToLower() == email)
            .Select(u => new
            {
                u.PasswordHash,
                u.Status,
                User = new UserResponse(u.IdUser, u.FullName, u.Email, u.IdRoleNavigation.Name,
                    u.Patient != null ? u.Patient.IdPatient : null,
                    u.Dentist != null ? u.Dentist.IdDentist : null)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var validPassword = VerifyPassword(request.Password, account?.PasswordHash ?? DummyHash);

        if (account is null || !validPassword || account.Status != RecordStatus.Active)
            return null;

        return CreateLoginResponse(account.User);
    }

    public async Task<UserResponse?> GetUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.IdUser == userId && u.Status == RecordStatus.Active)
            .Select(u => new UserResponse(u.IdUser, u.FullName, u.Email, u.IdRoleNavigation.Name,
                u.Patient != null ? u.Patient.IdPatient : null,
                u.Dentist != null ? u.Dentist.IdDentist : null))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LoginResponse> RegisterPatientAsync(RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var identification = IdentificationFormat.Normalize(request.Identification);

        if (await EmailInUseAsync(email, cancellationToken))
            throw ApiException.Conflict("El correo ya está registrado. Inicie sesión.");

        // Un paciente puede existir sin cuenta: recepción lo registra en persona o por teléfono
        var patient = await db.Patients
            .FirstOrDefaultAsync(p => p.Identification == identification, cancellationToken);

        var linkExistingPatient = patient is not null;
        if (patient is not null)
        {
            if (patient.IdUser is not null)
                throw ApiException.Conflict(
                    "La identificación ya tiene una cuenta registrada. Inicie sesión.");

            // Solo se vincula si el correo coincide con el que guardó recepción; si no, cualquiera que supiera
            // la cédula de otra persona podría quedarse con su historial
            if (patient.Status != RecordStatus.Active || patient.Email?.ToLowerInvariant() != email)
                throw ApiException.Conflict(
                    "La identificación ya está registrada en la clínica. Para activar su cuenta, comuníquese con recepción.");
        }

        var user = new User
        {
            IdRole = await GetRoleIdAsync(Roles.Patient, cancellationToken),
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = TrimOrNull(request.Phone),
            PasswordHash = BCryptNet.HashPassword(request.Password, BcryptWorkFactor),
            Status = RecordStatus.Active
        };
        db.Users.Add(user);

        if (patient is null)
        {
            patient = new Patient
            {
                IdUserNavigation = user,
                FullName = user.FullName,
                Identification = identification,
                BirthDate = request.BirthDate,
                Phone = user.Phone,
                Email = email,
                EmergencyContact = TrimOrNull(request.EmergencyContact),
                DataConsentAt = DateTime.UtcNow,
                Status = RecordStatus.Active
            };
            db.Patients.Add(patient);
        }
        else
        {
            // Se conserva la ficha que creó recepción: se le asocia la cuenta y solo se completan los datos
            // que estaban vacíos, sin sobrescribir lo que ya tenía
            patient.IdUserNavigation = user;
            patient.DataConsentAt ??= DateTime.UtcNow;
            patient.BirthDate ??= request.BirthDate;
            patient.Phone ??= user.Phone;
            patient.EmergencyContact ??= TrimOrNull(request.EmergencyContact);
        }

        // Un solo SaveChanges: el usuario y el paciente se guardan en la misma transacción
        if (!await TrySaveAsync(cancellationToken))
            throw ApiException.Conflict("El correo o la identificación ya están registrados.");

        logger.LogInformation("Cuenta de paciente creada: usuario {UserId}, paciente {PatientId}, vinculada {Linked}",
            user.IdUser, patient.IdPatient, linkExistingPatient);

        var response = new UserResponse(user.IdUser, user.FullName, user.Email, Roles.Patient,
            patient.IdPatient, null);
        return CreateLoginResponse(response);
    }

    public async Task<UserResponse> CreateStaffAsync(CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await EmailInUseAsync(email, cancellationToken))
            throw ApiException.Conflict("El correo ya está registrado.");

        var isDentist = request.Role == Roles.Dentist;
        var licenseNumber = request.LicenseNumber?.Trim();

        if (isDentist)
        {
            if (!await db.Specialties.AnyAsync(s => s.IdSpecialty == request.IdSpecialty, cancellationToken))
                throw ApiException.BadRequest("La especialidad no existe.");

            if (await db.Dentists.AnyAsync(d => d.LicenseNumber == licenseNumber, cancellationToken))
                throw ApiException.Conflict("El número de colegiado ya está registrado.");
        }

        var user = new User
        {
            IdRole = await GetRoleIdAsync(request.Role, cancellationToken),
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = TrimOrNull(request.Phone),
            PasswordHash = BCryptNet.HashPassword(request.Password, BcryptWorkFactor),
            Status = RecordStatus.Active
        };
        db.Users.Add(user);

        Dentist? dentist = null;
        if (isDentist)
        {
            dentist = new Dentist
            {
                IdUserNavigation = user,
                IdSpecialty = request.IdSpecialty!.Value,
                LicenseNumber = licenseNumber!,
                Status = RecordStatus.Active
            };
            db.Dentists.Add(dentist);
        }

        if (!await TrySaveAsync(cancellationToken))
            throw ApiException.Conflict("El correo o el número de colegiado ya están registrados.");

        logger.LogInformation("Cuenta de personal creada: usuario {UserId}, rol {Role}", user.IdUser, request.Role);

        return new UserResponse(user.IdUser, user.FullName, user.Email, request.Role, null, dentist?.IdDentist);
    }

    private LoginResponse CreateLoginResponse(UserResponse user)
    {
        var (token, expiresAt) = tokenService.CreateToken(user.Id, user.Role);
        return new LoginResponse(token, expiresAt, user);
    }

    private Task<bool> EmailInUseAsync(string email, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken);

    private Task<short> GetRoleIdAsync(string roleName, CancellationToken cancellationToken) =>
        db.Roles.Where(r => r.Name == roleName).Select(r => r.IdRole).SingleAsync(cancellationToken);

    /// <summary>
    ///     Guarda los cambios. Devuelve false si otra solicitud registró el mismo correo, cédula o colegiado justo
    ///     antes: las validaciones previas no lo pueden evitar, pero las restricciones UNIQUE de la base sí.
    /// </summary>
    private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCryptNet.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            // Un valor guardado que no es un hash de bcrypt (por ejemplo, HASH_DE_EJEMPLO del seed viejo) no coincide
            return false;
        }
    }
}
