namespace Jottai

[<AutoOpen>]
module SensorStateClient = 
    open Newtonsoft.Json
    
    let GetSensorStateResponse token = 
        let apiUrl = "api/sensors"
        Http.Get token apiUrl
    
    let GetSensorState token = 
        let response = GetSensorStateResponse token
        async { let! content = response |> Http.ContentOrFail
                let result = JsonConvert.DeserializeObject<List<ApiObjects.SensorState>>(content)
                return result |> Seq.toList }
    
    let GetSensorHistoryResponse token sensorId = 
        let apiUrl = sprintf "api/sensor/%s/history" sensorId
        Http.Get token apiUrl
    
    let GetSensorHistory token sensorId = 
        let response = GetSensorHistoryResponse token sensorId
        async { let! content = response |> Http.ContentOrFail
                return JsonConvert.DeserializeObject<ApiObjects.SensorHistory>(content) }