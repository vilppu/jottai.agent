namespace Jottai

module PushNotificationTests = 
    open Xunit
    open System.Diagnostics

    let waitForNotifications expectedNumberOfNotifications =
        let stopwatch = new Stopwatch()
        let maxWaitTimeInSeconds = 10.0

        stopwatch.Start()

        async {
            while (SentHttpRequestContents.Count < expectedNumberOfNotifications) && (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds) do
                do!
                System.Threading.Tasks.Task.Delay(100)
                |> Async.AwaitTask
        }

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

            context |> WriteMeasurementSynchronously(Fake.Measurement opened)
            context |> WriteMeasurementSynchronously(Fake.Measurement closed)
        
            do! waitForNotifications(2)

            Assert.Equal(2, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenContactChanges() = 
            async {
            use context = SetupContext()
            let opened = Measurement.Contact Measurement.Open

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurementSynchronously(Fake.Measurement opened)        
            context |> WriteMeasurementSynchronously(Fake.Measurement opened)

            do! waitForNotifications(1)

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let NotifyAboutPresenceOfWater() = 
        async {
            use context = SetupContext()
            let present = Measurement.PresenceOfWater Measurement.Present
            let notPresent = Measurement.PresenceOfWater Measurement.NotPresent

            context |> SetupToReceivePushNotifications
        
            context |> WriteMeasurementSynchronously(Fake.Measurement present)
            context |> WriteMeasurementSynchronously(Fake.Measurement notPresent)
            context |> WriteMeasurementSynchronously(Fake.Measurement present)
        
            do! waitForNotifications(3)

            Assert.Equal(3, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenPresenceOfWaterChanges() = 
        async {
            use context = SetupContext()
            let present = Measurement.PresenceOfWater Measurement.Present

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurementSynchronously(Fake.Measurement present)
            context |> WriteMeasurementSynchronously(Fake.Measurement present)

            do! waitForNotifications(1)

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let NotifyAboutMotion() = 
        async {
            use context = SetupContext()
            let motion = Measurement.Measurement.Motion Measurement.Motion
            let noMotion = Measurement.Measurement.Motion Measurement.NoMotion

            context |> SetupToReceivePushNotifications
        
            context |> WriteMeasurementSynchronously(Fake.Measurement motion)
            context |> WriteMeasurementSynchronously(Fake.Measurement noMotion)
            context |> WriteMeasurementSynchronously(Fake.Measurement motion)
        
            do! waitForNotifications(3)

            Assert.Equal(3, SentHttpRequests.Count)
            Assert.Equal("https://fcm.googleapis.com/fcm/send", SentHttpRequests.[0].RequestUri.ToString())
        }

    [<Fact>]
    let NotifyOnlyWhenHasMotionChanges() = 
        async {
            use context = SetupContext()
            let motion = Measurement.Measurement.Motion Measurement.Motion

            context |> SetupToReceivePushNotifications

            context |> WriteMeasurementSynchronously(Fake.Measurement motion)
            context |> WriteMeasurementSynchronously(Fake.Measurement motion)

            do! waitForNotifications(1)

            Assert.Equal(1, SentHttpRequests.Count)
        }

    [<Fact>]
    let SendSensorName() = 
        async {
            use context = SetupContext()
            let expectedName = "ExampleSensorName"

            context |> SetupToReceivePushNotifications
            
            context |> WriteMeasurementSynchronously(Fake.Measurement (Measurement.Contact Measurement.Open))
            ChangeSensorName context.DeviceGroupToken "ExampleDevice.contact" expectedName
            context |> WriteMeasurementSynchronously(Fake.Measurement (Measurement.Contact Measurement.Closed))

            do! waitForNotifications(1)

            Assert.Equal(expectedName, sentNotifications().[1].data.deviceNotification.sensorName)
        }
   