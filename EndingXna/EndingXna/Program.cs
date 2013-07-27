using System;

namespace EndingXna
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (XnaGame game = new XnaGame())
            {
                game.Run();
            }
        }
    }
#endif
}

