namespace Jottai

module internal EventBus =

    open System
    open FSharp.Control.Reactive

    let private eventsSubject =
        Subject<Event.Event>.broadcast

    let Events : IObservable<Event.Event> =
        eventsSubject :> IObservable<Event.Event>

    let Publish (event : Event.Event) : unit =
        eventsSubject.OnNext event
