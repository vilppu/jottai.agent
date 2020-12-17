namespace Jottai

module Http = 
    
    let private HttpClient = new System.Net.Http.HttpClient()

    let Send (request : System.Net.Http.HttpRequestMessage) : Async<System.Net.Http.HttpResponseMessage> =
        async {
            let! response = HttpClient.SendAsync request |> Async.AwaitTask
            return response 
        }
