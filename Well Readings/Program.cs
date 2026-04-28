using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Hubs;
using Well_Readings.Middleware;
using Well_Readings.Services;
using Well_Readings.Services.Api;
using Well_Readings.Settings;

var builder = WebApplication.CreateBuilder(args);

//
// -------------------- SERVICES --------------------
//

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddScoped<ScadaRealtimeService>();
builder.Services.AddScoped<Well_Readings.Services.ScadaRealtimeService>();


builder.Services.AddScoped<DailyEntryService>();
builder.Services.AddScoped<IDailyEntryService, DailyEntryService>();

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//
// -------------------- HTTP CLIENTS (FIXED) --------------------
//

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiSettings:BaseUrl is missing. Add it to appsettings.json.");

builder.Services.AddHttpClient<WellApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ReportApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IReportsClient, ReportsClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

//
// -------------------- APP BUILD --------------------
//

var app = builder.Build();

//
// -------------------- DATABASE SEEDING --------------------
//

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate();

    var existingNames = context.Wells
        .Select(w => w.Name)
        .ToHashSet();

    var wellsToAdd = new List<Well>
    {
        new() { Id = Guid.NewGuid(), Name = "Reeves Well A" },
        new() { Id = Guid.NewGuid(), Name = "Reeves Well B" },
        new() { Id = Guid.NewGuid(), Name = "Park Well" },
        new() { Id = Guid.NewGuid(), Name = "Park Well A" },
        new() { Id = Guid.NewGuid(), Name = "Park Well B" },
        new() { Id = Guid.NewGuid(), Name = "Catawissa" },
        new() { Id = Guid.NewGuid(), Name = "Woods" },
        new() { Id = Guid.NewGuid(), Name = "New Well" },
        new() { Id = Guid.NewGuid(), Name = "Oakwood Well" },
        new() { Id = Guid.NewGuid(), Name = "Ray" }
    };

    var newWells = wellsToAdd
        .Where(w => !existingNames.Contains(w.Name))
        .ToList();

    if (newWells.Any())
    {
        context.Wells.AddRange(newWells);
        context.SaveChanges();
    }
}

//
// -------------------- MIDDLEWARE PIPELINE --------------------
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.UseRouting();
app.MapRazorPages();
app.MapHub<ScadaHub>("/hubs/scada");
app.MapHub<Well_Readings.Hubs.ScadaHub>("/ScadaHub");

app.Run();
