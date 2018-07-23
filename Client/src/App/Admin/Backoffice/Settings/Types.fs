module Admin.Backoffice.Settings.Types 

open Shared

type State = 
    { BlogInfo : Remote<BlogInfo>
      IsChangingChanges : bool
      ShowingUserSettings : bool
      CurrentPassword : string 
      NewPassword : string 
      ConfirmNewPassword : string }

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
    | ChangesSaved of successMsg:string
    | SaveChangesError of string
    | ShowUserSettings 
    | ShowBlogSettings 
    | SubmitNewPassword
    | SetCurrentPassword of string 
    | SetNewPassword of string 
    | SetConfirmNewPassword of string