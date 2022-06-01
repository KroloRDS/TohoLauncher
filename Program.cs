using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using TohoLauncher;

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
	var game = files.Select(x => Path.GetFileName(x))
		.FirstOrDefault(x => Regex.IsMatch(x, @"^th\d{2,3}\.dat$"));

	if (game is null)
		throw new FileNotFoundException("Not a valid Toho game directory");

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

	CheckForPath(settings!.ThCrapPath, "thcrap");
	if (string.IsNullOrEmpty(settings.ThCrapArg))
	{
		throw new ArgumentNullException("Missing parameter for thcrap");
	}

	Process.Start(settings.ThCrapPath!, settings.ThCrapArg + " " + game);
}

void LaunchExe(string name)
{
	name += ".exe";
	Console.WriteLine("Launching " + name);

	if (!File.Exists(name))
		throw new FileNotFoundException("Can't find " + name);
	Process.Start(name);
	return;
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
	Console.WriteLine("Applying thprac, waiting for the game to launch...");
	CheckForPath(settings.ThPracPath, "thprac");

	while (true)
	{
		Thread.Sleep(500);
		if (Process.GetProcessesByName(game).Any())
		{
			Thread.Sleep(1000);
			Process.Start(settings!.ThPracPath!);
			return;
		}
	}
}
