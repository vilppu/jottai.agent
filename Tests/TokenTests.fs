namespace Jottai

module TokenTest = 
    open System.Net
    open Xunit
    
    [<Fact>]
    let DeviceGroupKeyIsRequiredToCreateDeviceGroupTokens() = 
        use context = SetupContext()
        let deviceGroupToken = InvalidToken
        let response = GetSensorStateResponse deviceGroupToken |> Async.RunSynchronously
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
    
    [<Fact>]
    let DeviceGroupTokenCanBeUsedToAccessDeviceGroup() = 
        use context = SetupContext()
        let deviceGroupToken = Authorization.GenerateUserToken context.DeviceGroupId
        let response = GetSensorStateResponse deviceGroupToken |> Async.RunSynchronously
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
