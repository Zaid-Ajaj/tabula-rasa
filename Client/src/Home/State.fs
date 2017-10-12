module Home.State

open Elmish
open Types

let init () : Model * Cmd<Msg> =
  "", Cmd.none

let update msg model : Model * Cmd<Msg> =
  match msg with
  | ChangeStr str ->
      str, Cmd.none
