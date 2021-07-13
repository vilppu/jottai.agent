namespace Jottai

[<AutoOpen>]
module DeviceSettingsClient = 
    
    let PostSensorName token propertyId name : Async<System.Net.Http.HttpResponseMessage>= 
        async {
            let apiUrl = sprintf "api/sensor/%s/name/%s" propertyId name          
            let! response = Http.Post token apiUrl ""

            return response
        }
    
    let PostSensorNameSynchronously token propertyId name : Async<System.Net.Http.HttpResponseMessage>= 
        async {
            use waiter = new System.Threading.SemaphoreSlim(0)
            use _ = EventBus.Subscribe(fun event ->
                async {
                    match event with
                    | Event.Event.SensorNameStored _ -> waiter.Release() |> ignore
                    | _ -> ()                
                })
            
            let! response = PostSensorName token propertyId name

            waiter.Wait(System.TimeSpan.FromSeconds(1.0)) |> ignore

            return response
        }
    
    let ChangeSensorName token propertyId deviceSettings = 

        PostSensorNameSynchronously token propertyId deviceSettings
        |> Async.RunSynchronously
        |> ignore