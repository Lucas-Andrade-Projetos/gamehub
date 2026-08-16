using System.Text;
using System.Text.Json.Serialization;
using gameHubBack.Data;
using gameHubBack.Hubs;
using gameHubBack.Interfaces;
using gameHubBack.services;
using gameHubBack.Services.BatalhaRural;
using gameHubBack.Services.Rooms;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // builder.Services.AddSingleton<Users>();

        builder.Services.AddCors();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IBatalhaRuralGameService, GameService>();
        builder.Services.AddSingleton<IRoomRegistry, RoomRegistry>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var tokenKey = builder.Configuration["TokenKey"]
                ?? throw new Exception("TokenKey not found in configuration");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(x => x
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hubs/game");

        app.Run();
    }
}