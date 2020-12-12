namespace Jottai

[<AutoOpen>]
module DeviceSettingsClient = 
    open System
    open System.Threading
    
    let PostSensorName token sensorId name : Async<System.Net.Http.HttpResponseMessage>= 
        async {
            let apiUrl = sprintf "api/sensor/%s/name/%s" sensorId name          
            let! response = Http.Post token apiUrl ""

            return response
        }
    
    let PostSensorNameSynchronously token sensorId name : Async<System.Net.Http.HttpResponseMessage>= 
        async {
            use waiter = new SemaphoreSlim(0)
            use subscription = Application.Events.Subscribe(fun event ->
                match event with
                | Event.Event.SensorNameChangeCompleted _ -> waiter.Release() |> ignore
                | _ -> ()
                )
            
            let! response = PostSensorName token sensorId name

            waiter.Wait(TimeSpan.FromSeconds(10.0)) |> ignore

            return response
        }
    
    let ChangeSensorName token sensorId deviceSettings = 

        PostSensorNameSynchronously token sensorId deviceSettings
        |> Async.RunSynchronously
        |> ignore
