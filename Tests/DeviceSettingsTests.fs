namespace Jottai

module DeviceSettingsTest = 
    open System.Net
    open Xunit
    
    [<Fact>]
    let DeviceGroupTokenIsRequiredToSaveSensorName() = 
        use context = SetupContext()
        let deviceId = "ExampleDevice"      
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        async {
            let! response = PostSensorName InvalidToken deviceId "ExampleName"

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
        
    [<Fact>]
    let SensorNameCanBeChanged() = 
        use context = SetupContext()
        let expectedName = "ExampleSensorName"
        let deviceId = "ExampleDevice"
        let sensorId = "ExampleDevice.temperature"
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken sensorId expectedName

        let result = context |> GetExampleSensorState
        let entry = result.Head
        Assert.Equal(expectedName, entry.SensorName)
    
    
    [<Fact>]
    let MeasurementsDoNotMessUpSensorName() = 
        use context = SetupContext()
        let expectedName = "ExampleSensorName"
        let deviceId = "ExampleDevice"
        let sensorId = "ExampleDevice.temperature"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken sensorId expectedName
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        let result = context |> GetExampleSensorState
        let entry = result.Head
        Assert.Equal(expectedName, entry.SensorName)
    
    
    [<Fact>]
    let SensorCanBeRenamed() = 
        use context = SetupContext()
        let deviceId = "ExampleDevice"
        let sensorId = "ExampleDevice.temperature"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken sensorId "SensorA"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken sensorId "SensorB"

        let result = context |> GetExampleSensorState
        let entry = result.Head
        Assert.Equal("SensorB", entry.SensorName)
