module Program

open Elmish
open Elmish.HMR
open Elmish.React
open Elmish.Debug
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation

open App.State
open App.View

open Fable.Core.JsInterop
importAll "../sass/app.sass"
importAll "../sass/spinner.css"


// App
Program.mkProgram init update render
|> Program.toNavigable (parseHash pageParser) urlUpdate
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
|> Program.run