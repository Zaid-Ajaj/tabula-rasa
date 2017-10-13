module Admin.Backoffice.View

open Admin.Backoffice.Types
open Fable.Helpers.React.Props
open Fable.Helpers.React

let render (state: State) dispatch = 
    h1 [ ] [ str "Hello Admin" ]