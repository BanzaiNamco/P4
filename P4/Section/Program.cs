using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Section.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddControllers();

// Add JWT authentication to validate the token passed to this service.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // For development, you can set RequireHttpsMetadata to false
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    // Use the same secret key and issuer/audience configuration from your Auth service
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])), // Same Secret Key as in Auth
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"], // Same Issuer as in Auth
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"] // Same Audience as in Auth
    };
});

// Add Swagger to your Course service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// Use Swagger UI to test your API (optional)
app.UseSwagger();
app.UseSwaggerUI();

// Enable HTTPS Redirection
app.UseHttpsRedirection();

// Use Authentication and Authorization middleware
app.UseAuthentication(); // To validate the token in each request
app.UseAuthorization();  // To check if the user has permission to access resources

// Map controllers
app.MapControllers();

app.Run();
