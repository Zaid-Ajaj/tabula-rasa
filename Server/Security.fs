/// Proviedes functions for encoding and decoding Json web tokens
module Security

open System
open System.IO
open System.Text
open Newtonsoft.Json
open System.Security.Cryptography

open Shared.DomainModels
open Shared.ViewModels

type AdminInfo = {
    Name: string
    Username: string
    PasswordHash: string
    PasswordSalt: string
    Email: string
    About: string
    ProfileImageUrl: string
}

//  Learn about JWT https://jwt.io/introduction/
//  This module uses the JOSE-JWT library https://github.com/dvsekhvalnov/jose-jwt

let private createRandomKey() = 
    let crypto = System.Security.Cryptography.RandomNumberGenerator.Create()
    let randomNumber = Array.init 32 byte
    crypto.GetBytes(randomNumber)
    randomNumber

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

let private utf8Bytes (input: string) = Encoding.UTF8.GetBytes(input)
let private base64 (input: byte[]) = Convert.ToBase64String(input)
let private sha256 = SHA256.Create()
let private sha256Hash (input: byte[]) : byte[] = sha256.ComputeHash(input)

/// Creates an initial guest user as the admin if admin data does not exist
let createAdmin (info: CreateAdminReq)  = 
    let adminInfoExists = File.Exists(Environment.adminFile)
    if not adminInfoExists then AdminAlreadyExists
    else 
        let salt = createRandomKey()
        let password = utf8Bytes info.Password
        let saltyPassword = Array.concat [ salt; password ]
        let passwordHash = sha256Hash saltyPassword
        let admin = {
            Name = info.Name
            Username = info.Username
            PasswordSalt = base64 salt
            PasswordHash = base64 passwordHash
            Email = info.Email
            About = info.About
            ProfileImageUrl = ""
        }
        Json.serialize admin
        |> fun json -> File.WriteAllText(Environment.adminFile, json)
        AdminCreatedSuccesfully


let verifyPassword password saltBase64 hash = 
    let salt = Convert.FromBase64String(saltBase64)
    Array.concat [ salt; utf8Bytes password ]
    |> sha256Hash
    |> base64
    |> (=) hash 