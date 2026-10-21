using System.Text;
using Gmach.API.Middleware;
using Gmach.API.Models;
using Gmach.Core.Interfaces;
using Gmach.Core.Settings;
using Gmach.Data.Context;
using Gmach.Data.Repositories;
using Gmach.Data.Seed;
using Gmach.Service.Mapping;
using Gmach.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;

// שורת אבחון: מופיעה בלוג של Render מיד עם עליית התהליך (לא כוללת סודות)
Console.WriteLine($"[startup] Gmach API starting. PORT={Environment.GetEnvironmentVariable("PORT") ?? "(none)"}");

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"[startup] Connection string configured: {!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("GmachDb"))}");

// ---------- לוגים: NLog במקום ספקי ברירת המחדל (הגדרות ב-nlog.config) ----------
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// ---------- בסיס נתונים ----------
builder.Services.AddDbContext<GmachDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("GmachDb")));

// ---------- Repositories ו-Services ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped<IRepaymentRepository, RepaymentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILoanRequestService, LoanRequestService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DbSeeder>();

// ה-hasher וה-token generator חסרי מצב ואין להם תלות ב-DbContext, לכן Singleton בטוח
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

// ---------- JWT ----------
// המפתח הוא סוד ומגיע מ-User Secrets (או ממשתנה סביבה בפריסה)
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
    throw new InvalidOperationException(
        "Missing or too short configuration value 'Jwt:Key' (at least 32 characters). " +
        "Set it with user-secrets (development) or an environment variable.");
builder.Services.AddSingleton(jwtSettings);

// ---------- ניקוד אמינות (ערכים ב-appsettings, ניתנים לשינוי בלי קומפילציה) ----------
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Scoring").Get<ScoringSettings>()
    ?? new ScoringSettings());

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // תשובות 401 ו-403 באותו פורמט JSON אחיד של שאר השגיאות
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return ErrorResponseWriter.WriteAsync(
                    context.HttpContext, StatusCodes.Status401Unauthorized,
                    "נדרשת התחברות.");
            },
            OnForbidden = context => ErrorResponseWriter.WriteAsync(
                context.HttpContext, StatusCodes.Status403Forbidden,
                "אין לך הרשאה לבצע פעולה זו.")
        };
    });
builder.Services.AddAuthorization();

// ---------- CORS: מאפשר ללקוח ה-React (כתובת אחרת) לפנות לשרת ----------
// הכתובות המותרות מגיעות מההגדרות (Cors:AllowedOrigins), ובפריסה ממשתנה סביבה
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------- Controllers ----------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // ולידציית ModelState (Data Annotations) מתבצעת אוטומטית בזכות [ApiController].
        // כאן רק קובעים שהתשובה תהיה בפורמט האחיד.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            return new BadRequestObjectResult(
                ErrorResponseWriter.Create(
                    context.HttpContext,
                    StatusCodes.Status400BadRequest,
                    "אחד או יותר מהשדות אינם תקינים.",
                    errors));
        };
    });

// ---------- Swagger (כולל כפתור Authorize ל-JWT) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "הדביקי כאן את ה-token שהתקבל בהתחברות."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// מחיל migrations ויוצר נתוני התחלה (מנהל, לקוח לדוגמה, קטגוריות, הלוואות).
// רץ ברקע מיד אחרי שהשרת התחיל להאזין, כדי שפלטפורמת הפריסה (Render) תראה פורט פתוח
// גם כשמסד הנתונים איטי, וכדי ששגיאת חיבור תיכתב בלוג ולא תשאיר את השרת תקוע בשקט.
app.Lifetime.ApplicationStarted.Register(() => _ = Task.Run(async () =>
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        logger.LogInformation("Applying migrations and seeding the database...");
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        var seedSettings = app.Configuration.GetSection("Seed").Get<SeedSettings>()
            ?? new SeedSettings();
        await seeder.SeedAsync(seedSettings);
        logger.LogInformation("Database is ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed FAILED.");
    }
}));

// ---------- ה-pipeline. הסדר חשוב! ----------
// 0. CORS ראשון, כדי שגם תשובות שגיאה (כמו 409) יכללו את כותרות ה-CORS והדפדפן יאפשר ללקוח לקרוא אותן
app.UseCors("Client");

// 1. טיפול בשגיאות ראשון, כדי לתפוס שגיאות מכל מה שאחריו
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. מזהה לכל בקשה ולוג של הבקשה, כך שכל שורה בלוג שייכת לבקשה מסוימת
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger פעיל בפיתוח, ובפריסה אם מגדירים Swagger__Enabled=true (כדי שהמורה יוכל לנסות את ה-API)
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// בפריסה (Render) ה-HTTPS מסתיים בשרת הקדמי של Render, והאפליקציה מקבלת HTTP פנימי.
// לכן ההפניה ל-HTTPS נשארת רק בפיתוח המקומי.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 3. Authentication (מי אתה?) תמיד לפני Authorization (מה מותר לך?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
