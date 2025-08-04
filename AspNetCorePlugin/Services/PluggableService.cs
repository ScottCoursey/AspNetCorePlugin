namespace AspNetCorePlugin.Services
{
    public class PluggableService : IPluggableService
    {
        public virtual string ComputeValue()
        {
            return "ORIGINATING FROM CORE";
        }
    }
}
