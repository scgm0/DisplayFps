using Cairo;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace DisplayFps;

public class Config {
	public string FontName { get; set; } = ClientSettings.DefaultFontName;

	[JsonConverter(typeof(StringEnumConverter))]
	public FontWeight FontWeight { get; set; } = FontWeight.Bold;

	public int FontSize { get; set; } = 18;

	[JsonConverter(typeof(StringEnumConverter))]
	public EnumDialogArea Alignment { get; set; } = EnumDialogArea.LeftTop;

	public double Interval { get; set; } = 0.5;
	public FpsType FpsType { get; set; } = FpsType.Average;
	public bool Detailed { get; set; } = false;
	public Vec2i Offset { get; init; } = Vec2i.Zero;
}