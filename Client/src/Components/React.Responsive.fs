module React.Responsive

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop

type IMediaQueryProps = 
    | MaxDeviceWidth of int
    | MinDeviceWidth of int
    | MaxWidth of int 
    | MinWidth of int 
    | Orientation of string
    interface IHTMLProp
    
let mediaQuery (props: IHTMLProp list) children = 
    ofImport "default" 
             "react-responsive" 
             (keyValueList CaseRules.LowerFirst props) 
             children