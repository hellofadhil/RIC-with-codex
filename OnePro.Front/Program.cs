using Core.Helpers;
using OnePro.Front.Middleware;
using OnePro.Front.Services.Implement;
using OnePro.Front.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRicService, RicService>();
builder.Services.AddScoped<IRicRollOutService, RicRollOutService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();
HttpContextHelper.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseMiddleware<AuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
