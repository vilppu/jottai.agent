namespace Jottai

open System.Net.Http
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open ApiObjects

[<Route("api")>]
type ApiController(httpSend : HttpRequestMessage -> Async<HttpResponseMessage>) = 
    inherit Controller()
    member private this.DeviceGroupId = GetDeviceGroupId this.User
    
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
    member this.GetSensorState() : Async<ApiObjects.SensorState list> = 
        async {
            return! Application.GetSensorState this.DeviceGroupId
        }

    [<Route("sensor/{sensorId}/history/")>]
    [<HttpGet>]
    [<Authorize(Policy = Roles.User)>]
    member this.GetSensorHistory (sensorId : string) : Async<ApiObjects.SensorHistory> =
        async {
            return! Application.GetSensorHistory this.DeviceGroupId sensorId
        }
    
    [<Route("push-notifications/subscribe/{token}")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.User)>]
    member this.SubscribeToPushNotifications (token : string) : Async<unit> = 
        async {
            return! Application.SubscribeToPushNotifications this.DeviceGroupId token
        }
    
    [<Route("sensor-data")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.Sensor)>]
    member this.PostDeviceData([<FromBody>]deviceData : DeviceData) : Async<StatusCodeResult> =
        async {
            return! Application.PostDeviceData this.DeviceGroupId deviceData
            return this.StatusCode(StatusCodes.Status201Created)                
        }
