namespace Jottai


module Authentication =
    open Newtonsoft.Json
    open System
    open System.Collections.Generic
    open System.Linq
    open System.Net
    open System.Net.Http
    open System.Security.Claims
    open ApiObjects
    

    let private GetManagementAccessToken (httpSend  : HttpRequestMessage -> Async<HttpResponseMessage>) = 
        async {            
            
            let url = sprintf "%soauth/token" (Application.Authority())
            use request = new HttpRequestMessage(HttpMethod.Post, url)
            let body = { grant_type = "client_credentials"
                         client_id = Application.ManagementClientId()
                         client_secret = Application.ManagementClientSecret()
                         audience = sprintf "%s/api/v2/" (Application.ManagementAudience()) } : OAuthObjects.TokenRequest
            let json = JsonConvert.SerializeObject(body)
            use content = new StringContent(json, Text.Encoding.UTF8, "application/json")            
            request.Content <- content            
            use! response = httpSend request
            let! responseJson = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let tokenResponse = JsonConvert.DeserializeObject<OAuthObjects.TokenResponse>(responseJson)
            return tokenResponse.access_token
                
        }
    let GetRefreshToken (httpSend  : HttpRequestMessage -> Async<HttpResponseMessage>) (code : string) (redirectUri : string) = 
        async {
            let url = sprintf "%soauth/token" (Application.Authority())
            use request = new HttpRequestMessage(HttpMethod.Post, url)
            let values = [
                KeyValuePair.Create("grant_type", "authorization_code")
                KeyValuePair.Create("client_id", Application.ClientId())
                KeyValuePair.Create("code",code)
                KeyValuePair.Create("redirect_uri", redirectUri)
                ]

            request.Content <- new FormUrlEncodedContent(values |> List.toSeq)
            
            use! response = httpSend request

            if response.IsSuccessStatusCode then
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let tokenResponse = JsonConvert.DeserializeObject<OAuthObjects.TokenResponse>(json)
                return Some { RefreshToken = tokenResponse.refresh_token }
            else
                return None
        }
    
    let GetAccessToken (httpSend  : HttpRequestMessage -> Async<HttpResponseMessage>) (refreshToken : string) (redirectUri : string) = 
        async {
            let url = sprintf "%soauth/token" (Application.Authority())
            use request = new HttpRequestMessage(HttpMethod.Post, url)
            let values = [
                KeyValuePair.Create("grant_type", "refresh_token")
                KeyValuePair.Create("client_id", Application.ClientId())
                KeyValuePair.Create("refresh_token", refreshToken)
                KeyValuePair.Create("redirect_uri", redirectUri)
                ]

            request.Content <- new FormUrlEncodedContent(values |> List.toSeq)
            
            use! response = httpSend request

            if response.IsSuccessStatusCode then
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let tokenResponse = JsonConvert.DeserializeObject<OAuthObjects.TokenResponse>(json)
                let acccessToken = tokenResponse.access_token
                let jwtSecurityTokenHandler = new IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                let token = jwtSecurityTokenHandler.ReadToken(acccessToken) :?> IdentityModel.Tokens.Jwt.JwtSecurityToken
                let expires = DateTimeOffset (DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc))
                return Some { AccessToken = acccessToken
                              Expires = expires }
            else
                return None
        }

    let StoreRefreshToken (httpSend  : HttpRequestMessage -> Async<HttpResponseMessage>) (user : ClaimsPrincipal) (refreshToken : string) = 
        async {            
            let nameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            let names = user.Claims.Where(fun claim -> claim.Type = nameClaim).Select(fun claim -> claim.Value).ToList()            
            let name = names.Single()
            let url = sprintf "%sapi/v2/users/%s" (Application.Authority()) name
            use request = new HttpRequestMessage(HttpMethod.Patch, url)
            let json = sprintf "{\"user_metadata\": {\"token\": \"%s\"}}" refreshToken
            let x = JsonConvert.DeserializeObject(json)
            use content = new StringContent(json, Text.Encoding.UTF8, "application/json")
            let! token = GetManagementAccessToken httpSend
            request.Content <- content
            request.Headers.TryAddWithoutValidation("Authorization", sprintf "Bearer %s" token) |> ignore
            use! response = httpSend request
            response.EnsureSuccessStatusCode() |> ignore
        }
