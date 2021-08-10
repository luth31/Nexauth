# Nexauth
Nexauth is an authentication system based on digital signature. 

Currently the project has 4 components:

[[litecrypt]] - Provides AES-CTR to encrypt traffic between the client and the server
[[Nexauth.Server]] - Provides the a TCP Server that the client can use to connect and authenticate with, and the API used by the authenticator to sign authentication requests
[[Nexauth.Client]] - A simple WPF application that acts as the client
[[Nexauth-Authenticator]] - A SwiftUI iOS application that allows the creation of an account and is used to sign authentication requests