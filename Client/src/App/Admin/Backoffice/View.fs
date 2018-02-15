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
       p [ ] [ str "Stories that you have published." ]  ]
    |> cardContainer 
       
let drafts = 
    div 
     [ ]
     [ h3 [ ] [ leftIcon "file-text-o"; str "Drafts" ]
       p [ ] [ str "Articles that you are still working on and havn't published yet." ]  ]
    |> cardContainer 

let settings = 
    div 
     [ ]
     [ h3 [ ] [ leftIcon "cogs"; str "Settings" ]
       p [ ] [ str "Public articles you have written." ]  ]
    |> cardContainer 

let writeArticle = 
    div 
     [  ]
     [ h3 [ ] [ leftIcon "plus"; str "New Article" ]
       p [ ] [ str "A story is the best way to share your ideas with the world." ]  ]
    |> cardContainer 

let subscribers = 
    div 
     [ ]
     [ h3 [ ] [ leftIcon "users"; str "Subscribers" ]
       p [ ] [ str "View who subscribes to your blog" ]  ]
    |> cardContainer 

let oneThirdPage child page dispatch = 
    div [ ClassName "col-md-4"; OnClick (fun _ -> dispatch (NavigateTo page)) ]
        [ child ]
        
let logout dispatch = 
    div [ ClassName "col-md-4"; OnClick (fun _ -> dispatch Logout) ]
        [ cardContainer <|
             div [ ] 
                 [ h3 [ ] [ leftIcon "power-off"; str "Logout" ]
                   p [ ] [ str "Return to your home page" ]] ]

let render (state: State) dispatch = 
    match state.CurrentPage with
    | Home -> 
        div 
         [ Style [ PaddingLeft 30 ]  ]
         [ div 
             [ ClassName "row" ]
             [ oneThirdPage stories Published dispatch
               oneThirdPage drafts Drafts dispatch
               oneThirdPage settings Settings dispatch
               oneThirdPage writeArticle NewArticle dispatch 
               oneThirdPage subscribers Subscribers dispatch
               logout dispatch] ]
    | NewArticle ->
        NewArticle.View.render state.NewArticleState (NewArticleMsg >> dispatch)
    | _ -> 
        div [ ] [ ]