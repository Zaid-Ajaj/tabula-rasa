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


let urlSubscription appState : Cmd<_> = 
    [ fun dispatch -> 
        let onChange _ = 
            match parseUrl window.location.hash with 
            | Some parsedPage -> dispatch (App.Types.AppMsg.UrlUpdated parsedPage)
            | None -> ()
        
        // listen to manual hash changes or page refresh
        window.addEventListener_hashchange(unbox onChange)
        // listen to custom navigation events published by `Urls.navigate [ . . .  ]`
        window.addEventListener(Urls.navigationEvent, unbox onChange) ]  

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