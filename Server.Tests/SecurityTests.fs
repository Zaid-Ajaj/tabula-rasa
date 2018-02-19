module SecurityTests

open Expecto

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"


let securityTests = 
    testList "Security Tests" [
        testCase "verifyPassword works" <| fun _ ->
            let guestAdmin = Admin.guestAdmin
            let salt = guestAdmin.PasswordSalt
            let hash = guestAdmin.PasswordHash
            match Security.verifyPassword "guest" salt hash with
            | true -> pass()
            | false -> fail()
            match Security.verifyPassword "wrong-password" salt hash with
            | false -> pass()
            | true -> fail()
    ]