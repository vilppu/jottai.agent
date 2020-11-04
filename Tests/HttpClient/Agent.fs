namespace Jottai

module Agent = 
    open System
    open System.Net.Http
    open Newtonsoft.Json
    
    let private getBaseUrl() = Environment.GetEnvironmentVariable("JOTTAI_BASE_URL")
    let private httpClient = new HttpClient(BaseAddress = Uri(getBaseUrl()))
    
    let PostWithMasterKey (key : string) (url : string) data =
        let json = JsonConvert.SerializeObject data
        async { 
            use content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            content.Headers.Add("jottai-key", key)
            let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let PostWithDeviceGroupKey (key : string) (deviceGroupId : string) (url : string) data =
        let json = JsonConvert.SerializeObject data
        async { 
            use content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            content.Headers.Add("jottai-device-group-key", key)
            content.Headers.Add("jottai-device-group-id", deviceGroupId)
            let! response = httpClient.PostAsync(url, data) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let PostWithSensorKey (key : string) (deviceGroupId : string) (url : string) data =
        let json = JsonConvert.SerializeObject data
        async { 
            use content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            content.Headers.Add("jottai-sensor-data-key", key)
            content.Headers.Add("jottai-bot-id", deviceGroupId)
            let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let GetWithMasterKey (key : string) (url : string) =
        async { 
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            request.Headers.Add("Accept", "application/json")
            request.Headers.Add("jottai-key", key)
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let GetWithDeviceGroupKey (key : string) (deviceGroupId : string) (url : string) =
        async { 
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            request.Headers.Add("Accept", "application/json")
            request.Headers.Add("jottai-device-group-key", key)
            request.Headers.Add("jottai-device-group-id", deviceGroupId)
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let GetWithSensorKey (key : string) (deviceGroupId : string) (url : string) = 
        async { 
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            request.Headers.Add("Accept", "application/json")
            request.Headers.Add("jottai-sensor-data-key", key)
            request.Headers.Add("jottai-device-group-id", deviceGroupId)
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }
    
    let GetWithBotKey (key : string) (deviceGroupId : string) (url : string) =
        async { 
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            request.Headers.Add("Accept", "application/json")
            request.Headers.Add("jottai-sensor-data-key", key)
            request.Headers.Add("jottai-bot-id", deviceGroupId)
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            return response |> Http.FailOnServerError
        }