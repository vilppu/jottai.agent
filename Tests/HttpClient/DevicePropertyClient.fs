namespace Jottai

[<AutoOpen>]
module DevicePropertyClient = 
    open Newtonsoft.Json    
        
    let GetDevicePropertiesResponse token : Async<System.Net.Http.HttpResponseMessage> = 
        let apiUrl = "api/device/properties"
        Http.Get token apiUrl
    
    let PostDevicePropertyValue token gatewayId deviceId propertyId propertyType propertyValue : Async<System.Net.Http.HttpResponseMessage> =
        let apiUrl = sprintf "api/gateway/%s/device/%s/property/%s/%s/value/%s" gatewayId deviceId propertyId propertyType propertyValue
        async {
            return! Http.Post token apiUrl ""          
        }
    
    let PostDevicePropertyName token gatewayId deviceId propertyId propertyType propertyName : Async<System.Net.Http.HttpResponseMessage> =
        let apiUrl = sprintf "api/gateway/%s/device/%s/property/%s/%s/name/%s" gatewayId deviceId propertyId propertyType propertyName
        async {
            return! Http.Post token apiUrl ""          
        }
    
    let PollDevicePropertyChangeRequest token : Async<ApiObjects.DevicePropertyChangeRequest> =

        let apiUrl = sprintf "api/device-property-change-request"
        let response = Http.Get token apiUrl
        async {
            let! content =  response |> Http.ContentOrFail
            let result = JsonConvert.DeserializeObject<ApiObjects.DevicePropertyChangeRequest>(content)
            return result
        }
    
    let GetDeviceProperties token : Async<ApiObjects.DeviceProperty list> = 
        let response = GetDevicePropertiesResponse token
        async { let! content = response |> Http.ContentOrFail
                let result = JsonConvert.DeserializeObject<List<ApiObjects.DeviceProperty>>(content)
                return result |> Seq.toList }
