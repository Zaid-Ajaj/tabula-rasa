module WebApp

open Shared
open Fable.Remoting.Suave
open BlogPosts

/// Composition root of the application
let createUsing store = 
    let database = Storage.createDatabaseUsing store
    let readFile file = Storage.readFile file database
    let writeFile filename content = Storage.saveFile filename content database

    // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists database Admin.guestAdmin 
    let adminData = Admin.readAdminData database
   
    let login info = async { return Admin.login database info }
    let getBlogInfo() = async {  return Admin.blogInfoFromAdmin adminData }
    let publishNewPost req = async { return BlogPosts.publishNewPost database req  }
    let savePostAsDraft req = async { return BlogPosts.saveAsDraft database req  }  
    let getPosts() = async { return BlogPosts.getPublishedArticles database }    
    let getPostBySlug slug = async { return BlogPosts.getPostBySlug database slug }
    
    let deleteDraft req = 
        async { 
            do! Async.Sleep 2000 
            return BlogPosts.deleteDraft database req 
        }

    let deletePublishedArticle req = async { return BlogPosts.deletePublishedArticle database req }

    let publishDraft req = 
      async {
          do! Async.Sleep 2000
          return BlogPosts.publishDraft database req 
        }

       
    let getDrafts (AuthToken(authToken)) = 
        async {
            match Security.validateJwt authToken with 
            | Some user -> 
                let drafts = BlogPosts.getAllDrafts database 
                return Ok drafts 
            | None ->
                return Error "Authorization token is not valid or has expired"
        }

    let serverProtocol : Protocol =
        {  getBlogInfo = getBlogInfo 
           login = login
           publishNewPost = publishNewPost
           getPosts = getPosts
           getPostBySlug =  getPostBySlug
           savePostAsDraft = savePostAsDraft
           getDrafts = getDrafts
           deleteDraftById = deleteDraft 
           publishDraft = publishDraft
           deletePublishedArticleById = deletePublishedArticle
           turnArticleToDraft = fun req -> async { return BlogPosts.turnArticleToDraft database req }  }
    
    let clientServerProtocol = 
        remoting serverProtocol {
            use_route_builder routes
        }

    clientServerProtocol