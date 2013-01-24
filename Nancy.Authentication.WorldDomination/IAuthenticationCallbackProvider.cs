using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Authentication.WorldDomination
{
    public interface IAuthenticationCallbackProvider
    {
        dynamic Process();
    }
}
