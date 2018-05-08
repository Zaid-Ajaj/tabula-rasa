module App.Types

open Shared

type Page =
  | About
  | Posts of Posts.Types.Page
  | Admin of Admin.Types.Page

type AppMsg =
  | LoadBlogInfo
  | BlogInfoLoaded of BlogInfo
  | BlogInfoLoadFailed of error:string
  | UrlUpdated of Page
  | NavigateTo of Option<Page>
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg

type AppState = {
    BlogInfo: Remote<BlogInfo>
    CurrentPage: Option<Page>
    Posts: Posts.Types.State
    Admin: Admin.Types.State
}
