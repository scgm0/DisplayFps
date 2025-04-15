using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace DisplayFps;

public sealed class FpsText : HudElement {
	private const string SettingPrefix = "displayfps:Config.Setting.";
	private readonly GuiElementDynamicText _text;
	private double _time;
	private double _minTime;
	private double _maxTime;
	private int _fps;
	public Config Config { get; set; }
	public override bool PrefersUngrabbedMouse => false;
	public override bool Focusable => false;
	public override bool Focused => false;
	public override double DrawOrder => 0.5;

	public override bool CaptureAllInputs() => false;
	public override bool CaptureRawMouse() => false;
	public override bool ShouldReceiveKeyboardEvents() => false;
	public override bool ShouldReceiveMouseEvents() => false;

	public FpsText(ICoreClientAPI api) : base(api) {
		try {
			Config = api.LoadModConfig<Config?>("DisplayFps.json") ?? new();
		} catch {
			Config = new();
		}

		api.StoreModConfig(Config, "DisplayFps.json");

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
	}

	public override void OnFinalizeFrame(float dt) {
		base.OnFinalizeFrame(dt);
		UpdateFps(dt);
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
		SingleComposer.Api.StoreModConfig(Config, "DisplayFps.json");
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

	public void UpdateFps(float time) {
		if (_time == 0 || time < _minTime) {
			_minTime = time;
		}

		if (_time == 0 || time > _maxTime) {
			_maxTime = time;
		}

		_time += time;

		if (_time >= Config.Interval) {
			switch (Config.FpsType) {
				case FpsType.RealTime: {
					UpdateFps(
						$"{(int)(1 / time)} FPS{(Config.Detailed
							? $"  ( {Lang.Get(SettingPrefix + "RealTime", (int)(1 / _time * _fps), (int)(1 / _minTime), (int)(1 / _maxTime))} )"
							: string.Empty)}");
					break;
				}
				case FpsType.Average: {
					UpdateFps(
						$"{(int)(1 / _time * _fps)} FPS{(Config.Detailed
							? $"  ( {Lang.Get(SettingPrefix + "Average", (int)(1 / time), (int)(1 / _minTime), (int)(1 / _maxTime))} )"
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