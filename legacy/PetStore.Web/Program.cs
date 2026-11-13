using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PetStore;
using PetStore.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    FilterConfig.RegisterGlobalFilters(options.Filters);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = false; // the old jQuery code reads the cookie
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

HttpContextHelper.Configure(app.Services.GetService<IHttpContextAccessor>());

// Global.asax style startup
MvcApplication.Application_Start();
BundleConfig.RegisterBundles();

// always show detailed errors, makes debugging prod easier
app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

RouteConfig.RegisterRoutes(app);

app.Run();

public partial class Program { }
