namespace Jottai

module Notification =

    type Subscription =
        { Token : string }

    let Subscription token : Subscription =
        { Token = token }
