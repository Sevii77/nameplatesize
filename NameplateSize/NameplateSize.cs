using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

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
	
	public unsafe NameplateSize() {
		CalculateNamePlateScaleHook = HookProv.HookFromAddress<CalculateNamePlateScaleDelegate>(SigScanner.ScanText("E8 ?? ?? ?? ?? 88 47 ?? 84 C0"), CalculateNamePlateScale);
		CalculateNamePlateScaleHook.Enable();
	}
	
	public void Dispose() {
		CalculateNamePlateScaleHook.Dispose();
	}
	
	private unsafe delegate int CalculateNamePlateScaleDelegate(float distance);
	private Hook<CalculateNamePlateScaleDelegate> CalculateNamePlateScaleHook;
	private int CalculateNamePlateScale(float distance) {
		return 100;
	}
}