module React.Select

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop

type SelectOption = { label: string; value: string }

[<Erase>]
type SelectValue = 
    | Single of string
    | Multiple of string[]
    
type ICreatableProps = 
    | Multi of bool 
    | [<CompiledName("options")>] SelectableOptions of SelectOption []
    | Value of SelectValue 
    | [<CompiledName("onChange")>] OnSelect of (SelectValue -> unit)
    
[<Emit("$0.constructor === Array")>]
let isMulti (value: SelectValue) : bool = jsNative

[<Emit("$0")>]
let toSingle (value: SelectValue) : string = jsNative 

[<Emit("$0")>]
let toMulti (value: SelectValue) : string[] = jsNative 

   
let creatable (props: ICreatableProps list) = 
    ofImport "Select"
             "react-select"
             (keyValueList CaseRules.LowerFirst props)
             [] 