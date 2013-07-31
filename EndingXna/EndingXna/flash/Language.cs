using System.Diagnostics;

namespace flash
{
    /// <summary>
    /// Represents a holder for global static Flash methods.
    /// </summary>
    public static class Language {
        
        /// <summary>
        /// Displays expressions while debugging. A single trace statement can support multiple arguments. If any argument in a trace statement includes a data type other than a String, the trace function invokes the associated ToString() method for that data type. 
        /// For example, if the argument is a Boolean value the trace function invokes Boolean.ToString() and displays the return value.
        /// </summary>
        /// <param name="obj">The object to trace.</param>
        /// <param name="ps"></param>
        public static void trace(object obj, params object[] ps) {
            Debug.WriteLine(obj.ToString());
            foreach(var p in ps) {
                Debug.WriteLine(p.ToString());
            }
        }
    }
}
