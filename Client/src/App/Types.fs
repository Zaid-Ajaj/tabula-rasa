module App.Types

type Page =
  | Posts
  | About
  | Admin

type BlogInfo = {
  Name: string
  About: string
  ProfileImageUrl: string
}

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
