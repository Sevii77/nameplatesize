using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Newtonsoft.Json;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Bindings.ImGui;

namespace NameplateSize;

public class NameplateSize: IDalamudPlugin {
	[PluginService] public static IDalamudPluginInterface Interface   {get; private set;} = null!;
	[PluginService] public static ICommandManager         Commands    {get; private set;} = null!;
	[PluginService] public static ISigScanner             SigScanner  {get; private set;} = null!;
	[PluginService] public static IDataManager            DataManager {get; private set;} = null!;
	[PluginService] public static IGameGui                GameGui     {get; private set;} = null!;
	[PluginService] public static IGameInteropProvider    HookProv    {get; private set;} = null!;
	[PluginService] public static IPluginLog              Logger      {get; private set;} = null!;
	
	public string Name => "Nameplate Size";
	private const string command = "/nameplatesize";
	
	private Config config;
	private bool drawWindow = false;
	
	private class Config {
		public int Scale = 100;
		
		public static Config Load() {
			return Interface.ConfigFile.Exists ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(Interface.ConfigFile.FullName)) ?? new() : new();
		}
		
		public void Save() {
			File.WriteAllText(Interface.ConfigFile.FullName, JsonConvert.SerializeObject(this));
		}
	}
	
	public unsafe NameplateSize() {
		config = Config.Load();
		
		Interface.UiBuilder.Draw += Draw;
		Interface.UiBuilder.OpenConfigUi += OpenConf;
		
		Commands.AddHandler(command, new CommandInfo(OnCommand) {
			ShowInHelp = true,
			HelpMessage = "Open the config",
		});
		
		CalculateNamePlateScaleHook = HookProv.HookFromAddress<CalculateNamePlateScaleDelegate>(SigScanner.ScanText("E8 ?? ?? ?? ?? 88 47 ?? 84 C0"), CalculateNamePlateScale);
		CalculateNamePlateScaleHook.Enable();
		
		TestHook = HookProv.HookFromAddress<TestDelegate>(SigScanner.ScanText("40 53 55 56 57 41 56 48 83 EC ?? 44 8B 81"), Test);
		TestHook.Enable();
	}
	
	public void Dispose() {
		Interface.UiBuilder.Draw -= Draw;
		Interface.UiBuilder.OpenConfigUi -= OpenConf;
		Commands.RemoveHandler(command);
		CalculateNamePlateScaleHook.Dispose();
		TestHook.Dispose();
	}
	
	private void OnCommand(string cmd, string args) {
		if(cmd != command)
			return;
		
		OpenConf();
	}
	
	private void OpenConf() {
		drawWindow = !drawWindow;
	}
	
	private void Draw() {
		if(!drawWindow)
			return;
		
		ImGui.Begin("Nameplate Size Config", ref drawWindow, ImGuiWindowFlags.AlwaysAutoResize);
		
		if(ImGui.SliderInt("Size %", ref config.Scale, 1, 500, $"{config.Scale}%")) {
			config.Save();
		}
		
		ImGui.End();
	}
	
	private unsafe delegate int CalculateNamePlateScaleDelegate(float distance);
	private Hook<CalculateNamePlateScaleDelegate> CalculateNamePlateScaleHook;
	private int CalculateNamePlateScale(float distance) {
		// return config.Scale;
		return 100;
	}
	
	private unsafe delegate void TestDelegate(nint a);
	private Hook<TestDelegate> TestHook;
	private unsafe void Test(nint a) {
		TestHook.Original(a);
		
		var man = AtkStage.Instance()->RaptureAtkUnitManager;
		var addon = man->GetAddonByName("NamePlate");
		if(addon == null)
			return;
		
		var uld = addon->UldManager;
		if(addon == null)
			return;
		
		var root = uld.NodeList[0];
		if(root == null)
			return;
		
		var scale = (float)config.Scale / 100.0f;
		root->ScaleX = scale;
		root->ScaleY = scale;
	}
}