module Shared

open System

type AuthToken = SecurityToken of string

type BlogInfo = {
    Name: string
    ProfileImageUrl: string
    About: string
    Bio : string
    BlogTitle: string
}

type LoginInfo = {
    Username: string
    Password: string
}

type Remote<'a> = 
    | Empty
    | Loading
    | LoadError of string
    | Body of 'a

type RequestMsg<'input, 'output, 'error> = 
    | Initiate of 'input
    | Fetched of 'output 
    | FetchError of 'error 

type LoginResult = 
    | Success of token: string
    | UsernameDoesNotExist
    | PasswordIncorrect
    | LoginError of string

type CreateAdminReq = {
    Name: string
    Username: string
    Password: string
    Email: string
    About: string
    BlogTitle: string
    ProfileImageUrl: string
    Bio : string
}

type CreateAdminRespose = 
    | AdminCreatedSuccesfully
    | AdminAlreadyExists
    | UnknownError
    
type BlogPostItem = {
    Id : int
    Title : string
    Slug : string
    Content : string
    Featured : bool
    DateAdded : DateTime
    Tags : string list
}

type SecureRequest<'t> = {
    Token : string
    Body : 't
}

type NewBlogPostReq = {
    Slug : string
    Title : string
    Content : string
    Tags : string list
}

type AuthError = 
    | TokenInvalid
    | UserUnauthorized 

type AddPostError =
    | PostWithSameTitleAlreadyExists
    | PostWithSameSlugAlreadyExists
    | DatabaseErrorWhileAddingPost
    member this.Message =
        match this with
        | PostWithSameSlugAlreadyExists -> "A post with this slug already exists."
        | PostWithSameTitleAlreadyExists -> "A post with this title already exists."
        | DatabaseErrorWhileAddingPost -> "Internal error occured on the server's database while saving the draft."

type AddPostResult =
    | AddedPostId of int 
    | AddPostError of AddPostError

type DeleteDraftResult = 
    | DraftDoesNotExist
    | DraftDeleted
    | DatabaseErrorWhileDeletingDraft

type DeletePostResult = 
    | PostDoesNotExist
    | PostDeleted
    | DatabaseErrorWhileDeletingPost

type PublishDraftResult = 
    | DraftDoesNotExist 
    | DraftPublished 
    | DatabaseErrorWhilePublishingDraft

type MakeDraftResult = 
    | ArticleDoesNotExist
    | ArticleTurnedToDraft
    | DatabaseErrorWhileMakingDraft    

type UpdatePasswordInfo = {
    CurrentPassword : string 
    NewPassword : string 
}

type SuccessMsg = SuccessMsg of string 
type ErrorMsg = ErrorMsg of string

type PostId = PostId of int 

type SecureResponse<'t> = Async<Result<'t, AuthError>> 

let routes typeName methodName = 
 sprintf "/api/%s/%s" typeName methodName
 
type IBlogApi = 
    {  getBlogInfo : unit -> Async<Result<BlogInfo, string>>
       login : LoginInfo -> Async<LoginResult>
       getPosts : unit -> Async<list<BlogPostItem>>
       getPostBySlug : string -> Async<Option<BlogPostItem>>
       getDrafts : AuthToken -> SecureResponse<list<BlogPostItem>>
       publishNewPost : SecureRequest<NewBlogPostReq> -> SecureResponse<AddPostResult> 
       savePostAsDraft : SecureRequest<NewBlogPostReq> -> SecureResponse<AddPostResult>
       deleteDraftById : SecureRequest<int> -> SecureResponse<DeleteDraftResult>
       publishDraft : SecureRequest<int> -> SecureResponse<PublishDraftResult>
       deletePublishedArticleById : SecureRequest<int> -> SecureResponse<DeletePostResult>
       turnArticleToDraft: SecureRequest<int> -> SecureResponse<MakeDraftResult>
       getPostById : SecureRequest<int> -> SecureResponse<Option<BlogPostItem>>
       savePostChanges : SecureRequest<BlogPostItem> -> SecureResponse<Result<bool, string>>
       updateBlogInfo : SecureRequest<BlogInfo> -> SecureResponse<Result<SuccessMsg, ErrorMsg>>
       togglePostFeatured : SecureRequest<int> -> SecureResponse<Result<string, string>>
       updatePassword : SecureRequest<UpdatePasswordInfo> -> SecureResponse<Result<string, string>> 
    }


// Message from the client, telling the server that a new post is added
type RemoteClientMsg = 
    | PostAdded 

// Message from the server, telling the client to reload posts
type RemoteServerMsg = 
    | ReloadPosts 

let socket = "/socket"