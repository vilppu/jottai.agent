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
          PropertyName : string
          MeasuredProperty : string
          MeasuredValue : obj
          Timestamp : DateTimeOffset }
   
    type private PushNotificationReason =
        | SensorStatePushNotification of SensorState
        | DevicePropertyStatePushNotification of DevicePropertyState

    let private DeviceGroupId reason =
        match reason with
        | SensorStatePushNotification state -> state.DeviceGroupId
        | DevicePropertyStatePushNotification state -> state.DeviceGroupId

    let private PushNotification reason =
        match reason with
        | SensorStatePushNotification state ->

            let (DeviceId deviceId) = state.DeviceId
            let (PropertyName propertyName) = state.PropertyName

            let pushNotification : DevicePushNotification =
                { DeviceId = deviceId
                  PropertyName = propertyName
                  MeasuredProperty = state.Measurement |> Measurement.Name
                  MeasuredValue = state.Measurement |> Measurement.Value
                  Timestamp = state.LastUpdated }
            pushNotification
        | DevicePropertyStatePushNotification state ->

            let (DeviceId deviceId) = state.DeviceId
            let (PropertyName propertyName) = state.PropertyName
            
            let pushNotification : DevicePushNotification =
                { DeviceId = deviceId
                  PropertyName = propertyName
                  MeasuredProperty = state.PropertyValue |> DeviceProperty.Name
                  MeasuredValue = state.PropertyValue |> DeviceProperty.Value
                  Timestamp = state.LastUpdated }
            pushNotification
        
    let private SendFirebasePushNotifications httpSend reason =
        async {
            let (DeviceGroupId deviceGroupId) =  DeviceGroupId reason
            let! subscriptions = PushNotificationSubscriptionStorage.ReadPushNotificationSubscriptions deviceGroupId

            let pushNotification = PushNotification reason
                
            let notification : FirebaseObjects.FirebaseDeviceNotificationContent =
                { deviceId = pushNotification.DeviceId
                  propertyName = pushNotification.PropertyName
                  measuredProperty = pushNotification.MeasuredProperty
                  measuredValue = pushNotification.MeasuredValue
                  timestamp = pushNotification.Timestamp }

            let pushNotificationRequestData : FirebaseObjects.FirebasePushNotificationRequestData =
                { deviceNotification = notification }

            let pushNotification : FirebaseObjects.FirebasePushNotification =
                { data = pushNotificationRequestData
                  registration_ids = subscriptions }
                      
            let! subsriptionChanges = SendFirebaseMessages httpSend subscriptions pushNotification
            do! PushNotificationSubscriptionStorage.RemoveRegistrations deviceGroupId subsriptionChanges.SubscriptionsToBeRemoved
            do! PushNotificationSubscriptionStorage.AddRegistrations deviceGroupId subsriptionChanges.SubscriptionsToBeAdded
        }

    let private SendPushNotifications httpSend reason =
        async {
            do! SendFirebasePushNotifications httpSend reason
        }
        
    let SendSensorStateNotification httpSend (state : SensorState) : Async<unit>=
        async {               
            let reason : PushNotificationReason = SensorStatePushNotification state
            do! SendPushNotifications httpSend reason
        }        
        
    let SendDevicePropertyStateNotification httpSend (state : DevicePropertyState) : Async<unit>=
        async {               
            let reason : PushNotificationReason = DevicePropertyStatePushNotification state
            do! SendPushNotifications httpSend reason
        }