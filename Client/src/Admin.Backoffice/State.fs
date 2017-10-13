module Admin.Backoffice.State

open Elmish
open Admin.Backoffice.Types

let update msg state = 
    match msg with
    | Any -> { state with Name = state.Name  }, Cmd.none

let init() = 
    { Name = "Hello" }, Cmd.none