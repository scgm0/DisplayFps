#nullable enable
using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo(name: "帧率显示", modID: "displayfps", Authors = ["神麤詭末"], Description = "显示游戏帧率", Side = "Client")]

namespace DisplayFps;

public class DisplayFpsModSystem : ModSystem {
	private FpsText? _fpsText = null;
	public override bool ShouldLoad(EnumAppSide forSide) { return forSide == EnumAppSide.Client; }

	public override void StartClientSide(ICoreClientAPI api) {
		base.StartClientSide(api);
		_fpsText ??= new(api);
		if (api.ModLoader.IsModEnabled("configlib")) {
			_ = new ConfigLibCompat(api, _fpsText);
		}

		api.Event.LevelFinalize += () => _fpsText?.TryOpen();
		api.Event.LeaveWorld += () => _fpsText?.TryClose();
	}

	public override void Dispose() {
		_fpsText?.Dispose();
		base.Dispose();
	}
}