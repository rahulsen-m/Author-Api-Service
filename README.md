# Author-Api-Service
This is dummy API application for author and book service. 
This project is based on the REST architecture. We have added seeding data , so then when the user will run the project, there will not be any empty db calls. 

This application has different tendpoints which will help the user to get, create, update fully or partially and delete the authors. User can search or create the author with collection of resources. User can also filter, pagging, short or shap the data dynamically.
We will add the snapshort and the request url when we will be done with the development.

# Built With
- ASP .Net Core 2.2
- Entity Framework core
- Automapper
- SQL Server

# The following aspects we have tried to cover in this application
**Automapper** - A convention-based object mapper. Use to map the Entities to the DTO.

**REST(Representational State Transfer)** - Architectural style for networked hypermedia applications.

**Correct Status Code which represent the proper API action**

**Content Negotiation** - Configure the request and response format

**Global error handleing**

**Paging**

**Filtering**

**Searching**

**Shorting**

**Shaping**

**HEAT OS**

**Versioning**

**Support other media types excepts json**

**Swagger**

**caching**

and others will be added as we will progress

# Roadmap

We will add the following features/tools in the application 

- Logger
- Docker
- CI & CD pipeline

# Getting Started

- Install the [dotnet core 2.2 sdk](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- Add your connection string in the startup.cs
- Open VS code
- Type the following code to for creating the database.
 > dotnet ef migrations add InitialMigration -o Migrations
 
 > dotnet ef database update
- Run the application by pressing ctrl+f5. 

:relaxed: **You are good to go** :relaxed:
# Author

**_Rahul Sen_** :sleeping:

# Acknowledgments

- Learn
- Inspire
