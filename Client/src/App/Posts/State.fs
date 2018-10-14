module Posts.State

open Elmish
open Elmish.SweetAlert 
open Fable.PowerPack
open Posts.Types
open Shared
open Common


let update securityToken (state: State) (msg: Msg) = 
    match msg with
    | NavigateToPost slug ->
        state, Urls.navigate [ Urls.posts; slug ]
    
    | LoadLatestPosts ->
        let nextState = { state with LatestPosts = Loading }
        nextState, Http.loadPosts
    
    | LoadLatestPostsFinished posts ->
        let nextState = { state with LatestPosts = Body posts }
        nextState, Cmd.none
    
    | LoadLatestPostsError errorMsg ->
        let nextState = { state with LatestPosts = LoadError errorMsg }
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
    
    | EditPost _ when state.DeletingPost.IsSome ->
         state, Toastr.warning (Toastr.message "Post is being deleted, please wait...")

    | EditPost postId ->
        state, Urls.navigate [ Urls.admin; Urls.editPost; string postId ]
    
    | AskPermissionToDeletePost _ when state.DeletingPost.IsSome  ->
        state, Toastr.warning (Toastr.message "Post is being deleted, please wait...")

    | AskPermissionToDeletePost postId ->
        let handleConfirm = function
        | ConfirmAlertResult.Confirmed -> DeletePost postId 
        | ConfirmAlertResult.Dismissed reason -> CancelPostDeletion

        let confirmAlert = 
            ConfirmAlert("You will not be able to undo this action", handleConfirm)
                .Title("Are you sure you want to delete this blog post?")
                .Type(AlertType.Question)

        state, SweetAlert.Run(confirmAlert)         
    
    | DeletePost postId ->
        match securityToken with 
        | None -> state, Toastr.error (Toastr.message "Oeps! You shouldn't be seeing this :p")
        | Some token -> 
            let nextState = { state with DeletingPost = Some postId }
            let request = { Token = token; Body = postId }
            let successHandler = function
                | Error authError -> DeletePostError "User was unauthorized to delete the article" 
                | Ok DeletePostResult.PostDeleted ->     
                    PostDeletedSuccessfully 
                | Ok DeletePostResult.PostDoesNotExist ->
                    DeletePostError "It seems that the article does not exist any more"
                | Ok DeletePostResult.DatabaseErrorWhileDeletingPost ->
                    DeletePostError "Internal error of the server's database while deleting the article"
            
            let deleteCmd = 
                Cmd.fromAsync 
                   {  Value = Server.api.deletePublishedArticleById request
                      Error = fun _ ->  DeletePostError "Network error while occured while deleting the article"
                      Success = fun result -> successHandler result }

            nextState,  deleteCmd   

    | DeletePostError errorMsg ->
        let nextState = { state with DeletingPost = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | PostDeletedSuccessfully ->
        let nextState = { state with DeletingPost = None; Post = Empty }
        nextState, Cmd.batch [ Urls.navigate [ Urls.posts ]; Toastr.info (Toastr.message "Post was successfully deleted") ]
    
    | CancelPostDeletion -> 
        state, Toastr.info (Toastr.message "Alright, we won't delete anything then")

    | DoNothing ->
        state, Cmd.none

let init() =
    let initialModel =  
     {  LatestPosts = Empty
        Post = Empty
        DeletingPost = None } 

    initialModel, Cmd.ofMsg LoadLatestPosts