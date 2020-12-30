namespace Jottai

[<AutoOpen>]
module SelfHost = 
    open System
    open System.IO
    open System.Net.Http
    open System.Threading.Tasks    
    open Microsoft.AspNetCore.Authorization
    open Microsoft.AspNetCore.Authentication.JwtBearer;
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.Mvc
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Logging
    open Microsoft.IdentityModel.Tokens
    open Newtonsoft.Json.Serialization
    open Options

    let private GetUrl() : Uri =
        
        let configuredUrl = Environment.GetEnvironmentVariable("JOTTAI_BASE_URL")
        let url = 
            if String.IsNullOrWhiteSpace(configuredUrl) then "http://localhost:18888/jottai"
            else configuredUrl

        new Uri(url)

    let private SigninKeyValidationParameters =                    
        let tokenValidationParameters = TokenValidationParameters()
        tokenValidationParameters.ValidateIssuerSigningKey <- true
        tokenValidationParameters.IssuerSigningKey <- SigningKey
        tokenValidationParameters.ClockSkew <- TimeSpan.Zero
        tokenValidationParameters.ValidateIssuer <- false
        tokenValidationParameters.ValidIssuer <- "NotUsed"
        tokenValidationParameters.ValidateAudience <- false
        tokenValidationParameters.ValidAudience <- "NotUsed"
        tokenValidationParameters.ValidateLifetime <- false 
        tokenValidationParameters
    
    type Startup(environment : IWebHostEnvironment, authenticationOptions : AuthenticationOptions) =
        member this.Configure(app : IApplicationBuilder,
                              env : IWebHostEnvironment,
                              loggerFactory : ILoggerFactory,
                              httpSend : HttpRequestMessage -> Async<HttpResponseMessage>) =

            app
                .UsePathBase(new Microsoft.AspNetCore.Http.PathString(GetUrl().PathAndQuery))
                .UseAuthentication()
                .UseCors(fun options ->
                    options
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader() |> ignore)
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)                
                |> ignore
             
            
        member this.ConfigureServices(services : IServiceCollection) =
            let configureJson (options : MvcNewtonsoftJsonOptions) = 
                options.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
            let configureJsonAction = new Action<MvcNewtonsoftJsonOptions>(configureJson)            
            
            services
                .AddLogging(fun options ->
                options
                  .AddConsole()
                  .SetMinimumLevel(LogLevel.Warning)
                  .AddFilter("Microsoft", LogLevel.Warning)
                  .AddFilter("System", LogLevel.Warning)
                  .AddFilter("Engine", LogLevel.Warning)
                  |> ignore)
                  |> ignore
            services
                .AddCors()
                .AddControllers()
                .AddNewtonsoftJson(configureJsonAction)
                |> ignore
            
            let configureAdminPolicy =
                let builder =
                    fun (policy : AuthorizationPolicyBuilder) ->
                        policy.Requirements.Add(PermissionRequirement(Roles.Administrator))
                new Action<AuthorizationPolicyBuilder>(builder)

            let configureUserPolicy =
                let builder =
                    fun (policy : AuthorizationPolicyBuilder) ->
                        policy.Requirements.Add(PermissionRequirement(Roles.User))
                new Action<AuthorizationPolicyBuilder>(builder)

            let configureSensorPolicy =
                let builder =
                    fun (policy : AuthorizationPolicyBuilder) ->
                        policy.Requirements.Add(PermissionRequirement(Roles.Device))
                new Action<AuthorizationPolicyBuilder>(builder)
            
            services.AddAuthorization(fun options ->
                options.AddPolicy(Roles.Administrator, configureAdminPolicy)
                options.AddPolicy(Roles.User, configureUserPolicy)
                options.AddPolicy(Roles.Device, configureSensorPolicy)
            ) |> ignore

            services
                .AddAuthentication(fun options -> 
                    options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
                    options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(fun options ->
                   match authenticationOptions with
                   | UseSigninKey _ ->
                        options.TokenValidationParameters <- SigninKeyValidationParameters
                   | UseAuthrority _ ->
                        options.Authority <- Application.Authority()
                        options.Audience <- Application.Audience())
                |> ignore

            services.AddSingleton<IAuthorizationHandler, PermissionHandler>()
            |> ignore

    let CreateHttpServer
        (authenticationOptions : AuthenticationOptions)
        (httpSend : HttpRequestMessage -> Async<HttpResponseMessage>)
        : Task = 

        let eventProcessor = Application.StartProcessingEvents httpSend
        let url = GetUrl()
        let host = url.Scheme + Uri.SchemeDelimiter + url.Host + ":" + url.Port.ToString()

        let host = 
            WebHostBuilder()
                .ConfigureServices(fun services -> 
                    services.AddSingleton(httpSend).AddSingleton(authenticationOptions)
                    |> ignore)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(host)
                .Build()

        Task.Run(fun () -> host.Run())
