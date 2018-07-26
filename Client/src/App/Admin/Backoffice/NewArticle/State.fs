module Admin.Backoffice.NewArticle.State

open Elmish
open Elmish.Bridge
open Admin.Backoffice.NewArticle.Types
open Shared
open Common

let init() = 
    let initialState : NewArticleState = {
        Content = ""
        Slug = ""
        Tags = []
        Title = ""
        NewTag = ""
        Preview = false
        IsSavingDraft = false
        IsPublishing = false
    }

    initialState, Cmd.none


let warning text = 
    Toastr.message text
    |> Toastr.withTitle "Tabula Rasa"
    |> Toastr.warning
    
let update authToken msg (state: NewArticleState) = 
    match msg with 
    | SetTitle title ->
        let nextState = { state with Title = title }
        nextState, Cmd.none

    | SetSlug slug ->
        let nextState = { state with Slug = slug }
        nextState, Cmd.none
    
    | SetContent content ->
        let nextState = { state with Content = content }
        nextState, Cmd.none
    
    | SetTag content -> 
        let nextState = { state with NewTag = content }
        nextState, Cmd.none 
         
    | TogglePreview -> 
        let nextState = { state with Preview = not state.Preview }
        nextState, Cmd.none 
        
    | Publish ->    
        if state.IsPublishing 
        then state, warning "Publishing in progress..."
        elif state.Title = "" 
        then state, warning "Title of your blog post cannot be empty" 
        elif state.Slug = "" 
        then state, warning "The slug cannot be empty"
        else 
          let nextState = { state with IsPublishing = true }
          let request : SecureRequest<NewBlogPostReq> = 
            { Token = authToken
              Body = { Title = state.Title; 
                       Slug = state.Slug; 
                       Content = state.Content; 
                       Tags = state.Tags } }
          let publishCmd = Cmd.fromAsync {
              Value = Server.api.publishNewPost request
              Error = fun ex -> PublishError "Could not publish post"
              Success = function 
                | AddedPostId id -> 
                    // let the server know, that a post was added
                    Bridge.Send (RemoteClientMsg.PostAdded)
                    Published
                | other -> PublishError "Could not publish post"
          }
          
          nextState, publishCmd
    
    | SaveAsDraft -> 
        if state.IsPublishing 
        then state, warning "Publishing in progress..."
        elif state.Title = "" 
        then state, warning "Title of your blog post cannot be empty" 
        elif state.Slug = "" 
        then state, warning "The slug cannot be empty"
        else 
          let nextState = { state with IsSavingDraft = true }
          let request : SecureRequest<NewBlogPostReq> = 
            { Token = authToken
              Body = { Title = state.Title; 
                       Slug = state.Slug; 
                       Content = state.Content; 
                       Tags = state.Tags } }
          let successHandler = function 
            | AddedPostId draftId -> DraftSaved
            | PostWithSameSlugAlreadyExists -> 
                SaveAsDraftError "A post with this slug already exists"
            | PostWithSameTitleAlreadyExists -> 
                SaveAsDraftError "A post with this title already exists"
            | AddPostResult.AuthError UserUnauthorized -> 
                SaveAsDraftError "User was unauthorized to publish the draft"
            | DatabaseErrorWhileAddingPost ->
                SaveAsDraftError "Internal error occured on the server's database while saving the draft"
             
          nextState, Cmd.ofAsync Server.api.savePostAsDraft request
                                 successHandler                             
                                 (fun ex -> SaveAsDraftError "Could not publish post")
    
    | Published ->
        // reset state and navigate to newly created post
        let slug = state.Slug
        let nextState, _ = init()
        nextState, Cmd.batch [ Toastr.success (Toastr.message "Post published successfully")
                               Urls.navigate [ Urls.posts; slug ] ]
    
    | DraftSaved ->
        // reset state and navigate to newly created post
        let nextState, _ = init()
        nextState, Cmd.batch [ Toastr.success (Toastr.message "Post saved as draft!")
                               Urls.navigate [ Urls.admin ] ] 
    
    | SaveAsDraftError errorMsg -> 
        let errorToast =
          Toastr.message errorMsg
          |> Toastr.withTitle "Publish Error"
          |> Toastr.error
        let nextState = { state with IsSavingDraft = false }
        nextState, errorToast
    | AddTag ->
        let tag = state.NewTag
        if String.length tag = 0 
        then state, Toastr.info (Toastr.message "Tag empty")
        else
        let existingTag = List.tryFind ((=) tag) state.Tags
        match existingTag with
        | Some _ -> 
            state, Toastr.info (Toastr.message "Tag already added")
        | None -> 
            let nextTags = tag :: state.Tags
            let nextState = { state with Tags = nextTags; NewTag = "" }
            nextState, Cmd.none
            
    | AddTags tags ->
        let nextState = { state with Tags = (Array.distinct >> List.ofArray) tags }
        nextState, Cmd.none
            
    | RemoveTag tag ->
        let nextState = { state with Tags = List.filter ((<>) tag) state.Tags }
        nextState, Cmd.none  
    | PublishError errorMsg ->
        let errorToast =
          Toastr.message errorMsg
          |> Toastr.withTitle "Publish Error"
          |> Toastr.error
        let nextState = { state with IsPublishing = false }
        nextState, errorToast