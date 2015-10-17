         ####################################################################
         ##    SimpleAuthentication - making authentication ... simple!    ##
         ####################################################################


   

Starting Options
^^^^^^^^^^^^^^^^
   1. Use the web.config custom provider
                            or
   2. Manually wire up the providers you will offer.


Easy vs Hard
^^^^^^^^^^^^
   1. EASY: Choose to use the provided Nancy Module or Mvc Controller to make setup -really- simple.
            If you do this, you will need to install-package ..
      a. Nancy Module: Install-Package Nancy.SimpleAuthentication
      b. Mvc Controller: Install-Package SimpleAuthentication.Mvc 
                            or
   2. HARD: Create your own module/controller, your own routes and code all of this. Manually.



Setup Instructions
^^^^^^^^^^^^^^^^^^

   1. MVC Automatic Setup - https://github.com/SimpleAuthentication/SimpleAuthentication/tree/master/Samples/SimpleAuthentication.Sample.MvcAuto
   2. NancyFX Automatic Setup - https://github.com/SimpleAuthentication/SimpleAuthentication/tree/master/Samples/SimpleAuthentication.Sample.NancyAuto
   


!! SPECIAL INSTRUCTIONS IF YOU WANT TO OFFER AUTHENTICATION VIA MICROSOFT LIVE !!
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    If you wish to provide authentication via MS Live, then you will need to do some special hacks to your
localhost if you want to develop and TEST the authentication on your development machine(s). This is
because the Microsoft Live team do NOT allow a callback to http://localhost:port . Yes, this is
very frustrating, but until they change their system, we're stuck with this issue.

This means a few things, for you.
1. You need to use a fully qualified domain name which resolves to localhost/127.0.0.1.
2. Your IIS or IISExpress needs to accept requests for that custom domain.

Here's the pro steps :)
1. There's a *free* domain we can all leverage called http://localtest.me . ~All~ subdomains resolve
   to 127.0.0.1. You can make ~any~ subdomain you want without having to go to some admin page or
   account page and 'creating' it. Just go and try: ping whateverYouWant.localtest.me and it should
   resolve to 127.0.0.1 ! Awesome! For more info: http://readme.localtest.me/
2. Make sure your machine can accept this domain, etc.
   a) Run command line AS ADMIN.
   b) type => netsh http add urlacl url=http://create-some-subdomain-here.localtest.me:1337/ user=everyone
3. Add this binding to your website.
   a) IIS: Just open up IIS -> your website -> Bindings -> add the domain name.
   b) IIS Express: open up \\MyDocuments\IISExpress\config\application.config  ... and look for the <sites> element (around line 155).
      Find your IIS Express website and add in a new binding, like :
      <bindings>
          <binding protocol="http" bindingInformation="*:1337:localhost" />
          <binding protocol="http" bindingInformation="*:1337:localtest.me" />
      </bindings>
      ** I added the 2nd binding, in. The first binding was already there.

And now you should be able to accept the callback from MS Live, when it tries to 'goto' that ***.localtest.me:port url :)


Bonus Pro Tips
^^^^^^^^^^^^^^
1. Free Login Images: Need some login buttons? Thought so: http://bit.ly/U3qSIL
3. Coding Choons: You can't get away without some pro coding choons. Here's four:
        a) http://www.youtube.com/watch?v=byZO3dMLtpA
        b) http://www.youtube.com/watch?v=ZG1AT6tylA4
        c) http://soundcloud.com/phazing-004/dirty-south-phazing-radio-004
        d) http://www.youtube.com/watch?v=cla38u6MrEI             

----------------------------------------------------------------------------------------


Now go forth and execute World Domination! Seriously. Go. Dominate. Even a wee bit. Go.


-Justin Adler [from Melbourne, Australia ... mate :) ]-
-Phillip Haydon [from Auckland, New Zealand... kia ora... BRO!]-
