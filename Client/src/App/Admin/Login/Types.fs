module Admin.Login.Types

type Msg =
    | Login
    | ChangeUsername of string
    | ChangePassword of string
    | LoginSuccess of adminSecureToken : string
    | LoginFailed of error : string
    | UpdateValidationErrors
    | UpdateCanLogin

type State =
    { LoggingIn : bool
      InputUsername : string
      UsernameValidationErrors : string list
      PasswordValidationErrors : string list
      InputPassword : string
      HasTriedToLogin : bool
      LoginError : string option
      CanLogin : bool }
