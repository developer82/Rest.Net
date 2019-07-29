REST.NET
===================

[![Build status](https://ci.appveyor.com/api/projects/status/f28u80focj5laoee?svg=true)](https://ci.appveyor.com/project/developer82/rest-net)
[![BuitlWithDot.Net shield](https://builtwithdot.net/project/39/rest.net/badge)](https://builtwithdot.net/project/39/rest.net)

REST.NET is a simple REST and HTTP API Client for .NET Core.
Why another REST client? When I started developing .NET core I couldn't find a suitable replacement for [RestSharp](https://github.com/restsharp/RestSharp) library that suits my needs, so I decided to create one. It's basically a wrapper around `HttpClient`, which makes API calls much simpler and with fewer lines of code.

> **Note:**
> I'm developing this library per my project needs on my free time. So not everything you expect might be here and I didn't have a chance to check everything up.
> You are welcome to help (even reporting known bugs or suggesting features by [opening](https://github.com/developer82/Rest.Net/issues/new) an issue is helpful).

----------


Installation
-------------

REST.NET is available via [NuGet](https://www.nuget.org/packages/Rest.Net/) package:

    Install-Package Rest.Net



Documentation
-------------
**Creating RestClient**

The first thing you will need to do is create a `RestClient`. This object is what's wrapping `HttpClient` and responsible for dispatching http requests. You can use the same object for multiple calls.
```
IRestClient client = new RestClient("http://www.yourapi.com/");
```

**Making API requests**

Most of the libraries I encountered (including the `HttpClient` object) requires you to first create a request object and pass that to the client - which for most use cases I found redundant and unnecessary. 90% of the time I found myself writing the same request object, so to simplify, REST.NET supports sending "simple" requests directly from the client object.

Synchronous requests examples
```
// GET requests
IRestResponse<string> result = client.Get("/person/123");
IRestResponse<Person> person = client.Get<Person>("/person/123");

// PUT requests
Person person = new Person()
{
    FirstName = "Ophir",
    LastName = "Oren"
};
IRestResponse<string> result = client.Put("/person", person);
IRestResponse<Person> person = client.Put<Person>("/person");

// POST requests
Person person = new Person()
{
    Id = 1,
    FirstName = "Ophir",
    LastName = "Oren"
};
IRestResponse<string> result = client.Post("/person", person);
IRestResponse<Person> person = client.Post<Person>("/person");

// POST requests
Person person = new Person()
{
    Id = 1
};
IRestResponse<string> result = client.Delete("/person", person);
IRestResponse<Person> person = client.Delete<Person>("/person");
```


More docs coming soon....
