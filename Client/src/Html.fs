module Html

open Fable.Helpers.React
open Fable.Helpers.React.Props

let titleLarge (props: IHTMLProp list) input = 
    let props = List.concat [ props; [ ClassName "title" ] ]
    h1 props [ str input ] 

let title (props: IHTMLProp list) input = 
    let props = List.concat [ props; [ ClassName "title" ] ]
    h3 props [ str input ] 