# exercise-jesse-quijano


# Purpose

The next step in our interview process is to share code with one another. Below we have described a simple app which once delivered should open up a nice dialog around what you think good code looks like. After completing the project described below, you will have an opportunity to share with our team all of your thoughts regarding your approach and implementation of this task. Afterword, we will take a deeper look at our production code base and have an open discussion about our approach and implementation.

# Project Description

This project is about exposing a single web api endpoint that returns the most recent post authored by each user from a third party data set.

## Requirements

- The data set is located here https://jsonplaceholder.typicode.com/posts
- Name Your Project FieldLevel so that you have a FieldLevel.csproj
- Results should display the most recent post authored by each user. Use post.id to determine recency.
- Regardless of the frequency at which your endpoint is requested across multi devices, the third party data set should not be requested more than once per minute.
- Please do not spend more than a 4 hours on this task.

# Project Deliverable

- Please provide a single solution file (\*.sln) that can be compiled and launched from Visual Studio.
- Submit your solution as a pull request.

# Requirements (Tested On)
- .NET Core 3.1
- Redis 4 (4.0.14 Azure, :4 Docker)
- Windows 10, macOS 10.15, or Ubuntu 20.04

# How To Run
This application leverages Redis.
- The easiest way to run the application is to create a Redis Docker container
- `docker run -p 6379:6379 --name redis-local -d redis:4`

_Note: an Azure instance has been setup to make things easier for deployment, the connection string will be included via email (never check in passwords!)_

You will need to update the `appsettings.json` files before running the project if not running locally using the above Docker configuration:
- Tests (integration/unit) at `FieldLevel.Tests\appsettings.json`
- Application at `FieldLevel\appsettings.Development.json`
- using the format: `"password=@server:port?ssl=true"`

To validate everything is setup properly, run the unit tests. Assuming you are in the project root:

`cd FieldLevel.Tests`

`dotnet test`

Because some of the tests are testing cache expiration, the total execution time should be about 30 seconds.

That's it, let's run it.
**Using Visual Studio and IIS Express**, click the button. :) It should navigate to the only endpoint directly, if not go here:

`https://localhost:44300/api/posts`

**Using Mac, Windows, Linux** Again assuming you are in the root solution directory:

`cd FieldLevel`

`dotnet run`

Then go to `https://localhost:5001/api/posts`

# Design Decisions
- Leverage TDD. Built tests first to prove out my functionality, then injected and used those services in the controller. 
- Use Redis. Why build out a caching mechanism, when such a thing already exists? And it's a good one.
- In trying to keep with the spirit of the 4 hour rule (I did go a little over), I did not secure the endpoint. Initially I was going to leverage Auth0 and JWT, but it quickly became clear that I wouldn't have enough time.
- The endpoint outputs JSON - as good endpoints should.

# Deployment
This will be auto deployed to an azure site
https://{git-repo}.azurewebsites.net

Customizing
https://github.com/projectkudu/kudu/wiki/Customizing-deployments

# FAQ

Q: **Can I ask questions?**

A: Of Course. Often, the answer will be "You choose".
