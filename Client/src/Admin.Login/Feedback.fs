module Admin.Login.Feedback

open Elmish
open Admin.Login.Types

let private clientError (msg: string) : (unit -> Cmd<Msg>) = 
  msg
  |> Toastr.message 
  |> Toastr.withTitle "Client"
  |> Toastr.error 

let usernameEmpty = clientError "Username field cannot be empty"
let usernameTooShort = clientError "Username too short, at least 6 characters are required"
let passwordEmpty = clientError "Password field cannot be empty"
let passwordTooShort = clientError "Password too short, at least 6 characters are required"

let loginSuccess : unit -> Cmd<Msg> = 
    "Succesfully logged in"
    |> Toastr.message
    |> Toastr.withTitle "Client"
    |> Toastr.success