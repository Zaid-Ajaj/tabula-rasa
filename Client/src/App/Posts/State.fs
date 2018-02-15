module Posts.State

open Elmish
open Posts.Types
open Fable.PowerPack

let loadPostsAsync() = 
    promise {
        do! Promise.sleep 3000
        return [| "Elmish is Awesome" |]
    }

let loadPostsCmd = 
    Cmd.ofPromise loadPostsAsync ()
        (Some >> LoadingPostsFinished)
        (fun ex -> LoadingPostsFinished None)

let update (state: Model) (msg: Msg) = 
    match msg with
    | LoadLatestPosts ->
        let nextState = { state with IsLoadingPosts = true }
        nextState, loadPostsCmd
    | LoadingPostsFinished (Some posts) ->
        let nextState = 
            { Posts = posts
              IsLoadingPosts = false
              Error = None }
        nextState, Cmd.none
    | LoadingPostsFinished None ->
        let nextState = 
            { state with 
                Posts = [| |]
                IsLoadingPosts = false
                Error = Some "Error while loading latest posts" }
        nextState, Cmd.none

let init() =
    let initialModel =  
     {  Posts = [|  |]; 
        IsLoadingPosts = false
        Error = None }

    initialModel, Cmd.ofMsg LoadLatestPosts