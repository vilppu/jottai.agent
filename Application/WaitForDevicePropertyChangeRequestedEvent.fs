namespace Jottai

module WaitForDevicePropertyChangeRequestedEvent =

    let For (deviceGroupId : DeviceGroupId) =
        async {            
            let eventFilter (event : Event.Event) =
                match event with
                | Event.DevicePropertyChangeRequested event ->
                    printf "Event.DevicePropertyChangeRequested %s %s" event.DeviceGroupId.AsString deviceGroupId.AsString
                    if event.DeviceGroupId = deviceGroupId
                    then Some event
                    else None
                | _ -> None

            let! devicePropertyChangeRequest =
                EventBus.Events
                |> WaitForObservable.ThatPasses eventFilter 

            printf "return devicePropertyChangeRequest"
            return devicePropertyChangeRequest
        }
