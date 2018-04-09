module Posts.View

open Posts.Types
open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Shared.ViewModels
open React.EventTimeline

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

let msgStyle color = 
    Style [ Color color; 
            Margin 20;
            Padding 20 
            Border (sprintf "2px solid %s" color); 
            BorderRadius 10 ]

let errorMsg msg = h2 [ msgStyle "crimson" ] [ str msg ]
       
let infoMsg msg = h2 [ msgStyle "green" ] [ str msg ]
    

let render currentPage (state: State) dispatch = 
    match currentPage with
    | AllPosts -> 
        match state.LatestPosts with
        | Body [] -> infoMsg "There aren't any stories published yet"
        | Body posts -> latestPosts posts dispatch
        | Loading -> Common.spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> errorMsg msg
    | Post _ -> 
        match state.Post with
        | Body post -> Marked.marked [ Marked.Content post.Content; Marked.Options [ Marked.Sanitize false ] ]
        | Loading -> Common.spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> errorMsg msg