using BCrypt.Net;
using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace clinica_dental_api_cloud.Services;

public class AuthService(ClinicaDbContext db, ITokenService tokenService) : IAuthService
{
    private const string ActiveStatus = "ACTIVE";

    // Verified when the email doesn't exist, so that case takes as long as a wrong password
    private static readonly string DummyHash = BCryptNet.HashPassword(Guid.NewGuid().ToString(), 11);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.IdRoleNavigation)
            // EF Core lo traduce a lower(email) = @email; string.Equals con StringComparison no se puede traducir
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        var validPassword = VerifyPassword(request.Password, user?.PasswordHash ?? DummyHash);

        if (user is null || !validPassword || user.Status != ActiveStatus)
            return null;

        var role = user.IdRoleNavigation.Name;
        var (token, expiresAt) = tokenService.CreateToken(user.IdUser, role);

        return new LoginResponse(token, expiresAt, new UserResponse(user.IdUser, user.FullName, user.Email, role));
    }

    public async Task<UserResponse?> GetUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.IdUser == userId && u.Status == ActiveStatus)
            .Select(u => new UserResponse(u.IdUser, u.FullName, u.Email, u.IdRoleNavigation.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCryptNet.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            // A stored value that isn't a bcrypt hash (e.g. the seed's HASH_DE_EJEMPLO) can't match
            return false;
        }
    }
}