module Posts


open Shared
open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open React.EventTimeline

open Elmish
open Elmish.SweetAlert
open Common

type Page =
    | AllPosts
    | Post of slug : string

type Msg =
    | LoadLatestPosts
    | LoadLatestPostsFinished of list<BlogPostItem>
    | LoadLatestPostsError of error : string
    | LoadSinglePost of slug : string
    | LoadSinglePostFinished of BlogPostItem
    | LoadSinglePostError of error : string
    | NavigateToPost of slug : string
    | EditPost of postId : int
    | AskPermissionToDeletePost of postId : int
    | DeletePost of postId : int
    | CancelPostDeletion
    | PostDeletedSuccessfully
    | DeletePostError of error : string
    | DoNothing

type State =
    { Post : Remote<BlogPostItem>
      LatestPosts : Remote<list<BlogPostItem>>
      DeletingPost : Option<int> }


let monthName =
    function 
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

let normalize (n : int) =
    if n < 10 then (sprintf "0%d" n)
    else (string n)

let formatDate (date : DateTime) =
    sprintf "%d/%s/%s %s:%s" date.Year (normalize date.Month) (normalize date.Day) (normalize date.Hour) 
        (normalize date.Minute)

let postItem (post : BlogPostItem) dispatch =
    let datePublished = formatDate post.DateAdded
    let createdAt = sprintf "Published %s" datePublished
    let subtitle = sprintf "Tags: %s" (String.concat ", " post.Tags)
    
    let color =
        if post.Featured then "blue"
        else "green"
    
    let icon =
        if post.Featured then i [ ClassName "fa fa-star" ] []
        else i [ ClassName "fa fa-calendar " ] []
    
    timelineEvent [ ClassName "blogpost"
                    Style [ Padding 10
                            BorderRadius 5 ]
                    OnClick(fun _ -> dispatch (NavigateToPost post.Slug))
                    Title(h5 [] [ str post.Title ])
                    Subtitle subtitle
                    CreatedAt createdAt
                    Icon icon
                    IconColor color ] []

let timelineEvents name (blogPosts : list<BlogPostItem>) dispatch =
    let title = h3 [ ClassName "title" ] [ str name ]
    let postedNewestToOldest = List.sortByDescending (fun post -> post.DateAdded) blogPosts
    let timelineEvents = List.map (fun post -> postItem post dispatch) postedNewestToOldest
    div [ Style [ MarginTop 5 ] ] [ title
                                    timeline timelineEvents ]

/// Groups posts by month from most recent to oldest
let latestPosts (blogPosts : list<BlogPostItem>) dispatch =
    blogPosts
    |> List.sortByDescending (fun post -> post.DateAdded)
    |> List.groupBy (fun post -> post.DateAdded.Year, post.DateAdded.Month)
    |> List.map (fun ((year, month), posts) -> 
           let title = (monthName month) + " " + string year
           timelineEvents title posts dispatch)
    |> div []

let msgStyle color =
    Style [ Color color
            Margin 20
            Padding 20
            Border(sprintf "2px solid %s" color)
            BorderRadius 10 ]

let infoMsg msg = h2 [ msgStyle "green" ] [ str msg ]

let adminActions blogPost state dispatch =
    div [] 
        [ span [] 
              [ button [ ClassName "btn btn-info"
                         Style [ Margin 5 ]
                         OnClick(fun _ -> dispatch (EditPost blogPost.Id)) ] [ span [] [ i [ Style [ Margin 5 ]
                                                                                             ClassName "fa fa-edit" ] []
                                                                                         str "Edit" ] ]
                
                button [ ClassName "btn btn-danger"
                         Style [ Margin 5 ]
                         OnClick(fun _ -> dispatch (AskPermissionToDeletePost blogPost.Id)) ] 
                    [ span [] [ i [ Style [ Margin 5 ]
                                    ClassName "fa fa-times" ] []
                                str "Delete" ] ] ] ]

let render currentPage isAdminLoggedIn (state : State) dispatch =
    match currentPage with
    | AllPosts -> 
        match state.LatestPosts with
        | Body [] -> infoMsg "There aren't any stories published yet"
        | Body posts -> latestPosts posts dispatch
        | Loading -> Common.spinner
        | Remote.Empty -> div [] []
        | LoadError msg -> Common.errorMsg msg
    | Post _ -> 
        match state.Post with
        | Body post -> 
            if not isAdminLoggedIn then 
                Marked.marked [ Marked.Content post.Content
                                Marked.Options [ Marked.Sanitize false ] ]
            else 
                div [] [ ofList [ if isAdminLoggedIn then yield adminActions post state dispatch ]
                         hr []
                         Marked.marked [ Marked.Content post.Content
                                         Marked.Options [ Marked.Sanitize false ] ] ]
        | Loading -> Common.spinner
        | Remote.Empty -> div [] []
        | LoadError msg -> Common.errorMsg msg

let loadPosts =
    Cmd.ofAsync Server.api.getPosts () LoadLatestPostsFinished 
        (fun _ -> LoadLatestPostsError "Network error: could not retrieve the blog posts")

let loadSinglePost slug =
    Cmd.ofAsync Server.api.getPostBySlug slug (function 
        | Some post -> LoadSinglePostFinished post
        | None -> LoadSinglePostError("Could not find the requested blog post '" + slug + "'.")) 
        (fun _ -> LoadSinglePostError "Network error: could not retrieve the requested blog post")

let update securityToken (state : State) (msg : Msg) =
    match msg with
    | NavigateToPost slug -> state, Urls.navigate [ Urls.posts; slug ]
    | LoadLatestPosts -> 
        let nextState = { state with LatestPosts = Loading }
        nextState, loadPosts
    | LoadLatestPostsFinished posts -> 
        let nextState = { state with LatestPosts = Body posts }
        nextState, Cmd.none
    | LoadLatestPostsError errorMsg -> 
        let nextState = { state with LatestPosts = LoadError errorMsg }
        nextState, Cmd.none
    | LoadSinglePost slug -> 
        let nextState = { state with Post = Loading }
        nextState, loadSinglePost slug
    | LoadSinglePostFinished content -> 
        let nextState = { state with Post = Body content }
        nextState, Cmd.none
    | LoadSinglePostError errorMsg -> 
        let nextState = { state with Post = LoadError errorMsg }
        nextState, Cmd.none
    | EditPost _ when state.DeletingPost.IsSome -> 
        state, Toastr.warning (Toastr.message "Post is being deleted, please wait...")
    | EditPost postId -> 
        state, 
        Urls.navigate [ Urls.admin
                        Urls.editPost
                        string postId ]
    | AskPermissionToDeletePost _ when state.DeletingPost.IsSome -> 
        state, Toastr.warning (Toastr.message "Post is being deleted, please wait...")
    | AskPermissionToDeletePost postId -> 
        let handleConfirm =
            function 
            | ConfirmAlertResult.Confirmed -> DeletePost postId
            | ConfirmAlertResult.Dismissed reason -> CancelPostDeletion
        
        let confirmAlert =
            ConfirmAlert("You will not be able to undo this action", handleConfirm)
                .Title("Are you sure you want to delete this blog post?").Type(AlertType.Question)
        state, SweetAlert.Run(confirmAlert)
    | DeletePost postId -> 
        match securityToken with
        | None -> state, Toastr.error (Toastr.message "Oeps! You shouldn't be seeing this :p")
        | Some token -> 
            let nextState = { state with DeletingPost = Some postId }
            
            let request =
                { Token = token
                  Body = postId }
            
            let successHandler =
                function 
                | Error authError -> DeletePostError "User was unauthorized to delete the article"
                | Ok DeletePostResult.PostDeleted -> PostDeletedSuccessfully
                | Ok DeletePostResult.PostDoesNotExist -> 
                    DeletePostError "It seems that the article does not exist any more"
                | Ok DeletePostResult.DatabaseErrorWhileDeletingPost -> 
                    DeletePostError "Internal error of the server's database while deleting the article"
            
            let deleteCmd =
                Cmd.fromAsync { Value = Server.api.deletePublishedArticleById request
                                Error =
                                    fun _ -> DeletePostError "Network error while occured while deleting the article"
                                Success = fun result -> successHandler result }
            
            nextState, deleteCmd
    | DeletePostError errorMsg -> 
        let nextState = { state with DeletingPost = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    | PostDeletedSuccessfully -> 
        let nextState =
            { state with DeletingPost = None
                         Post = Remote.Empty }
        nextState, 
        Cmd.batch [ Urls.navigate [ Urls.posts ]
                    Toastr.info (Toastr.message "Post was successfully deleted") ]
    | CancelPostDeletion -> state, Toastr.info (Toastr.message "Alright, we won't delete anything then")
    | DoNothing -> state, Cmd.none

let init() =
    let initialModel =
        { LatestPosts = Remote.Empty
          Post = Remote.Empty
          DeletingPost = None }
    initialModel, Cmd.ofMsg LoadLatestPosts
