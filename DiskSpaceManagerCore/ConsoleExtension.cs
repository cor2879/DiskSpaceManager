namespace DiskSpaceManager
{
    using System;

    public static class ConsoleExtension
    {
        public static void Pause()
        {
            Pause(Console.ForegroundColor);
        }

        public static void Pause(ConsoleColor textColor)
        {
            ConsoleExtension.WriteLine(textColor, "Press [Enter] to continue.");
            Console.ReadLine();
        }

        public static void WriteLine(ConsoleColor color, string text, params string[] args)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ForegroundColor = originalColor;
        }

        public static void Write(ConsoleColor color, string text, params string[] args)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text, args);
            Console.ForegroundColor = originalColor;
        }
    }
}