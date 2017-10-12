module App.Types

type Page =
  | Posts
  | Featured
  | Archive
  | Contact
  | Home

type AppMsg =
  | PostsMsg of Posts.Types.Msg
  | HomeMsg of Home.Types.Msg
  | ViewPage of Page

type Model = {
    SecurityToken : string option
    CurrentPage: Page
    Posts: Posts.Types.Model
    Home: Home.Types.Model
}
