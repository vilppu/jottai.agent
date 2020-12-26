﻿namespace Jottai

module SensorDataTest = 
    open System.Net
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open Xunit
    
    [<Fact>]
    let SensorKeyIsChecked() = 
        use context = SetupContext()
        let event = Fake.DeviceData |> WithMeasurement(Measurement.Temperature 25.5<C>)
        let response = PostDevicData InvalidToken event |> Async.RunSynchronously
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
    
    [<Fact>]
    let AgentCanHandleLotsOfRequests() =
        use context = SetupContext()
        let timer = new System.Diagnostics.Stopwatch()
        let requestsPerBatch = 100

        timer.Start()

        let writeMeasurement = fun index ->
            async {
                let isEven x = (x % 2) = 0
                let even = isEven index         
                let example =
                    if even then Measurement.Contact Measurement.Open
                    else Measurement.Contact Measurement.Closed
                let! response = (context |> WriteMeasurement (Fake.Measurement example))
                response |> ignore
            }

        let batchWriteMeasurements = fun () ->
            [for i in 1 .. requestsPerBatch -> writeMeasurement i |> Async.RunSynchronously]            
            |> ignore
        
        batchWriteMeasurements()

        timer.Stop()
        Assert.True(timer.ElapsedMilliseconds < int64(10000))
        