module App.Types

open Shared.ViewModels

type BackofficePage =
  | Home
  | NewArticle
  | Settings
  | Published 
  | Drafts 
  | Subscribers

type AdminPage = 
  | Login 
  | Backoffice of BackofficePage

type AppPage =
  | Posts
  | About
  | Admin of AdminPage

type AppMsg =
  | LoadBlogInfo
  | BlogInfoLoaded of BlogInfo
  | BlogInfoLoadFailed 
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  | UrlUpdated of AppPage
  | SetCurrentPage of AppPage

type Model = {
    LoadingBlogInfo: bool
    CurrentPage: AppPage
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
    BlogInfo: BlogInfo option
}
