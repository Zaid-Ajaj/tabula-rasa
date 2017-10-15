module Admin.Login.Feedback

open Elmish
open Admin.Login.Types

let errorToast (msg: string) : (unit -> Cmd<Msg>) = 
  msg
  |> Toastr.message 
  |> Toastr.withTitle "Client"
  |> Toastr.error 

let usernameEmpty = errorToast "Username field cannot be empty"
let usernameTooShort = errorToast "Username too short, at least 5 characters are required"
let passwordEmpty = errorToast "Password field cannot be empty"
let passwordTooShort = errorToast "Password too short, at least 5 characters are required"

let loginSuccess : unit -> Cmd<Msg> = 
    "Succesfully logged in"
    |> Toastr.message
    |> Toastr.withTitle "Client"
    |> Toastr.success