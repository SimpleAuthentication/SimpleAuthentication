[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.WorldDominationConfig), "Start")]
namespace $rootnamespace$.App_Start
{
    public class WorldDominationConfig
    {
        public static void Start()
        {
            ...route an any other setup here...
        }
    }
}