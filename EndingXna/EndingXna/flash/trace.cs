using System.Diagnostics;

namespace flash
{
    public static class language {
        
        public static void trace(object obj, params object[] ps) {
            Debug.WriteLine(obj.ToString());
            foreach(var p in ps) {
                Debug.WriteLine(p.ToString());
            }
        }
    }
}
