module App.Types

open Shared.ViewModels

type AppPage =
  | Posts
  | About
  | Admin of Admin.Types.Page

type AppMsg =
  | LoadBlogInfo
  | BlogInfoLoaded of BlogInfo
  | BlogInfoLoadFailed 
  | UrlUpdated of AppPage
  | SetCurrentPage of Option<AppPage>
  // messages from children
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg

type AppState = {
    // state of root app
    LoadingBlogInfo: bool
    BlogInfo: BlogInfo option
    CurrentPage: Option<AppPage>
    // states of children
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
}
