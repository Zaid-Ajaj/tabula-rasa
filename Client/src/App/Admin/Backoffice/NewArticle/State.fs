module Admin.Backoffice.NewArticle.State

open Elmish
open Admin.Backoffice.NewArticle.Types
open Fable.PowerPack
open Shared.ViewModels

let init() = 
    let initialState : NewArticleState = {
        Content = ""
        Slug = ""
        Tags = []
        Title = ""
        Preview = false
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
          let server = Server.createProxy()
          let request : SecureRequest<NewBlogPostReq> = 
            { Token = authToken
              Body = { Title = state.Title; Slug = state.Slug; Content = state.Content; Tags = [] } }
          
          nextState, Cmd.ofAsync server.publishNewPost request
                                   (fun result -> Published) 
                                   (fun ex -> PublishError "Could not publish post")
    | Published ->
        { state with IsPublishing = false }, Toastr.message "Post published successfully" |> Toastr.success
    
    | AddTag tag ->
        let existingTag = List.tryFind ((=) tag) state.Tags
        match existingTag with
        | Some _ -> state, Cmd.none
        | None -> 
            let nextTags = tag :: state.Tags
            let nextState = { state with Tags = nextTags }
            nextState, Cmd.none 
    | _ ->
        state, Cmd.none