using Auth.Core.Configuration;
using Auth.Core.Entities;
using Auth.Core.Repository;
using Auth.Core.Service;
using Auth.Core.UnitofWork;
using Auth.Data;
using Auth.Data.Repositpries;
using Auth.Data.UnitOfWork;

using Auth.Service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedLibrary.Configurations;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//optionspattern
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));



builder.Services.Configure<Client>(builder.Configuration.GetSection("Clients"));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>(
    x =>

        x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), option =>
        {
            option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
        }
        ));

builder.Services.AddScoped<IUnitofWork, UnitOfWork>();

builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>
{
    Opt.User.RequireUniqueEmail = true;
    Opt.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme =JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => {

    var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {

        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        IssuerSigningKey=SignService.GetSymmetricSecurityKey(tokenOptions.Issuer),

        ValidateIssuerSigningKey = true,
        ValidateAudience=true,
        ValidateIssuer=true,
        ValidateLifetime=true,
        ClockSkew=TimeSpan.Zero
    };

});


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IAuthenricationService, Auth.Service.Services.AuthenticationService>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IUserService,UserService>();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});



app.Run();
