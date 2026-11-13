using System.Text;
using clinica_dental_api_cloud.Conventions;
using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.OpenApi;
using clinica_dental_api_cloud.Options;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

const string corsPolicy = "FrontendCorsPolicy";

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Get the connection string from the configuration
var connectionString = configuration.GetConnectionString("ClinicaDental")
                       ?? throw new InvalidOperationException(
                           "Falta la cadena de conexión 'ClinicaDental'. Defina ConnectionStrings__ClinicaDental.");

builder.Services.AddDbContext<ClinicaDbContext>(options => options.UseNpgsql(connectionString));

// Every controller answers under /api/v1/...
builder.Services.AddControllers(options =>
    options.Conventions.Add(new GlobalRoutePrefixConvention("api/v1")));

// Consistent error responses (RFC 7807)
builder.Services.AddProblemDetails();

// CORS limited to the configured origins
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT authentication
var jwtSection = configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new InvalidOperationException("Falta la clave JWT. Defina Jwt__Key.");

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = AuthClaims.Subject,
            RoleClaimType = AuthClaims.Role,
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// OpenAPI with the Bearer scheme so Scalar shows the token field
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecuritySchemeTransformer>();

    // Sin "servers", Scalar usa la dirección de la página. Detrás del ingress de Azure el API recibe HTTP,
    // y si el documento anunciara http:// el navegador bloquearía las pruebas desde la página HTTPS
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// La documentación queda disponible en todos los entornos (el enunciado la pide en el despliegue)
// y bajo /api/, igual que el resto del API
const string openApiRoute = "/api/openapi/{documentName}.json";
app.MapOpenApi(openApiRoute);
app.MapScalarApiReference("/api/scalar", options =>
{
    options.Title = "Clinica Dental API";
    options.OpenApiRoutePattern = openApiRoute;
    options.Theme = ScalarTheme.DeepSpace;
    options.ForceThemeMode = ThemeMode.Dark;
    options.AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName);
});

// En Azure el HTTPS termina en el ingress y el contenedor solo recibe HTTP por el puerto 8080
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();