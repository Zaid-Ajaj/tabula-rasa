module Admin.Login.Types

open Shared.ViewModels

type Msg = 
    | Login
    | ChangeUsername of string
    | ChangePassword of string
    | LoginSuccess of adminSecureToken: string
    | LoginFailed of error:string

type State = {
    LoggingIn: bool
    InputUsername: string
    InputPassword: string
}