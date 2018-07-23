module Admin.Backoffice.Settings.View

open Shared
open Admin.Backoffice.Settings.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props 

let classNames classes =
    classes 
    |> List.filter snd 
    |> List.map fst
    |> String.concat " "
    |> ClassName

let blogSettingsEditor (blogInfo: BlogInfo) dispatch = 
    form [  ]   
         [  div [ ClassName "form-group" ]  
                [ 
                    label [ HtmlFor "txtBlogTitle" ] [ str "Blog Title" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogTitle"
                            Key "txtBlogTitle"
                            Type "text"
                            DefaultValue blogInfo.BlogTitle
                            Common.onTextChanged (SetTitle >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtBlogBio" ] [ str "Biography" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogBio"
                            Key "txtBlogBio"
                            Type "text"
                            DefaultValue blogInfo.Bio
                            Common.onTextChanged (SetBio >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtBlogName" ] [ str "Name" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogName"
                            Key "txtBlogName"
                            Type "text"
                            DefaultValue blogInfo.Name
                            Common.onTextChanged (SetName >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtProfileImgUrl" ] [ str "Profile Image Url" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtProfileImgUrl"
                            Key "txtProfileImgUrl"
                            Type "text"
                            DefaultValue blogInfo.ProfileImageUrl
                            Common.onTextChanged (SetProfileImgUrl >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtAbout" ] [ str "About" ]
                    textarea [ ClassName "form-control"
                               DefaultValue blogInfo.About 
                               Id "txtAbout"
                               Key "txtAbout"
                               Rows 8.0
                               Common.onTextChanged (SetAbout >> dispatch) ] 
                             [ ]
                ] ] 

let userSettings state dispatch = 
    form [  ]   
         [  div [ ClassName "form-group" ]  
                [ 
                    label [ HtmlFor "txtCurrentPassword" ] [ str "Current Password" ]
                    input [ ClassName "form-control form-control-lg"
                            Key "txtCurrentPassword"
                            Id "txtCurrentPassword"
                            DefaultValue state.CurrentPassword 
                            Common.onTextChanged (SetCurrentPassword >> dispatch)
                            Type "password" ] 
                ]
            div [ ClassName "form-group" ]  
                [ 
                    label [ HtmlFor "txtNewPassword" ] [ str "New Password" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtNewPassword"
                            Key "txtNewPassword"
                            DefaultValue state.NewPassword 
                            Common.onTextChanged (SetNewPassword >> dispatch)
                            Type "password" ] 
                ]
            div [ ClassName "form-group" ]  
                [ 
                    label [ HtmlFor "txtNewPasswordConfirm" ] [ str "Confirm New Password" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtNewPasswordConfirm"
                            Key "txtNewPasswordConfirm"
                            DefaultValue state.ConfirmNewPassword 
                            Common.onTextChanged (SetConfirmNewPassword >> dispatch)
                            Type "password" ] 
                ] ]
                 
let tabs state dispatch = 
    ul [ ClassName "nav nav-tabs" ] 
       [ li [ ClassName "nav-item" ] 
            [ div [ classNames [ "nav-link", true; "active", not state.ShowingUserSettings ]
                    Style [ FontSize 18; Cursor "pointer" ]
                    OnClick (fun _ -> dispatch ShowBlogSettings) ] 
                  [ str "Blog Settings" ] ] 
         li [ ClassName "nav-item" ] 
            [ div [ classNames [ "nav-link", true; "active", state.ShowingUserSettings ]
                    Style [ FontSize 18; Cursor "pointer" ]
                    OnClick (fun _ -> dispatch ShowUserSettings) ]  
                  [ str "Change Password" ] ] ]

let settings state blogInfo dispatch = 
    if not state.ShowingUserSettings 
    then 
        div [ Style [ Padding 10 ] ] 
            [ blogSettingsEditor blogInfo dispatch 
              button [ ClassName "btn btn-success"
                       OnClick (fun _ -> dispatch SaveChanges) ] 
                     [ str "Save Changes" ] ]
    else div [ Style [ Padding 10 ] ] 
             [ userSettings state dispatch
               button [ ClassName "btn btn-success"
                        OnClick (fun _ -> dispatch SubmitNewPassword) ] 
                      [ str "Change Password" ] ] 

let render state dispatch = 
    match state.BlogInfo with 
    | Remote.Empty -> div [ ] [ ] 
    | Loading -> Common.spinner
    | LoadError error -> Common.errorMsg error 
    | Body blogInfo -> 
        div [ ]
            [ tabs state dispatch
              br [ ]    
              settings state blogInfo dispatch ]
