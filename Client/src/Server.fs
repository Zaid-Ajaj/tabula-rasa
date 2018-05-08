module Server

open Fable.Remoting.Client
open Shared

let api : Protocol = Proxy.createWithBuilder<Protocol> routes
let createProxy() = api