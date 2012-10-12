![WTF](http://i.imgur.com/Jx9vL.jpg)

![Not impressed](http://i.imgur.com/Uq4Wm.jpg)

![I just don't get it](http://i.imgur.com/AUEc3.jpg)

![Angry](http://i.imgur.com/hvYIx.jpg)

![Forever code alone](http://i.imgur.com/KIMGE.jpg)

![Insane programmer](http://i.imgur.com/m7gGt.jpg)

![Success Programmer](http://i.imgur.com/yQVJU.jpg)

# A .NET package for using Facebook or Twitter to Authenticate your Users #

I'm blond. I'm dum. But I program. 

So I want a <insert deity of your choice> damn simple way to authenticate with Facebook, Twitter or wherever.

**I DON'T CARE IF IT'S OAUTH OR OPENID OR OH-GO-SCREW-YOURSELF.**

I just want to do

1. Send me off to Facebook, Twitter, wherever.
2. Come back to my site and the site now has whatever user data they handed over.

That's It.

- No dabasase crap.
- No Session stuff.
- No over-generic-crazy one-solution-fits-every-provider-on-the-interwebs.

## Code or GTFO ##

```
public RedirectResult FacebookAuthentication()
{
    Session.Add("a", "a"); // Keep the SessionId constant.
    return AuthenticationService.RedirectToFacebookAuthentication(Session.SessionID);
}

public ActionResult AuthenticateCallback()
{
    var client = AuthenticationService.CheckCallback(Request, Session.SessionID);

    var model = new AuthenticateCallbackViewModel();
            
    if (client is FacebookClient)
    {
        var facebookClient = client as FacebookClient;
        model.AccessToken = facebookClient.AccessToken;
        model.Name =
            (facebookClient.UserInformation.FirstName + " " + facebookClient.UserInformation.LastName).Trim();
        model.UserName = facebookClient.UserInformation.UserName;
        model.Message = "Authenticated with Facebook successfully.";
    }

    return View(model);
}
```

## Play it forward ##

Don't be scared to fork and then make some pull requests. I :heart: pull requests!

Then this simple library can actually be really helpful to more than 1 person (le moi) on rock called Earth.

#### Disclaimer ####
*No blonds or Unicorns were harmed in the coding of this library.*

![Pew Pew](http://i.imgur.com/94PHAl.jpg)