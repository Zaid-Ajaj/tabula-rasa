module Program

open Elmish
open Elmish.HMR
open Elmish.React
open Elmish.Debug
open Elmish.Browser.Navigation

open App.State
open App.View

open Fable.Core.JsInterop
open Fable.Import.Browser

importAll "../sass/app.sass"
importAll "../sass/spinner.css"


let locationChanged appState dispatch = 
    let onChange _ = 
        console.log(sprintf "Url changed to %s, parsing page..." window.location.hash)
        match parseUrl window.location.hash with 
        | Some page -> 
            console.log(sprintf "ParsedPage %A" page)
            dispatch (App.Types.AppMsg.UrlUpdated page)
        | None -> 
            console.log "could not parse page"
            ()

    window.addEventListener_hashchange(unbox onChange)
    window.addEventListener(Urls.navigationEvent, unbox onChange)

let urlSubscription appState : Cmd<_> = 
    [ fun dispatch -> locationChanged appState dispatch ]  

// App
Program.mkProgram init update render
#if DEBUG
|> Program.withConsoleTrace
|> Program.withSubscription urlSubscription
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
|> Program.run