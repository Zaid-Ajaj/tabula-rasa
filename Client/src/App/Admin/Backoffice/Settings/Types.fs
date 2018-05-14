module Admin.Backoffice.Settings.Types 

open Shared

type State = 
    { BlogInfo : Remote<BlogInfo>
      ShowingUserSettings : bool }

type Msg = 
    | LoadBlogInfo 
    | BlogInfoLoaded of BlogInfo 
    | LoadBlogInfoError of string  
    | SetTitle of string 
    | SetName of string 
    | SetBio of string 
    | SetAbout of string
    | SetProfileImgUrl of string
    | SaveChanges 
    | ChangesSaved 
    | SaveChangedError of string
    | ShowUserSettings 
    | ShowBlogSettings 