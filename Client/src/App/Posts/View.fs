module Posts.View

open Posts.Types
open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Shared.ViewModels
open React.Marked
open React.EventTimeline

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]

let monthName = function 
    | 1 -> "January" 
    | 2 -> "February"
    | 3 -> "March" 
    | 4 -> "April"
    | 5 -> "May" 
    | 6 -> "June" 
    | 7 -> "July" 
    | 8 -> "August"
    | 9 -> "September"
    | 10 -> "October"
    | 11 -> "November"
    | 12 -> "December"
    | _ -> ""
    
let normalize (month: int) = 
    if month < 10 
    then (sprintf "0%d" month)
    else (string month)
    
let formatDate (date : DateTime) = 
    sprintf "%d/%s/%d %d:%d" date.Year (normalize date.Month) date.Day date.Hour date.Minute
    
let postItem (post: BlogPostItem) dispatch = 
    let datePublished = formatDate post.DateAdded
    let createdAt = sprintf "Published %s" datePublished
    let subtitle = sprintf "Tags: %s" (String.concat ", " post.Tags) 
    let color = if post.Featured then "blue" else "green"  
    let icon = 
        if post.Featured 
        then i [ ClassName "fa fa-star" ] [ ]
        else i [ ClassName "fa fa-calendar " ] [ ]
    
    timelineEvent 
        [ ClassName "blogpost"
          Style [ Padding 10; BorderRadius 5 ]
          OnClick (fun _ -> dispatch (NavigateToPost post.Slug)) 
          Title (h5 [ ] [ str post.Title ])
          Subtitle subtitle
          CreatedAt createdAt
          Icon icon
          IconColor color ] 
        [  ]
             
let timelineEvents name (blogPosts : list<BlogPostItem>) dispatch =
    let title = h3 [ ClassName "title" ] [ str name ]
    let postedNewestToOldest = List.sortByDescending (fun post -> post.DateAdded) blogPosts
    let timelineEvents = List.map (fun post -> postItem post dispatch) postedNewestToOldest
    div 
      [ Style [ MarginTop 5 ] ] 
      [ title; timeline [ yield! timelineEvents ] ]    

let latestPosts (blogPosts : list<BlogPostItem>) dispatch = 
    blogPosts
    |> List.groupBy (fun post -> post.DateAdded.Year, post.DateAdded.Month)
    |> List.map (fun ((year, month), posts) -> 
        let title = (monthName month) + " " + string year  
        timelineEvents title posts dispatch)
    |> div [ ]

let errorMsg msg = 
    h1 [ Style [ Color "crimson"; Margin 20 ] ] 
       [ str msg ]
       
let render currentPage (state: State) dispatch = 
    match currentPage with
    | AllPosts -> 
        match state.LatestPosts with
        | Body posts -> div [ ] [ latestPosts posts dispatch ]
        | Loading -> spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> errorMsg msg
    | Post _ -> 
        match state.Post with
        | Body post -> marked [ Content post.Content; Options [ Sanitize false ] ]
        | Loading -> spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> errorMsg msg