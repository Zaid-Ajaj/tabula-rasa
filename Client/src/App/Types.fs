module App.Types

open Shared.ViewModels

type Page =
  | About
  | Posts of Posts.Types.Page
  | Admin of Admin.Types.Page

type AppMsg =
  | LoadBlogInfo
  | BlogInfoLoaded of BlogInfo
  | BlogInfoLoadFailed 
  | UrlUpdated of Page
  | NavigateTo of Option<Page>
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg

type AppState = {
    LoadingBlogInfo: bool
    BlogInfo: BlogInfo option
    CurrentPage: Option<Page>
    Posts: Posts.Types.State
    Admin: Admin.Types.State
}
