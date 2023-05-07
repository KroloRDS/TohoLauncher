using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TohoLauncher;
public class LaunchManager
{
	const string EOSD_KANJI = "東方紅魔郷";
	private readonly GlobalSettings? GlobalSettings;
	private readonly GameSettings GameSettings;
	private readonly string Game;

	public LaunchManager(GlobalSettings? globalSettings,
		GameSettings gameSettings)
	{
		GlobalSettings = globalSettings;
		GameSettings = gameSettings;
		Game = DetermineGame();
	}

	private string DetermineGame()
	{
		Console.Write("Checking game... ");
		var gamePath = GameSettings.GamePath ??
			throw new ArgumentNullException(
				$"No game path for {GameSettings.GameName}");

		var files = Directory.GetFiles(gamePath);
		var fileNames = files.Select(x => Path.GetFileName(x));
		var game = fileNames.FirstOrDefault(
			x => Regex.IsMatch(x, @"^th\d{2,3}(tr)?\.dat$"));

		if (game is null)
		{
			if (!fileNames.Any(x => x.StartsWith(EOSD_KANJI)))
				throw new FileNotFoundException("Not a valid Toho game directory");

			game = EOSD_KANJI;
		}
		else
			game = game[0..^4];

		Console.WriteLine("OK");
		return game;
	}

	public void Launch()
	{
		if (GameSettings.SaveInGameFolder ?? false) SetAppData();
		StartGame();
		if (GameSettings.UseThPrac ?? false) ApplyPrac();
	}

	private void StartGame()
	{
		if (GameSettings.UseThCrap ?? false)
		{
			LaunchThCrap();
			return;
		}

		if (GameSettings.UseVPatch ?? false)
		{
			LaunchExe("vpatch");
			return;
		}

		LaunchExe(Game);
	}

	private void LaunchThCrap()
	{
		Console.WriteLine("Launching thcrap...");

		CheckForPath(GlobalSettings?.ThCrapPath, "thcrap");
		if (string.IsNullOrEmpty(GameSettings.ThCrapArg))
		{
			throw new ArgumentNullException("Missing parameter for thcrap");
		}

		var arg = GameSettings.ThCrapArg + " ";
		arg += Game == EOSD_KANJI ? "th06" : Game;
		Process.Start(GlobalSettings!.ThCrapPath!, arg);
	}

	private void LaunchExe(string name)
	{
		var gamePath = GameSettings.GamePath ??
			throw new ArgumentNullException(
				$"No game path for {GameSettings.GameName}");

		var exeFile = $"{gamePath}\\{name}.exe";
		Console.WriteLine("Launching " + exeFile);

		if (!File.Exists(exeFile))
			throw new FileNotFoundException("Can't find " + exeFile);

		if (Game == EOSD_KANJI)
			TryLaunchTroughLocale(exeFile);
		else
			Process.Start(exeFile);
	}

	private void TryLaunchTroughLocale(string exe)
	{
		var localePath = GlobalSettings?.LocaleEmulatorPath;
		if (localePath is null)
			Process.Start(exe);
		else
			Process.Start(localePath, $"\"{exe}\"");
	}

	private static void CheckForPath(string? path, string name)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException("Missing path for " + name);
		}
	}

	private void ApplyPrac()
	{
		var pracPath = GlobalSettings?.ThPracPath;
		CheckForPath(pracPath, "thprac");

		Console.WriteLine("Applying thprac, waiting for the game to launch...");

		if (!WaitForGameLaunch(10))
		{
			Console.WriteLine("Looks like the game is not launching properly.");
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
			return;
		}

		if (Game == EOSD_KANJI)
			TryLaunchTroughLocale(pracPath!);
		else
			Process.Start(pracPath!);
	}

	private bool WaitForGameLaunch(int seconds)
	{
		var procName = Game.Replace("tr", string.Empty);
		for (int i = 0; i < seconds; i++)
		{
			Thread.Sleep(1000);
			if (Process.GetProcessesByName(procName).Any())
				return true;
			
			Console.WriteLine(".");
		}
		return false;
	}

	private static void SetAppData()
	{
		Console.WriteLine("Setting save location to game folder...");
		Environment.SetEnvironmentVariable(
			"APPDATA", Directory.GetCurrentDirectory() + "/save");
	}
}
