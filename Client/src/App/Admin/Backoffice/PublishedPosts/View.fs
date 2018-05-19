module Admin.Backoffice.PublishedPosts.View

open Shared
open Admin.Backoffice.PublishedPosts.Types 
open Fable.Helpers.React.Props
open Fable.Helpers.React


let postActions isDeleting makingDraft (article: BlogPostItem) dispatch = 
    [ button [ ClassName "btn btn-info";
               OnClick (fun _ -> dispatch (EditPost article.Id)) 
               Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon false "edit"; str "Edit" ] ]
      button [ ClassName "btn btn-success";
               OnClick (fun _ -> dispatch (MakeIntoDraft article.Id))
               Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon makingDraft "rocket"; str "Make Draft" ] ]
      button [ ClassName "btn btn-danger"; 
               Style [ Margin 5 ]
               OnClick (fun _ -> dispatch (AskPermissionToDeletePost article.Id))  ] 
             [ span [ ] [ Common.icon isDeleting "times"; str "Delete"; ] ] ]

let render state dispatch = 
    match state.PublishedPosts with  
    | Remote.Empty -> div [ ] [ str "Still empty" ] 
    | Loading -> Common.spinner
    | LoadError msg ->  Common.errorMsg msg
    | Body loadedPosts ->
        div 
         [ ]
         [ h1 [ ] [ str "Published Stories" ]    
           table [ ClassName "table table-bordered" ]
                 [ thead [ ] 
                         [ tr [ ] 
                              [ th [ ] [ str "ID" ]
                                th [ ] [ str "Title" ]
                                th [ ] [ str "Tags" ]
                                th [ ] [ str "Slug" ]
                                th [ ] [ str "Actions" ] ] ]
                   tbody [ ]  
                         [ for post in loadedPosts -> 
                             let isMakingDraft = (state.MakingDraft = Some post.Id) 
                             let isDeleting = (state.DeletingPost = Some post.Id)
                             let actionSection = postActions isDeleting isMakingDraft post dispatch
                             tr [ ] 
                                [ td [ ] [ str (string post.Id) ]
                                  td [ ] [ str post.Title ]
                                  td [ ] [ str (String.concat ", " post.Tags) ]
                                  td [ ] [ str post.Slug  ]
                                  td [ Style [ Width "360px" ] ] actionSection ] ] ] ] 