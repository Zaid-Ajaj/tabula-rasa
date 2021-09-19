module Admin.Types

type Page =
    | Login
    | Backoffice of Backoffice.Types.Page

type Msg =
    | LoginMsg of Admin.Login.Types.Msg
    | BackofficeMsg of Backoffice.Types.Msg

type State =
    { SecurityToken: string option
      Backoffice: Backoffice.Types.State
      Login: Login.Types.State }
