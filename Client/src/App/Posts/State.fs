module Posts.State

open Elmish
open Posts.Types
open Fable.PowerPack
open Shared.ViewModels

let loadPostsAsync() = 
    promise {
        do! Promise.sleep 3000
        let posts = """[[{"Id":1,"Title":"Blog Post","DateAdded":"2018-02-18T18:44:36.0000000"}]]"""
        return Fable.Core.JsInterop.ofJson<list<BlogPostItem>> posts
    }

let server = Server.createProxy()

let loadPostsCmd = 
    Cmd.ofAsync server.getPosts ()
        (LoadingPostsFinished)
        (fun ex -> 
            printfn "Error while requesting posts from server"
            printfn "%s\n%s" ex.Message ex.StackTrace
            LoadingPostsError)

let update (state: Model) (msg: Msg) = 
    match msg with
    | LoadLatestPosts ->
        let nextState = { state with IsLoadingPosts = true }
        nextState, loadPostsCmd
    | LoadingPostsFinished posts ->
        let nextState = 
            { Posts = posts
              IsLoadingPosts = false
              Error = None }
        nextState, Cmd.none
    | LoadingPostsError ->
        let nextState = 
            { state with 
                Posts = [] 
                IsLoadingPosts = false
                Error = Some "Error while loading latest posts" }
        nextState, Cmd.none

let init() =
    let initialModel =  
     {  Posts = []
        IsLoadingPosts = false
        Error = None }

    initialModel, Cmd.ofMsg LoadLatestPosts