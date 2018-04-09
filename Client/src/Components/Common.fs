module Common

open Fable.Helpers.React
open Fable.Helpers.React.Props

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]


let msgStyle color = 
    Style [ Color color; 
            Margin 20;
            Padding 20 
            Border (sprintf "2px solid %s" color); 
            BorderRadius 10 ]

let errorMsg msg = h2 [ msgStyle "crimson" ] [ str msg ]