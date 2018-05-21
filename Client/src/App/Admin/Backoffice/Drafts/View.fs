module Admin.Backoffice.Drafts.View

open Shared
open Admin.Backoffice.Drafts.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props

let draftActions isDeleting isPublishing (draft: BlogPostItem) dispatch = 
    [ button [ ClassName "btn btn-info";
               OnClick (fun _ -> dispatch (EditDraft draft.Id))  
               Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon false "edit"; str "Edit" ] ]
      button [ ClassName "btn btn-success";
               OnClick (fun _ -> dispatch (PublishDraft draft.Id))
               Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon isPublishing "rocket"; str "Publish" ] ]
      button [ ClassName "btn btn-danger"; 
               Style [ Margin 5 ]
               OnClick (fun _ -> dispatch (AskPermissionToDeleteDraft draft.Id))  ] 
             [ span [ ] [ Common.icon isDeleting "times"; str "Delete"; ] ] ]

let render state dispatch = 
    match state.Drafts with  
    | Remote.Empty -> div [ ] [ str "Still empty" ] 
    | Loading -> Common.spinner
    | LoadError msg ->  Common.errorMsg msg
    | Body loadedDrafts ->
        div 
         [ ]
         [ h1 [ ] [ str "Drafts" ]    
           table [ ClassName "table table-bordered" ]
                 [ thead [ ] 
                         [ tr [ ] 
                              [ th [ ] [ str "ID" ]
                                th [ ] [ str "Title" ]
                                th [ ] [ str "Tags" ]
                                th [ ] [ str "Feauted?" ]
                                th [ ] [ str "Slug" ]
                                th [ ] [ str "Actions" ] ] ]
                   tbody [ ]  
                         [ for draft in loadedDrafts -> 
                             let isPublishing = (state.PublishingDraft = Some draft.Id) 
                             let isDeleting = (state.DeletingDraft = Some draft.Id)
                             let actionSection = draftActions isDeleting isPublishing draft dispatch
                             let featuredButton = 
                                let className = 
                                  if draft.Featured 
                                  then "btn btn-success" 
                                  else "btn btn-secondary"
                                button [ ClassName className; 
                                         Style [ Margin 10 ]
                                         OnClick (fun ev -> dispatch (ToggleFeatured draft.Id)) ] 
                                       [ str "Featured" ]
                             tr [ ] 
                                [ td [ ] [ str (string draft.Id) ]
                                  td [ ] [ str draft.Title ]
                                  td [ ] [ str (String.concat ", " draft.Tags) ]
                                  td [ ] [ featuredButton ]
                                  td [ ] [ str draft.Slug  ]
                                  td [ Style [ Width "340px" ] ] actionSection ] ] ] ] 
           