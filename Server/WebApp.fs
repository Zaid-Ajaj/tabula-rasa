module WebApp

open Shared
open Fable.Remoting.Suave
open BlogPosts

let liftAsync x = async { return x }

/// Composition root of the application
let createUsing store = 
    let database = Storage.createDatabaseUsing store
    // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists database Admin.guestAdmin 
    let adminData = Admin.readAdminData database
   
    let getBlogInfo() = async { return Admin.blogInfoFromAdmin adminData }
    let getPosts() = async { return BlogPosts.getPublishedArticles database }    

    let serverProtocol : IBlogApi =
        {   getBlogInfo = getBlogInfo
            getPosts = getPosts 
            login = Admin.login database >> liftAsync
            publishNewPost = BlogPosts.publishNewPost database >> liftAsync
            getPostBySlug =  BlogPosts.getPostBySlug database >> liftAsync 
            savePostAsDraft = BlogPosts.saveAsDraft database >> liftAsync
            getDrafts = BlogPosts.getAllDrafts database >> liftAsync
            deleteDraftById = BlogPosts.deleteDraft database >> liftAsync 
            publishDraft = BlogPosts.publishDraft database >> liftAsync
            deletePublishedArticleById = BlogPosts.deletePublishedArticle database >> liftAsync
            turnArticleToDraft = BlogPosts.turnArticleToDraft database >> liftAsync
            getPostById = BlogPosts.getPostById database >> liftAsync
            savePostChanges = BlogPosts.savePostChanges database >> liftAsync
            updateBlogInfo = Admin.updateBlogInfo database >> liftAsync }
    
    remoting serverProtocol { use_route_builder routes }
