module Admin.Types

type Page = 
    | Login
    | Backoffice of Admin.Backoffice.Types.Page

type Msg = 
    | LoginMsg of Admin.Login.Types.Msg
    | BackofficeMsg of Admin.Backoffice.Types.Msg
    | SetCurrentPage of Page

type State =  
  { SecurityToken : string option 
    Backoffice : Admin.Backoffice.Types.State
    Login : Admin.Login.Types.State
    CurrentPage : Option<Page> }