﻿namespace YogRobot

module SensorDataTest = 
    open System.Net
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open Xunit
    
    [<Fact>]
    let SensorKeyIsChecked() = 
        use context = SetupContext()
        let event = Fake.SomeSensorData |> WithMeasurement(Temperature 25.5<C>)
        let response = PostSensorData (SensorKeyToken("12345")) context.DeviceGroupId event |> Async.RunSynchronously
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
    
