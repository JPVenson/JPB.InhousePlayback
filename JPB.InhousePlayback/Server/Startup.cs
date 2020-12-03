using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Server.Auth;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.Thumbnail;
using JPB.InhousePlayback.Server.Services.TitleEnumeration;
using JPB.InhousePlayback.Server.Settings;
using MediaToolkit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JPB.InhousePlayback.Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			var dbCache = new DbConfig(true);

			services.AddTransient<DbService>(provider => new DbService(Configuration, dbCache));
			var ffpmegPath = Configuration["ffpmegPath"];
			services.AddMediaToolkit(ffpmegPath);
			services.AddSingleton<TitleEnumerationService>();
			services.AddSingleton<DbInitService>();
			services.AddSingleton<ThumbnailService>();
			services.AddScoped<IUserStore<AppUser>, AppUserStore>();
			services.AddScoped<IRoleStore<AppRole>, AppRoleStore>();

			services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));
			//services.AddSingleton<UserManager<AppUser>, AppUserManager>();
			//services.AddSingleton<RoleManager<AppRole>, AppRoleManager>();


			services.AddIdentity<AppUser, AppRole>()
				.AddUserStore<AppUserStore>()
				.AddUserManager<AppUserManager>()
				.AddRoleManager<AppRoleManager>()
				.AddSignInManager<AppUserSignInManager>()
				.AddDefaultTokenProviders();
			services.AddControllersWithViews();
			services.AddRazorPages();
			services.AddCors();
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			IdentityModelEventSource.ShowPII = true;
			services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.Events = new JwtBearerEvents();
					//options.Events.OnAuthenticationFailed = context =>
					//{
					//	Console.WriteLine();
					//	return Task.CompletedTask;
					//};
					//options.Events.OnChallenge = context =>
					//{
					//	Console.WriteLine();
					//	return Task.CompletedTask;
					//};
					//options.Events.OnForbidden = context =>
					//{
					//	Console.WriteLine();
					//	return Task.CompletedTask;
					//};
					//options.Events.OnMessageReceived = context =>
					//{
					//	Console.WriteLine();
					//	return Task.CompletedTask;
					//};
					//options.Events.OnTokenValidated = context =>
					//{
					//	Console.WriteLine();
					//	return Task.CompletedTask;
					//};

					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidIssuer = Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
						ValidateIssuer = false,
						ValidAudience = Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
						ValidateAudience = false,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
						ValidateIssuerSigningKey = false,
						ValidateLifetime = false,
					};
				});

			InitServices = services.Where(e =>
					typeof(IRequireInit).IsAssignableFrom(e.ImplementationType) &&
					e.Lifetime == ServiceLifetime.Singleton)
				.Select(f => f.ServiceType).ToArray();
			InitAsyncServices = services.Where(e =>
					typeof(IRequireInitAsync).IsAssignableFrom(e.ImplementationType) &&
					e.Lifetime == ServiceLifetime.Singleton)
				.Select(f => f.ServiceType).ToArray();
		}

		public Type[] InitAsyncServices { get; set; }
		public Type[] InitServices { get; set; }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			using (app.ApplicationServices.CreateScope())
			{
				foreach (var initService in InitServices)
				{
					(app.ApplicationServices.GetService(initService) as IRequireInit).Init();
				}

				foreach (var initService in InitAsyncServices)
				{
					(app.ApplicationServices.GetService(initService) as IRequireInitAsync).Init().GetAwaiter().GetResult();
				}
			}
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();
			
			app.UseRouting();
	
			app.UseCors(option => option
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader());
 
			app.UseAuthentication();
			app.UseAuthorization();


			app.UseEndpoints(endpoints =>
		{
			endpoints.MapRazorPages();
			endpoints.MapControllers();
			endpoints.MapFallbackToFile("index.html");
		});
		}
	}
}
