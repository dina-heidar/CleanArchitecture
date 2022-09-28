using System.Security.Cryptography.X509Certificates;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.WebUIMvc.Filters;
using CleanArchitecture.WebUIMvc.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using WebUIMvc.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddWebUIMvcServices(this IServiceCollection services,
         IConfigurationRoot configuration, IHostEnvironment environment)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddControllersWithViews(options =>
            options.Filters.Add<ApiExceptionFilterAttribute>())
                .AddFluentValidation(x => x.AutomaticValidationEnabled = false);

        services.AddRazorPages();

        //****************************************************************************************

        services.AddAuthentication(sharedOptions =>
        {
            sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
          .AddSamlCore(options =>
          {
              // SignOutPath- The endpoint for the idp to perform its signout action. default is "/signedout"
              options.SignOutPath = "/signedout";

              // EntityId (REQUIRED) - The Relying Party Identifier 
              //must use the entityID that was in your metadata
              options.ServiceProvider.EntityId = configuration["AppConfiguration:ServiceProvider:EntityId"];

              // the adfs federated metadata url
              options.MetadataAddress = configuration["AppConfiguration:IdentityProvider:MetadataAddress"];

              // (REQUIRED IF) signing AuthnRequest with Sp certificate to Idp.
              // ADFS encrypts the incoming claims using this the cert public key
              // this app will use the private key of this cert to decrypt incoming claims 
              if (environment.IsDevelopment())
              { 
                  //make sure the path to it is correct. 
                  //password value is needed to access private keys for signature and decryption.
                  options.ServiceProvider.X509Certificate2 = new X509Certificate2(configuration["AppConfiguration:ServiceProvider:Certificate"],
                    "0n3wh33L", X509KeyStorageFlags.Exportable);
              }
              else
              {
                  //if you want to search in cert store by certificate serial number - can be used for production
                  options.ServiceProvider.X509Certificate2 = new Cryptography.X509Certificates.Extension.X509Certificate2(
                      configuration["AppConfiguration:ServiceProvider:CertificateSerialNumber"],
                      StoreName.My,
                      StoreLocation.LocalMachine,
                      X509FindType.FindBySerialNumber, false);
              }
            
              options.WantAssertionsSigned = true; 
              options.RequireMessageSigned = false;

              //Events - Modify events below if you want to log errors, add custom claims, etc.
              options.Events.OnRemoteFailure = context =>
           {
               return Task.FromResult(0);
           };
              options.Events.OnTicketReceived = context =>
              {
                  // you can add or remove claims here
                  // but if you need the db to get values then use a claimstransform instead
                  return Task.FromResult(0);
              };
          })
          .AddCookie();

        //name the cookie differntly for each environment
        //to avoid login conflict 
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = configuration["AppConfiguration:CookieName"];
        });

        services.AddTransient<IClaimsTransformation, WebUIMvcClaimsTransformation>();
        //****************************************************************************************

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        //open api stuff that we don't need since this is a monolithic app
        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "CleanArchitecture API";
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        return services;
    }
}
