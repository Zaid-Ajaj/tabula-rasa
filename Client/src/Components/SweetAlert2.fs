[<RequireQualifiedAccess>]
module SweetAlert 

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.JS

[<RequireQualifiedAccess>]
[<StringEnum>]
type ModalType = 
    | Success 
    | Warning 
    | Error
    | Info
    | Question

type Options = 
    | Title of string 
    | Text of string 
    | Type of ModalType 
    | [<CompiledName "showCancelButton">] CancelButtonEnabled of bool 
    | ConfirmButtonText of string 
    | ConfirmButtonColor of string 
    | CancelButtonColor of string

[<Pojo>]
type ModalResult = { value: bool }

let private swal : obj -> Promise<ModalResult> = importDefault "sweetalert2"

let render (config: Options list) = swal (keyValueList CaseRules.LowerFirst config)
