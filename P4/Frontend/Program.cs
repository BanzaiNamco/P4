using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpClientFactory for making HTTP requests to the auth API
builder.Services.AddHttpClient();

// Register session services to store the JWT token (or other data)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // Ensure the cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Make the session cookie essential for app functionality
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Send cookies only over HTTPS
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session expiration time
});


// Add authentication and authorization services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login"; // Define the login path for cookie-based authentication
    options.LogoutPath = "/logout"; // Define the logout path
})
.AddJwtBearer(options =>
{
    options.Authority = "https://localhost:8001"; // Auth server URL (make sure it's the right URL)
    options.Audience = "ArcherHub"; // Adjust according to your JWT config
    options.RequireHttpsMetadata = false; // Set to true in production for security

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();  // Avoid default redirect behavior
            context.Response.Redirect("/login"); // Redirect unauthorized users to login page
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(); // Add authorization to use the [Authorize] attribute
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Login/Error");
    app.UseHsts(); // Set HSTS for production
}

app.UseHttpsRedirection(); // Ensure the app uses HTTPS
app.UseStaticFiles(); // Serve static files (CSS, JS, images)

app.UseRouting(); // Enable routing for controllers
app.UseSession();
app.UseAuthentication();  // Add authentication middleware
app.UseAuthorization();   // Add authorization middleware

// Ensure session is available before authorization middleware


// Define default route for the app, mapping to the Login controller
app.MapControllerRoute(
    name: "rootRedirect",
    pattern: "", // Matches the root URL "/"
    defaults: new { controller = "Dashboard", action = "Index" }
);
app.MapGet("/", context =>
{
    context.Response.Redirect("/dashboard");
    return Task.CompletedTask;
});

// Example of an explicit redirect from root ("/") to dashboard

app.Run(); // Start the application
