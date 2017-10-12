module Counter.State

open Elmish
open Types

let init () : Model * Cmd<Msg> =
  0, Cmd.none

let update msg model =
  match msg with
  | Increment -> model + 1, Cmd.none
  | Decrement -> model - 1, Cmd.none
  | Reset -> 0, Cmd.none