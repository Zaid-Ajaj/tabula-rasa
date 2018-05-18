module Admin.Backoffice.Settings.Types 

open Shared

type State = 
    { BlogInfo : Remote<BlogInfo>
      IsChangingChanges : bool
      ShowingUserSettings : bool }

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
    | ChangePassword