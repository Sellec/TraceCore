/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        System.AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, System.ResolveEventArgs args)
    {
        if (args.Name.Contains("EntityFramework"))
        {

        }
        return null;
        //throw new System.NotImplementedException();
    }
}