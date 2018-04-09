module Admin.Backoffice.Drafts.View

open Shared.ViewModels
open Admin.Backoffice.Drafts.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props

let render ({ Drafts = drafts }) dispatch = 
    match drafts with  
    | Remote.Empty -> div [ ] [ str "Still empty" ] 
    | Loading -> Common.spinner
    | LoadError msg ->  Common.errorMsg msg
    | Body loadedDrafts ->
        div 
         [ ]
         [ h1 [ ] [ str "Drafts" ] 
           hr [ ]   
           table [ ClassName "table table-bordered" ]
                 [ thead [ ] 
                         [ th [ ] [ str "Title"   ]
                           th [ ] [ str "Slug"    ]
                           th [ ] [ str "Actions" ]  ]
                   tbody [ ]  
                         [ for draft in loadedDrafts -> 
                             tr [ ] 
                                [ td [ ] [ str draft.Title ]
                                  td [ ] [ str draft.Slug  ]
                                  td [ ] [   ] ]  ] ] ] 
           