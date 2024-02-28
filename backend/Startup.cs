using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace YSpotify;

public class Startup(IConfiguration Configuration)
{
	// private const string corsPolicyName = "_allowFromLocal";
	public void ConfigureServices(IServiceCollection services)
    {
		// services.AddCors(options =>
		// {
		// 	options.AddPolicy(name: corsPolicyName,
		// 		policy  =>
		// 		{
		// 			policy.WithOrigins("http://localhost");
		// 		});
		// });

		JwtOptions jwtOptions = Configuration.GetSection(JwtOptions.JwtOptionsLabel)
			.Get<JwtOptions>()!;
		services.AddSingleton(jwtOptions);

		SpotifyOptions spotifyOptions = Configuration.GetSection(SpotifyOptions.SpotifyOptionsLabel)
			.Get<SpotifyOptions>()!;
		services.AddSingleton(spotifyOptions);


		services.AddDbContext<AppDbContext>(
			opt => 
			{
				opt.UseNpgsql( Configuration.GetConnectionString("DefaultConnection") );
			}
		);


        services.AddControllers();

		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				In = ParameterLocation.Header,
				Description = "Please enter a valid token",
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				BearerFormat = "JWT",
				Scheme = "Bearer"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type=ReferenceType.SecurityScheme,
							Id="Bearer"
						}
					},
					[]
				}
			});

			string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			options.IncludeXmlComments( Path.Combine(AppContext.BaseDirectory, xmlFilename) );
		});

		services.AddAuthentication(options =>
		{
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		})
			.AddJwtBearer(options => 
			{			
				#if DEBUG
					options.RequireHttpsMetadata = false;
				#else
					options.RequireHttpsMetadata = true;
				#endif
				
				options.MapInboundClaims = false;
			
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ClockSkew = TimeSpan.Zero,
			
					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,
			
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,
			
					ValidateLifetime = true,
			
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = jwtOptions.GetSecurityKey()
				};
			});

		services.AddAuthorizationBuilder()
			.AddDefaultPolicy("Authenticated", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireClaim(JwtRegisteredClaimNames.Sub);
			});
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
		app.UseSwagger(options =>
		{
			options.RouteTemplate = "api-docs/{documentName}/swagger.json";
		});
		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/api-docs/v1/swagger.json", "v1");
			options.RoutePrefix = "api-docs";
		});

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
		app.UseCors(builder => builder
			.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()
		);

        app.UseHttpsRedirection();
        // app.UseStaticFiles(
        //     new StaticFileOptions
        //     {
        //         FileProvider = new PhysicalFileProvider( Path.Combine(Directory.GetCurrentDirectory(), "Resources") ),
        //         RequestPath = "/Resources"
        //     }
        // );


        app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}