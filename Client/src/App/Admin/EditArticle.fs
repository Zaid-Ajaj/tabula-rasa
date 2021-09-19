module EditArticle

open Shared
open Fable.Helpers.React.Props
open Fable.Helpers.React
open React.Select
open Elmish
open Common

type State =
    { ArticleToEdit: Remote<BlogPostItem>
      Preview: bool
      SavingChanges: bool }

type Msg =
    | LoadArticleToEdit of postId: int
    | ArticleLoaded of BlogPostItem
    | LoadArticleError of string
    | SetSlug of string
    | SetTitle of string
    | SetContent of string
    | TogglePreview
    | SaveChanges
    | SavedChanges
    | SaveChangesError of string
    | AddTags of string []
    | DoNothing



let title state dispatch =
    let publishBtnContent =
        if state.SavingChanges then
            i [ ClassName "fa fa-circle-o-notch fa-spin" ] []
        else
            str "Save Changes"

    h1 [] [
        str "Edit Post"
        button [ ClassName "btn btn-info"
                 Style [ MarginLeft 15 ]
                 OnClick(fun _ -> dispatch TogglePreview) ] [
            str (
                if state.Preview then
                    "Back to Post"
                else
                    "Preview"
            )
        ]
        button [ ClassName "btn btn-success"
                 Style [ MarginLeft 15 ]
                 OnClick(fun _ -> dispatch SaveChanges) ] [
            publishBtnContent
        ]
    ]

let spacing = Style [ Margin 5 ]

let contentEditor (article: BlogPostItem) dispatch =
    div [ ClassName "form-group"; spacing ] [
        label [] [ str "Content" ]
        textarea [ ClassName "form-control"
                   Rows 13.0
                   DefaultValue article.Content
                   Common.onTextChanged (SetContent >> dispatch) ] []
    ]

let titleAndSlug (article: BlogPostItem) dispatch =
    div [ ClassName "row" ] [
        div [ ClassName "col"; spacing ] [
            label [ spacing ] [ str "Title" ]
            input [ ClassName "form-control"
                    DefaultValue article.Title
                    Common.onTextChanged (SetTitle >> dispatch)
                    spacing ]
        ]
        div [ ClassName "col"; spacing ] [
            label [ spacing ] [ str "Slug" ]
            input [ ClassName "form-control"
                    DefaultValue article.Slug
                    Common.onTextChanged (SetSlug >> dispatch)
                    spacing ]
        ]
    ]

let tagsCreatable (article: BlogPostItem) dispatch =
    let options =
        article.Tags
        |> List.rev
        |> List.map (fun tag -> { value = tag; label = tag })
        |> Array.ofList

    let asTag { value = tag; label = _ } = tag

    div [ ClassName "row"
          Style [ MarginLeft -3 ] ] [
        div [ ClassName "col-md-1" ] [
            label [ spacing ] [ str "Tags" ]
        ]
        div [ ClassName "col-md-8" ] [
            creatable [ Multi true
                        SelectableOptions options
                        Values(Array.ofList article.Tags)
                        OnValuesChanged(Array.map asTag >> AddTags >> dispatch) ]
        ]
    ]

let editor (article: BlogPostItem) dispatch =
    div [ Style [ Margin 10 ] ] [
        div [] [
            titleAndSlug article dispatch
            br []
            tagsCreatable article dispatch
            br []
            contentEditor article dispatch
        ]
    ]

let preview (article: BlogPostItem) =
    div [ ClassName "card"
          Style [ Padding 20 ] ] [
        Marked.marked [ Marked.Content article.Content
                        Marked.Options [ Marked.Sanitize false ] ]
    ]

let body isPreview (article: BlogPostItem) dispatch =
    if isPreview then
        preview article
    else
        editor article dispatch

let render (state: State) dispatch =
    match state.ArticleToEdit with
    | Body article ->
        div [ ClassName "container" ] [
            title state dispatch
            br []
            body state.Preview article dispatch
        ]
    | Remote.Empty -> div [] []
    | Loading -> Common.spinner
    | LoadError errorMsg -> Common.errorMsg errorMsg

let init () =
    { ArticleToEdit = Remote.Empty
      Preview = false
      SavingChanges = false },
    Cmd.none

let update authToken msg state =
    match state.ArticleToEdit with
    | Body article ->
        match msg with
        | SetSlug slug ->
            { state with
                  ArticleToEdit = Body({ article with Slug = slug }) },
            Cmd.none
        | SetTitle title ->
            { state with
                  ArticleToEdit = Body({ article with Title = title }) },
            Cmd.none
        | SetContent content ->
            { state with
                  ArticleToEdit = Body({ article with Content = content }) },
            Cmd.none
        | AddTags tags ->
            { state with
                  ArticleToEdit =
                      Body(
                          { article with
                                Tags = List.ofArray tags }
                      ) },
            Cmd.none
        | TogglePreview ->
            { state with
                  Preview = not state.Preview },
            Cmd.none
        | SaveChanges ->
            let nextState = { state with SavingChanges = true }

            let request = { Token = authToken; Body = article }

            let saveChangesCmd =
                Cmd.fromAsync
                    { Value = Server.api.savePostChanges request
                      Error = fun ex -> SaveChangesError "Network error while saving changes to blog post"
                      Success =
                          function
                          | Error authError -> SaveChangesError "User was unauthorized"
                          | Ok result ->
                              match result with
                              | Ok true -> SavedChanges
                              | Error err -> SaveChangesError err
                              | otherwise -> DoNothing }

            nextState, saveChangesCmd
        | SaveChangesError errorMsg ->
            let nextState = { state with SavingChanges = false }
            nextState, Toastr.error (Toastr.message errorMsg)
        | SavedChanges ->
            let nextState = { state with SavingChanges = false }
            nextState, Toastr.success (Toastr.message "Changes have been successfully updated")
        | _ -> state, Cmd.none
    | _ ->
        match msg with
        | TogglePreview ->
            let nextState =
                { state with
                      Preview = not state.Preview }

            nextState, Cmd.none
        | LoadArticleToEdit postId ->
            let nextState = { state with ArticleToEdit = Loading }

            let request = { Token = authToken; Body = postId }

            let successHandler =
                function
                | Error authError -> LoadArticleError "User is unauthorized"
                | Ok None -> LoadArticleError "Article was not found"
                | Ok (Some article) -> ArticleLoaded article

            nextState, Cmd.ofAsync Server.api.getPostById request successHandler (fun ex -> DoNothing)
        | LoadArticleError errorMsg -> state, Toastr.error (Toastr.message errorMsg)
        | ArticleLoaded article ->
            let nextState =
                { state with
                      ArticleToEdit = Body article }

            nextState, Cmd.none
        | _ -> state, Cmd.none
