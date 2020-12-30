namespace Jottai

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Newtonsoft.Json
open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open ApiObjects

[<Route("api")>]
type ApiController (httpSend : HttpRequestMessage -> Async<HttpResponseMessage>) = 
    inherit Controller()
    member private this.DeviceGroupId =
        let deviceGroupId = GetDeviceGroupId this.User
        deviceGroupId

    [<Route("user/tokens/refresh-token/{code}/{redirectUri}")>]
    [<HttpPost>]
    member this.GetRefreshToken (code : string) (redirectUri : string) : Async<ActionResult> = 
        async {
            let url = sprintf "%soauth/token" (Application.Authority())
            use request = new HttpRequestMessage(HttpMethod.Post, url)
            let values = [
                KeyValuePair.Create("grant_type", "authorization_code")
                KeyValuePair.Create("client_id", Application.ClientId())
                KeyValuePair.Create("code",code)
                KeyValuePair.Create("redirect_uri", WebUtility.UrlDecode(redirectUri))
                ]

            request.Content <- new FormUrlEncodedContent(values |> List.toSeq)
            
            use! response = httpSend request

            if response.IsSuccessStatusCode then
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let tokenResponse = JsonConvert.DeserializeObject<OAuthObjects.TokenResponse>(json)
                return this.Json({ RefreshToken = tokenResponse.refresh_token }) :> ActionResult
            else
                return this.StatusCode(int response.StatusCode) :> ActionResult                
        }

    [<Route("user/tokens/access-token/{refreshToken}/{redirectUri}")>]
    [<HttpPost>]
    member this.GetAccessToken (refreshToken : string) (redirectUri : string) : Async<ActionResult> = 
        async {
            let url = sprintf "%soauth/token" (Application.Authority())
            use request = new HttpRequestMessage(HttpMethod.Post, url)
            let values = [
                KeyValuePair.Create("grant_type", "refresh_token")
                KeyValuePair.Create("client_id", Application.ClientId())
                KeyValuePair.Create("refresh_token", refreshToken)
                KeyValuePair.Create("redirect_uri", WebUtility.UrlDecode(redirectUri))
                ]

            request.Content <- new FormUrlEncodedContent(values |> List.toSeq)
            
            use! response = httpSend request

            if response.IsSuccessStatusCode then
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let tokenResponse = JsonConvert.DeserializeObject<OAuthObjects.TokenResponse>(json)
                let acccessToken = tokenResponse.access_token
                let jwtSecurityTokenHandler = new IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()                
                let token = jwtSecurityTokenHandler.ReadToken(acccessToken) :?> IdentityModel.Tokens.Jwt.JwtSecurityToken
                let expires = DateTimeOffset (DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc))
                return this.Json({ AccessToken = acccessToken
                                   Expires = expires }) :> ActionResult
            else
                return this.StatusCode(int response.StatusCode) :> ActionResult       
        }

    [<Route("device-group-id/new")>]
    [<HttpPost>]
    member this.NewGenerateDeviceGroupId() =
        Application.GenerateDeviceGroupId()
    
    [<Route("sensor/{sensorId}/name/{sensorName}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.PostSensorName (sensorId : string) (sensorName : string) : Async<unit> = 
        async {
            do! Application.PostSensorName this.DeviceGroupId sensorId sensorName
        }
    
    [<Route("sensors")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetSensorStates() : Async<ApiObjects.SensorState list> = 
        async {
            return! Application.GetSensorStates this.DeviceGroupId
        }

    [<Route("sensor/{sensorId}/history/")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetSensorHistory (sensorId : string) : Async<ApiObjects.SensorHistory> =
        async {
            return! Application.GetSensorHistory this.DeviceGroupId sensorId
        }
    
    [<Route("device/properties")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetDeviceProperties() : Async<ApiObjects.DevicePropertyState list> = 
        async {
            return! Application.GetDeviceProperties this.DeviceGroupId
        }
    
    //[<Route("device/{deviceId}/property/{devicePropertyId}/name/{devicePartName}")>]
    //[<HttpPost>]
    //[<Authorize(Policy = Roles.User)>]
    //member this.PostDevicePropertyName (deviceId : string) (devicePartId : string) (devicePartName : string) : Async<unit> = 
    //    async {
    //        do! Application.PostDevicePropertyName this.DeviceGroupId sensorId sensorName
    //    }

    [<Route("gateway/{gatewayId}/device/{deviceId}/property/{propertyId}/{propertyType}/{propertyValue}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.Device)>]
    member this.PostDevicePropertyValue
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (propertyType : string)
        (propertyValue : string)
        : Async<StatusCodeResult> =
        async {
            do! Application.PostDevicePropertyValue this.DeviceGroupId gatewayId deviceId propertyId propertyType propertyValue
            return this.StatusCode(StatusCodes.Status202Accepted)   
        }
    
    [<Route("push-notifications/subscribe/{token}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.SubscribeToPushNotifications (token : string) : Async<unit> = 
        async {
            return! Application.SubscribeToPushNotifications this.DeviceGroupId token
        }
    
    [<Route("device-data")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.Device)>]
    member this.PostDeviceData([<FromBody>]deviceData : DeviceData) : Async<StatusCodeResult> =
        async {
            do! Application.PostDeviceData this.DeviceGroupId deviceData
            return this.StatusCode(StatusCodes.Status201Created)                
        }
    
    [<Route("device-property-change-request")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.Device)>]
    member this.GetDevicePropertyChangeRequest() : Async<ActionResult> =
        async {
            let! devicePropertyChangeRequest = Application.GetDevicePropertyChangeRequest this.DeviceGroupId
            let result =
                match devicePropertyChangeRequest with
                | Some devicePropertyChangeRequest -> this.Json(devicePropertyChangeRequest) :> ActionResult
                | None -> this.StatusCode(StatusCodes.Status204NoContent) :> ActionResult

            return result
        }
