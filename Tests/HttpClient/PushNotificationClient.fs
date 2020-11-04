//push-notifications/subscribe/{token}
namespace Jottai

[<AutoOpen>]
module PushNotificationClient = 
    
    let SubscribeToPushNotifications token pushNotificationToken= 
        let apiUrl = sprintf "api/push-notifications/subscribe/%s" pushNotificationToken
        Http.Post token apiUrl ""
