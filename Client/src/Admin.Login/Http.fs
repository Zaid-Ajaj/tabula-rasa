module Admin.Login.Http

open Admin.Login.Types
open Elmish
open Shared.ViewModels
open Fable.PowerPack

let private loginPromise() = 
    promise {
        do! Promise.sleep 1000
        return "Hello"
    }

let login (info: LoginInfo) = 
    Cmd.ofPromise loginPromise ()
                  LoginSuccess
                  (fun ex -> LoginFailed "error")
