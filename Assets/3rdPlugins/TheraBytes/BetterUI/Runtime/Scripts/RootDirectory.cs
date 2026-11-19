
namespace TheraBytes.BetterUi
{
    public partial class RootDirectory
    {
        public const string DefaultRoot = "TheraBytes";

        public static string GetPath(string pathRelativeToRoot, bool prefixAssets = false)
        {
            if (prefixAssets)
            {
                return string.Format("Assets/{0}/{1}",
                    OverrideRoot.TrimEnd('/', '\\'),
                    pathRelativeToRoot.TrimStart('/', '\\'));
            }
            else
            {
                return string.Format("{0}/{1}",
                    OverrideRoot.TrimEnd('/', '\\'),
                    pathRelativeToRoot.TrimStart('/', '\\'));
            }
        }

        public static string GetAbsolutePath(string pathRelativeToRoot = "")
        {
            var p = pathRelativeToRoot.TrimStart('/', '\\');
            if (p == "")
            {
                return string.Format("{0}/{1}",
                    UnityEngine.Application.dataPath,
                    OverrideRoot.TrimEnd('/', '\\'));
            }
            else
            {
                return string.Format("{0}/{1}/{2}",
                    UnityEngine.Application.dataPath,
                    OverrideRoot.TrimEnd('/', '\\'),
                    pathRelativeToRoot.TrimStart('/', '\\'));
            }
        }
    }
}
