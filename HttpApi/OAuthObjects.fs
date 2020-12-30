namespace Jottai

module OAuthObjects =

    type TokenResponse = 
        { access_token : string
          refresh_token : string
          id_token : string
          scope : string
          expires_in : int 
          token_type : string }

    type TokenRequest = 
        { grant_type : string
          client_id : string
          client_secret : string
          audience : string }

 