module App.Types

type Page =
  | Posts
  | About
  | Admin

type AppMsg =
  | PostsMsg of Posts.Types.Msg
  | AdminMsg of Admin.Types.Msg
  | ViewPage of Page

type Model = {
    CurrentPage: Page
    Posts: Posts.Types.Model
    Admin: Admin.Types.State
}
