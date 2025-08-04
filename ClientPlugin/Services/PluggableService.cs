namespace ClientPlugin.Services
{
    public class PluggableService : AspNetCorePlugin.Services.PluggableService
    {
        public override string ComputeValue()
        {
            return "FROM PLUGIN";
        }
    }
}
