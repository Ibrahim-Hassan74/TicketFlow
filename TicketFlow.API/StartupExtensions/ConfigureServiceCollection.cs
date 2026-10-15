using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using TicketFlow.API.Exceptions;
using TicketFlow.API.Services;
using TicketFlow.Application.Common.Interfaces;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using TicketFlow.Infrastructure.Persistence;

namespace TicketFlow.API.StartupExtensions
{
    /// <include file='../Docs/ConfigureServicesDocs.xml' path='docs/members[@name="ConfigureServiceCollection"]/Class/*' />
    public static class ConfigureServiceCollection
    {
        /// <include file='../Docs/ConfigureServicesDocs.xml' path='docs/members[@name="ConfigureServiceCollection"]/ConfigureServices/*' />
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddProblemDetails();

            services.AddSignalR();

            services.AddHttpContextAccessor();

            services.AddMemoryCache();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    policy
                        .WithOrigins(allowedOrigins)
                        .WithHeaders("Content-Type", "Authorization", "Accept", "Accept-Language")
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(
                    System.IO.Path.Combine(System.AppContext.BaseDirectory, "TicketFlow.API.xml"),
                    includeControllerXmlComments: true
                );

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });

                options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo {  Title = "Ticket Flow Web API", Version = "1.0" });

                options.SwaggerDoc("v2", new Microsoft.OpenApi.OpenApiInfo { Title = "Ticket Flow Web API", Version = "2.0" });
                
                options.UseInlineDefinitionsForEnums();

            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            // JWT Authentication
            var jwtKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.");
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudiences = configuration.GetSection("Jwt:Audiences").Get<List<string>>() ?? [];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtIssuer,
                    ValidAudiences = jwtAudiences,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                        var detail = context.AuthenticateFailure is SecurityTokenExpiredException
                            ? "The access token has expired. Please refresh your token."
                            : "You are not authorized to access this resource. A valid Bearer token is required.";

                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = new ProblemDetails
                            {
                                Title = "Unauthorized",
                                Status = StatusCodes.Status401Unauthorized,
                                Detail = detail,
                                Instance = context.HttpContext.Request.Path
                            }
                        });
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = new ProblemDetails
                            {
                                Title = "Forbidden",
                                Status = StatusCodes.Status403Forbidden,
                                Detail = "You do not have permission to access this resource.",
                                Instance = context.HttpContext.Request.Path
                            }
                        });
                    }
                };
            });

            // Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(UserRole.Admin), policy =>
                    policy.RequireRole(nameof(UserRole.Admin)));

                options.AddPolicy(nameof(UserRole.SupportAgent), policy =>
                    policy.RequireRole(nameof(UserRole.SupportAgent)));

                options.AddPolicy(nameof(UserRole.Customer), policy =>
                    policy.RequireRole(nameof(UserRole.Customer)));

                options.AddPolicy("NotAuthorized", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return !(context.User?.Identity?.IsAuthenticated ?? false);
                    });
                });
            });



            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddHttpClient();

            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 15,
                            Window = TimeSpan.FromSeconds(1),
                            AutoReplenishment = true,
                            QueueLimit = 0
                        });
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context.HttpContext,
                        ProblemDetails = new ProblemDetails
                        {
                            Title = "Too Many Requests",
                            Status = StatusCodes.Status429TooManyRequests,
                            Detail = "Rate limit exceeded. Please try again later.",
                            Instance = context.HttpContext.Request.Path
                        }
                    });
                };
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Title = "Validation Failed",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more validation errors occurred.",
                        Instance = context.HttpContext.Request.Path
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}
