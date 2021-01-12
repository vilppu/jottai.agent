namespace Jottai

module SensorDataTest = 
    open System.Net
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open Xunit
    
    [<Fact>]
    let SensorKeyIsChecked() = 
        use context = SetupContext()
        let event = Fake.DeviceData |> WithMeasurement(Measurement.Temperature 25.5<C>)
        let response = PostDeviceData InvalidToken event |> Async.RunSynchronously
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
    
    // [<Fact>]
    // let AgentCanHandleLotsOfRequests() =
    //     use context = SetupContext()
    //     let timer = new System.Diagnostics.Stopwatch()
    //     let requestsPerBatch = 100

    //     timer.Start()

    //     let writeMeasurement = fun index ->
    //         async {
    //             let isEven x = (x % 2) = 0
    //             let even = isEven index         
    //             let example =
    //                 if even then Measurement.Contact Measurement.Open
    //                 else Measurement.Contact Measurement.Closed
    //             let! response = (context |> WriteMeasurement (Fake.Measurement example))
    //             response |> ignore
    //         }

    //     let batchWriteMeasurements = fun () ->
    //         [for i in 1 .. requestsPerBatch -> writeMeasurement i |> Async.RunSynchronously]            
    //         |> ignore
        
    //     batchWriteMeasurements()

    //     timer.Stop()
    //     Assert.True(timer.ElapsedMilliseconds < int64(10000))
    
    [<Theory>]
    [<InlineData("C", "3.4", "Temperature", 3.4)>]
    let SensorCanSendUnitOfMeasurementAsSymbol(symbol : string, sentValue : string, expectedType: string, expectedValue : obj) = 
        use context = SetupContext()
        
        let deviceDatum = { Fake.ZWavePlusDevicePropertyDatum with
                                                              propertyType = ApiObjects.PropertyType.Sensor
                                                              value = sentValue
                                                              unitOfMeasurement = symbol }
        let deviceData = { Fake.DeviceData with data = [deviceDatum] }

        PostDeviceData context.DeviceToken deviceData
        |> WaitUntilSensorStateIsChanged

        let result = (context |> SensorState).Head

        Assert.Equal(expectedType, result.MeasuredProperty)
        Assert.Equal(expectedValue, result.MeasuredValue)
        