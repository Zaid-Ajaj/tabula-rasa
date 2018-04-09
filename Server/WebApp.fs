module WebApp

open ClientServer
open Shared.ViewModels
open Shared.DomainModels
open Fable.Remoting.Suave

open Suave
open Suave.Operators
open Suave.Filters

/// Composition root of the application
let createUsing store = 
    let database = Storage.createDatabaseUsing store
    let readFile file = Storage.readFile file database
    let writeFile filename content = Storage.saveFile filename content database

    // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists Admin.guestAdmin writeFile readFile
    let adminData = Admin.readAdminData readFile
   
    let login info = async { return Admin.login readFile info }
    let getBlogInfo() = async {  return Admin.blogInfoFromAdmin adminData }
    let publishNewPost req = async { return BlogPosts.publishNewPost database req  }
    let savePostAsDraft req = async { return BlogPosts.saveAsDraft database req  }  
    let getPosts() = async { return BlogPosts.getAll database }    
    let getPostBySlug slug = async { return BlogPosts.getPostBySlug database slug }
    let getDrafts (AuthToken(authToken)) = 
        async {
            match Security.validateJwt authToken with 
            | Some user -> 
                let drafts = BlogPosts.getAllDrafts database 
                return Ok drafts 
            | None ->
                return Error "Authorization token is not valid or has expired"
        }

    let serverProtocol =
        {  getBlogInfo = getBlogInfo 
           login = login
           publishNewPost = publishNewPost
           getPosts = getPosts
           getPostBySlug =  getPostBySlug
           savePostAsDraft = savePostAsDraft
           getDrafts = getDrafts }
    
    let clientServerProtocol = 
        remoting serverProtocol {
            use_route_builder routeBuilder
        }

    clientServerProtocol