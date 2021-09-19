module Settings

open Shared

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Common
open System
open Elmish

type State =
    { BlogInfo: Remote<BlogInfo>
      IsChangingChanges: bool
      ShowingUserSettings: bool
      CurrentPassword: string
      NewPassword: string
      ConfirmNewPassword: string
      IsUpdatingPassword: bool }

type Msg =
    | LoadBlogInfo
    | BlogInfoLoaded of Result<BlogInfo, string>
    | LoadBlogInfoError of string
    | SetTitle of string
    | SetName of string
    | SetBio of string
    | SetAbout of string
    | SetProfileImgUrl of string
    | SaveChanges
    | ChangesSaved of successMsg: string
    | SaveChangesError of string
    | ShowUserSettings
    | ShowBlogSettings
    | SubmitNewPassword
    | UpdatePasswordError of string
    | UpdatePasswordSuccess
    | SetCurrentPassword of string
    | SetNewPassword of string
    | SetConfirmNewPassword of string


let classNames classes =
    classes
    |> List.filter snd
    |> List.map fst
    |> String.concat " "
    |> ClassName

let blogSettingsEditor (blogInfo: BlogInfo) dispatch =
    form [] [
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtBlogTitle" ] [
                str "Blog Title"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtBlogTitle"
                    Key "txtBlogTitle"
                    HTMLAttr.Type "text"
                    DefaultValue blogInfo.BlogTitle
                    Common.onTextChanged (SetTitle >> dispatch) ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtBlogBio" ] [
                str "Biography"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtBlogBio"
                    Key "txtBlogBio"
                    HTMLAttr.Type "text"
                    DefaultValue blogInfo.Bio
                    Common.onTextChanged (SetBio >> dispatch) ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtBlogName" ] [
                str "Name"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtBlogName"
                    Key "txtBlogName"
                    HTMLAttr.Type "text"
                    DefaultValue blogInfo.Name
                    Common.onTextChanged (SetName >> dispatch) ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtProfileImgUrl" ] [
                str "Profile Image Url"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtProfileImgUrl"
                    Key "txtProfileImgUrl"
                    HTMLAttr.Type "text"
                    DefaultValue blogInfo.ProfileImageUrl
                    Common.onTextChanged (SetProfileImgUrl >> dispatch) ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtAbout" ] [
                str "About"
            ]
            textarea [ ClassName "form-control"
                       DefaultValue blogInfo.About
                       Id "txtAbout"
                       Key "txtAbout"
                       Rows 8.0
                       Common.onTextChanged (SetAbout >> dispatch) ] []
        ]
    ]

let userSettings state dispatch =
    form [] [
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtCurrentPassword" ] [
                str "Current Password"
            ]
            input [ ClassName "form-control form-control-lg"
                    Key "txtCurrentPassword"
                    Id "txtCurrentPassword"
                    DefaultValue state.CurrentPassword
                    Common.onTextChanged (SetCurrentPassword >> dispatch)
                    HTMLAttr.Type "password" ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtNewPassword" ] [
                str "New Password"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtNewPassword"
                    Key "txtNewPassword"
                    DefaultValue state.NewPassword
                    Common.onTextChanged (SetNewPassword >> dispatch)
                    HTMLAttr.Type "password" ]
        ]
        div [ ClassName "form-group" ] [
            label [ HtmlFor "txtNewPasswordConfirm" ] [
                str "Confirm New Password"
            ]
            input [ ClassName "form-control form-control-lg"
                    Id "txtNewPasswordConfirm"
                    Key "txtNewPasswordConfirm"
                    DefaultValue state.ConfirmNewPassword
                    Common.onTextChanged (SetConfirmNewPassword >> dispatch)
                    HTMLAttr.Type "password" ]
        ]
    ]

let tabs state dispatch =
    ul [ ClassName "nav nav-tabs" ] [
        li [ ClassName "nav-item" ] [
            div [ classNames [ "nav-link", true
                               "active", not state.ShowingUserSettings ]
                  Style [ FontSize 18; Cursor "pointer" ]
                  OnClick(fun _ -> dispatch ShowBlogSettings) ] [
                str "Blog Settings"
            ]
        ]
        li [ ClassName "nav-item" ] [
            div [ classNames [ "nav-link", true
                               "active", state.ShowingUserSettings ]
                  Style [ FontSize 18; Cursor "pointer" ]
                  OnClick(fun _ -> dispatch ShowUserSettings) ] [
                str "Change Password"
            ]
        ]
    ]

let settings state blogInfo dispatch =
    if not state.ShowingUserSettings then
        div [ Style [ Padding 10 ] ] [
            blogSettingsEditor blogInfo dispatch
            button [ ClassName "btn btn-success"
                     OnClick(fun _ -> dispatch SaveChanges) ] [
                str "Save Changes"
            ]
        ]
    else
        div [ Style [ Padding 10 ] ] [
            userSettings state dispatch
            button [ ClassName "btn btn-success"
                     OnClick(fun _ -> dispatch SubmitNewPassword) ] [
                str "Change Password"
            ]
        ]

let render state dispatch =
    match state.BlogInfo with
    | Remote.Empty -> div [] []
    | Loading -> Common.spinner
    | LoadError error -> Common.errorMsg error
    | Body blogInfo ->
        div [] [
            tabs state dispatch
            br []
            settings state blogInfo dispatch
        ]



let init () =
    { BlogInfo = Remote.Empty
      IsChangingChanges = false
      ShowingUserSettings = false
      CurrentPassword = ""
      NewPassword = ""
      IsUpdatingPassword = false
      ConfirmNewPassword = "" },
    Cmd.none

let update authToken msg state =
    match msg with
    | LoadBlogInfo ->
        let nextState = { state with BlogInfo = Loading }

        let loadBlogInfoCmd =
            Cmd.fromAsync
                { Value = Server.api.getBlogInfo ()
                  Error = fun ex -> LoadBlogInfoError "Network error while loading blog information"
                  Success = BlogInfoLoaded }

        nextState, loadBlogInfoCmd
    | BlogInfoLoaded (Ok blogInfo) ->
        let nextState = { state with BlogInfo = Body blogInfo }
        nextState, Cmd.none
    | BlogInfoLoaded (Error errorMsg) ->
        let nextState =
            { state with
                  BlogInfo = LoadError errorMsg }

        nextState, Toastr.error (Toastr.message errorMsg)
    | LoadBlogInfoError errorMsg ->
        let nextState =
            { state with
                  BlogInfo = LoadError errorMsg }

        nextState, Toastr.error (Toastr.message errorMsg)
    | ShowBlogSettings ->
        let nextState =
            { state with
                  ShowingUserSettings = false }

        nextState, Cmd.none
    | ShowUserSettings ->
        let nextState =
            { state with
                  ShowingUserSettings = true }

        nextState, Cmd.none
    | otherMsg ->
        match state.BlogInfo with
        | Body blogInfo ->
            match otherMsg with
            | SetTitle title ->
                let nextBlogInfo = { blogInfo with BlogTitle = title }

                let nextState =
                    { state with
                          BlogInfo = Body nextBlogInfo }

                nextState, Cmd.none
            | SetName name ->
                let nextBlogInfo = { blogInfo with Name = name }

                let nextState =
                    { state with
                          BlogInfo = Body nextBlogInfo }

                nextState, Cmd.none
            | SetBio bio ->
                let nextBlogInfo = { blogInfo with Bio = bio }

                let nextState =
                    { state with
                          BlogInfo = Body nextBlogInfo }

                nextState, Cmd.none
            | SetAbout about ->
                let nextBlogInfo = { blogInfo with About = about }

                let nextState =
                    { state with
                          BlogInfo = Body nextBlogInfo }

                nextState, Cmd.none
            | SetProfileImgUrl url ->
                let nextBlogInfo = { blogInfo with ProfileImageUrl = url }

                let nextState =
                    { state with
                          BlogInfo = Body nextBlogInfo }

                nextState, Cmd.none
            | SaveChanges ->
                let nextState = { state with IsChangingChanges = true }

                let request = { Token = authToken; Body = blogInfo }

                let updateBlogInfoCmd =
                    Cmd.fromAsync
                        { Value = Server.api.updateBlogInfo request
                          Error = fun ex -> SaveChangesError "Network error occurred while update the blog info"
                          Success =
                              function
                              | Error authError -> SaveChangesError "User was unauthorized"
                              | Ok (Ok (SuccessMsg msg)) -> ChangesSaved msg
                              | Ok (Error (ErrorMsg msg)) -> SaveChangesError msg }

                nextState, updateBlogInfoCmd
            | SaveChangesError errorMsg -> state, Toastr.error (Toastr.message errorMsg)
            | ChangesSaved msg -> state, Toastr.success (Toastr.message msg)
            | SetCurrentPassword pwd -> { state with CurrentPassword = pwd }, Cmd.none
            | SetNewPassword pwd -> { state with NewPassword = pwd }, Cmd.none
            | SetConfirmNewPassword pwd -> { state with ConfirmNewPassword = pwd }, Cmd.none
            | SubmitNewPassword when String.IsNullOrWhiteSpace state.CurrentPassword ->
                state, Toastr.error (Toastr.message "Current password cannot be empty")
            | SubmitNewPassword when String.IsNullOrWhiteSpace state.NewPassword ->
                state, Toastr.error (Toastr.message "New password cannot be empty")
            | SubmitNewPassword when
                String.IsNullOrWhiteSpace state.ConfirmNewPassword
                || state.ConfirmNewPassword <> state.NewPassword
                ->
                state, Toastr.error (Toastr.message "New password confirmation is not correct")
            | SubmitNewPassword when state.IsUpdatingPassword ->
                state, Toastr.error (Toastr.message "Updating the password is still on-going...")
            | SubmitNewPassword ->
                let updatePwdInfo: SecureRequest<UpdatePasswordInfo> =
                    { Token = authToken
                      Body =
                          { CurrentPassword = state.CurrentPassword
                            NewPassword = state.NewPassword } }

                let updatePwdCmd =
                    Cmd.fromAsync
                        { Value = Server.api.updatePassword updatePwdInfo
                          Error = fun ex -> UpdatePasswordError "Network error occured while updating your password"
                          Success =
                              function
                              | Error authError -> UpdatePasswordError "User was unauthorized"
                              | Ok successMsg -> UpdatePasswordSuccess }

                let nextState = { state with IsUpdatingPassword = true }
                nextState, updatePwdCmd
            | UpdatePasswordError errorMsg ->
                { state with
                      IsUpdatingPassword = false },
                Toastr.error (Toastr.message errorMsg)
            | UpdatePasswordSuccess ->
                // reset input fields
                let nextState =
                    { state with
                          IsUpdatingPassword = false
                          NewPassword = ""
                          CurrentPassword = ""
                          ConfirmNewPassword = "" }

                nextState, Toastr.success (Toastr.message "Password updated")
            | _ -> state, Cmd.none
        | _ -> state, Cmd.none
