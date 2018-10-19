module Posts.Http

open Elmish
open Posts.Types

let loadPosts =
    Cmd.ofAsync Server.api.getPosts () LoadLatestPostsFinished 
        (fun _ -> LoadLatestPostsError "Network error: could not retrieve the blog posts")

let loadSinglePost slug =
    Cmd.ofAsync Server.api.getPostBySlug slug (function 
        | Some post -> LoadSinglePostFinished post
        | None -> LoadSinglePostError("Could not find the requested blog post '" + slug + "'.")) 
        (fun _ -> LoadSinglePostError "Network error: could not retrieve the requested blog post")
