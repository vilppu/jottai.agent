﻿namespace Jottai

[<AutoOpen>]
module Authorization = 
    open System
    open System.Collections.Generic
    open System.Linq
    open System.Security.Cryptography
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
    
    let SigningKey=
        let secretKey = Application.StoredTokenSecret()
        SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))

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
    
    let private validMasterKeyHeaderIsPresent request =
        async {
            let headers = request |> FindHeader "jottai-key"
            match headers with
            | key :: _ ->                 
                return! Application.IsValidMasterKey key
            | [] -> return false
        }
    
    let private validDeviceGroupKeyHeaderIsPresent request = 
        async {
            let key = request |> FindHeader "jottai-device-group-key"
            let deviceGroupIds = request |> FindHeader "jottai-device-group-id"
        
            let headers = 
                if key.Length = deviceGroupIds.Length then key |> List.zip deviceGroupIds
                else []
            match headers with
            | head :: _ -> 
                let (deviceGroupId, key) = head
                let now = DateTime.UtcNow
                return! Application.IsValidDeviceGroupKey deviceGroupId key now
            | [] -> return false
        }

    let private validSensorDataKeyHeaderIsPresent request = 
        async {
            let key = request |> FindHeader "jottai-sensor-data-key"
            let deviceGroupIdHeader = request |> FindHeader "jottai-device-group-id"
            let botIdIdHeader = request |> FindHeader "jottai-bot-id"
            let deviceGroupIds = deviceGroupIdHeader |> List.append botIdIdHeader 
        
            let headers = 
                if key.Length = deviceGroupIds.Length then key |> List.zip deviceGroupIds
                else []
            match headers with
            | head :: _ -> 
                let (deviceGroupId, key) = head
                let now = DateTime.UtcNow
                return! Application.IsValidSensorKey deviceGroupId key now
            | [] -> return false
        }
    
    let MasterKeyIsMissing request =
        async {
            let! isPresent = validMasterKeyHeaderIsPresent request
            return not(isPresent)
        }

    let DeviceGroupKeyIsMissing request =
        async {
            let! isPresent = validDeviceGroupKeyHeaderIsPresent request
            return not(isPresent)
        }
    
    let SensorKeyIsMissing request =
        async {
            let! isPresent = validSensorDataKeyHeaderIsPresent request
            return not(isPresent)
        }

    let GetDeviceGroupId(user : ClaimsPrincipal) = 
        user.Claims.Single(fun claim -> claim.Type = "DeviceGroupId").Value    

    let FindDeviceGroupId request =
        let deviceGroupIdHeader = request |> FindHeader "jottai-device-group-id"
        let botIdIdHeader = request |> FindHeader "jottai-bot-id"
        let headers = deviceGroupIdHeader |> List.append botIdIdHeader 

        headers
        |> List.head 

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
