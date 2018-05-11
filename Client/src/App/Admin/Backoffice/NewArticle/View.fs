module Admin.Backoffice.NewArticle.View

open Admin.Backoffice.NewArticle.Types

open Fable.Helpers.React.Props
open Fable.Helpers.React
open Fable.Core.JsInterop
open React.Select

let title state dispatch = 
  let publishBtnContent = 
    if state.IsPublishing 
    then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [ ]
    else str "Publish" 
  let saveDraftContent = 
    if state.IsSavingDraft 
    then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [ ]
    else str "Save As Draft" 
  h1 
    [ ] 
    [ str "New Post" 
      button 
        [ ClassName "btn btn-info"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch TogglePreview) ] 
        [ str (if state.Preview then "Back to Post" else "Preview") ]
      button 
        [ ClassName "btn btn-info"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch SaveAsDraft) ] 
        [ saveDraftContent ]
      button 
        [ ClassName "btn btn-success"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch Publish) ] 
        [ publishBtnContent ] ]
        


let spacing = Style [ Margin 5 ]
 
let contentEditor state dispatch = 
   div 
     [ ClassName "form-group"; spacing ]
     [ label [] [ str "Content" ]
       textarea 
           [ ClassName "form-control"
             Rows 13.0 
             DefaultValue state.Content
             Common.onTextChanged (SetContent >> dispatch) ] 
           [ ] ]

let titleAndSlug state dispatch = 
  div 
    [ ClassName "row" ]
    [ div 
        [ ClassName "col"; spacing ]
        [ label [ spacing ] [ str "Title" ] 
          input [ ClassName "form-control"; 
                  DefaultValue state.Title
                  Common.onTextChanged (SetTitle >> dispatch)
                  spacing ] ]
      div 
        [ ClassName "col"; spacing ]
        [ label [ spacing ] [ str "Slug" ] 
          input [ ClassName "form-control";
                  DefaultValue state.Slug
                  Common.onTextChanged (SetSlug >> dispatch)
                  spacing ] ] ] 

let tagsCreatable state dispatch = 
  let options = 
    state.Tags
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
                     Values (Array.ofList state.Tags)
                     OnValuesChanged (Array.map asTag >> AddTags >> dispatch) ] ] ]
                 
let editor state dispatch = 
  div  
    [ Style [ Margin 10 ] ]
    [ div 
        [ ] 
        [ titleAndSlug state dispatch
          br [ ]
          tagsCreatable state dispatch
          br [ ]
          contentEditor state dispatch ] ]

     
let preview state = 
  div 
    [ ClassName "card"; Style [ Padding 20 ] ] 
    [ Marked.marked [ Marked.Content state.Content; Marked.Options [ Marked.Sanitize false ] ] ] 

let body state dispatch = 
  if state.Preview 
  then preview state
  else editor state dispatch
  
let render (state : NewArticleState) dispatch = 
    div 
      [ ClassName "container" ]
      [ title state dispatch
        br [ ]
        body state dispatch ] 