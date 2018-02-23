module Admin.Backoffice.NewArticle.State

open Elmish
open Admin.Backoffice.NewArticle.Types
open Elmish.Browser.Navigation
open Fable.PowerPack
open Shared.ViewModels

let init() = 
    let initialState : NewArticleState = {
        Content = ""
        Slug = ""
        Tags = []
        Title = ""
        NewTag = ""
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
          let server = Server.createProxy()
          let request : SecureRequest<NewBlogPostReq> = 
            { Token = authToken
              Body = { Title = state.Title; 
                       Slug = state.Slug; 
                       Content = state.Content; 
                       Tags = state.Tags } }
          
          nextState, Cmd.ofAsync server.publishNewPost request
                                   (fun result -> Published) 
                                   (fun ex -> PublishError "Could not publish post")
    | Published ->
        // reset state and navigate to newly created post
        let slug = state.Slug
        let nextState, _ = init()
        nextState, Cmd.batch [ Toastr.success (Toastr.message "Post published successfully")
                               Navigation.newUrl ("#posts/" + slug) ]
    
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
            
    | RemoveTag tag ->
        let nextState = { state with Tags = List.filter ((<>) tag) state.Tags }
        nextState, Cmd.none  
    | _ ->
        state, Cmd.none