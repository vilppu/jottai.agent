namespace Jottai

module SensorStateTests = 
    open System
    open System.Net
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open Xunit

    [<Fact>]
    let AuthenticationTokenIsChecked() = 
        use context = SetupContext()
        context.DeviceGroupToken <- InvalidToken

        let response = context |> SensorStateResponse

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

    [<Fact>]
    let ShouldAcceptJsonRequest() = 
        use context = SetupContext() 
        let example = Measurement.RelativeHumidity 78.0
        let (measurement, deviceId) = Fake.Measurement example
        let deviceDataJson = "{   \"gatewayId\": \"4035277665\",  \"deviceId\": \"9\",  \"protocol\": \"ZWavePlus\",  \"manufacturerName\": \"Telldus\",  \"batteryVoltage\": \"\",  \"rssi\": \"\",  \"timestamp\": \"2021-04-26T13:56:35Z\",  \"data\": [  {    \"propertyId\": \"155795472\",    \"propertyType\": \"TwoWaySwitch\",    \"propertyName\": \"Switch\",    \"propertyDescription\": \"Turn On/Off Device\",    \"unitOfMeasurement\": \"\",    \"valueType\": \"Boolean\",    \"value\": \"True\",    \"formattedValue\": \"True\",    \"minimumValue\": \"0\",    \"maximumValue\": \"0\"  }  ]}"

        WriteDeviceDataJsonSynchronously deviceDataJson context
        
    
    [<Fact>]
    let TellOnlyTheLatestMeasurentFromAnSensor() = 
        use context = SetupContext()
        let previous = Measurement.RelativeHumidity 80.0
        let newest = Measurement.RelativeHumidity 78.0
        context |> WriteMeasurementSynchronously(Fake.Measurement previous)
        context |> WriteMeasurementSynchronously(Fake.Measurement previous)
        context |> WriteMeasurementSynchronously(Fake.Measurement newest)        

        let result = context |> SensorState

        Assert.Equal(1, result.Length)
        Assert.Equal(78.0, result.Head.MeasuredValue :?> float)
    
    [<Fact>]
    let TellTheDeviceId() = 
        use context = SetupContext()
        let deviceId = "ExampleDevice"
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice deviceId)

        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal(deviceId, entry.DeviceId)
    
    [<Fact>]
    let TellTheLastActiveTimestamp() = 
        use context = SetupContext()
        let example = Measurement.RelativeHumidity 78.0
        context |> WriteMeasurementSynchronously(Fake.Measurement example)

        let result = context |> SensorState
        let entry = result.Head

        Assert.True((System.DateTimeOffset.UtcNow - entry.LastActive).TotalMinutes < 1.0)

    [<Fact>]
    let TellTheLastUpdatedTimestamp() = 
        use context = SetupContext()
        let example = Measurement.RelativeHumidity 78.0
        context |> WriteMeasurementSynchronously(Fake.Measurement example)

        let result = context |> SensorState
        let entry = result.Head

        Assert.True((System.DateTimeOffset.UtcNow - entry.LastUpdated).TotalMinutes < 1.0)

    [<Fact>]
    let TellTheLatestMeasurementFromEachKnownSensor() = 
        use context = SetupContext()
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice "device-1")
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice "device-2")
        context |> WriteMeasurementSynchronously(Fake.SomeMeasurementFromDevice "device-3")

        let result = context |> SensorState

        Assert.Equal(3, result.Length)
    
    [<Fact>]
    let TellDifferentKindOfMeasurementsFromSameDevice() = 
        use context = SetupContext()
        let example = Measurement.RelativeHumidity 78.0
        let anotherExample = Measurement.Temperature 25.0<C>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        context |> WriteMeasurementSynchronously(Fake.Measurement anotherExample)

        let result = context |> SensorState

        Assert.Equal(2, result.Length)
    
    [<Fact>]
    let TellOnlyOwnMeasurements() = 
        use context = SetupContext()

        context |> WriteMeasurementSynchronously(Measurement.Temperature 25.5<C> |> Fake.Measurement)

        context.DeviceGroupToken <- context.AnotherDeviceGroupToken
        let result = context |> SensorState

        Assert.Empty(result)  
  
    [<Fact>]
    let TellTemperature() = 
        use context = SetupContext()
        let example = Measurement.Temperature 25.0<C>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Temperature", entry.MeasuredProperty)
        Assert.Equal(25.0, entry.MeasuredValue :?> float)

    [<Fact>]
    let TellRelativeHumidity() = 
        use context = SetupContext()
        let example = Measurement.RelativeHumidity 78.0
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("RelativeHumidity", entry.MeasuredProperty)
        Assert.Equal(78.0, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellPresenceOfWater() = 
        use context = SetupContext()
        let example = Measurement.PresenceOfWater Measurement.Present
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("PresenceOfWater", entry.MeasuredProperty)
        Assert.Equal(true, entry.MeasuredValue :?> bool)        
    
    [<Fact>]
    let TellOpenDoor() = 
        use context = SetupContext()
        let example = Measurement.Contact Measurement.Open
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Contact", entry.MeasuredProperty)
        Assert.Equal(false, entry.MeasuredValue :?> bool)
    
    [<Fact>]
    let TellClosedDoor() = 
        use context = SetupContext()
        let example = Measurement.Contact Measurement.Closed
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Contact", entry.MeasuredProperty)
        Assert.Equal(true, entry.MeasuredValue :?> bool)
    
    [<Fact>]
    let TellLuminance() = 
        use context = SetupContext()
        let example = Measurement.Luminance 5.004<lx>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Luminance", entry.MeasuredProperty)
        Assert.Equal(5.0, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellSeismicIntensity() = 
        use context = SetupContext()
        let example = Measurement.SeismicIntensity 5.0<Measurement.MM>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("SeismicIntensity", entry.MeasuredProperty)
        Assert.Equal(5.0, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellAcceleration() = 
        use context = SetupContext()
        let example = Measurement.Acceleration 5.0<m/s^2>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Acceleration", entry.MeasuredProperty)
        Assert.Equal(5.0, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellVoltage() = 
        use context = SetupContext()
        let example = Measurement.Voltage 3.4<V>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Voltage", entry.MeasuredProperty)
        Assert.Equal(3.4, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellSignalStrength() = 
        use context = SetupContext()
        let example = Measurement.Rssi 3.4
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head

        Assert.Equal("Rssi", entry.MeasuredProperty)
        Assert.Equal(3.4, entry.MeasuredValue :?> float)
    
    [<Fact>]
    let TellBatteryVoltageOfDevice() = 
        use context = SetupContext() 
        let example = Measurement.RelativeHumidity 78.0
        let (measurement, deviceId) = Fake.Measurement example
        let deviceData =
            { Fake.DeviceData with batteryVoltage = "3.4" }
            |> WithMeasurement(measurement)

        WriteDeviceDataSynchronously deviceData context
        
        let result = context |> SensorState
        Assert.Equal(3.4, result.Head.BatteryVoltage)

    [<Fact>]
    let TellSignalStrengthOfDevice() = 
        use context = SetupContext() 
        let example = Measurement.RelativeHumidity 78.0
        let (measurement, deviceId) = Fake.Measurement example
        let deviceData =
            { Fake.DeviceData with rssi = "50.0" }
            |> WithMeasurement(measurement)

        WriteDeviceDataSynchronously deviceData context
        
        let result = context |> SensorState
        Assert.Equal(50.0, result.Head.SignalStrength)
  
    [<Fact>]
    let SensorNameIsByDefaultDeviceIdAndMeasuredPropertySeparatedByDot() = 
        use context = SetupContext()
        let example = Measurement.Temperature 25.0<C>
        context |> WriteMeasurementSynchronously(Fake.Measurement example)
        
        let result = context |> SensorState
        let entry = result.Head
        
        Assert.Equal("ExampleDevice.Temperature", entry.PropertyName)
