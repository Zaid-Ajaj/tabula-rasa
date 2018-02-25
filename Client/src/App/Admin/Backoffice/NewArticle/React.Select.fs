module React.Select

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop

[<Pojo>]
type SelectOption = { label: string; value: string }

[<Erase>]
type SelectValue = 
    | Single of string
    | Multiple of string[]
    
type ICreatableProps = 
    | Multi of bool 
    | [<CompiledName("options")>] SelectableOptions of SelectOption []
    | [<CompiledName("value")>] Value of string 
    | [<CompiledName("value")>] Values of string[]  
    | [<CompiledName("onChange")>] OnValueChanged of (string -> unit)
    | [<CompiledName("onChange")>] OnValuesChanged of (SelectOption [] -> unit)
    interface IHTMLProp
    
[<Emit("$0.constructor === Array")>]
let isMulti (value: SelectValue) : bool = jsNative

[<Emit("$0")>]
let toSingle (value: SelectValue) : string = jsNative 

[<Emit("$0")>]
let toMulti (value: SelectValue) : string[] = jsNative 

importAll "react-select/dist/react-select.css";
   
let creatable (props: IHTMLProp list) = 
    ofImport "default.Creatable"
             "react-select"
             (keyValueList CaseRules.LowerFirst props)
             [] 