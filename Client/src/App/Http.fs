module App.Http

open App.Types
open Elmish
open Shared

let loadBlogInfo =
    Cmd.ofAsync Server.api.getBlogInfo () BlogInfoLoaded 
        (fun _ -> BlogInfoLoadFailed "Network error: could not retrieve initial blog information from server")
