namespace Jottai

module EventBus =

    open System
    open FSharp.Control.Reactive

    let private EventsSubject =
        Subject<Event.Event>.broadcast

    let Events : IObservable<Event.Event> =
        EventsSubject :> IObservable<Event.Event>

    let Publish (event : Event.Event) : unit =
        EventsSubject.OnNext event
        
    let Disposable =
        EventsSubject :> IDisposable
