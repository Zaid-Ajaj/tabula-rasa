module Admin.Types

type Msg = 
    | LoginMsg of Admin.Login.Types.Msg
    | BackofficeMsg of Admin.Backoffice.Types.Msg

type State =  
  { SecurityToken : string option 
    Backoffice : Admin.Backoffice.Types.State
    Login : Admin.Login.Types.State }