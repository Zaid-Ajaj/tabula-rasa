module Posts.State

open Elmish
open Elmish.Browser.Navigation
open Posts.Types
open Shared.ViewModels

let update (state: State) (msg: Msg) = 
    match msg with
    | NavigateToPost slug ->
        state, Navigation.newUrl ("#posts/" + slug)
    | LoadLatestPosts ->
        let nextState = { state with LatestPosts = Loading }
        nextState, Http.loadPosts
    | LoadLatestPostsFinished posts ->
        let nextState = { state with LatestPosts = Body posts }
        nextState, Cmd.none
    | LoadLatestPostsError ex ->
        let nextState = { state with LatestPosts = LoadError ex }
        nextState, Cmd.none
    | LoadSinglePost slug ->
        let nextState = { state with Post = Loading }
        nextState, Http.loadSinglePost slug
    | LoadSinglePostFinished content ->
        let nextState = { state with Post = Body content }
        nextState, Cmd.none
    | LoadSinglePostError errorMsg ->
        let nextState = { state with Post = LoadError errorMsg }
        nextState, Cmd.none 


let init() =
    let initialModel =  
     {  LatestPosts = Empty
        Post = Empty } 

    initialModel, Cmd.ofMsg LoadLatestPosts