module Server

open Fable.Remoting.Client
open Shared

let api = Proxy.createWithBuilder<IBlogApi> routes