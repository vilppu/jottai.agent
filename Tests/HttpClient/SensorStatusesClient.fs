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
    
    let GetSensorHistoryResponse token propertyId = 
        let apiUrl = sprintf "api/sensor/%s/history" propertyId
        Http.Get token apiUrl
    
    let GetSensorHistory token propertyId = 
        let response = GetSensorHistoryResponse token propertyId
        async { let! content = response |> Http.ContentOrFail
                return JsonConvert.DeserializeObject<ApiObjects.SensorHistory>(content) }