namespace Jottai

module Http =
    open System
    open System.Net.Http
    open System.Net.Http.Headers
    open Newtonsoft.Json
    open Newtonsoft.Json.Converters
    
    let private jsonOptions = new StringEnumConverter()
    let private GetBaseUrl() = Environment.GetEnvironmentVariable("JOTTAI_BASE_URL")
    let private HttpClient = new HttpClient(BaseAddress = Uri(GetBaseUrl()))
    
    let FailOnServerErrorOrBadRequest(response : HttpResponseMessage) : HttpResponseMessage = 
        if (int response.StatusCode) >= 500 || (int response.StatusCode) = 400 then
            let content = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            failwith (response.StatusCode.ToString() + ": " + content)
        else
            response

    let PostJson (token : string) (url : string) (json : string) =
        async {            
            use requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            use content = new StringContent(json, Text.Encoding.UTF8, "application/json")
            requestMessage.Content <- content
            requestMessage.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)
            let! response = HttpClient.SendAsync(requestMessage) |> Async.AwaitTask
            return response |> FailOnServerErrorOrBadRequest
        }

    let Post (token : string) (url : string) data = 
        async {
            let json = JsonConvert.SerializeObject(data, jsonOptions)
            return! PostJson token url json
        }
    
    let Get (token : string) (url : string) = 
        async { 
            use requestMessage = new HttpRequestMessage(HttpMethod.Get, url)
            requestMessage.Headers.Add("Accept", "application/json")
            requestMessage.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)            
            let! response = HttpClient.SendAsync(requestMessage) |> Async.AwaitTask
            return response |> FailOnServerErrorOrBadRequest
        }
    
    let ContentOrFail(response : Async<HttpResponseMessage>) : Async<string> = 
        async { 
            let! response = response
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            match response.IsSuccessStatusCode with
            | true -> return content
            | false -> return failwith (response.StatusCode.ToString() + " " + response.ReasonPhrase + ": " + content)
        }
    
    let ThrowExceptionOnFailure(response : Async<HttpResponseMessage>) : Async<unit> = 
        async { 
            let! response = response
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            match response.IsSuccessStatusCode with
            | true -> return ()
            | false -> return failwith (response.StatusCode.ToString() + " " + response.ReasonPhrase + ": " + content)
        }