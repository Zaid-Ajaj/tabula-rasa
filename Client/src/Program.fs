module Program

open App
open Elmish
open Elmish.HMR
open Elmish.React
open Elmish.Debug
open Elmish.Browser.Navigation
open Elmish.Bridge
open Fable.Core.JsInterop
open Fable.Import.Browser

importAll "../sass/app.sass"
importAll "../sass/spinner.css"

let urlSubscription appState : Cmd<_> =
    [ fun dispatch -> 
        let onChange _ =
            match parseUrl window.location.hash with
            | Some parsedPage -> dispatch (App.AppMsg.UrlUpdated parsedPage)
            | None -> ()
        // listen to manual hash changes or page refresh
        window.addEventListener_hashchange (unbox onChange)
        // listen to custom navigation events published by `Urls.navigate [ . . .  ]`
        window.addEventListener (Urls.navigationEvent, unbox onChange) ]

// App
Program.mkProgram init update render
|> Program.withSubscription urlSubscription
|> Program.withBridgeConfig (Bridge.endpoint Shared.socket |> Bridge.withMapping App.AppMsg.ServerMsg)
#if DEBUG
|> Program.withConsoleTrace
|> Program.withDebugger
|> Program.withHMR
#endif

|> Program.withReact "elmish-app"
|> Program.run
