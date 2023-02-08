namespace TohoLauncher;

public class Settings
{
	public GlobalSettings? GlobalSettings { get; set; }
	public IEnumerable<GameSettings> GameSettings { get; set; } = 
		Enumerable.Empty<GameSettings>();
}

public class GlobalSettings
{
	public string? LocaleEmulatorPath { get; set; }
	public string? ThPracPath { get; set; }
	public string? ThCrapPath { get; set; }

	public bool UseVPatch { get; set; }
	public bool UseThPrac { get; set; }
	public bool UseThCrap { get; set; }
	public string? ThCrapArg { get; set; }
	public bool SaveInGameFolder { get; set; }
}

public class GameSettings
{
	public string? GamePath { get; set; }
	public string? GameName { get; set; }

	public bool? UseVPatch { get; set; }
	public bool? UseThPrac { get; set; }
	public bool? UseThCrap { get; set; }
	public string? ThCrapArg { get; set; }
	public bool? SaveInGameFolder { get; set; }
	
}
