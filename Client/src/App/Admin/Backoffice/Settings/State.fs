module Admin.Backoffice.Settings.State 

open Shared
open Elmish
open Admin.Backoffice.Settings.Types 

let init() = { BlogInfo = Empty; ShowingUserSettings = false }, Cmd.none

let update msg state = 
    match msg with 
    | LoadBlogInfo ->
        let nextState = { state with BlogInfo = Loading }
        nextState, Cmd.ofAsync Server.api.getBlogInfo () BlogInfoLoaded (fun ex -> LoadBlogInfoError "Network error while loading blog information")
    
    | BlogInfoLoaded blogInfo ->
        let nextState = { state with BlogInfo = Body blogInfo } 
        nextState, Cmd.none 

    | LoadBlogInfoError errorMsg ->
        let nextState = { state with BlogInfo = LoadError errorMsg }
        nextState, Toastr.error (Toastr.message errorMsg)

    | ShowBlogSettings -> 
        let nextState = { state with ShowingUserSettings = false }
        nextState, Cmd.none
         
    | ShowUserSettings ->
        let nextState = { state with ShowingUserSettings = true }
        nextState, Cmd.none

    | otherMsg -> 
        match state.BlogInfo with 
        | Body blogInfo -> 
            match otherMsg with 
            | SetTitle title -> 
                let nextBlogInfo = { blogInfo with BlogTitle = title }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 
            
            | SetName name ->
                let nextBlogInfo = { blogInfo with Name = name }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 

            | SetBio bio -> 
                let nextBlogInfo = { blogInfo with Bio = bio }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none            
            
            | SetAbout about -> 
                let nextBlogInfo = { blogInfo with About = about }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 

            | SetProfileImgUrl url ->
                let nextBlogInfo = { blogInfo with ProfileImageUrl = url }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none

            | _ -> state, Cmd.none 
        
        | _ -> state, Cmd.none 