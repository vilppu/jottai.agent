namespace Jottai

module EventBus =

    open System

    let mutable private subscriptions : Map<Guid, Event.Event -> Async<unit>> = Map.empty

    type private DisposableSubscription(onEvent) =

        let subscriptionId = Guid.NewGuid()
        do subscriptions <- subscriptions |> Map.add subscriptionId onEvent

        interface IDisposable with
            member __.Dispose() =
                subscriptions <- subscriptions |> Map.remove subscriptionId

    let Publish (event: Event.Event) : Async<unit> =
        async {
            for KeyValue(_, onEvent) in subscriptions do
                do! (onEvent event)            
        }

    let Subscribe (onEvent: Event.Event -> Async<unit>) : IDisposable =
        new DisposableSubscription(onEvent) :> IDisposable
