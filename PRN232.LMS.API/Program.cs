using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Middleware;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;
using FluentValidation;
using PRN232.LMS.Services.Validation;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repository Layer ────────────────────────────────────────────────────────
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// ── Service Layer ───────────────────────────────────────────────────────────
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── FluentValidation ────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<StudentRequestValidator>();

// ── API Versioning ──────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── JWT Authentication ──────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "ThisIsAVeryLongAndSecureSecretKeyForPRN232Lab2LmsAPI!";
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "PRN232LmsAPI";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "PRN232LmsClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

// ── Controllers & Content Negotiation ───────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true; // Returns HTTP 406 for unsupported formats (e.g. Accept: text/plain)
    options.Filters.Add<PRN232.LMS.API.Filters.ValidationFilter>(); // Apply auto validation globally
})
.AddXmlDataContractSerializerFormatters() // XML support
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();

// ── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PRN232 LMS API v1",
        Version = "v1",
        Description = "PRN232 LAB 2 — Advanced REST API & Security (Version 1)"
    });
    c.SwaggerDoc("v2", new()
    {
        Title = "PRN232 LMS API v2",
        Version = "v2",
        Description = "PRN232 LAB 2 — Advanced REST API & Security (Version 2)"
    });

    // Swagger Authorize button config
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.TagActionsBy(api => new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;
        var versions = methodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<ApiVersionAttribute>()
            .SelectMany(a => a.Versions);

        var maps = methodInfo
            .GetCustomAttributes(true)
            .OfType<MapToApiVersionAttribute>()
            .SelectMany(a => a.Versions);

        var version = docName.ToLower();
        if (maps.Any())
        {
            return maps.Any(v => $"v{v}" == version);
        }
        return versions != null && versions.Any(v => $"v{v}" == version);
    });
});

// ── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ── Migrate & Seed on startup ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    await DbSeeder.SeedAsync(db);
}

// ── Middlewares ──────────────────────────────────────────────────────────────
app.UseRequestLogging(); // Custom request logging middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "PRN232 LMS API v2");
        c.RoutePrefix = string.Empty;
    });
}

app.UseGlobalExceptionHandler(); // Custom global exception handler middleware

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
