
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority =
            "http://localhost:5001";

        options.Audience =
            "orders-api";

        options.RequireHttpsMetadata =
            false;
    });


builder.Services.AddAuthorization();


var app = builder.Build();


app.UseAuthentication();

app.UseAuthorization();


app.MapGet(
    "/api/orders",
    () =>
    {
        return Results.Ok(new
        {
            message = "You are authorized!",
            orders = new[]
            {
                "Order 1001",
                "Order 1002",
                "Order 1003"
            }
        });
    })
.RequireAuthorization();


app.Run();