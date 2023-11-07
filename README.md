# Song Repository

<p align="center">
  <a href="http://51.222.111.162:1234/swagger/index.html" target="_blank">Click here to view the site.<a/>
  <img alt="API Dashboard" src="/ReadMePictures/Dashboard.png">
</p>

This REST API has been developed to simulate a database of songs hosted over the cloud. It offers full Create, Read, Update, and Delete functionalities on the songs. It has also been implemented using proper data management and security. 

#### Functionalities of the API:
* Full authentication with the use of JWT bearer tokens and password hashing.
* Daily background processes to clean up the database values overnight.
* Docker implementation for easy deployment and testing.
* The database is hosted and managed on AWS with the use of DynamoDB.
* Swagger API dashboard and custom UI.
* Full Unit testing for the entirety of the application.
* Complete documentation of every function and return code.
* Proper error handling for every function.


## Authentication
You will need a username and password to log in. Logging in will give you a bearer token which will be used to get access to the API functions. Here is a table of the pre-entered users.

<table>
 <tr>
  <th>
   Username
  </th>
  <th>
   Password
  </th>
  <th>
   Access Rights
  </th>
 </tr>
 <tr>
  <td>
   admin
  </td>
  <td>
   admin
  </td>
  <td>
   Create, Read, Update, Delete
  </td>
 </tr>
 <tr>
  <td>
   guest
  </td>
  <td>
   password
  </td>
  <td>
   Read
  </td>
 </tr>
</table>

#### Please refer to these next steps to go through the authentication process.
1. Visit your running instance of the application.
2. Click on the function called "Login" in the UserLogin section.
3. Click on the button at the top right of the section called "Try it out".
4. Enter the username and password. There are multiple users you can choose from, who each come with different access levels. Please refer to the user's table above. 
5. Click on the "Execute" button at the bottom of the section.
6. You should then be able to see the response from the server. Copy the value returned in the "Response body". That is your Bearer token.
7. Go back to the top of the page and click on the "Authorize" button on the top right.
8. Paste the value of the token in the "Value" textbox and then click on the "Authorize" button.

You are now fully logged in and ready to use the application as you wish. Remember that the user that you used to log in will dictate which API functions you will have access to.


## API Endpoints
#### Song
|URI|Verb|Operation|Description|Success|Failure|
|:-|:-|:-|:-|:-|:-|
|/api/Song/CreateSong|POST|CREATE|Creates a new Song.|201 Created|400 Bad Request<br />404 Not Found|
|/api/Song/GetAllSongs|GET|READ|Gets all Songs.|200 OK|400 Bad Request<br />404 Not Found|
|/api/Song/GetSongsByTitle/{title}|GET|READ|Gets all Songs with which match given Title.|200 OK|400 Bad Request<br />401 Unauthorized<br />409 Conflict<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/GetSongsByAlbum/{album}|GET|READ|Gets all Songs with which match given Album.|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/GetSongsByArtist/{artist}|GET|READ|Gets all Songs with which match given Artist.|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/GetSongsByGenre/{genre}|GET|READ|Gets all Songs with which match given Genre.|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/GetSongsByReleaseYear/{releaseYear}|GET|READ|Gets all Songs with which match given Release Year.|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/UpdateSong|PUT|UPDATE|Updates the desired Song.|201 Created|400 Bad Request<br />401 Unauthorized<br />404 Not Found<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/Song/DeleteSong/{title}/{album}|DELETE|DELETE|Deletes the desired Song.|204 No Content|400 Bad Request<br />401 Unauthorized<br />404 Not Found<br />500 Internal Server Error<br />503 Service Unavailable|


#### User
|URI|Verb|Operation|Description|Success|Failure|
|:-|:-|:-|:-|:-|:-|
|/api/UserLogin/Login/{username}/{password}|POST|READ|Logs the user in and returns a bearer token|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/UserLogin/CreateUser|POST|CREATE|Creates a new User.|201 Created|400 Bad Request<br />401 Unauthorized<br />409 Conflict<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/UserLogin/GetAllUsers|GET|READ|Gets all Users.|200 OK|400 Bad Request<br />401 Unauthorized<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/UserLogin/ChangePassword/{newPassword}|PUT|UPDATE|Updates the desired User.|201 Created|400 Bad Request<br />401 Unauthorized<br />409 Conflict<br />500 Internal Server Error<br />503 Service Unavailable|
|/api/UserLogin/DeleteUser/{username}|DELETE|DELETE|Deletes the desired User.|204 No Content|400 Bad Request<br />401 Unauthorized<br />409 Conflict<br />500 Internal Server Error<br />503 Service Unavailable|


## Technologies and Methodologies used in this project
* C# / .NET 6
* AWS DynamoDB
* Docker 
* Swagger REST API
* MVC
* Unit Testing
* .NET 6 Background Processes


## Database 
The Database is hosted on AWS using DynamoDB which is a NoSQL Database. There are two tables which are the User and Song table.

#### Song
|Accessor|Name|Type|
|:-:|:-:|:-:|
|Public|Title|string|
|Public|Album|string|
|Public|Artist|nullable string|
|Public|Genre|nullable string|
|Public|ReleaseYear|nullable int|

#### User
|Accessor|Name|Type|
|:-:|:-:|:-:|
|Public|Username|string|
|Public|Password|string|
|Public|Age|nullable int|
|Public|Role|nullable string|


When creating a User, you must enter a role. If the role you enter is not valid, the User will be given the 'basic' role by default which only allows Reading permissions. The allowed roles are:
- admin -> Allows full Create, Read, Update, and Delete permissions.
- basic -> Allows only Read permissions.


## Docker
The application has been developed and published using Docker and is available to download [here](https://hub.docker.com/repository/docker/carmeloscribano/songrepository/general) to easily run, debug, and develop locally. To run the container:
- Go [here](https://hub.docker.com/repository/docker/carmeloscribano/songrepository/general) and download the image.
- After downloading the image, enter '*docker run -p 8080:80 carmeloscribano/songrepository:latest*' in your command prompt. 
- You can then go to your browser and enter www.localhost:8080/swagger/index.html to reach the application.
