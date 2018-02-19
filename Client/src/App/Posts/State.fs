module Posts.State

open System
open Elmish
open Elmish.Browser.Navigation
open Posts.Types
open Fable.PowerPack
open Shared.ViewModels

let server = Server.createProxy()

let loadPostsCmd = 
    Cmd.ofAsync server.getPosts ()
        LoadLatestPostsFinished
        LoadLatestPostsError

let loadSinglePostCmd slug = 
    Cmd.ofAsync server.getPostBySlug slug
        (function | Some post -> LoadSinglePostFinished post 
                  | None -> LoadSinglePostError (new Exception("Could not find post")))
        LoadSinglePostError

let update (state: State) (msg: Msg) = 
    match msg with
    | NavigateToPost slug ->
        state, Navigation.newUrl ("#posts/" + slug)
    | LoadLatestPosts ->
        let nextState = { state with LatestPosts = Loading }
        nextState, loadPostsCmd
    | LoadLatestPostsFinished posts ->
        let nextState = { state with LatestPosts = Body posts }
        nextState, Cmd.none
    | LoadLatestPostsError ex ->
        let nextState = { state with LatestPosts = LoadError ex }
        nextState, Cmd.none
    | LoadSinglePost slug ->
        let nextState = { state with PostContent = Loading }
        nextState, loadSinglePostCmd slug
    | LoadSinglePostFinished content ->
        let nextState = { state with PostContent = Body content }
        nextState, Cmd.none
    | LoadSinglePostError ex ->
        let nextState = { state with PostContent = LoadError ex }
        nextState, Cmd.none 


let init() =
    let initialModel =  
     {  LatestPosts = Empty
        PostContent = Empty } 

    initialModel, Cmd.ofMsg LoadLatestPosts