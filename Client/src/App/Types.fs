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
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  | UrlUpdated of AppPage
  | SetCurrentPage of Option<AppPage>

type Model = {
    LoadingBlogInfo: bool
    CurrentPage: Option<AppPage>
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
    BlogInfo: BlogInfo option
}
