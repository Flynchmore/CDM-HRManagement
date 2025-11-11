using CDM.HRManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register FirestoreStore
builder.Services.AddSingleton<FirestoreStore>();

var app = builder.Build();

// Optional: one-time migration if env var MIGRATE_IN_MEMORY = "true"
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("MigrateFromInMemory"))
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<FirestoreStore>();
    // Async call in fire-and-forget (for large data sets use a proper migration tool)
    _ = store.MigrateFromInMemoryAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/", () => Results.Redirect("/HR/Dashboard"));

app.Run();