module Server

open Fable.Remoting.Client
open Shared.ViewModels
open Shared.DomainModels
open ClientServer

let serverProxy : Protocol = Proxy.createWithBuilder<Protocol> routeBuilder
let createProxy() = serverProxy