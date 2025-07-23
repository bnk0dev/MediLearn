using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Services;
using Medilearn.Services.Interfaces;
using Medilearn.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. Localization ayarları ve desteklenen diller
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { "tr", "en", "fr" };
var cultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();

// 2. Scoped servisler ve bağımlılıklar
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseMaterialService, CourseMaterialService>();

// 3. DbContext ayarı
builder.Services.AddDbContext<MedilearnDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. MVC ve View Localization
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

// 5. Authentication (Cookie) ayarları
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

// 6. HttpContextAccessor mutlaka eklenmeli
builder.Services.AddHttpContextAccessor();

// 7. Dosya yükleme boyut limiti (max 10 MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

// 8. Localization middleware ayarları
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
};
app.UseRequestLocalization(localizationOptions);

// 9. Pipeline yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 10. Route tanımlaması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
