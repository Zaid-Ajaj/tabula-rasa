module Admin.Login.View

open System
open Fable.Core.JsInterop
open Admin.Login.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props

type InputType = Text | Password 
let textInput inputLabel initial inputType (onChange: string -> unit) = 
  let inputType = match inputType with 
                  | Text -> "input"
                  | Password -> "password"
  div 
    [ ClassName "form-group" ]
    [ input [ ClassName "form-control form-control-lg"
              Type inputType
              DefaultValue initial
              Placeholder inputLabel
              OnChange (fun e -> onChange !!e.target?value) ] ]

let loginFormStyle = 
  Style [ Width "400px"
          MarginTop "70px"
          TextAlign "center" ]

let cardBlockStyle = 
  Style [ Padding "30px"
          TextAlign "left"
          BackgroundImage "url('/img/login-bg.jpg')" ]

let blogIcon = 
  img [ Src "/img/favicon-book.png"
        Style [ Height 32; Width 32; Margin 10 ] ]

let render (state: State) dispatch = 
    let validUsername = String.IsNullOrWhiteSpace(state.InputUsername) |> not
    let validPassword = String.IsNullOrWhiteSpace(state.InputPassword) |> not
    let loginBtnContent = 
      if state.LoggingIn then i [ ClassName "fa fa-circle-o-notch fa-spin" ] []
      else str "Login"

    let canLogin = 
      [ validUsername 
        validPassword
        state.InputUsername.Length >= 5
        state.InputPassword.Length >= 5 ]
      |> Seq.forall id
     
    let btnClass = 
      if canLogin 
      then "btn btn-success btn-lg"
      else "btn btn-info btn-lg"
    div 
      [ ClassName "container" ; loginFormStyle ]
      [ div 
         [ ClassName "card" ]
         [ div
             [ ClassName "card-block"; cardBlockStyle ]
             [ h4 
                [ ClassName "card-title" ] 
                [ span [ ] [ blogIcon ] 
                  str "Admin Login" ]
               br []
               textInput "Username" state.InputUsername Text (ChangeUsername >> dispatch)
               textInput "Password" state.InputPassword Password (ChangePassword >> dispatch)
               button 
                 [ ClassName btnClass
                   OnClick (fun e -> dispatch Login) ] 
                 [ loginBtnContent ] ] ] ] 