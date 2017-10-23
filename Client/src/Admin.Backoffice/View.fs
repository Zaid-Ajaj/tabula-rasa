module Admin.Backoffice.View

open Admin.Backoffice.Types
open Fable.Helpers.React.Props
open Fable.Helpers.React

let leftIcon name = 
    span [ Style [ Margin 10 ] ] 
         [ i [ ClassName (sprintf "fa fa-%s" name) ] [ ] ]

let cardContainer child = 
    div 
     [ ClassName "card admin-section"; ]
     [ div
        [ ClassName "card-block" ]
        [ div 
           [ ClassName "card-title"; Style [ Margin 20 ] ]
           [ child ] ] ]

let stories = 
    div 
     [ ]
     [ h3 [ ] [ leftIcon "book"; str "Stories" ]
       p [ ] [ str "Public articles you have written." ]  ]
    |> cardContainer 
       
let drafts = 
    div 
     [ ]
     [ h3 [ ] [ leftIcon "file-text-o"; str "Drafts" ]
       p [ ] [ str "Stories you havn't published yet." ]  ]
    |> cardContainer 

let settings = 
    div 
     [ ]
     [ h3 [ ]  [ leftIcon "cogs"; str "Settings" ]
       p [ ] [ str "Public articles you have written." ]  ]
    |> cardContainer 

let writeStory = 
    div 
     [ ]
     [ h3 [ ]  [ leftIcon "plus"; str "New Story" ]
       p [ ] [ str "Write a story" ]  ]
    |> cardContainer 

let subscribers = 
    div 
     [ ]
     [ h3 [ ]  [ leftIcon "users"; str "Subscribers" ]
       p [ ] [ str "View who subscribes to your blog" ]  ]
    |> cardContainer 

let oneThirdPage child = 
    div [ ClassName "col-md-4" ]
        [ child ]

let render (state: State) dispatch = 
    div 
     [ Style [ PaddingLeft 30 ]  ]
     [ div 
         [ ClassName "row" ]
         [ oneThirdPage stories
           oneThirdPage drafts 
           oneThirdPage settings
           oneThirdPage writeStory
           oneThirdPage subscribers ] ]