![WTF](http://i.imgur.com/Jx9vL.jpg)

![Not impressed](http://i.imgur.com/Uq4Wm.jpg)

![I just don't get it](http://i.imgur.com/AUEc3.jpg)

![Angry](http://i.imgur.com/hvYIx.jpg)

![Forever code alone](http://i.imgur.com/KIMGE.jpg)

![Insane programmer](http://i.imgur.com/m7gGt.jpg)

![Success Programmer](http://i.imgur.com/yQVJU.jpg)

# A .NET package for using Facebook or Twitter to Authenticate your Users #

I'm blond. I'm dumb. But I program. 

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
public RedirectResult FacebookAuthentication(string providerKey)
{
	// NOTE: ProviderKey? WTF is that? well .. where do you want to go? Facebook? Twitter? Google? that's this value.
	//       This is what button you usually press, on your web page UI.
	//       A 'Provider' is the fancy word for the website we goto to login, at. Eg. FB/T/Goog, etc..

    // Create some session, so the 'state' is remembered on the callback 
	// (when we return from the authentication website).
    Session.Add("a", "a"); // Keep the SessionId constant.

	// Grab the uri which we need to redirect to, based on which provider we want to authenticate against.
    var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey, Session.SessionID);

	// GO! GO! GO!
    return Redirect(uri.AbsoluteUri);
}

public ActionResult AuthenticateCallback(string providerKey)
{
    // NOTE: we need to know where we *were* ... so the provider includes this piece of info 
    //       when they *callback* to us.        
    if (string.IsNullOrEmpty(providerKey))
    {
        throw new ArgumentNullException("providerKey");
    }

    // Ye standard Ole View model. 
    // PRO-TIP: You do use view models, right? (There is only one correct answer, here)
    var model = new AuthenticateCallbackViewModel();
    try
    {
        // Get the user details. If this works, we've authenticated AND have *some* user details.
        // Depending on who you authenticated against, some data is not provided.
        model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request.Params,
                                                                         Session.SessionID);
    }
    catch (Exception exception)
    {
        // Shit happened .. or the user manually said 'No - do not give permissions' when asked if they 
        // can hand over some of their personal info after they've entered their username/password.
        model.Exception = exception;
    }

    // $5 if you can guess what this does.
    return View(model);
}
```

## Play it forward ##

Don't be scared to fork and then make some pull requests. I :heart: pull requests!

Then this simple library can actually be really helpful to more than 1 person (le moi) on this rock called Earth.

#### Disclaimer ####
*No blonds or Unicorns were harmed in the coding of this library.*

![Pew Pew](http://i.imgur.com/94PHAl.jpg)