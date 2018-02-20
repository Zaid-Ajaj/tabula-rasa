module Posts.Http

open Elmish
open Posts.Types

let server = Server.createProxy()

let loadPosts = 
    Cmd.ofAsync server.getPosts ()
        LoadLatestPostsFinished
        (fun _ -> LoadLatestPostsError "Network error: could not retrieve the blog posts")

let loadSinglePost slug = 
    Cmd.ofAsync server.getPostBySlug slug
        (function | Some post -> LoadSinglePostFinished post 
                  | None -> LoadSinglePostError ("Could not find the requested blog post '" + slug + "'."))
        (fun _ -> LoadSinglePostError "Network error: could not retrieve the requested blog post")