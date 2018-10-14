/// Proviedes functions for encoding and decoding Json web tokens
module Security

open System
open System.IO
open System.Text
open Newtonsoft.Json
open System.Security.Cryptography

open Shared

type UserInfo = {
    Username: string
    Claims: string[]
}

//  Learn about JWT https://jwt.io/introduction/
//  This module uses the JOSE-JWT library https://github.com/dvsekhvalnov/jose-jwt

let createRandomKey() = 
    let generator = System.Security.Cryptography.RandomNumberGenerator.Create()
    let randomKey = Array.init 32 byte
    generator.GetBytes(randomKey)
    randomKey

/// A pass phrase you create only once and save to a file on the server
/// The next time the server runs, the pass phrase is read and used
let private passPhrase =
    let securityTokenFile = FileInfo(Environment.securityTokenFile)
    if not securityTokenFile.Exists then
        let passPhrase = createRandomKey()
        File.WriteAllBytes(securityTokenFile.FullName, passPhrase)
    File.ReadAllBytes(securityTokenFile.FullName)

let private encodeString (payload:string) =
    Jose.JWT.Encode(payload, passPhrase, Jose.JweAlgorithm.A256KW, Jose.JweEncryption.A256CBC_HS512)

let private decodeString (jwt: string) =
    Jose.JWT.Decode(jwt, passPhrase, Jose.JweAlgorithm.A256KW, Jose.JweEncryption.A256CBC_HS512)

/// Encodes an object as a JSON web token.
let encodeJwt token =
    JsonConvert.SerializeObject token
    |> encodeString

/// Decodes a JSON web token to an object.
let decodeJwt<'a> (jwt: string) : 'a =
    decodeString jwt
    |> JsonConvert.DeserializeObject<'a>

/// Returns true if the JSON Web Token is successfully decoded and the signature is verified.
let validateJwt (jwt:string) : UserInfo option =
    try
        let token = decodeJwt jwt
        Some token
    with
    | _ -> None

let utf8Bytes (input: string) = Encoding.UTF8.GetBytes(input)
let base64 (input: byte[]) = Convert.ToBase64String(input)
let sha256 = SHA256.Create()
let sha256Hash (input: byte[]) : byte[] = sha256.ComputeHash(input)

let verifyPassword password saltBase64 hashBase64 = 
    let salt = Convert.FromBase64String(saltBase64)
    Array.concat [ salt; utf8Bytes password ]
    |> sha256Hash
    |> base64
    |> (=) hashBase64 

let authorize (claims: string list) (f: 'u -> UserInfo -> 't) : SecureRequest<'u> -> SecureResponse<'t> = 
    fun request -> 
        match validateJwt request.Token with 
        | None -> async { return Result.Error AuthError.TokenInvalid }
        | Some user ->
            let userHasAllClaims = 
                claims 
                |> List.forall (fun claim -> Array.contains claim user.Claims) 
            if not userHasAllClaims 
            then async { return Result.Error AuthError.UserUnauthorized }
            else async {
                let output = f request.Body user
                return Result.Ok output
            }

let authorizeAsync (claims: string list) (f: 'u -> UserInfo -> Async<'t>) : SecureRequest<'u> -> SecureResponse<'t> = 
    fun request -> 
        match validateJwt request.Token with 
        | None -> async { return Result.Error AuthError.TokenInvalid }
        | Some user ->
            let userHasAllClaims = 
                claims 
                |> List.forall (fun claim -> Array.contains claim user.Claims) 
            if not userHasAllClaims 
            then async { return Result.Error AuthError.UserUnauthorized }
            else async {
                let! output = f request.Body user
                return Result.Ok output
            }

let authorizeAny (f: UserInfo -> 't) : AuthToken -> SecureResponse<'t> = 
    fun (SecurityToken(token)) -> 
        match validateJwt token with 
        | None -> async { return Result.Error AuthError.TokenInvalid }
        | Some user ->
            async {
                let output = f user
                return Result.Ok output
            }  

let authorizeAnyAsync (f: UserInfo -> Async<'t>) : AuthToken -> SecureResponse<'t> = 
    fun (SecurityToken(token)) -> 
        match validateJwt token with 
        | None -> async { return Result.Error AuthError.TokenInvalid }
        | Some user ->
            async {
                let! output = f user
                return Result.Ok output
            }    

let authorizeAdmin f = authorize [ "admin" ] f
  