using System;
using System.Collections.Generic;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders;
using WorldDomination.Web.Authentication.ExtraProviders.OpenId;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class OpenIdProviderFacts
    {
        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenAValidHttpOpenIdIdentifier_RedirectToAuthenticate_ReturnsAValidEndPoint()
            {
                // Arrange.
                const string xrdsLocation = "https://www.myopenid.com/xrds";

                // 302 Redirect :P
                var mockRestResponseRedirect = new Mock<IRestResponse>();
                mockRestResponseRedirect.Setup(x => x.StatusCode).Returns(HttpStatusCode.Redirect);
                mockRestResponseRedirect.Setup(x => x.Headers)
                                        .Returns(new List<Parameter>
                                        {
                                            new Parameter
                                            {
                                                Name = "Location",
                                                Type = ParameterType.HttpHeader,
                                                Value = "https://myopenid.com/"
                                            }
                                        });
                var mockRestClientRedirect = new Mock<IRestClient>();
                mockRestClientRedirect.Setup(x => x.BaseUrl).Returns("http://myopenId.com/");
                mockRestClientRedirect.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                                      .Returns(mockRestResponseRedirect.Object);

                // 200 OK with the Xrds location.
                var mockRestResponseOk = new Mock<IRestResponse>();
                mockRestResponseOk.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseOk.Setup(x => x.Headers)
                                  .Returns(new List<Parameter>
                                  {
                                      new Parameter
                                      {
                                          Name = "X-XRDS-Location",
                                          Type = ParameterType.HttpHeader,
                                          Value = xrdsLocation
                                      }
                                  });
                var mockRestClientOk = new Mock<IRestClient>();
                mockRestClientOk.Setup(x => x.BaseUrl).Returns("https://myopenId.com/");
                mockRestClientOk.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                                .Returns(mockRestResponseOk.Object);


                const string yahooXrdsXml =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><xrds:XRDS    xmlns:xrds=\"xri://$xrds\" xmlns:openid=\"http://openid.net/xmlns/1.0\" xmlns=\"xri://$xrd*($v*2.0)\"><XRD><Service priority=\"0\"><Type>http://specs.openid.net/auth/2.0/server</Type><Type>http://specs.openid.net/extensions/pape/1.0</Type><Type>http://openid.net/srv/ax/1.0</Type><Type>http://specs.openid.net/extensions/oauth/1.0</Type><Type>http://specs.openid.net/extensions/ui/1.0/lang-pref</Type><Type>http://specs.openid.net/extensions/ui/1.0/mode/popup</Type><Type>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier</Type><Type>http://www.idmanagement.gov/schema/2009/05/icam/no-pii.pdf</Type><Type>http://www.idmanagement.gov/schema/2009/05/icam/openid-trust-level1.pdf</Type><Type>http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf</Type><URI>https://open.login.yahooapis.com/openid/op/auth</URI></Service></XRD></xrds:XRDS>";
                const string myOpenIdXrdsXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><xrds:XRDS xmlns:xrds=\"xri://$xrds\" xmlns:ux=\"http://specs.openid.net/extensions/ux/1.0\" xmlns=\"xri://$xrd*($v*2.0)\"> <XRD> <Service priority=\"0\"><Type>http://specs.openid.net/auth/2.0/server</Type><Type>http://openid.net/sreg/1.0</Type><URI priority=\"0\">https://www.myopenid.com/server</URI></Service><Service><Type>http://specs.openid.net/extensions/ux/1.0/friendlyname</Type><ux:friendlyname>MyOpenID</ux:friendlyname></Service><Service><Type>http://specs.openid.net/extensions/ux/1.0/img</Type><ux:img url=\"https://www.myopenid.com/static/images/myopenid_selector.png\" width=\"48\" height=\"48\"></ux:img></Service></XRD></xrds:XRDS>";
                var mockRestResponseXrds = new Mock<IRestResponse>();
                mockRestResponseXrds.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseXrds.Setup(x => x.Content).Returns(myOpenIdXrdsXml);

                var mockRestClientXrds = new Mock<IRestClient>();
                mockRestClientXrds.Setup(x => x.BaseUrl).Returns(xrdsLocation);
                mockRestClientXrds.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                                  .Returns(mockRestResponseXrds.Object);

                var mockRestClients = new List<IRestClient>
                                      {
                                          mockRestClientRedirect.Object,
                                          mockRestClientOk.Object,
                                          mockRestClientXrds.Object
                                      };
                var openIdProvider = new OpenIdProvider
                {
                    RestClientFactory = new RestClientFactory(mockRestClients)
                };
                var authenticationServiceSettings = new OpenIdAuthenticationServiceSettings
                {
                    Identifier = new Uri("http://myopenId.com/"),
                    CallBackUri = new Uri("http://whatever.com:9999")
                };
                
                
                // Act.
                var result = openIdProvider.RedirectToAuthenticate(authenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("https://www.myopenid.com/server?openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select&openid.identity=http://specs.openid.net/auth/2.0/identifier_select&openid.mode=checkid_setup&openid.ns=http://specs.openid.net/auth/2.0&openid.ns.sreg=http://openid.net/extensions/sreg/1.1&openid.sreg.required=nickname&openid.sreg.optional=email,fullname,gender,language&no_ssl=true&openid.return_to=http%3A%2F%2Fwhatever.com%3A9999%2F%26state%3D&openid.realm=http%3A%2F%2Fwhatever.com%3A9999%2F%26state%3D",
                    result.AbsoluteUri);
            }

            [Fact(Skip = "")]
            public void GivenA302RedirectWithoutAnyLocationHeader_RedirectToAuthenticate_ReturnsNull()
            {
                // Mock the response, but don't add a Location header.
            }

            [Fact(Skip = "")]
            public void GivenAnyHttpIdentifierButTheresSomeNetworkProblems_RedirectToAuthenticate_ThrowsAnException()
            {
                // Mock the response, failing with an exception (eg. server was down).
                // NOTE: What about a valid (but unknown) endpoint. eg. http://xx.aa.bb.cc/
            }
        }
    }

    // ReSharper restore InconsistentNaming
}