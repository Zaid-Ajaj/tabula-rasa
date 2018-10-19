module Admin.Login.Http

open Admin.Login.Types
open Elmish
open Shared

let login (loginInfo : LoginInfo) =
    let successHandler =
        function 
        | Success token -> LoginSuccess token
        | UsernameDoesNotExist -> LoginFailed "Username does not exist"
        | PasswordIncorrect -> LoginFailed "The password you entered is incorrect"
        | LoginError _ -> LoginFailed "Unknown error occured while logging you in"
    Cmd.ofAsync Server.api.login loginInfo successHandler 
        (fun ex -> LoginFailed "Unknown error occured while logging you in")
