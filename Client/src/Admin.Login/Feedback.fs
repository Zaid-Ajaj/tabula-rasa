module Admin.Login.Feedback

open Elmish
open Admin.Login.Types

let private errorFromClient (msg: string) : (unit -> Cmd<Msg>) = 
  msg
  |> Toastr.message 
  |> Toastr.withTitle "Client"
  |> Toastr.error 

let usernameEmpty = errorFromClient "Username field cannot be empty"
let usernameInvalid = errorFromClient "Username too short, at least 6 characters are required"
let passwordEmpty = errorFromClient "Password field cannot be empty"
let passwordInvalid = errorFromClient "Password too short, at least 6 characters are required"

let loginSuccess : unit -> Cmd<Msg> = 
    "Succesfully logged in"
    |> Toastr.message
    |> Toastr.withTitle "Client"
    |> Toastr.success