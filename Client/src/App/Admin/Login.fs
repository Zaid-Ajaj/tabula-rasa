module Login

open System
open Elmish
open Shared

open Fable.Core.JsInterop
open Fable.Core
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React

type Msg =
    | Login
    | ChangeUsername of string
    | ChangePassword of string
    | LoginSuccess of adminSecureToken: string
    | LoginFailed of error: string
    | UpdateValidationErrors
    | UpdateCanLogin

type State =
    { LoggingIn: bool
      InputUsername: string
      UsernameValidationErrors: string list
      PasswordValidationErrors: string list
      InputPassword: string
      HasTriedToLogin: bool
      LoginError: string option
      CanLogin: bool }


let login (loginInfo: LoginInfo) =
    let successHandler =
        function
        | Success token -> LoginSuccess token
        | UsernameDoesNotExist -> LoginFailed "Username does not exist"
        | PasswordIncorrect -> LoginFailed "The password you entered is incorrect"
        | LoginError _ -> LoginFailed "Unknown error occured while logging you in"

    Cmd.ofAsync
        Server.api.login
        loginInfo
        successHandler
        (fun ex -> LoginFailed "Unknown error occured while logging you in")


type InputType =
    | Text
    | Password

let textInput inputLabel initial inputType (onChange: string -> unit) =
    let inputType =
        match inputType with
        | Text -> "input"
        | Password -> "password"

    div [ ClassName "form-group" ] [
        input [ ClassName "form-control form-control-lg"
                Type inputType
                DefaultValue initial
                Placeholder inputLabel
                OnChange(fun e -> onChange !!e.target?value) ]
    ]

let loginFormStyle =
    Style [ Width "400px"
            MarginTop "70px"
            TextAlign "center" ]

let cardBlockStyle =
    Style [ Padding "30px"
            TextAlign "left"
            BorderRadius 10 ]

[<Emit("null")>]
let emptyElement: ReactElement = jsNative

let errorMessagesIfAny triedLogin =
    function
    | [] -> emptyElement
    | _ when not triedLogin -> emptyElement
    | errors ->
        let errorStyle = Style [ Color "crimson"; FontSize 12 ]

        ul [] [
            for error in errors -> li [ errorStyle ] [ str error ]
        ]

let appIcon =
    img [ Src "/img/favicon-book.png"
          Style [ Height 60; Width 80 ] ]

let render (state: State) dispatch =
    let loginBtnContent =
        if state.LoggingIn then
            i [ ClassName "fa fa-circle-o-notch fa-spin" ] []
        else
            str "Login"

    let btnClass =
        if state.CanLogin then
            "btn btn-success btn-lg"
        else
            "btn btn-info btn-lg"

    div [ ClassName "container"
          loginFormStyle ] [
        div [ ClassName "card" ] [
            form [ ClassName "card-block"
                   cardBlockStyle ] [
                h1 [ Style [ TextAlign "center" ] ] [
                    str "Tabula Rasa"
                ]
                div [ Style [ TextAlign "center" ] ] [
                    appIcon
                ]
                br []
                textInput "Username" state.InputUsername Text (ChangeUsername >> dispatch)
                errorMessagesIfAny state.HasTriedToLogin state.UsernameValidationErrors
                textInput "Password" state.InputPassword Password (ChangePassword >> dispatch)
                errorMessagesIfAny state.HasTriedToLogin state.PasswordValidationErrors

                div [ Style [ TextAlign "center" ] ] [
                    button [ ClassName btnClass
                             OnClick(fun ev -> dispatch Login) ] [
                        loginBtnContent
                    ]
                ]
            ]
        ]
    ]

let init () =
    { InputUsername = ""
      InputPassword = ""
      UsernameValidationErrors = []
      PasswordValidationErrors = []
      HasTriedToLogin = false
      LoginError = None
      CanLogin = false
      LoggingIn = false },
    Cmd.batch [ Cmd.ofMsg UpdateValidationErrors ]

let validateInput (state: State) =
    let usernameRules =
        [ String.IsNullOrWhiteSpace(state.InputUsername), "Field 'Username' cannot be empty"
          state.InputUsername.Trim().Length < 5, "Field 'Username' must at least have 5 characters" ]

    let passwordRules =
        [ String.IsNullOrWhiteSpace(state.InputPassword), "Field 'Password' cannot be empty"
          state.InputPassword.Trim().Length < 5, "Field 'Password' must at least have 5 characters" ]

    let usernameValidationErrors =
        usernameRules |> List.filter fst |> List.map snd

    let passwordValidationErrors =
        passwordRules |> List.filter fst |> List.map snd

    usernameValidationErrors, passwordValidationErrors

let update msg (state: State) =
    match msg with
    | ChangeUsername name ->
        let nextState = { state with InputUsername = name }
        nextState, Cmd.ofMsg UpdateValidationErrors
    | ChangePassword pass ->
        let nextState = { state with InputPassword = pass }
        nextState, Cmd.ofMsg UpdateValidationErrors
    | UpdateValidationErrors ->
        let usernameErrors, passwordErrors = validateInput state

        let nextState =
            { state with
                  UsernameValidationErrors = usernameErrors
                  PasswordValidationErrors = passwordErrors }

        nextState, Cmd.ofMsg UpdateCanLogin
    | UpdateCanLogin ->
        let canLogin =
            [ state.InputUsername.Trim().Length >= 5
              state.InputPassword.Trim().Length >= 5
              List.isEmpty state.UsernameValidationErrors
              List.isEmpty state.PasswordValidationErrors ]
            |> Seq.forall id

        let nextState = { state with CanLogin = canLogin }
        nextState, Cmd.none
    | Login ->
        let state = { state with HasTriedToLogin = true }
        let usernameErrors, passwordErrors = validateInput state

        let startLogin =
            List.isEmpty usernameErrors
            && List.isEmpty passwordErrors

        if not startLogin then
            state, Cmd.none
        else
            let nextState = { state with LoggingIn = true }

            let credentials =
                { Username = state.InputUsername
                  Password = state.InputPassword }

            nextState, login credentials
    | LoginSuccess token ->
        // this message is *intercepted* by parent component
        let nextState = { state with LoggingIn = false }

        let successFeedback =
            Toastr.message "Login succesful"
            |> Toastr.withTitle "Login"
            |> Toastr.success

        nextState, successFeedback
    | LoginFailed error ->
        let nextState =
            { state with
                  LoginError = Some error
                  LoggingIn = false }

        let showErrorMsg =
            Toastr.message error
            |> Toastr.withTitle "Login"
            |> Toastr.error

        nextState, showErrorMsg
