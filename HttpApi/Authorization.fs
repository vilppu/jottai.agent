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
        let Administrator = "RequiresMasterToken"
        [<Literal>]
        let User = "RequiresDeviceGroupToken"
        [<Literal>]
        let Sensor = "RequiresSensorToken"
    
    let SigningKey : SymmetricSecurityKey =
        let tokenSecret = Application.TokenSecret()
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
        user.Claims.Single(fun claim -> claim.Type = "DeviceGroupId").Value

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
        
    let GenerateMasterAccessToken() =
        BuildRoleToken Roles.Administrator ""

    let GenerateDeviceGroupAccessToken deviceGroupId = 
        BuildRoleToken Roles.User deviceGroupId
    
    let GenerateSensorAccessToken deviceGroupId = 
        BuildRoleToken Roles.Sensor deviceGroupId

    type PermissionRequirement(role) = 
        interface IAuthorizationRequirement
        member val Permission = role with get
    
    type PermissionHandler() = 
        inherit AuthorizationHandler<PermissionRequirement>()
        override __.HandleRequirementAsync(context : AuthorizationHandlerContext, requirement : PermissionRequirement) = 
            let isInRequiredRole = context.User.IsInRole requirement.Permission
           
            if isInRequiredRole then
                context.Succeed requirement
                
            System.Threading.Tasks.Task.FromResult(0) :> System.Threading.Tasks.Task
