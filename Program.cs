using Data.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<NFCDbContext>(options =>
{
	options.UseSqlServer(connectionString);
});
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
builder.Services.AddControllersWithViews();

//Authentication/Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddQuartz(q =>
{
	q.UseMicrosoftDependencyInjectionJobFactory(); // Sử dụng dependency injection cho Job
	q.ScheduleJob<UploadNFCDataJob>(trigger => trigger
		.WithIdentity("UploadNFCDataJob", "default") // Identity cho Job
		.WithCronSchedule("0 0/1 * * * ?") // Chạy mỗi 5 phút
	);
});

// Thêm Quartz vào pipeline middleware
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

//Add Repositories
builder.Services.AddTransient<ISensorRepository, SensorRepository>();
builder.Services.AddTransient<IHearingRepository, HearingRepository>();
builder.Services.AddTransient<IKT_MIC_WF_SPLRepository, KT_MIC_WF_SPLRepository>();
builder.Services.AddTransient<IKT_TW_SPLRepository, KT_TW_SPLRepository>();
builder.Services.AddTransient<IProductionLineRepository, ProductionLineRepository>();

//Add Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<INFCService, NFCService>();



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
