using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using TohoLauncher;

const string EOSD_KANJI = "東方紅魔郷";
var settings = new LauncherSettings();
var game = "";

try
{
	settings = ReadConfig();
	game = DetermineGame();

	SetAppData();
	Launch();
	if (settings.UseThPrac) ApplyPrac();
}
catch (Exception ex)
{
	Console.WriteLine("ERROR");
	Console.WriteLine(ex.ToString());
	Console.ReadKey();
}

LauncherSettings ReadConfig()
{
	Console.Write("Reading config... ");

	var settingsFile = File.ReadAllText("TohoLauncherSettings.json");
	var settings = JsonSerializer.Deserialize<LauncherSettings>(settingsFile);
	if (settings is null)
	{
		throw new Exception("Can't read config file");
	}

	Console.WriteLine("OK");
	return settings;
}

string DetermineGame()
{
	Console.Write("Checking game... ");

	var files = Directory.GetFiles(Directory.GetCurrentDirectory());
	var fileNames = files.Select(x => Path.GetFileName(x));
	var game = fileNames.FirstOrDefault(
		x => Regex.IsMatch(x, @"^th\d{2,3}\.dat$"));

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


void SetAppData()
{
	if (!settings.SaveInGameFolder) return;

	Console.WriteLine("Setting save location to game folder...");
	Environment.SetEnvironmentVariable(
		"APPDATA", Directory.GetCurrentDirectory() + "/save");
}

void Launch()
{
	if (settings.UseThCrap)
	{
		LaunchThCrap();
		return;
	}

	if (settings.UseVPatch)
	{
		LaunchExe("vpatch");
		return;
	}

	LaunchExe(game);
}

void LaunchThCrap()
{
	Console.WriteLine("Launching thcrap...");

	CheckForPath(settings?.ThCrapPath, "thcrap");
	if (string.IsNullOrEmpty(settings?.ThCrapArg))
	{
		throw new ArgumentNullException("Missing parameter for thcrap");
	}

	var arg = settings.ThCrapArg + " ";
	arg += game == EOSD_KANJI ? "th06" : game;
	Process.Start(settings.ThCrapPath!, arg);
}

void LaunchExe(string name)
{
	name += ".exe";
	Console.WriteLine("Launching " + name);

	if (!File.Exists(name))
		throw new FileNotFoundException("Can't find " + name);
	
	if (game == EOSD_KANJI)
		TryLaunchTroughLocale(name);
	else
		Process.Start(name);
}

void TryLaunchTroughLocale(string exe)
{
	var localePath = settings?.LocaleEmulatorPath;
	if (localePath is null)
		Process.Start(exe);
	else
		Process.Start(localePath, exe);
}

void CheckForPath(string? path, string name)
{
	if (string.IsNullOrEmpty(path))
	{
		throw new ArgumentNullException("Missing path for " + name);
	}
}

void ApplyPrac()
{
	var pracPath = settings?.ThPracPath;
	CheckForPath(pracPath, "thprac");

	Console.WriteLine("Applying thprac, waiting for the game to launch...");
	
	if (!WaitForGameLaunch(10))
	{
		Console.WriteLine("Looks like the game is not launching properly.");
		Console.WriteLine("Press any key to exit.");
		Console.ReadKey();
		return;
	}

	if (game == EOSD_KANJI)
		TryLaunchTroughLocale(pracPath!);
	else
		Process.Start(pracPath!);
}

bool WaitForGameLaunch(int seconds)
{
	for (int i = 0; i < seconds; i++)
	{
		if (Process.GetProcessesByName(game).Any())
		{
			Thread.Sleep(1000);
			return true;
		}

		Thread.Sleep(1000);
		Console.WriteLine(".");
	}
	return false;
}
