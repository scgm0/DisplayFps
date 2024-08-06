using System;
using OpenTK.Windowing.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.Server;

namespace DisplayFps;

public sealed class FpsText : HudElement {
	private const string SettingPrefix = "displayfps:Config.Setting.";
	private readonly GuiElementDynamicText _text;
	private double _time;
	private double _minTime;
	private double _maxTime;
	private int _fps;
	public Config Config { get; set; }

	public FpsText(ICoreClientAPI api) : base(api) {
		try {
			Config = api.LoadModConfig<Config>("DisplayFps.json");
			if (Config == null) {
				Config = new();
				api.StoreModConfig(Config, "DisplayFps.json");
			}
		} catch {
			Config = new();
			api.StoreModConfig(Config, "DisplayFps.json");
		}

		var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(Config.Alignment)
			.WithFixedAlignmentOffset(Config.Offset.X, Config.Offset.Y).WithFixedPadding(10, 0);
		var textBounds = ElementBounds.FixedOffseted(EnumDialogArea.LeftMiddle, 0, 0, 50, 50);

		SingleComposer = api.Gui.CreateCompo("FpsDialog", dialogBounds)
			.AddDynamicText("",
				CairoFont.WhiteDetailText()
					.WithColor([1, 1, 1, 1])
					.WithFont(Config.FontName)
					.WithWeight(Config.FontWeight)
					.WithStroke([0, 0, 0, 1], 1.75)
					.WithFontSize(Config.FontSize),
				textBounds,
				"fps")
			.Compose();
		_text = SingleComposer.GetDynamicText("fps");
		api.Forms.Window.RenderFrame += UpdateFps;
	}

	public void UpdateConfig() {
		_text.Font.WithFont(Config.FontName)
			.WithFontSize(Config.FontSize)
			.WithWeight(Config.FontWeight);
		SingleComposer.Bounds.Alignment = Config.Alignment;
		SingleComposer.Bounds.WithFixedAlignmentOffset(Config.Offset.X, Config.Offset.Y);
		_text.Bounds.CalcWorldBounds();
		SingleComposer.Bounds.CalcWorldBounds();
	}

	public override void Dispose() {
		capi.Forms.Window.RenderFrame -= UpdateFps;
		TryClose();
		base.Dispose();
	}

	public void UpdateFps(string text) {
		_text.Text = text;
		_text.Font.AutoBoxSize(text, _text.Bounds);
		_text.Bounds.CalcWorldBounds();
		SingleComposer.Bounds.CalcWorldBounds();
		_text.RecomposeText(true);
	}

	public void UpdateFps(FrameEventArgs args) {
		if (_time == 0 || args.Time < _minTime) {
			_minTime = args.Time;
		}

		if (_time == 0 || args.Time > _maxTime) {
			_maxTime = args.Time;
		}

		_time += args.Time;

		if (_time >= Config.Interval) {
			switch (Config.FpsType) {
				case FpsType.RealTime: {
					UpdateFps(
						$"{(int)(1 / args.Time)} FPS{(Config.Detailed
							? $"  ( {Lang.Get(SettingPrefix + "RealTime", (int)(1 / (_time / _fps)), (int)(1 / _minTime), (int)(1 / _maxTime))} )"
							: string.Empty)}");
					break;
				}
				case FpsType.Average: {
					UpdateFps(
						$"{(int)(1 / (_time / _fps))} FPS{(Config.Detailed
							? $"  ( {Lang.Get(SettingPrefix + "Average", (int)(1 / args.Time), (int)(1 / _minTime), (int)(1 / _maxTime))} )"
							: string.Empty)}");
					break;
				}
				default: throw new ArgumentOutOfRangeException();
			}

			_time = 0;
			_fps = 0;
		}

		_fps++;
	}
}