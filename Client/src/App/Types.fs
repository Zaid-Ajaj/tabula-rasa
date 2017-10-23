module App.Types

open Shared.ViewModels

type Page =
  | Posts
  | About
  | Admin

type AppMsg =
  | LoadBlogInfo
  | BlogInfoLoaded of BlogInfo
  | BlogInfoLoadFailed 
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  | ViewPage of Page

type Model = {
    LoadingBlogInfo: bool
    CurrentPage: Page
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
    BlogInfo: BlogInfo option
}
