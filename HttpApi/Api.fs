namespace Jottai

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open System.Linq
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
    member this.StoreRefreshToken ([<FromBody>] token : RefreshToken) : Async<unit> = 
        async {
            do! Authentication.StoreRefreshToken httpSend (this.User) (token.RefreshToken)                  
        }

    [<Route("device-group-id/new")>]
    [<HttpPost>]
    member this.NewGenerateDeviceGroupId() =
        Application.GenerateDeviceGroupId()
    
    [<Route("sensor/{propertyId}/name/{propertyName}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.PostSensorName (propertyId : string) (propertyName : string) : Async<unit> = 
        async {
            do! Application.PostSensorName this.DeviceGroupId propertyId propertyName
        }
    
    [<Route("sensors")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetSensorStates() : Async<ApiObjects.SensorState list> = 
        async {
            return! Application.GetSensorStates this.DeviceGroupId
        }

    [<Route("sensor/{propertyId}/history/")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetSensorHistory (propertyId : string) : Async<ApiObjects.SensorHistory> =
        async {
            return! Application.GetSensorHistory (DeviceGroupId this.DeviceGroupId) (PropertyId propertyId)
        }
    
    [<Route("device/properties")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetDeviceProperties() : Async<ApiObjects.DeviceProperty list> = 
        async {
            return! Application.GetDeviceProperties (DeviceGroupId this.DeviceGroupId)
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
        : Async<unit> =
        async {
            do! Application.PostDevicePropertyValue this.DeviceGroupId gatewayId deviceId propertyId propertyType propertyValue              
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
        : Async<unit> = 
        async {
            do! Application.PostDevicePropertyName this.DeviceGroupId gatewayId deviceId propertyId propertyType propertyName            
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
    member this.PostDeviceData([<FromBody>]deviceData : DeviceData) : Async<ActionResult> =
        async {
            if not this.ModelState.IsValid
            then
                let errors = this.ModelState.Values.SelectMany(fun value -> value.Errors.Select(fun e -> e.ErrorMessage))                                
                return this.BadRequest(errors) :> ActionResult
            elif deviceData :> obj |> isNull || deviceData.data :> obj |> isNull
            then
                return this.BadRequest() :> ActionResult            
            else
                do! Application.PostDeviceData this.DeviceGroupId deviceData
                return this.StatusCode(StatusCodes.Status201Created) :> ActionResult
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
