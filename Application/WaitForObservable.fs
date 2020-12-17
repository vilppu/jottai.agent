namespace Jottai

module internal WaitForObservable =
    open System
    open System.Threading
    open FSharp.Control.Reactive

    let private timeout = TimeSpan.FromSeconds(60.0)

    let ThatPasses filter observable =
        async {
            use waiter = new SemaphoreSlim(0)            
            let result = Subject.behavior (None)
 
            use _ = 
                observable  
                |> Observable.subscribe(fun event ->
                    match event |> filter with
                    | Some event ->
                        waiter.Release() |> ignore
                        result.OnNext (event |> Some)
                    | _ -> ()
                )

            do!
                waiter.WaitAsync(timeout)
                |> Async.AwaitTask
                |> Async.Ignore

            return result.Value
        }
