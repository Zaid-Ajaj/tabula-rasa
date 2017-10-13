module Admin.State

open Elmish
open Admin.Types

let update msg model =
    model, Cmd.none

let init() = { Name = "Zaid" }, Cmd.none
