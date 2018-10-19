module Admin.Login.View

open Fable.Core.JsInterop
open Admin.Login.Types
open Fable.Core
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React

type InputType =
    | Text
    | Password

let textInput inputLabel initial inputType (onChange : string -> unit) =
    let inputType =
        match inputType with
        | Text -> "input"
        | Password -> "password"
    div [ ClassName "form-group" ] [ input [ ClassName "form-control form-control-lg"
                                             Type inputType
                                             DefaultValue initial
                                             Placeholder inputLabel
                                             OnChange(fun e -> onChange !!e.target?value) ] ]

let loginFormStyle =
    Style [ Width "400px"
            MarginTop "70px"
            TextAlign "center" ]

let cardBlockStyle =
    Style [ Padding "30px"
            TextAlign "left"
            BorderRadius 10 ]

[<Emit("null")>]
let emptyElement : ReactElement = jsNative

let errorMessagesIfAny triedLogin =
    function 
    | [] -> emptyElement
    | _ when not triedLogin -> emptyElement
    | errors -> 
        let errorStyle =
            Style [ Color "crimson"
                    FontSize 12 ]
        ul [] [ for error in errors -> li [ errorStyle ] [ str error ] ]

let appIcon =
    img [ Src "/img/favicon-book.png"
          Style [ Height 60
                  Width 80 ] ]

let render (state : State) dispatch =
    let loginBtnContent =
        if state.LoggingIn then i [ ClassName "fa fa-circle-o-notch fa-spin" ] []
        else str "Login"
    
    let btnClass =
        if state.CanLogin then "btn btn-success btn-lg"
        else "btn btn-info btn-lg"
    
    div [ ClassName "container"
          loginFormStyle ] 
        [ div [ ClassName "card" ] 
              [ div [ ClassName "card-block"
                      cardBlockStyle ] 
                    [ h1 [ Style [ TextAlign "center" ] ] [ str "Tabula Rasa" ]
                      div [ Style [ TextAlign "center" ] ] [ appIcon ]
                      br []
                      textInput "Username" state.InputUsername Text (ChangeUsername >> dispatch)
                      errorMessagesIfAny state.HasTriedToLogin state.UsernameValidationErrors
                      textInput "Password" state.InputPassword Password (ChangePassword >> dispatch)
                      errorMessagesIfAny state.HasTriedToLogin state.PasswordValidationErrors
                      
                      div [ Style [ TextAlign "center" ] ] 
                          [ button [ ClassName btnClass
                                     OnClick(fun _ -> dispatch Login) ] [ loginBtnContent ] ] ] ] ]
