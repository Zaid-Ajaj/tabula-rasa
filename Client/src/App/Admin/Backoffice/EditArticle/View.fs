module Admin.Backoffice.EditArticle.View

open Admin.Backoffice.EditArticle.Types

open Fable.Helpers.React.Props
open Fable.Helpers.React
open React.Select
open Shared

let title state dispatch = 
  let publishBtnContent = 
    if state.SavingChanges 
    then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [ ]
    else str "Save Changes" 
  h1 
    [ ] 
    [ str "Edit Post" 
      button 
        [ ClassName "btn btn-info"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch TogglePreview) ] 
        [ str (if state.Preview then "Back to Post" else "Preview") ]
      button 
        [ ClassName "btn btn-success"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch SaveChanges) ] 
        [ publishBtnContent ] ]

let spacing = Style [ Margin 5 ]
 
let contentEditor (article: BlogPostItem) dispatch = 
   div 
     [ ClassName "form-group"; spacing ]
     [ label [] [ str "Content" ]
       textarea 
           [ ClassName "form-control"
             Rows 13.0 
             DefaultValue article.Content
             Common.onTextChanged (SetContent >> dispatch) ] 
           [ ] ]

let titleAndSlug (article: BlogPostItem) dispatch = 
  div 
    [ ClassName "row" ]
    [ div 
        [ ClassName "col"; spacing ]
        [ label [ spacing ] [ str "Title" ] 
          input [ ClassName "form-control"; 
                  DefaultValue article.Title
                  Common.onTextChanged (SetTitle >> dispatch)
                  spacing ] ]
      div 
        [ ClassName "col"; spacing ]
        [ label [ spacing ] [ str "Slug" ] 
          input [ ClassName "form-control";
                  DefaultValue article.Slug
                  Common.onTextChanged (SetSlug >> dispatch)
                  spacing ] ] ]

let tagsCreatable (article: BlogPostItem) dispatch = 
  let options = 
    article.Tags
    |> List.rev
    |> List.map (fun tag -> { value = tag; label = tag })
    |> Array.ofList
  
  let asTag { value = tag; label = _ } = tag  
  
  div 
    [ ClassName "row"; Style [ MarginLeft -3 ] ] 
    [ div 
       [ ClassName "col-md-1" ] 
       [ label [ spacing ] [ str "Tags" ] ]
      div
       [ ClassName "col-md-8" ]
       [ creatable [ Multi true; 
                     SelectableOptions options;
                     Values (Array.ofList article.Tags)
                     OnValuesChanged (Array.map asTag >> AddTags >> dispatch) ] ] ]


let editor (article: BlogPostItem) dispatch = 
  div  
    [ Style [ Margin 10 ] ]
    [ div 
        [ ] 
        [ titleAndSlug article dispatch
          br [ ]
          tagsCreatable article dispatch
          br [ ]
          contentEditor article dispatch ] ]

     
let preview (article: BlogPostItem) = 
  div 
    [ ClassName "card"; Style [ Padding 20 ] ] 
    [ Marked.marked [ Marked.Content article.Content; Marked.Options [ Marked.Sanitize false ] ] ] 

let body isPreview (article: BlogPostItem) dispatch = 
  if isPreview 
  then preview article
  else editor article dispatch
  
let render (state : State) dispatch = 
    match state.ArticleToEdit with 
    | Body article ->
        div 
          [ ClassName "container" ]
          [ title state dispatch
            br [ ]
            body state.Preview article dispatch ] 
    | Empty -> div [ ] [ ]
    | Loading -> Common.spinner
    | LoadError errorMsg -> Common.errorMsg errorMsg