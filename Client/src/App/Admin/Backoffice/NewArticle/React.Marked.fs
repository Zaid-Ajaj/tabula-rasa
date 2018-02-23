module React.Marked

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop

type IMarkedOptions = 
    | [<CompiledName("gfc")>] GithubFlavoured
    | Tables
    | Sanitize
    | SmartLists
    | Pedantic
    | Breaks
    | Smartypants

type IMarkedProps = 
    | [<CompiledName("value")>] Content of string
    | [<CompiledName("markedOptions")>] Options of IMarkedOptions list
    interface IHTMLProp 

let marked (props : IHTMLProp list) = 
    ofImport "MarkdownPreview" 
             "react-marked-markdown"
             (keyValueList CaseRules.LowerFirst props)
             [ ]