using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Services.Service;
using PRN222_Restaurant.Repositories.IRepository;
using PRN222_Restaurant.Repositories.Repository;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Services.Service;
using PRN222_Restaurant.Hubs;
using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddServerSideBlazor();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add DbContextFactory for Blazor components to avoid concurrency issues
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)), ServiceLifetime.Scoped);

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình Authentication với JwtBearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };

        // Lấy token từ Session thay vì header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add your services
builder.Services.Configure<VNPayConfig>(builder.Configuration.GetSection("Vnpay"));
builder.Services.Configure<PointsConfig>(builder.Configuration.GetSection("Points"));
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IPointsService, PointsService>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationRepository>(provider =>
    new PRN222_Restaurant.Repositories.Repository.NotificationRepository(
        provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()));
builder.Services.AddScoped<INotificationService, PRN222_Restaurant.Services.Service.NotificationService>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationSessionService, ReservationSessionService>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PRN222_Restaurant.Helpers.NotificationHelper>();
builder.Services.AddScoped<AuthenticationStateProvider, PRN222_Restaurant.Services.CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IEmailService, EmailService>();
// Add Chat services
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// Add HttpClient
builder.Services.AddHttpClient();

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Add Order services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Đăng ký DI cho OrderService với đầy đủ dependency
builder.Services.AddScoped<IOrderService, OrderService>(provider =>
{
    var orderRepo = provider.GetRequiredService<IOrderRepository>();
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
    var tableService = provider.GetRequiredService<ITableService>();
    var notificationHelper = provider.GetRequiredService<PRN222_Restaurant.Helpers.NotificationHelper>();
    return new OrderService(orderRepo, dbContext, tableService, notificationHelper);
});

// Add Statistics service
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IStatisticsNotificationService, StatisticsNotificationService>();

// Add Hangfire
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// Session phải đứng trước Authentication & Authorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithRedirects("/");

// Map endpoints
app.MapRazorPages();
app.MapBlazorHub();
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToPage("/blazor/{*clientPath}", "/Blazor/_Host");
app.UseHangfireDashboard();
// Đăng ký background job tự động hủy đơn chưa thanh toán quá 10 phút
RecurringJob.AddOrUpdate<IOrderService>(
    "auto-cancel-unpaid-orders",
    service => service.AutoCancelUnpaidOrdersAsync(),
    Cron.Minutely
);

app.Run();
