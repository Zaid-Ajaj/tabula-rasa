module ClientServer

open System
open Shared.ViewModels
open Shared.DomainModels

let routeBuilder typeName methodName = 
 sprintf "/api/%s/%s" typeName methodName
 
type Protocol = 
    {  getBlogInfo : unit -> Async<BlogInfo>
       login : LoginInfo -> Async<LoginResult>
       getPosts : unit -> Async<list<BlogPostItem>>
       getPostBySlug : string -> Async<Option<BlogPostItem>>
       getDrafts : AuthToken -> Async<Result<list<BlogPostItem>, string>>
       publishNewPost : SecureRequest<NewBlogPostReq> -> Async<Result<int, string>> 
       savePostAsDraft : SecureRequest<NewBlogPostReq> -> Async<Result<int, string>> }