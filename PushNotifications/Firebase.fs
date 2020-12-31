namespace Jottai

module private Firebase =
    open System
    open System.Collections.Generic
    open System.Net.Http
    open Newtonsoft.Json

    let StoredFirebaseKey() = Environment.GetEnvironmentVariable("JOTTAI_FCM_KEY")

    type SubscriptionChanges = 
        { SubscriptionsToBeRemoved : string list
          SubscriptionsToBeAdded : string list }

    let SoSubscriptionChanges =
        { SubscriptionsToBeRemoved = []
          SubscriptionsToBeAdded = [] }

    let private ShouldBeRemoved (result : FirebaseObjects.FirebaseResult * String) =
        let (firebaseResult, subscription) = result
        not(String.IsNullOrWhiteSpace(firebaseResult.registration_id)) || firebaseResult.error = "InvalidRegistration"
    
    let private GetSubscriptionChanges (subscriptions : string seq) (firebaseResponse : FirebaseObjects.FirebaseResponse)
        : Async<SubscriptionChanges> =

        async {         
            let firebaseResults = firebaseResponse.results |> Seq.toList
            let results = subscriptions |> Seq.toList |> List.zip firebaseResults

            let subscriptionsToBeRemoved =
                results
                |> List.filter ShouldBeRemoved
                |> List.map (fun result ->                        
                    let (firebaseResult, subscription) = result
                    subscription)

            let subscriptionsToBeAdded =
                firebaseResults
                |> List.map (fun result -> result.registration_id)
                |> List.filter (String.IsNullOrWhiteSpace >> not)

            return 
                { SubscriptionsToBeRemoved = subscriptionsToBeRemoved
                  SubscriptionsToBeAdded = subscriptionsToBeAdded }
        }
          
    let private SendMessages (httpSend : HttpRequestMessage -> Async<HttpResponseMessage>) (subscriptions : List<string>) (pushNotification : FirebaseObjects.FirebasePushNotification)
        : Async<SubscriptionChanges> =

        async {
            let storedFirebaseKey = StoredFirebaseKey()
            let url = "https://fcm.googleapis.com/fcm/send"
            let token = "key=" + storedFirebaseKey
                   
            let json = JsonConvert.SerializeObject pushNotification
            use requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            use content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")

            requestMessage.Content <- content
            requestMessage.Headers.TryAddWithoutValidation("Authorization", token) |> ignore
            
            let! response = httpSend requestMessage
            let! responseJson = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let firebaseResponse = JsonConvert.DeserializeObject<FirebaseObjects.FirebaseResponse> responseJson

            if not(firebaseResponse :> obj |> isNull) then
                return! GetSubscriptionChanges subscriptions firebaseResponse
            else
                return SoSubscriptionChanges

    }
    
    let private SendFirebaseMessages httpSend (subscriptions : List<string>) (pushNotification : FirebaseObjects.FirebasePushNotification) =
        async {
            let storedFirebaseKey = StoredFirebaseKey()
            if not(String.IsNullOrWhiteSpace(storedFirebaseKey)) then
                if subscriptions.Count > 0 then
                    return! SendMessages httpSend subscriptions pushNotification 
                else
                    return SoSubscriptionChanges
            else
                return SoSubscriptionChanges
        }

    type private DevicePushNotification =
        { DeviceId : string
          SensorName : string
          MeasuredProperty : string
          MeasuredValue : obj
          Timestamp : DateTimeOffset }
   
    type private PushNotificationReason =
        { SensorState : SensorState }
        
    let private SendFirebasePushNotifications httpSend reason =
        async {
            let measurement = reason.SensorState.Measurement
            let sensorName = reason.SensorState.SensorName
                
            let deviceGroupId = reason.SensorState.DeviceGroupId
            let! subscriptions = PushNotificationSubscriptionStorage.ReadPushNotificationSubscriptions deviceGroupId.AsString

            let pushNotification : DevicePushNotification =
                { DeviceId = reason.SensorState.DeviceId.AsString
                  SensorName = sensorName.AsString
                  MeasuredProperty = measurement |> Measurement.Name
                  MeasuredValue = measurement |> Measurement.Value
                  Timestamp = reason.SensorState.LastUpdated }
                
            let notification : FirebaseObjects.FirebaseDeviceNotificationContent =
                { deviceId = pushNotification.DeviceId
                  sensorName = pushNotification.SensorName
                  measuredProperty = pushNotification.MeasuredProperty
                  measuredValue = pushNotification.MeasuredValue
                  timestamp = pushNotification.Timestamp }

            let pushNotificationRequestData : FirebaseObjects.FirebasePushNotificationRequestData =
                { deviceNotification = notification }

            let pushNotification : FirebaseObjects.FirebasePushNotification =
                { data = pushNotificationRequestData
                  registration_ids = subscriptions }
                      
            let! subsriptionChanges = SendFirebaseMessages httpSend subscriptions pushNotification
            do! PushNotificationSubscriptionStorage.RemoveRegistrations deviceGroupId.AsString subsriptionChanges.SubscriptionsToBeRemoved
            do! PushNotificationSubscriptionStorage.AddRegistrations deviceGroupId.AsString subsriptionChanges.SubscriptionsToBeAdded
        }

    let private SendPushNotifications httpSend reason =
        async {
            do! SendFirebasePushNotifications httpSend reason
        }
        
    let Send httpSend (sensorState : SensorState) : Async<unit>=
        async {               
            let reason : PushNotificationReason =
                { SensorState = sensorState }
            do! SendPushNotifications httpSend reason
        }
