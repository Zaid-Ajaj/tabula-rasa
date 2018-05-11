module Admin.Backoffice.Articles.View

open Shared
open Admin.Backoffice.Articles.Types 
open Fable.Helpers.React.Props
open Fable.Helpers.React


let articleActions isDeleting makingDraft (article: BlogPostItem) dispatch = 
    [ button [ ClassName "btn btn-info"; Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon false "edit"; str "Edit" ] ]
      button [ ClassName "btn btn-success";
               OnClick (fun _ -> dispatch (MakeIntoDraft article.Id))
               Style [ Margin 5 ]  ] 
             [ span [ ] [ Common.icon makingDraft "rocket"; str "Make Draft" ] ]
      button [ ClassName "btn btn-danger"; 
               Style [ Margin 5 ]
               OnClick (fun _ -> dispatch (AskPermissionToDeleteArticle article.Id))  ] 
             [ span [ ] [ Common.icon isDeleting "times"; str "Delete"; ] ] ]

let render state dispatch = 
    match state.Articles with  
    | Remote.Empty -> div [ ] [ str "Still empty" ] 
    | Loading -> Common.spinner
    | LoadError msg ->  Common.errorMsg msg
    | Body loadedArticles ->
        div 
         [ ]
         [ h1 [ ] [ str "Published Stories" ]    
           table [ ClassName "table table-bordered" ]
                 [ thead [ ] 
                         [ th [ ] [ str "ID" ]
                           th [ ] [ str "Title" ]
                           th [ ] [ str "Tags" ]
                           th [ ] [ str "Slug" ]
                           th [ ] [ str "Actions" ] ]
                   tbody [ ]  
                         [ for article in loadedArticles -> 
                             let isMakingDraft = (state.MakingDraft = Some article.Id) 
                             let isDeleting = (state.DeletingArticle = Some article.Id)
                             let actionSection = articleActions isDeleting isMakingDraft article dispatch
                             tr [ ] 
                                [ td [ ] [ str (string article.Id) ]
                                  td [ ] [ str article.Title ]
                                  td [ ] [ str (String.concat ", " article.Tags) ]
                                  td [ ] [ str article.Slug  ]
                                  td [ Style [ Width "360px" ] ] actionSection ] ] ] ] 