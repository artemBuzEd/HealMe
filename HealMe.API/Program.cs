using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Modules.Doctors.Infrastructure;
using Modules.Identity.Core.Entities;
using Modules.Identity.Core.Interfaces;
using Modules.Identity.Infrastructure;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;
using Modules.Doctors.Infrastructure.Persistence;
using Modules.Patients.Infrastructure;
using Modules.Patients.Infrastructure.Persistence;
using Modules.AI.Infrastructure;
using Modules.AI.Infrastructure.Persistence;
using Modules.Appointments.Infrastructure;
using Modules.Appointments.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);


// Identity Module
builder.Services.AddIdentityModule();

// Doctor Module
// Doctor Module
builder.Services.AddDoctorModule(builder.Configuration);

// Patient Module
builder.Services.AddPatientModule(builder.Configuration);

// AI Module
builder.Services.AddAiModule(builder.Configuration);

// Appointments Module
builder.Services.AddAppointmentsModule(builder.Configuration);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") 
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
// Add services to the container.

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Modules.Identity.Api.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(Modules.Doctors.Api.Controllers.DoctorsController).Assembly)
    .AddApplicationPart(typeof(Modules.Patients.Api.Controllers.PatientsController).Assembly)
    .AddApplicationPart(typeof(Modules.AI.Api.Controllers.AiController).Assembly)
    .AddApplicationPart(typeof(Modules.Appointments.Api.Controllers.AppointmentsController).Assembly);

// Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    jwt.RequireHttpsMetadata = false;
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = false,
        ValidateLifetime = true
    };
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealMe API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
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
            new string[] {}
        }
    });
});

// Doctor Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorPolicy", policy => policy.RequireRole("Doctor"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    identityContext.Database.EnsureCreated();

    var doctorsContext = scope.ServiceProvider.GetRequiredService<DoctorsDbContext>();
    doctorsContext.Database.Migrate();

    var patientsContext = scope.ServiceProvider.GetRequiredService<PatientsDbContext>();
    patientsContext.Database.Migrate();

    var aiContext = scope.ServiceProvider.GetRequiredService<AiDbContext>();
    aiContext.Database.Migrate();

    var appointmentsContext = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
    appointmentsContext.Database.Migrate();
}

app.Run();