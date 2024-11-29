using Data.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NFC.Data;
using NFC.Data.Entities;
using NFC.RabbitMQ;
using NFC.Services;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<NFCDbContext>(options =>
	options.UseSqlServer(connectionString,
		b => b.MigrationsAssembly("Data")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
	using (var dbContext = scope.ServiceProvider.GetRequiredService<NFCDbContext>())
	{
		if (dbContext.Database.GetPendingMigrations().Any())
		{
			dbContext.Database.Migrate();
		}
	}
}

builder.Services.AddIdentity<NFCUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<NFCDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/Login";
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//Authentication/Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

//Add Repositories
builder.Services.AddTransient<ISensorRepository, SensorRepository>();
builder.Services.AddTransient<IHearingRepository, HearingRepository>();
builder.Services.AddTransient<IKT_MIC_WF_SPLRepository, KT_MIC_WF_SPLRepository>();
builder.Services.AddTransient<IKT_TW_SPLRepository, KT_TW_SPLRepository>();
builder.Services.AddTransient<IProductionLineRepository, ProductionLineRepository>();
builder.Services.AddTransient<IHistoryUploadRepository, HistoryUploadRepository>();
builder.Services.AddTransient<IIdentityRepository, IdentityRepository>();

//Add Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<INFCService, NFCService>();

//Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration.GetSection("Redis")["ConnectionString"].ToString();
});

//RabbitMQ
builder.Services.Configure<RabbitMQSetting>(builder.Configuration.GetSection("RabbitMq"));
var serviceProvider = builder.Services.BuildServiceProvider();
var settingRabbit = serviceProvider.GetService<IOptions<RabbitMQSetting>>();
var rabbitMqSetting = settingRabbit != null ? settingRabbit.Value : new RabbitMQSetting
{
	ConnectionString = "amqp://guest:guest@localhost:5672",
};
builder.Services.AddSingleton<IConnectionFactory>(x =>
{
	return new ConnectionFactory
	{
		Uri = new Uri(rabbitMqSetting.ConnectionString),
	};
});

builder.Services.AddMassTransit(x =>
{
	x.UsingRabbitMq((context, cfg) =>
	{
		var uri = rabbitMqSetting.ConnectionString;
		cfg.Host(uri, host =>
		{
			host.Username(rabbitMqSetting.UserName);
			host.Password(rabbitMqSetting.Password);
			host.Heartbeat(60);
		});

		cfg.ConfigureEndpoints(context);
		cfg.AutoStart = true;
		cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
