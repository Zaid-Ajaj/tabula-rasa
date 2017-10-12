module Info.View

open Fable.Helpers.React
open Fable.Helpers.React.Props

let root =
  div
    [ ClassName "content" ]
    [ h1
        [ ]
        [ str "About Me" ]
      hr [ ]
      p [ ]
        [ str "Here is where I tell you about myself" ] ]