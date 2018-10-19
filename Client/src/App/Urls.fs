module Urls

open Fable.Import.Browser

let hashPrefix = sprintf "#%s"

[<Literal>]
let about = "about"

[<Literal>]
let admin = "admin"

[<Literal>]
let posts = "posts"

[<Literal>]
let newPost = "new-post"

[<Literal>]
let drafts = "drafts"

[<Literal>]
let publishedPosts = "published-posts"

[<Literal>]
let login = "login"

[<Literal>]
let settings = "settings"

[<Literal>]
let subscribers = "subscribers"

[<Literal>]
let editPost = "edit-article"

let combine xs = List.fold (sprintf "%s/%s") "" xs

[<Literal>]
let navigationEvent = "NavigationEvent"

let newUrl (newUrl : string) : Elmish.Cmd<_> =
    [ fun _ -> 
        history.pushState ((), "", newUrl)
        let ev = document.createEvent_CustomEvent()
        ev.initCustomEvent (navigationEvent, true, true, obj())
        window.dispatchEvent ev |> ignore ]

let navigate xs : Elmish.Cmd<_> =
    let nextUrl = hashPrefix (combine xs)
    [ fun _ -> 
        history.pushState ((), "", nextUrl)
        let ev = document.createEvent_CustomEvent()
        ev.initCustomEvent (navigationEvent, true, true, obj())
        window.dispatchEvent ev |> ignore ]

let (|Int|_|) input =
    match System.Int32.TryParse input with
    | true, n -> Some n
    | _ -> None
