namespace Jottai

[<AutoOpen>]
module Authorization = 
    open System
    open System.Linq
    open System.Text
    open System.IdentityModel.Tokens.Jwt
    open System.Security.Claims    
    open Microsoft.AspNetCore.Authorization
    open Microsoft.AspNetCore.Http
    open Microsoft.IdentityModel.Tokens
    
    module Roles = 
        [<Literal>]
        let None = "None"
        [<Literal>]
        let Administrator = "administrator"
        [<Literal>]
        let User = "user"
        [<Literal>]
        let Device = "device"

    let private TokenSecret() : string =
        let tokenSecret = Environment.GetEnvironmentVariable("JOTTAI_TOKEN_SECRET")
        if tokenSecret |> isNull then
            eprintfn "Environment variable JOTTAI_TOKEN_SECRET is not set."
            String.Empty
        else
            tokenSecret
    
    let SigningKey : SymmetricSecurityKey =
        let tokenSecret = TokenSecret()
        let tokenSecretBytes = Encoding.ASCII.GetBytes(tokenSecret)
        let signingKey = SymmetricSecurityKey(tokenSecretBytes)
        signingKey

    let SecureSigningCredentials = 
        let credentials = SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
        credentials
    
    let FindHeader headerName (request : HttpRequest) = 
        request.Headers
        |> Seq.toList
        |> List.filter (fun header -> header.Key = headerName)
        |> List.map (fun header -> 
               header.Value
               |> Seq.toList
               |> Seq.head)   

    let GetDeviceGroupId(user : ClaimsPrincipal) = 
        user.Claims
          .Where(fun claim -> claim.Type = "https://jottai.eu/claims/device-group-id")
          .Select(fun claim -> claim.Value)
          .DefaultIfEmpty("")
          .Single()

    let BuildRoleToken role deviceGroupId = 
        let roleClaim = Claim(ClaimTypes.Role, role)
        let deviceGroupIdClaim = Claim("DeviceGroupId", deviceGroupId)
        let claimsIdentity = ClaimsIdentity([roleClaim; deviceGroupIdClaim], "Jottai")
        let securityTokenDescriptor = SecurityTokenDescriptor()
        securityTokenDescriptor.Subject <- claimsIdentity
        securityTokenDescriptor.SigningCredentials <- SecureSigningCredentials
        let tokenHandler = JwtSecurityTokenHandler()
        let token = tokenHandler.CreateEncodedJwt(securityTokenDescriptor)
        token   
        
    let GenerateAdministratorToken() =
        BuildRoleToken Roles.Administrator ""

    let GenerateUserToken deviceGroupId = 
        BuildRoleToken Roles.User deviceGroupId
    
    let GenerateDeviceToken deviceGroupId = 
        BuildRoleToken Roles.Device deviceGroupId

    type PermissionRequirement(role) = 
        interface IAuthorizationRequirement
        member val Permission = role with get
    
    type PermissionHandler() = 
        inherit AuthorizationHandler<PermissionRequirement>()
        override __.HandleRequirementAsync(context : AuthorizationHandlerContext, requirement : PermissionRequirement) =

            let isInRequiredRole = context.User.IsInRole requirement.Permission
           
            if isInRequiredRole then
                context.Succeed requirement
                
            Threading.Tasks.Task.FromResult(0) :> Threading.Tasks.Task