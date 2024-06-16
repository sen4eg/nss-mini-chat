







### How to startup
1. Install docker-compose or docker desktop ui and run docker-compose.yml to startup database
2. Prepare database using ```dotnet ef update``` command
3. Start the Program.cs file in root of the project





#### OUATH2 Scenarios:
### Registering a New User
1. Send a registration request to the server.
2. The server may create a new user and return an authentication token and a refresh token.
3. The client should store the token and refresh token in a secure place.
4. The client should use the refresh token approximately every hour to refresh the authentication token.

### Authorizing an Existing User
An existing user can authorize in the following ways:
1. **Using a Token**:
    - If the token is not expired, it can be used to access the service.
2. **Using a Refresh Token**:
    - Use the refresh token to acquire a new authentication token.
3. **Using Username and Password**:
    - Use the username and password to acquire a new authentication token and refresh token.
#### DATABASE:
To work with database out of c# db contexts use dotnet ef command line tool.