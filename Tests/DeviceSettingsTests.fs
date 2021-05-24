namespace Jottai

module SensorSettingsTest = 
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
        let propertyId = "ExampleDevice.temperature"
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken propertyId expectedName

        let result = context |> SensorState
        let entry = result.Head
        Assert.Equal(expectedName, entry.PropertyName)
    
    [<Fact>]
    let MeasurementsDoNotMessUpSensorName() = 
        use context = SetupContext()
        let expectedName = "ExampleSensorName"
        let deviceId = "ExampleDevice"
        let propertyId = "ExampleDevice.temperature"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken propertyId expectedName
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        let result = context |> SensorState
        let entry = result.Head
        Assert.Equal(expectedName, entry.PropertyName)
    
    
    [<Fact>]
    let SensorCanBeRenamed() = 
        use context = SetupContext()
        let deviceId = "ExampleDevice"
        let propertyId = "ExampleDevice.temperature"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken propertyId "SensorA"
        
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        ChangeSensorName context.DeviceGroupToken propertyId "SensorB"

        let result = context |> SensorState
        let entry = result.Head
        Assert.Equal("SensorB", entry.PropertyName)
