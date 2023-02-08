namespace TohoLauncher;

public class Menu
{
	public static string Select(string[] options)
	{
		if (!options.Any())
			throw new ArgumentException("No options to select from");

		int position = 0;
		var length = options.Length;

		while (true)
		{
			Draw(options, position);
			var key = Console.ReadKey().Key;
			switch (key)
			{
				case ConsoleKey.Escape:
					Console.Clear();
					return string.Empty;
				case ConsoleKey.Z:
				case ConsoleKey.Enter:
					Console.Clear();
					return options[position];
				case ConsoleKey.UpArrow:
					position = position - 1 < 0 ? length - 1 : position - 1;
					break;
				case ConsoleKey.DownArrow:
					position = position + 1 >= length ? 0 : position + 1;
					break;
				default:
					break;
			}
		}
	}

	private static void Draw(string[] options, int position)
	{
		Console.Clear();
		for (int i = 0; i < options.Length; i++)
		{
			if (i == position)
			{
				Console.BackgroundColor = ConsoleColor.White;
				Console.ForegroundColor = ConsoleColor.Black;
			}
			Console.WriteLine(options[i]);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
