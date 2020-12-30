namespace Jottai

module OAuthObjects =

    type TokenResponse = 
        { access_token : string
          refresh_token : string
          id_token : string
          scope : string
          expires_in : int 
          token_type : string }

 