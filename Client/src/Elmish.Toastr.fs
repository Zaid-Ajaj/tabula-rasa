module Elmish.Toastr

open Elmish
open Fable.Core.JsInterop

importAll "toastr/build/toastr.min.css"

type ToastrMsg = { Message : string; Title: string }

let private successToast (msg: string) : unit = import "success" "toastr" 
let private successToastWithTitle (msg: string) (title: string)  : unit = import "success" "toastr" 

let private errorToast (msg: string) : unit = import "error" "toastr" 
let private errorToastWithTitle (msg: string) (title: string)  : unit = import "error" "toastr" 

let message msg = { Message = msg; Title = ""  }
let withTitle title msg = { msg with Title = title }

let success (msg: ToastrMsg) : (unit -> Cmd<'a>) = 
    fun () ->
        if System.String.IsNullOrEmpty(msg.Title) 
        then successToast msg.Message
        else successToastWithTitle msg.Message msg.Title
        Cmd.none

let error (msg: ToastrMsg) : (unit -> Cmd<'a>) = 
    fun () ->
        if System.String.IsNullOrEmpty(msg.Title) 
        then errorToast msg.Message
        else errorToastWithTitle msg.Message msg.Title
        Cmd.none