module Admin.View

open Admin.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

let render (state: State) dispatch = 
    h1 [ ] [ str "Admin" ]