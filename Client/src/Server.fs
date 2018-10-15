module Server

open Fable.Remoting.Client
open Shared

/// The API proxy provides direct and easy access to the server's function
let api = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder routes 
    |> Remoting.buildProxy<IBlogApi> 