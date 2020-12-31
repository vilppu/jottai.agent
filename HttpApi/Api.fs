namespace Jottai

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open System.Net
open System.Net.Http
open ApiObjects

[<Route("api")>]
type ApiController (httpSend : HttpRequestMessage -> Async<HttpResponseMessage>) = 
    inherit Controller()
    member private this.DeviceGroupId =
        let deviceGroupId = GetDeviceGroupId this.User
        deviceGroupId

    [<Route("user/tokens/refresh-token/")>]
    [<HttpPost>]
    member this.GetRefreshToken ([<FromBody>]request : RefreshTokenRequest) : Async<ActionResult> = 
        async {
            let! refreshToken = Authentication.GetRefreshToken httpSend request.Code request.RedirectUri
            match refreshToken with
            | Some refreshToken -> return this.Json(refreshToken) :> ActionResult
            | None _ -> return this.StatusCode(int HttpStatusCode.BadRequest) :> ActionResult                
        }

    [<Route("user/tokens/access-token/")>]
    [<HttpPost>]
    member this.GetAccessToken ([<FromBody>]request : AccessTokenRequest) : Async<ActionResult> = 
        async {
            let! accessToken = Authentication.GetAccessToken httpSend request.RefreshToken ""
            match accessToken with
            | Some accessToken -> return this.Json(accessToken) :> ActionResult
            | None _ -> return this.StatusCode(int HttpStatusCode.BadRequest) :> ActionResult
        }

    [<Route("user/tokens/refresh-token/store/")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.StoreRefreshToken ([<FromBody>] token : RefreshToken) : Async<ActionResult> = 
        async {
            do! Authentication.StoreRefreshToken httpSend (this.User) (token.RefreshToken)
            return this.StatusCode(int HttpStatusCode.OK) :> ActionResult            
        }

    [<Route("device-group-id/new")>]
    [<HttpPost>]
    member this.NewGenerateDeviceGroupId() =
        Application.GenerateDeviceGroupId()
    
    [<Route("sensor/{sensorId}/name/{sensorName}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.PostSensorName (sensorId : string) (sensorName : string) : Async<StatusCodeResult> = 
        async {
            do! Application.PostSensorName this.DeviceGroupId sensorId sensorName
            return this.StatusCode(StatusCodes.Status202Accepted)   
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

    [<Route("gateway/{gatewayId}/device/{deviceId}/property/{propertyId}/{propertyType}/value/{propertyValue}")>]
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
    
    [<Route("gateway/{gatewayId}/device/{deviceId}/property/{propertyId}/{propertyType}/name/{propertyName}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.PostDevicePropertyName
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (propertyType : string)
        (propertyName : string)
        : Async<StatusCodeResult> = 
        async {
            do! Application.PostDevicePropertyName this.DeviceGroupId gatewayId deviceId propertyId propertyType propertyName
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
