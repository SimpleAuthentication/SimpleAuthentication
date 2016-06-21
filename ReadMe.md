![SimpleAuthentication - making authentication ... simple](http://i.imgur.com/eEJBOiY.png)

**SimpleAuthentication** is a ASP.NET library that makes it really easy and simple for developers to add *Social Authentication* to an ASP.NET application. It currently targets .NET 4.x framework, not the new .NET Core framework.

branch @ GitHub | status
----- | -----
master | [![Build status](https://ci.appveyor.com/api/projects/status/ekgco74ae4cyu31g)](https://ci.appveyor.com/project/PureKrome/simpleauthentication-294)
dev | [![Build status](https://ci.appveyor.com/api/projects/status/okhor81tbvxucdoo)](https://ci.appveyor.com/project/PureKrome/simpleauthentication-xud0a)

Package | `master` @ NuGet | `dev` @ MyGet
----- | ------ | ---
SimpleAuthentication.Core | [![NuGet Badge](https://buildstats.info/nuget/SimpleAuthentication.Core)](https://www.nuget.org/packages/SimpleAuthentication.Core/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/SimpleAuthentication.Core)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/SimpleAuthentication.Core)
SimpleAuthentication.ExtraProviders | [![NuGet Badge](https://buildstats.info/nuget/SimpleAuthentication.ExtraProviders)](https://www.nuget.org/packages/SimpleAuthentication.ExtraProviders/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/SimpleAuthentication.ExtraProviders)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/SimpleAuthentication.ExtraProviders)
SimpleAuthentication.Mvc4 | [![NuGet Badge](https://buildstats.info/nuget/SimpleAuthentication.Mvc4)](https://www.nuget.org/packages/SimpleAuthentication.Mvc4/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/SimpleAuthentication.Mvc4)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/SimpleAuthentication.Mvc4)
SimpleAuthentication.Mvc3 | [![NuGet Badge](https://buildstats.info/nuget/SimpleAuthentication.Mvc3)](https://www.nuget.org/packages/SimpleAuthentication.Mvc3/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/SimpleAuthentication.Mvc3)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/SimpleAuthentication.Mvc3)
Nancy.SimpleAuthentication | [![NuGet Badge](https://buildstats.info/nuget/Nancy.SimpleAuthentication)](https://www.nuget.org/packages/Nancy.SimpleAuthentication/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/Nancy.SimpleAuthentication)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/Nancy.SimpleAuthentication)
SimpleAuthentication.Mvc3 | [![NuGet Badge](https://buildstats.info/nuget/Glimpse.SimpleAuthentication)](https://www.nuget.org/packages/Glimpse.SimpleAuthentication/) | [![MyGet Badge](https://buildstats.info/myget/simpleauthentication/Glimpse.SimpleAuthentication)](https://www.myget.org/feed/SimpleAuthentication/package/nuget/Glimpse.SimpleAuthentication)



## What does the term "Social Authentication" mean"?

Social Authentication are login buttons that use popular social websites (like Facebook or Google) as the way to login to your own website.
These social websites Facebook/Google/etc are referred to as Social Authentication Providers (aka. AP's).

![Sample Login Buttons](http://i.imgur.com/2X35uaQ.png)

## Why do we want to offer Social Authentication?

A few reasons:

  - People are getting tired of creating new usernames/passwords all the time.
  - People generally only use the same few passwords for all their accounts. This means that if one of those websites is compromised, then there is a high chance those compromised credentials can be reused on other sites the user has an account on.
  - If you store passwords, then your server is now a possible target/attack vector and now you have to make sure you're protecting your sensitive user data. 
  - The Authentication Providers now have to deal with the security of storing passwords. You've just delegated a huge security responsibility to them :)

## What Authentication Providers are available?

Out of the box, it offers **Facebook**, **Google**, **Twitter** and **Microsoft Live** integration for either [ASP.NET MVC](http://www.asp.net/mvc) or [NancyFX](http://nancyfx.org) applications. 

## How does this compare to ASP.NET Identity / ASP.NET Membership?

 - **Simple Authentication**: An extremely *lightweight* library that only deals with the *authentication*. No database code. No rules forcing you to implement contracts. *No passwords*
 - **ASP.NET Identity / Membership**: a heavy, enterprisy, one-huge-hammer-fits-all approach that is strongly tied to sql server and entity framework.

Simple Authentication doesn't want to tie you into any particular database, data access layer or forcing / maintaining passwords. In essence, we've tried to pass this security concern onto other systems. Once you've received some *authenticated* user information, *you* decide what you want to do with.
On the other hand, ASP.NET Identity/Membership is a full end-to-end stack for user credentials. It's tied to Sql Server and you're tied to implementing all the interface contracts. But most importantly, passwords are still stored in your database if forms authentication was used. It's a one-big-hammer approach.

[Read this wiki page](http://asd) for an elaborate discussion on the differences (pro's/con's) of Simple Authentication vs ASP.NET.

## Extensible

Have an OAuth 1.0a or OAuth 2.0 Authentication Provider? It's really easy to create your own providers, extending what's already out of the box.
These AP's are also available:

- GitHub
- 37 Signals
- Instagram
- LinkedIn

Developer friendly!
--
Take advantage of the [Glimpse plugin](http://getglimpse.com) so you can see what magic is happening under the hood if you need to debug a problem or just want to see what happens :)

![Glimpse plugin](http://i.imgur.com/ALO3rab.png)



And Finally ...
--
* Still having problems? Create an issue with your question/problem.
* We accept Pull Requests.
* License : [MIT](http://www.tldrlegal.com/license/mit-license)
* No Unicorns were harmed in the coding of this library.
