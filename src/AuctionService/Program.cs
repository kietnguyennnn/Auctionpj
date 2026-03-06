using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();



app.UseAuthorization();

app.MapControllers();
try
{
    DbInitializer.InitData(app);
}
catch (Exception ex)
{
    throw new InvalidOperationException("Failed to initialize database", ex);
}
app.Run();
