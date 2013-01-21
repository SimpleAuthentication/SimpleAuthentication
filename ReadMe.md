![WTF](http://i.imgur.com/Jx9vL.jpg)

![Not impressed](http://i.imgur.com/K2b91.jpg)

![I just don't get it](http://i.imgur.com/AUEc3.jpg)

![Angry](http://i.imgur.com/hvYIx.jpg)

![Forever code alone](http://i.imgur.com/KIMGE.jpg)

![Insane programmer](http://i.imgur.com/m7gGt.jpg)

![Success Programmer](http://i.imgur.com/yQVJU.jpg)

# A .NET package for using Facebook, Google or Twitter to Authenticate your Users #

I'm blond. I'm dumb. But I program. 

So I want a <insert deity of your choice> damn simple way to authenticate with Facebook, Twitter or wherever.

**I DON'T CARE IF IT'S OAUTH OR OPENID OR O-GO-SCREW-YOURSELF.**

I just want to do

1. Send me off to Facebook, Google or Twitter.
2. Come back to my site and the site now has whatever user data they handed over.

That's It.

- No dabasase crap.
- No Session stuff.
- No over-generic-crazy one-solution-fits-every-provider-on-the-interwebs.

So install this bad boy :

[![Yes! Install this package!!](http://i.imgur.com/FM21h.png)](http://nuget.org/packages/WorldDomination.Web.Authentication)

## Code or GTFO ##

Here's the main code that does what we want (excluding error checking, etc, for brevity).

```c#
public RedirectResult RedirectToAuthenticate(string providerKey)
{
    var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey);
    return Redirect(uri.AbsoluteUri);
}

public ActionResult AuthenticateCallback(string providerKey)
{
    var model = new AuthenticateCallbackViewModel();
    model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request.Params);
    return View(model);
}
```

## Ok. You had me at Bad Luck Brian. Now what?

1. Read the [sample code pages in the project's Wiki](https://github.com/PureKrome/WorldDomination.Web.Authentication/wiki) - take 1 minute to grok.
2. Install nuget pacakge.
3. Win.

## Play it forward ##

Don't be scared to fork and then make some pull requests. I :heart: pull requests!

Then this simple library can actually be really helpful to more than 1 person (le moi) on this rock called Earth.

#### Disclaimer ####
*No blonds or Unicorns were harmed in the coding of this library.*

![Pew Pew](http://i.imgur.com/94PHAl.jpg)
