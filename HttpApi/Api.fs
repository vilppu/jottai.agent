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
            do! Application.PostSensorName httpSend this.DeviceGroupId sensorId sensorName
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
            return! Application.SubscribeToPushNotifications httpSend this.DeviceGroupId token
        }
    
    [<Route("sensor-data")>]
    [<HttpPost>]
    [<Authorize(Policy = Roles.Sensor)>]
    member this.PostSensorData([<FromBody>]sensorData : SensorData) : Async<StatusCodeResult> =
        async {
            return! Application.PostSensorData httpSend this.DeviceGroupId sensorData
            return this.StatusCode(StatusCodes.Status201Created)                
        }
