using System.Text.Json;
using TohoLauncher;

Settings allSettings = new();

Directory.SetCurrentDirectory("D:\\Toho\\TohoLauncher");

try
{
	ReadConfig();
	var game = SelectGame();
	var gameSettings = GetGameSettings(game);
	var manager = new LaunchManager(
		allSettings.GlobalSettings, gameSettings);

	
	manager.Launch();
}
catch (Exception ex)
{
	Console.WriteLine("ERROR");
	Console.WriteLine(ex.ToString());
	Console.ReadKey();
}

void ReadConfig()
{
	Console.Write("Reading config... ");

	var settingsFile = File.ReadAllText("TohoLauncherSettings.json");
	allSettings = JsonSerializer.Deserialize<Settings>(settingsFile) ??
		throw new Exception("Can't read config file");

	Console.WriteLine("OK");
}

string SelectGame()
{
	var gameNames = allSettings.GameSettings
		.Where(x => x.GameName is not null)
		.Select(x => x.GameName)
		.ToArray();
	if (!gameNames.Any()) throw new Exception("No games in config");

	return Menu.Select(gameNames!);
}

GameSettings GetGameSettings(string gameName)
{
	var globalSettings = allSettings.GlobalSettings;
	var gameSettings = allSettings.GameSettings
		.First(x => x.GameName == gameName);
	
	gameSettings.UseVPatch ??= globalSettings?.UseVPatch ?? false;
	gameSettings.UseThPrac ??= globalSettings?.UseThPrac ?? false;
	gameSettings.UseThCrap ??= globalSettings?.UseThCrap ?? false;
	gameSettings.ThCrapArg ??= globalSettings?.ThCrapArg;
	gameSettings.SaveInGameFolder ??= globalSettings?.SaveInGameFolder ?? false;

	return gameSettings;
}
