module Admin.Backoffice.NewArticle.View

open Admin.Backoffice.NewArticle.Types

open Fable.Helpers.React.Props
open Fable.Helpers.React
open Fable.Core.JsInterop
open React.Marked

let title state dispatch = 
  let publishBtnContent = 
    if state.IsPublishing 
    then i [ ClassName "fa fa-circle-o-notch fa-spin" ] [ ]
    else str "Publish" 
    
  h1 
    [ ] 
    [ str "Write a new post" 
      button 
        [ ClassName (if state.Preview then "btn btn-success" else "btn btn-info")
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch TogglePreview) ] 
        [ str "Preview" ]
      button 
        [ ClassName "btn btn-success"
          Style [ MarginLeft 15 ]
          OnClick (fun _ -> dispatch Publish) ] 
        [ publishBtnContent ] ]
        
let onTextChanged disptach = 
  OnChange <| fun (ev: Fable.Import.React.FormEvent) ->
    let value : string = !!ev.target?value
    value |> disptach 

let contentEditor state dispatch = 
  textarea 
    [ ClassName "form-control"
      Rows 13.0 
      DefaultValue state.Content
      onTextChanged (SetContent >> dispatch) ] 
    [ ] 

let editor state dispatch = 
 let spacing = Style [ Margin 5 ]
 div
  [ ]  
  [ div 
      [ ClassName "row" ]
      [ div 
          [ ClassName "col"; spacing ]
          [ label [ spacing ] [ str "Title" ] 
            input [ ClassName "form-control"; 
                    DefaultValue state.Title
                    onTextChanged (SetTitle >> dispatch)
                    spacing ] ]
        div 
          [ ClassName "col"; spacing ]
          [ label [ spacing ] [ str "Slug" ] 
            input [ ClassName "form-control";
                    DefaultValue state.Slug
                    onTextChanged (SetSlug >> dispatch)
                    spacing ] ] ]
    form 
      [ Style [ Margin 10 ] ]
      [ div 
          [ ClassName "form-group" ]
          [ label [] [ str "Content" ]
            contentEditor state dispatch ] ] ]
                
let preview state = 
  div 
    [ ClassName "card"; Style [ Padding 20 ] ] 
    [ marked [ Content state.Content ] ] 

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