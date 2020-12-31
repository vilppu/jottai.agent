namespace Jottai

module internal WaitForObservable =
    open System
    open System.Threading
    open FSharp.Control.Reactive

    let private timeout = TimeSpan.FromSeconds(60.0)

    let ThatPasses filter observable =
        async {
            printf "ThatPasses"
            use waiter = new SemaphoreSlim(0)            
            let result = Subject.behavior (None)
 
            use _ = 
                observable  
                |> Observable.subscribe(fun event ->
                    printf "->"
                    match event |> filter with
                    | Some event ->
                        printf "Some event"
                        waiter.Release() |> ignore
                        result.OnNext (event |> Some)
                    | _ -> ()
                )

            do!
                waiter.WaitAsync(timeout)
                |> Async.AwaitTask
                |> Async.Ignore

            printf "return result.Value"
            return result.Value
        }
