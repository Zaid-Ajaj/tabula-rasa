module Urls

open Fable.Import.Browser 

let hashPrefix = sprintf "#%s"
let [<Literal>] about = "about"
let  [<Literal>] admin = "admin"
let [<Literal>] posts = "posts"
let [<Literal>] newPost = "new-post" 
let [<Literal>] drafts = "drafts"
let [<Literal>] publishedPosts = "published-posts"
let [<Literal>] login = "login"
let [<Literal>] settings = "settings"
let [<Literal>] subscribers = "subscribers"
let [<Literal>] editPost = "edit-article"
let combine xs = List.fold (sprintf "%s/%s") "" xs

let  [<Literal>]  navigationEvent = "NavigationEvent"

let newUrl (newUrl:string ): Elmish.Cmd<_> =
    [fun _ -> history.pushState((), "", newUrl)
              let ev = document.createEvent_CustomEvent()
              ev.initCustomEvent (navigationEvent, true, true, obj())
              window.dispatchEvent ev
              |> ignore ]

         
let navigate xs : Elmish.Cmd<_> = 
    let nextUrl = hashPrefix (combine xs)
    [fun _ -> history.pushState((), "", nextUrl)
              let ev = document.createEvent_CustomEvent()
              ev.initCustomEvent (navigationEvent, true, true, obj())
              window.dispatchEvent ev
              |> ignore ]

let (|Int|_|) input = 
    match System.Int32.TryParse input with 
    | true, n -> Some n 
    | _ -> None 