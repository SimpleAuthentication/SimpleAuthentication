![SimpleAuthentication - making authentication ... simple](http://i.imgur.com/eEJBOiY.png)

--
**SimpleAuthentication** is a ASP.NET library that makes it really simple to for developers to add *Social Authentication* code to an ASP.NET application.

![Sample Login Buttons](http://i.imgur.com/2X35uaQ.png)

Out of the box, it offers **Facebook**, **Google**, **Twitter** and **Microsoft Live** integration for either [ASP.NET MVC](http://www.asp.net/mvc) or [NancyFX](http://nancyfx.org) applications. 

There's also some less used authentication providers like GitHub or Amazon supplied but anyone can take advantage of the api and create your own provider extension. There are plenty of defaults in place (such as Routes, etc) but most things are available to adjust if you want to do advanced stuff.


Developer friendly!
--
Take advantage of the [Glimpse plugin](http://getglimpse.com) so you can see what magic is happening under the hood if you need to debug a problem or just want to see what happens :)

![Glimpse plugin](http://i.imgur.com/ALO3rab.png)

<br/>

**The library only deals with authentication** - once we give you the user details for the person logging in, you can whatever you want with that (such as, create a new user or update an existing user).
We do not attempt to insert data into a particular type of database or make any other assumptions about what you do with user data.


The "How simple is this?" example 
--
#### Adding a 'Log in with Facebook' to an existing ASP.NET MVC web application.


#### Quick summary
* Create a Login button on some View.
* Create a class which will have all the User data once they have authenticated
* Add your provider keys to the `.config` file

#### Simple steps
1. Find the View you wish to modify.
2. Add the Button or hyper link that will be used to kick start the authentication process.
3. Set the button route to be `/authentication/redirect/fakefacebook`
4. Now we grab the library -> `install-package SimpleAuthentication.Mvc`
5. Create a class which will be called -after- we come back from Facebook (or any provider). We have to do something with that user data, right?
   `public class HandleCallback();`
   A good example of what people do here is: save user to database then redirect to homepage or where they were originally referred from.
6. Wire up the new class with our `ServiceLocator / Di-IoC` so the SimpleAuthentication code knows what to do when it's finished.
7. Build and run the site :)

Once this works.

8. Goto [developer.facebook.com](http://developer.facebook.com) and create an application. This will give you a `Client Key` and `Secret Key`.
9. Enter the Client and Secret key to the `web.config`, `<providers>` section.
10. Change the button route to `/authentication/redirect/facebook`  <-- notice we've removed the `fake` prepended text? :)

Done.


You had me at *Simple* ... what now?
--
* Detailed guide to adding SimpleAuthentication to [an ASP.NET MVC](https://github.com/SimpleAuthentication/SimpleAuthentication/wiki/Mvc-automatic-setup) or NancyFx web application.
* Detailed Guide to using the Extra Providers to a web application.
* Detailed guide to using the Glimpse Plugin.
* How to create your own provider.

And Finally ...
--
* Still having problems? We hang out in [JabbR](https://jabbr.net/#/rooms/SimpleAuthentication) so you can ask questions in there :)
* We accept Pull Requests.
* Please use the GitHub issues for any other problems.
* License : [MIT](http://www.tldrlegal.com/license/mit-license)
* No Unicorns were harmed in the coding of this library.
