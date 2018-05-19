module App.Types

open Shared

type Page =
  // Pages of App
  | About
  // Sub pages of App
  | Posts of Posts.Types.Page
  | Admin of Admin.Types.Page

type AppMsg =
  // The messages coming from children
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  // the app messages 
  | LoadBlogInfo
  | BlogInfoLoaded of Result<BlogInfo, string> 
  | BlogInfoLoadFailed of error:string
  | UrlUpdated of Page
  | NavigateTo of Page
  | DoNothing 

type AppState = {
    // the state of the children  
    Posts: Posts.Types.State
    Admin: Admin.Types.State
    // App's own state
    BlogInfo: Remote<BlogInfo>
    CurrentPage: Option<Page>
}
