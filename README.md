## SingleBlog

SingleBlog is a Microsoft NET 5.0 Web API solution that provides a minimal set of API interface useful to implement a single blog backend.

The main components/dependencies are:

*SingleBlog Project:*

- Microsoft NET 5.0 framework

- Microsoft C# 9.0 programming language

- Microsoft.EntityFrameworkCore v5.0.13

- Microsoft.EntityFrameworkCore.SqlLite v5.0.13 (SQLite Database Driver)

- Swashbucle.AspNetCore.Swagger v6.23(Web interface to interact with APIs)

*SingleBlog.Test Project:*

- Microsoft.AspNetCore.Mvc Testing v.5.0.13 (useful to perform APIs integration tests)

- NUnit v.3.13.2

- NUnit3TestAdapter v.4.1.0

*Host server component:*

- Microsoft.AspNetCore.Server.Kestrel v.5.0.13

SingleBlog has been developed with Microsoft Visual Studio Community v.19.0

Installed extensions:

- Fine Code Coverage v1.1.154

- SQLite and SQL Server Compact Toolbox v4.8.735


It's possible to clone the public git repository with the command: 

**git clone https&#58;//bitbucket.org/silviopola/singleblog.git**

In Visual Studio IDE it's possible to compile and start the application in Debug/Release

After compilation and publishing it's possible to manually start the application by launching th executable **SingleBlog.exe** in the Publish destination folder

I also pushed in my Docker repository 20061113 an image of the application
Its possible to download and start it with the command:

**docker run -d -p 8090:8090 20061113/singleblog**

The application starts on the port number 8090

**http://localhost:8090**

It's possible to interact with the application APISs by using Swagger UI at this address:

**http://localhost:8090/swagger**
