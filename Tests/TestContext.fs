namespace Jottai

[<AutoOpen>]
module TestContext = 
    open System
    open System.Net.Http
    open System.Security.Cryptography
    open System.Threading.Tasks

    [<assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)>]
    do()

    let mutable serverTask : Task = null

    let InvalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzY290Y2guaW8iLCJleHAiOjEzMDA4MTkzODAsIm5hbWUiOiJDaHJpcyBTZXZpbGxlamEiLCJhZG1pbiI6dHJ1ZX0.03f329983b86f7d9a9f5fef85305880101d5e302afafa20154d094b229f75773"    
    let TestDeviceGroupId = "TestDeviceGroup"
    let AnotherTestDeviceGroupId = "AnotherTestDeviceGroupId"

    let private SetupEmptyEnvironmentUsing httpSend = 
        Environment.SetEnvironmentVariable("JOTTAI_BASE_URL", "http://127.0.0.1:18888/jottai/")
        Environment.SetEnvironmentVariable("JOTTAI_MONGODB_DATABASE", "Jottai_Test")
        Environment.SetEnvironmentVariable("JOTTAI_TOKEN_SECRET", "fake-token-secret")
        Environment.SetEnvironmentVariable("JOTTAI_FCM_KEY", "fake")
        SensorEventStorage.Drop TestDeviceGroupId
        SensorEventStorage.Drop AnotherTestDeviceGroupId
        SensorStateStorage.Drop()
        SensorHistoryStorage.Drop()
        DevicePropertyStorage.Drop()
    
        if serverTask |> isNull then
            serverTask <- CreateHttpServer Options.UseSigninKey httpSend

    let SentHttpRequests = Collections.Generic.List<HttpRequestMessage>()
    let SentHttpRequestContents = Collections.Generic.List<string>()

    let private SetupEmptyEnvironment() =   
        SentHttpRequests.Clear()
        SentHttpRequestContents.Clear()
        let httpSend (request : HttpRequestMessage) : Async<HttpResponseMessage> =
            async {
                let requestContent =
                    request.Content.ReadAsStringAsync()                    
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                SentHttpRequests.Add request
                SentHttpRequestContents.Add requestContent

                let response = new HttpResponseMessage()
                response.Content <- new StringContent("")
                return response
            }
        SetupEmptyEnvironmentUsing httpSend



    let private GenerateSecureToken() =         
        let tokenBytes = Array.zeroCreate<byte> 16
        RandomNumberGenerator.Create().GetBytes tokenBytes
        let tokenWithDashes = BitConverter.ToString tokenBytes
        tokenWithDashes.Replace("-", "")

    type Context() = 
        do
            SetupEmptyEnvironment()

        member val DeviceGroupId = GenerateSecureToken() with get, set
        member val AnotherDeviceGroupId = GenerateSecureToken() with get, set        
        member val DeviceGroupToken = "DeviceGroupToken" with get, set
        member val AnotherDeviceGroupToken = "AnotherDeviceGroupToken" with get, set
        member val SensorToken = "SensorToken" with get, set
        member val AnotherSensorToken = "AnotherSensorToken" with get, set

        interface IDisposable with
            member this.Dispose() =
                ()
    
    let SetupContext () =        
        let context = new Context()
        context.DeviceGroupId <- TestDeviceGroupId
        context.AnotherDeviceGroupId <- AnotherTestDeviceGroupId        
        context.DeviceGroupToken <- GenerateDeviceGroupAccessToken context.DeviceGroupId
        context.AnotherDeviceGroupToken <- GenerateDeviceGroupAccessToken context.AnotherDeviceGroupId
        context.SensorToken <- GenerateSensorAccessToken context.DeviceGroupId
        context.AnotherSensorToken <- GenerateSensorAccessToken context.AnotherDeviceGroupId
        context
