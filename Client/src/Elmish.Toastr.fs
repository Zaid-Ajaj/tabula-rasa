module Elmish.Toastr

open Elmish
open Fable.Core.JsInterop

importAll "toastr/build/toastr.min.css"

type ToastrMsg =
    { Message : string
      Title : string }

let private successToast (msg : string) : unit = import "success" "toastr"
let private successToastWithTitle (msg : string) (title : string) : unit = import "success" "toastr"
let private errorToast (msg : string) : unit = import "error" "toastr"
let private errorToastWithTitle (msg : string) (title : string) : unit = import "error" "toastr"
let private infoToast (msg : string) : unit = import "info" "toastr"
let private infoToastWithTitle (msg : string) (title : string) : unit = import "info" "toastr"
let private warningToast (msg : string) : unit = import "warning" "toastr"
let private warningToastWithTitle (msg : string) (title : string) : unit = import "warning" "toastr"

let message msg =
    { Message = msg
      Title = "" }

let withTitle title msg = { msg with Title = title }

let success (msg : ToastrMsg) : Cmd<_> =
    [ fun _ -> 
        if System.String.IsNullOrEmpty(msg.Title) then successToast msg.Message
        else successToastWithTitle msg.Message msg.Title ]

let error (msg : ToastrMsg) : Cmd<_> =
    [ fun _ -> 
        if System.String.IsNullOrEmpty(msg.Title) then errorToast msg.Message
        else errorToastWithTitle msg.Message msg.Title ]

let info (msg : ToastrMsg) : Cmd<_> =
    [ fun _ -> 
        if System.String.IsNullOrEmpty(msg.Title) then infoToast msg.Message
        else infoToastWithTitle msg.Message msg.Title ]

let warning (msg : ToastrMsg) : Cmd<_> =
    [ fun _ -> 
        if System.String.IsNullOrEmpty(msg.Title) then warningToast msg.Message
        else warningToastWithTitle msg.Message msg.Title ]
