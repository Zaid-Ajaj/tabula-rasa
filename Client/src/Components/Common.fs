module Common

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]

let icon isLoading name = 
    let className =
        if isLoading 
        then "fa fa-circle-o-notch fa-spin fa-fw"
        else sprintf "fa fa-%s" name
    i [ ClassName className; Style [ MarginRight 5 ] ] [ ]

let msgStyle color = 
    Style [ Color color; 
            Margin 20;
            Padding 20 
            Border (sprintf "2px solid %s" color); 
            BorderRadius 10 ]

let errorMsg msg = h2 [ msgStyle "crimson" ] [ str msg ]

let onTextChanged doSomethingWith = 
  OnChange <| fun (ev: Fable.Import.React.FormEvent) ->
    let inputTextValue : string = !!ev.target?value
    doSomethingWith inputTextValue

[<AutoOpen>]
module Cmd = 
    type AsyncValParams<'a, 'b> = 
        { Value: Async<'a>
          Success : 'a -> 'b 
          Error : exn -> 'b }

    let fromAsync (asyncParams: AsyncValParams<'a, 'b>) : Elmish.Cmd<'b> = 
        Elmish.Cmd.ofAsync
            (fun () -> asyncParams.Value) ()  
            asyncParams.Success
            asyncParams.Error