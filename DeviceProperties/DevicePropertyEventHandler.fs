namespace Jottai

module DevicePropertyEventHandler =

    let Handle (publish : Event.Event->Async<unit>)  (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.DevicePropertyChanged event ->
                let devicePropertyUpdate = event |> Event.ToDevicePropertyUpdate
                let! devicePropertyState = Action.GetDevicePropertyState devicePropertyUpdate

                do! Action.StoreDeviceProperty devicePropertyState
                do! publish (Event.DevicePropertyStored devicePropertyState)

            | Event.DevicePropertyNameChanged event ->                
                do! DevicePropertyStorage.StoreDevicePropertyName event.DeviceGroupId event.GatewayId event.DeviceId event.PropertyId event.PropertyName
                do! publish (Event.DevicePropertyNameStored event)
            | _ -> ()
        }