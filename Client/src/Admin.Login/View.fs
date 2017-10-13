module Admin.Login.View

open System
open Fable.Core.JsInterop
open Admin.Login.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props

let textInput inputLabel valid (onChange: string -> unit) = 
  let inputClasses = 
    classList [ "form-control", true 
                "form-control-success", valid ]
  div 
    [ ClassName "form-group" ]
    [ label [ ClassName "form-control-label" ] [ str inputLabel ]
      input [ inputClasses; OnChange (fun e -> onChange !!e.target?value) ] ]


let render (state: State) dispatch = 
    let validUsername = String.IsNullOrWhiteSpace(state.InputUsername)
    let validPassword = String.IsNullOrWhiteSpace(state.InputPassword)
    div 
      [ ClassName "container" ; Style [ TextAlign "center"; MarginTop "50px" ] ]
      [ div 
         [ ClassName "card text-center" ]
         [ div
             [ ClassName "card-block" ]
             [ h4 [ ClassName "card-title" ] [ str "Admin Login" ]
               textInput "Username" validUsername (ChangeUsername >> dispatch)
               textInput "Password" validPassword (ChangeUsername >> dispatch) ] ] ] 