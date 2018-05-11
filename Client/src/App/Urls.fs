module Urls

let hashPrefix = sprintf "#%s"
let about = "about"
let admin = "admin"
let posts = "posts"
let newPost = "new-post" 
let drafts = "drafts"
let publishedArticles = "published-articles"
let login = "login"
let settings = "settings"
let subscribers = "subscribers"
let editArticle = "edit-article"
let combine xs = List.fold (sprintf "%s/%s") "" xs
let navigate xs = Elmish.Browser.Navigation.Navigation.newUrl (hashPrefix (combine xs))