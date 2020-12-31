namespace Jottai

module WaitForDevicePropertyChangeRequestedEvent =

    let For (deviceGroupId : DeviceGroupId) =
        async {            
            let eventFilter (event : Event.Event) =
                match event with
                | Event.DevicePropertyChangeRequested event ->
                    if event.DeviceGroupId = deviceGroupId
                    then Some event
                    else None
                | _ -> None

            let! devicePropertyChangeRequest =
                EventBus.Events
                |> WaitForObservable.ThatPasses eventFilter
            
            return devicePropertyChangeRequest
        }
