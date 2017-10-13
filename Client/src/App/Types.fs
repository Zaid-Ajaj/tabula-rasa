module App.Types

type Page =
  | Posts
  | Featured
  | Archive
  | Contact
  | Home
  | Admin

type AppMsg =
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  | ViewPage of Page

type Model = {
    AdminSecurityToken : string option
    CurrentPage: Page
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
}
