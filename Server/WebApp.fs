module WebApp

open System
open Shared.ViewModels
open Shared.DomainModels
open LiteDB.FSharp
open ClientServer
open Security
open Fable.Remoting.Suave


open Shared.ViewModels
open Suave
open Suave.Successful
open Suave.Operators
open Suave.Writers
open Suave.Filters

module Async = 
    let lift (x: 'a) =
        async { return x }


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
    let publishNewPost req = async { return BlogPosts.publishNewPost req database }
    let getPosts() = async { return BlogPosts.getAll database }    
    
    let serverProtocol =
        {  getBlogInfo = getBlogInfo 
           login = login
           publishNewPost = publishNewPost
           getPosts = getPosts }
    
    let clientServerProtocol = FableSuaveAdapter.webPartWithBuilderFor serverProtocol routeBuilder
   
    let webApp = 
        choose 
          [ GET >=> Files.browseHome
            clientServerProtocol ]
    webApp