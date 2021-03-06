﻿namespace Jottai

module DevicePropertyTest =     
    open Xunit
    
    [<Fact>]
    let TellZWavePlusDeviceProperty() = 
        use context = SetupContext()
        
        let expecteGatewayId = "4035277665"
        let expectedDeviceId = "9"
        let expectedPropertyId = "0x0002000001dc8013"
        let expectedPropertyType = "TwoWaySwitch"
        let expectedPropertyName = "Switch"
        let expectedPropertyDescription = "Turn On/Off Device"
        let expectedPropertyValue = true :> obj
        let expectedProtocol = "Z-Wave Plus"
        let expectedTimestamp = new System.DateTimeOffset(2020, 12, 24, 13, 14, 15, 16, System.TimeSpan.Zero)

        let deviceDatum : ApiObjects.DeviceDatum =
            { Fake.ZWavePlusDevicePropertyDatum with                 
                  propertyId = expectedPropertyId
                  propertyType = ApiObjects.PropertyType.TwoWaySwitch
                  propertyName = expectedPropertyName
                  propertyDescription = expectedPropertyDescription                  
                  unitOfMeasurement = ""
                  valueType = ApiObjects.ValueType.Boolean
                  value = "True"
                  minimumValue = "0"
                  maximumValue = "0"
              }

        let deviceData = { 
            Fake.DeviceData with
                data = [deviceDatum]
                gatewayId = expecteGatewayId
                deviceId = expectedDeviceId
                protocol = ApiObjects.Protocol.ZWavePlus
                timestamp = expectedTimestamp.ToString("o")
            }

        PostDeviceData context.DeviceToken deviceData
        |> WaitUntilDevicePropertyIsUpdated

        let result = context |> DeviceProperties

        Assert.Equal(1, result.Length)
        Assert.Equal(context.DeviceGroupId, result.Head.DeviceGroupId)
        Assert.Equal(expecteGatewayId, result.Head.GatewayId)
        Assert.Equal(expectedDeviceId, result.Head.DeviceId)
        Assert.Equal(expectedPropertyId, result.Head.PropertyId)
        Assert.Equal(expectedPropertyType, result.Head.PropertyType)
        Assert.Equal(expectedPropertyName, result.Head.PropertyName)
        Assert.Equal(expectedPropertyDescription, result.Head.PropertyDescription)
        Assert.Equal(expectedPropertyValue, result.Head.PropertyValue)
        Assert.Equal(expectedProtocol, result.Head.Protocol)
        Assert.Equal(expectedTimestamp, result.Head.LastUpdated)
        Assert.Equal(expectedTimestamp, result.Head.LastActive)
        
    [<Fact>]
    let DevicePropertyValueWillBeOverriddenByNewValues() = 
        use context = SetupContext()
        
        let initialDeviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with
                value = "True"
        }

        let initialDeviceData = { 
            Fake.DeviceData with
                data = [initialDeviceDatum]
            }
        
        let updatedDeviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with
                value = "False"
        }

        let updatedDeviceData = { 
            Fake.DeviceData with
                data = [updatedDeviceDatum]
            }

        PostDeviceData context.DeviceToken initialDeviceData
        |> WaitUntilDevicePropertyIsUpdated

        PostDeviceData context.DeviceToken updatedDeviceData
        |> WaitUntilDevicePropertyIsUpdated

        let result = context |> DeviceProperties

        Assert.Equal(1, result.Length)
        Assert.Equal(false :> obj, result.Head.PropertyValue)
        
    [<Fact>]
    let ZWavePlusBinarySwitchOn() = 
        use context = SetupContext()
        
        let deviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with
                value = "True"
        }

        let deviceData = { 
            Fake.DeviceData with
                data = [deviceDatum]
            }
        PostDeviceData context.DeviceToken deviceData
        |> WaitUntilDevicePropertyIsUpdated

        let result = context |> DeviceProperties

        Assert.Equal(1, result.Length)
        Assert.Equal(true :> obj, result.Head.PropertyValue)
        
    [<Fact>]
    let ZWavePlusBinarySwitchOff() = 
        use context = SetupContext()
        
        let deviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with
                value = "False"
        }

        let deviceData = { 
            Fake.DeviceData with
                data = [deviceDatum]
            }

        PostDeviceData context.DeviceToken deviceData
        |> WaitUntilDevicePropertyIsUpdated

        let result = context |> DeviceProperties

        Assert.Equal(1, result.Length)
        Assert.Equal(false :> obj, result.Head.PropertyValue)

    [<Fact>]
    let DoNotUpdateSensorStateFromDevicePropertyThatIsNotSensor() = 
        use context = SetupContext()        

        let deviceDatum = Fake.ZWavePlusDevicePropertyDatum
        let deviceData = { 
            Fake.DeviceData with
                data = [deviceDatum]
            }

        PostDeviceData context.DeviceToken deviceData
        |> WaitUntilDevicePropertyIsUpdated
        
        let result = context |> SensorState

        Assert.Equal(0, result.Length)

    [<Fact>]
    let ExcludeCommandsWithUnkownCommandTypeId() = 
        use context = SetupContext()

        let deviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with
                propertyType = ApiObjects.PropertyType.TwoWaySwitch
            }

        let deviceData = { 
            Fake.DeviceData with
                data = [deviceDatum]
            }

        PostDeviceData context.DeviceToken deviceData |> Async.RunSynchronously |> ignore

        let result = context |> DeviceProperties

        Assert.Equal(0, result.Length)
    
    [<Fact>]
    let ZWavePlusDevicePropertyCanBeChanged() = 
        use context = SetupContext()
        
        let deviceId = "9"
        let gatewayId = "4035277665"
        let propertyId = "0x0002000001dc8013"
        let propertyType = "TwoWaySwitch"
        let propertyValue = "False"

        let deviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with 
                propertyId = propertyId
                value = "True"
        }

        let deviceData = { 
            Fake.DeviceData with
                gatewayId = gatewayId
                deviceId = deviceId
                data = [deviceDatum]
            }

        async {
            let! devicePropertyChangeRequest =
                PollDevicePropertyChangeRequest context.DeviceToken
                |> WaitUntilPollingDevicePropertyChangeRequests

            PostDeviceData context.DeviceToken deviceData
            |> WaitUntilDevicePropertyIsUpdated

            PostDevicePropertyValue context.DeviceToken gatewayId deviceId propertyId propertyType propertyValue
            |> Async.RunSynchronously
            |> ignore

            let! result = devicePropertyChangeRequest
        
            Assert.Equal("False" :> obj, result.PropertyValue)
        }
    
    [<Fact>]
    let ZWavePlusDevicePropertyNameCanBeChanged() = 
        use context = SetupContext()
        
        let deviceId = "9"
        let gatewayId = "4035277665"
        let expected = "ChangedPropertyName"

        let deviceDatum = {
            Fake.ZWavePlusDevicePropertyDatum with        
                propertyName = "InitialPropertyName"                
        }

        let deviceData = { 
            Fake.DeviceData with
                gatewayId = gatewayId
                deviceId = deviceId
                data = [deviceDatum]
            }

        async {
            PostDeviceData context.DeviceToken deviceData
            |> WaitUntilDevicePropertyIsUpdated

            PostDevicePropertyName context.DeviceToken deviceData.gatewayId deviceData.deviceId deviceDatum.propertyId "TwoWaySwitch" expected
            |> WaitUntilDevicePropertyNameIsChanged            

            PostDevicePropertyValue context.DeviceToken deviceData.gatewayId deviceData.deviceId deviceDatum.propertyId "TwoWaySwitch" "True"
            |> Async.RunSynchronously
            |> ignore
            
            PostDeviceData context.DeviceToken deviceData
            |> WaitUntilDevicePropertyIsUpdated
            
            let result = context |> DeviceProperties
            
            Assert.Equal(1, result.Length)
            Assert.Equal(expected, result.Head.PropertyName)
        }
    
    [<Fact>]
    let SwitchZWavePlusBinarySwitchOn() = 
        use context = SetupContext()        
        
        let propertyType = "TwoWaySwitch"
        let propertyValue = "True"

        async {
            let! devicePropertyChangeRequest =
                PollDevicePropertyChangeRequest context.DeviceToken
                |> WaitUntilPollingDevicePropertyChangeRequests

            PostDevicePropertyValue context.DeviceToken "1" "2" "3" propertyType propertyValue
            |> Async.RunSynchronously
            |> ignore

            let! result = devicePropertyChangeRequest
        
            Assert.Equal(propertyValue :> obj, result.PropertyValue)
        }
    
    [<Fact>]
    let SwitchZWavePlusBinarySwitchOff() = 
        use context = SetupContext()        
        
        let propertyType = "TwoWaySwitch"
        let propertyValue = "False"

        async {
            let! devicePropertyChangeRequest =
                PollDevicePropertyChangeRequest context.DeviceToken
                |> WaitUntilPollingDevicePropertyChangeRequests

            PostDevicePropertyValue context.DeviceToken "1" "2" "3" propertyType propertyValue
            |> Async.RunSynchronously
            |> ignore

            let! result = devicePropertyChangeRequest
        
            Assert.Equal(propertyValue :> obj, result.PropertyValue)
        }