module Admin.Backoffice.Settings.State 

open Common
open System
open Shared
open Elmish
open Admin.Backoffice.Settings.Types 

let init() = { BlogInfo = Empty;
               IsChangingChanges = false;  
               ShowingUserSettings = false;
               CurrentPassword = "";
               NewPassword = "";
               IsUpdatingPassword = false
               ConfirmNewPassword = "" }, Cmd.none


let update authToken msg state = 
    match msg with 
    | LoadBlogInfo ->
        let nextState = { state with BlogInfo = Loading }
        let loadBlogInfoCmd = 
            Cmd.fromAsync {
                Value = Server.api.getBlogInfo()
                Error = fun ex -> LoadBlogInfoError "Network error while loading blog information"
                Success = BlogInfoLoaded
            } 
        nextState, loadBlogInfoCmd
    
    | BlogInfoLoaded (Ok blogInfo) ->
        let nextState = { state with BlogInfo = Body blogInfo } 
        nextState, Cmd.none 

    | BlogInfoLoaded (Error errorMsg) ->
        let nextState = { state with BlogInfo = LoadError errorMsg }
        nextState, Toastr.error (Toastr.message errorMsg)

    | LoadBlogInfoError errorMsg ->
        let nextState = { state with BlogInfo = LoadError errorMsg }
        nextState, Toastr.error (Toastr.message errorMsg)

    | ShowBlogSettings -> 
        let nextState = { state with ShowingUserSettings = false }
        nextState, Cmd.none
         
    | ShowUserSettings ->
        let nextState = { state with ShowingUserSettings = true }
        nextState, Cmd.none

    | otherMsg -> 
        match state.BlogInfo with 
        | Body blogInfo -> 
            match otherMsg with 
            | SetTitle title -> 
                let nextBlogInfo = { blogInfo with BlogTitle = title }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 
            
            | SetName name ->
                let nextBlogInfo = { blogInfo with Name = name }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 

            | SetBio bio -> 
                let nextBlogInfo = { blogInfo with Bio = bio }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none            
            
            | SetAbout about -> 
                let nextBlogInfo = { blogInfo with About = about }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none 

            | SetProfileImgUrl url ->
                let nextBlogInfo = { blogInfo with ProfileImageUrl = url }
                let nextState = { state with BlogInfo = Body nextBlogInfo }
                nextState, Cmd.none

            | SaveChanges ->
                let nextState = { state with IsChangingChanges = true  }
                let request = { Token = authToken; Body = blogInfo }
                let updateBlogInfoCmd = 
                    Cmd.fromAsync {
                        Value = Server.api.updateBlogInfo request
                        Error = fun ex -> SaveChangesError "Network error occurred while update the blog info"
                        Success = function 
                            | Ok (SuccessMsg msg) -> ChangesSaved msg
                            | Error (ErrorMsg msg) -> SaveChangesError msg
                    }

                nextState, updateBlogInfoCmd 
            
            | SaveChangesError errorMsg ->
                state, Toastr.error (Toastr.message errorMsg)

            | ChangesSaved msg ->
                state, Toastr.success (Toastr.message msg)

            | SetCurrentPassword pwd ->
                { state with CurrentPassword = pwd }, Cmd.none

            | SetNewPassword pwd ->
                { state with NewPassword = pwd }, Cmd.none
             
            | SetConfirmNewPassword pwd ->
                { state with ConfirmNewPassword = pwd }, Cmd.none
            
            | SubmitNewPassword when String.IsNullOrWhiteSpace state.CurrentPassword ->
                state, Toastr.error (Toastr.message "Current password cannot be empty") 

            | SubmitNewPassword when String.IsNullOrWhiteSpace state.NewPassword ->
                state, Toastr.error (Toastr.message "New password cannot be empty")
             
            | SubmitNewPassword when String.IsNullOrWhiteSpace state.ConfirmNewPassword || state.ConfirmNewPassword <> state.NewPassword ->
                state, Toastr.error (Toastr.message "New password confirmation is not correct")
            
            | SubmitNewPassword when state.IsUpdatingPassword -> 
                state, Toastr.error (Toastr.message "Updating the password is still on-going...") 

            | SubmitNewPassword ->
                let updatePwdInfo : SecureRequest<UpdatePasswordInfo> = {
                    Token = authToken
                    Body = { CurrentPassword = state.CurrentPassword; NewPassword = state.NewPassword }
                 } 
                let updatePwdCmd = 
                    Cmd.fromAsync {
                        Value = Server.api.updatePassword updatePwdInfo
                        Error = fun ex -> UpdatePasswordError "Network error occured while updating your password"
                        Success = function 
                            | Ok successMsg -> UpdatePasswordSuccess 
                            | Error errorMsg -> UpdatePasswordError errorMsg 
                    }

                let nextState = { state with IsUpdatingPassword = true }
                nextState, updatePwdCmd
            | UpdatePasswordError errorMsg -> 
                { state with IsUpdatingPassword = false }, Toastr.error (Toastr.message errorMsg) 

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