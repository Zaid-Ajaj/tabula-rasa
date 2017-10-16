module ClientServer

open Shared.ViewModels
open Shared.DomainModels

let routeBuilder typeName methodName = 
 sprintf "/api/%s/%s" typeName methodName
 
type Protocol = 
    { login: LoginInfo -> Async<LoginResult> }