using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Data;
using SupportTicketSystem.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using SupportTicketSystem.Services;
using SupportTicketSystem.Hubs; // Add this import for TicketNotificationHub
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new MySqlServerVersion(new Version(8, 0, 29)))); // MySQL server version

// Add authentication (JWT) for token-based authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    })
    // Add OAuth authentication for Google and Facebook
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Google:ClientId"];
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"];
        options.CallbackPath = "/signin-google"; // The path where users will be redirected after a successful login
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["OAuth:Facebook:AppId"];
        options.AppSecret = builder.Configuration["OAuth:Facebook:AppSecret"];
        options.CallbackPath = "/signin-facebook"; // The path for the Facebook login callback
    });

// Add authorization services (Policies for roles)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Agent", policy => policy.RequireRole("Agent"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
});

// Add SignalR for real-time notifications
builder.Services.AddSignalR(); // Register SignalR services

// Register AIReplyService with OpenAI API key from configuration
builder.Services.AddSingleton<AIReplyService>(provider =>
    new AIReplyService(builder.Configuration["OpenAI:ApiKey"])); // Ensure you add your OpenAI key in appsettings.json

// Change this to Scoped or Transient service to resolve DbContext usage issue
builder.Services.AddScoped<TicketAssignmentService>(); // Change from AddHostedService to AddScoped or AddTransient

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Enable Swagger for dev environment
    app.UseSwaggerUI();  // Swagger UI for testing endpoints
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();  // Enables authentication (JWT, Google, Facebook)
app.UseAuthorization();   // Enables authorization based on roles

// Enable SignalR
app.MapHub<TicketNotificationHub>("/ticketNotificationHub"); // Map the SignalR Hub route

// Map controllers for API routes
app.MapControllers();

app.Run();
