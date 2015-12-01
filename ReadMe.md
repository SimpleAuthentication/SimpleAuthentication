![SimpleAuthentication - making authentication ... simple](http://i.imgur.com/eEJBOiY.png)

**SimpleAuthentication** is a ASP.NET library that makes it really easy and simple for developers to add *Social Authentication* to an ASP.NET application.

branch | status
----- | -----
master | [![Build status](https://ci.appveyor.com/api/projects/status/ekgco74ae4cyu31g)](https://ci.appveyor.com/project/PureKrome/simpleauthentication-294)
dev | [![Build status](https://ci.appveyor.com/api/projects/status/vhba8xt4alg4jg6a)](https://ci.appveyor.com/project/PureKrome/simpleauthentication-g3cxx)

Package | `master` @ NuGet | `dev` @ MyGet
----- | ------ | ---
SimpleAuthentication.Core | [![](http://img.shields.io/nuget/v/SimpleAuthentication.Core.svg?style=flat-square)](https://www.nuget.org/packages/SimpleAuthentication.Core) ![](http://img.shields.io/nuget/dt/SimpleAuthentication.Core.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/SimpleAuthentication.Core.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/SimpleAuthentication.Core.svg?style=flat-square)
SimpleAuthentication.ExtraProviders |  [![](http://img.shields.io/nuget/v/SimpleAuthentication.ExtraProviders.svg?style=flat-square)](https://www.nuget.org/packages/SimpleAuthentication.ExtraProviders) ![](http://img.shields.io/nuget/dt/SimpleAuthentication.ExtraProviders.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/SimpleAuthentication.ExtraProviders.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/SimpleAuthentication.ExtraProviders.svg?style=flat-square)
SimpleAuthentication.Mvc4 | [![](http://img.shields.io/nuget/v/SimpleAuthentication.Mvc4.svg?style=flat-square)](https://www.nuget.org/packages/SimpleAuthentication.Mvc4) ![](http://img.shields.io/nuget/dt/SimpleAuthentication.Mvc4.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/SimpleAuthentication.Mvc4.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/SimpleAuthentication.Mvc4.svg?style=flat-square)
SimpleAuthentication.Mvc3 | [![](http://img.shields.io/nuget/v/SimpleAuthentication.Mvc3.svg?style=flat-square)](https://www.nuget.org/packages/SimpleAuthentication.Mvc3) ![](http://img.shields.io/nuget/dt/SimpleAuthentication.Mvc3.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/SimpleAuthentication.Mvc3.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/SimpleAuthentication.Mvc3.svg?style=flat-square)
Nancy.SimpleAuthentication | [![](http://img.shields.io/nuget/v/Nancy.SimpleAuthentication.svg?style=flat-square)](https://www.nuget.org/packages/Nancy.SimpleAuthentication) ![](http://img.shields.io/nuget/dt/Nancy.SimpleAuthentication.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/Nancy.SimpleAuthentication.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/Nancy.SimpleAuthentication.svg?style=flat-square)
Glimpse.SimpleAuthentication | [![](http://img.shields.io/nuget/v/Glimpse.SimpleAuthentication.svg?style=flat-square)](https://www.nuget.org/packages/Glimpse.SimpleAuthentication) ![](http://img.shields.io/nuget/dt/Glimpse.SimpleAuthentication.svg?style=flat-square) | ![](http://img.shields.io/myget/SimpleAuthentication/vpre/Glimpse.SimpleAuthentication.svg?style=flat-square)![](http://img.shields.io/myget/SimpleAuthentication/dt/Glimpse.SimpleAuthentication.svg?style=flat-square)


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
* Still having problems? We hang out in [JabbR](https://jabbr.net/#/rooms/SimpleAuthentication) so you can ask questions in there :)
* We accept Pull Requests.
* Please use the GitHub issues for any other problems.
* License : [MIT](http://www.tldrlegal.com/license/mit-license)
* No Unicorns were harmed in the coding of this library.
