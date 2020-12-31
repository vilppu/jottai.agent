namespace Jottai

module PushNotificationTests = 
    open Xunit

    let sentNotifications() =
        SentHttpRequestContents
        |> Seq.map (fun request -> request |> Newtonsoft.Json.JsonConvert.DeserializeObject<FirebaseObjects.FirebasePushNotification>)
        |> Seq.toList

    [<Fact>]
    let NotifyAboutContact() = 
        async {
            use context = SetupContext()
            let opened = Measurement.Contact Measurement.Open
            let closed = Measurement.Contact Measurement.Closed

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurement(Fake.Measurement opened) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement closed) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(2, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenContactChanges() = 
            async {
            use context = SetupContext()
            let opened = Measurement.Contact Measurement.Open

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurement(Fake.Measurement opened) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement opened) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let NotifyAboutPresenceOfWater() = 
        async {
            use context = SetupContext()
            let present = Measurement.PresenceOfWater Measurement.Present
            let notPresent = Measurement.PresenceOfWater Measurement.NotPresent

            context |> SetupToReceivePushNotifications
        
            context |> WriteMeasurement(Fake.Measurement present) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement notPresent) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement present) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(3, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenPresenceOfWaterChanges() = 
        async {
            use context = SetupContext()
            let present = Measurement.PresenceOfWater Measurement.Present

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurement(Fake.Measurement present) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement present) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let NotifyAboutMotion() = 
        async {
            use context = SetupContext()
            let motion = Measurement.Measurement.Motion Measurement.Motion
            let noMotion = Measurement.Measurement.Motion Measurement.NoMotion

            context |> SetupToReceivePushNotifications
        
            context |> WriteMeasurement(Fake.Measurement motion) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement noMotion) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement motion)|> WaitUntilPushNotificationsAreSent

            Assert.Equal(3, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenHasMotionChanges() = 
        async {
            use context = SetupContext()
            let motion = Measurement.Measurement.Motion Measurement.Motion

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurement(Fake.Measurement motion) |> WaitUntilPushNotificationsAreSent
            context |> WriteMeasurement(Fake.Measurement motion) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let SendSensorName() = 
        async {
            use context = SetupContext()
            let expectedName = "ExampleSensorName"

            context |> SetupToReceivePushNotifications
            
            context |> WriteMeasurement(Fake.Measurement (Measurement.Contact Measurement.Open)) |> WaitUntilPushNotificationsAreSent
            ChangeSensorName context.DeviceGroupToken "ExampleDevice.contact" expectedName
            context|> WriteMeasurement(Fake.Measurement (Measurement.Contact Measurement.Closed)) |> WaitUntilPushNotificationsAreSent

            Assert.Equal(expectedName, sentNotifications().[1].data.deviceNotification.sensorName)
        }   

    [<Fact>]
    let NotifyDevicePropertyChange() =
        async {
            use context = SetupContext()
            
            let switcthOn = { Fake.ZWavePlusDevicePropertyDatum with value = "True" }
            let switcthOff = { Fake.ZWavePlusDevicePropertyDatum with value = "False" }

            PostDeviceData context.DeviceToken { Fake.DeviceData with data = [switcthOn] }
            |> WaitUntilPushNotificationsAreSent

            PostDeviceData context.DeviceToken { Fake.DeviceData with data = [switcthOff] }
            |> WaitUntilPushNotificationsAreSent

            Assert.Equal(2, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }