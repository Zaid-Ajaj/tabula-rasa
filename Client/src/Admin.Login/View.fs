module Admin.Login.View

open System
open Fable.Core.JsInterop
open Admin.Login.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props

let textInput inputLabel valid (onChange: string -> unit) = 
  let inputClasses = 
    classList [ "form-control", true 
                "form-control-lg", true
                "form-control-success", valid ]
  div 
    [ ClassName "form-group has-success" ]
    [ input [ inputClasses
              Placeholder inputLabel
              OnChange (fun e -> onChange !!e.target?value) ] ]


let loginFormStyle = 
  Style [ Width "400px"
          MarginTop "70px"
          TextAlign "center" ]

let cardBlockStyle = 
  Style [ Padding "30px"
          TextAlign "left"
          BorderRadius "15px" ]

let render (state: State) dispatch = 
    let validUsername = String.IsNullOrWhiteSpace(state.InputUsername) |> not
    let validPassword = String.IsNullOrWhiteSpace(state.InputPassword) |> not
    
    let loginBtnContent = 
      if state.LoggingIn then i [ ClassName "fa fa-circle-o-notch fa-spin" ] []
      else str "Login"

    let canLogin = 
      [ validUsername 
        validPassword
        state.InputUsername.Length >= 4
        state.InputPassword.Length >= 4 ]
      |> Seq.forall id
     
    let btnClass = 
      if canLogin 
      then "btn btn-success btn-lg"
      else "btn btn-primary btn-lg"
    div 
      [ ClassName "container" ; loginFormStyle ]
      [ div 
         [ ClassName "card" ]
         [ div
             [ ClassName "card-block"; cardBlockStyle ]
             [ h4 [ ClassName "card-title" ] [ str "Admin Login" ]
               br []
               textInput "Username" validUsername (ChangeUsername >> dispatch)
               textInput "Password" validPassword (ChangePassword >> dispatch)
               button 
                 [ ClassName btnClass
                   OnClick (fun e -> dispatch Login) ] 
                 [ loginBtnContent ] ] ] ] 