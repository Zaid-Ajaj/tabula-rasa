module Admin.Login.Http

open Admin.Login.Types
open Elmish
open Shared
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types
open Fable.Core.JsInterop

let private loginPromise (info: LoginInfo) = 
    promise {
        let body = toJson info
        let requestProps = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Body !^body
              Fetch.requestHeaders [ HttpRequestHeaders.ContentType "application/json" ] ]
        let! response = Fetch.tryFetch "/api/login" requestProps
        match response with
        | Ok response -> 
            let! json = response.text()
            match ofJson<LoginResult> json with 
            | Success token -> return LoginSuccess token
            | UsernameDoesNotExist -> return LoginFailed "Username does not exist"
            | PasswordIncorrect -> return LoginFailed "The password you entered is incorrect"
            | LoginError _ -> return LoginFailed "Unknown error occured while logging you in"
        | Error ex -> return LoginFailed "Unknown error occured while logging you in"
    }

let login (loginInfo: LoginInfo) =
    
    let successHandler = function
        | Success token -> LoginSuccess token
        | UsernameDoesNotExist -> LoginFailed "Username does not exist"
        | PasswordIncorrect -> LoginFailed "The password you entered is incorrect"
        | LoginError _ -> LoginFailed "Unknown error occured while logging you in"
    
    Cmd.ofAsync Server.api.login loginInfo
                successHandler
                (fun ex -> LoginFailed "Unknown error occured while logging you in")
