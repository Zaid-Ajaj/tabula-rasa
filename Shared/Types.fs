module Shared

open System

type AuthToken = AuthToken of string

type BlogInfo = {
    Name: string
    ProfileImageUrl: string
    About: string
    Bio : string
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
    | UserUnauthorized 

type AddPostResult =
    | AddedPostId of int 
    | PostWithSameTitleAlreadyExists
    | PostWithSameSlugAlreadyExists
    | DatabaseErrorWhileAddingPost
    | AuthError of AuthError

type DeleteDraftResult = 
    | DraftDoesNotExist
    | DraftDeleted
    | AuthError of AuthError
    | DatabaseErrorWhileDeletingDraft

type PublishDraftResult = 
    | DraftDoesNotExist 
    | DraftPublished 
    | AuthError of AuthError
    | DatabaseErrorWhilePublishingDraft

let routes typeName methodName = 
 sprintf "/api/%s/%s" typeName methodName
 
type Protocol = 
    {  getBlogInfo : unit -> Async<BlogInfo>
       login : LoginInfo -> Async<LoginResult>
       getPosts : unit -> Async<list<BlogPostItem>>
       getPostBySlug : string -> Async<Option<BlogPostItem>>
       getDrafts : AuthToken -> Async<Result<list<BlogPostItem>, string>>
       publishNewPost : SecureRequest<NewBlogPostReq> -> Async<AddPostResult> 
       savePostAsDraft : SecureRequest<NewBlogPostReq> -> Async<AddPostResult>
       deleteDraftById : SecureRequest<int> -> Async<DeleteDraftResult>
       publishDraft : SecureRequest<int> -> Async<PublishDraftResult> }