module App.Http

open App.Types
open Elmish
let server = Server.createProxy()

let loadBlogInfo = 
  Cmd.ofAsync server.getBlogInfo ()
              BlogInfoLoaded
              (fun _ -> BlogInfoLoadFailed "Network error: could not retrieve initial blog information from server")
