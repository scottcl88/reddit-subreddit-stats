using Microsoft.Extensions.Configuration;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
builder.Services.AddSingleton<AppSettings>(appSettings);
builder.Services.AddHttpClient();
builder.Services.AddScoped<IBaseRedditService, BaseRedditService>();
builder.Services.AddScoped<IRedditAuthService, RedditAuthService>();
builder.Services.AddScoped<ISubRedditService, SubRedditService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
