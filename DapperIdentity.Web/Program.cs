using Common.Extensions;
using Common.Middlewares;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Dependency Injection Achieve through other extension class
builder.Services.AddInfrastructureToServiceContainer(builder.Configuration);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; // make url lower case
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();


// camel case the query key but leave the query value untouched
app.UseMiddleware<CamelCaseQueryKeysMiddleware>();

// add global exception middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();


app.MapControllerRoute(
    name: "default",
    pattern: "{area=Guest}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


app.Run();
