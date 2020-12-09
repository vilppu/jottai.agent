namespace Jottai

module internal SensorStateNotifications =

    open System
    open FSharp.Control.Reactive

    let private sensorStateSubject =
        Subject<SensorState>.broadcast

    let SensorStates : IObservable<SensorState> =
        sensorStateSubject :> IObservable<SensorState>

    let Publish (sensorState : SensorState) : unit =
        sensorStateSubject.OnNext sensorState

    let Disposable =
        sensorStateSubject :> IDisposable