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


let blogSettingsEditor (blogInfo: BlogInfo) dispatch = 
    form [  ]   
         [  div [ ClassName "form-group" ]  
                [ 
                    label [ HtmlFor "txtBlogTitle" ] [ str "Blog Title" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogTitle"
                            Type "text"
                            DefaultValue blogInfo.BlogTitle
                            Common.onTextChanged (SetTitle >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtBlogBio" ] [ str "Biography" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogBio"
                            Type "text"
                            DefaultValue blogInfo.Bio
                            Common.onTextChanged (SetBio >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtBlogName" ] [ str "Name" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtBlogName"
                            Type "text"
                            DefaultValue blogInfo.Name
                            Common.onTextChanged (SetName >> dispatch) ] 
                ]
            div [ ClassName "form-group" ]
                [ 
                    label [ HtmlFor "txtProfileImgUrl" ] [ str "Profile Image Url" ]
                    input [ ClassName "form-control form-control-lg"
                            Id "txtProfileImgUrl"
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
                               Rows 8.0
                               Common.onTextChanged (SetAbout >> dispatch) ] 
                             [ ]
                ] ] 

let render state dispatch = 
    match state.BlogInfo with 
    | Remote.Empty -> div [ ] [ ] 
    | Loading -> Common.spinner
    | LoadError error -> Common.errorMsg error 
    | Body blogInfo -> 
        div [ ]
            [ h1 [ ] [ str "Settings" ]
              hr [ ]
              blogSettingsEditor blogInfo dispatch 
              button [ ClassName "btn btn-success"
                       OnClick (fun _ -> dispatch SaveChanges) ] 
                     [ str "Save Changes" ]  ] 
