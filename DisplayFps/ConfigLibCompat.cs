using System;
using System.Linq;
using ConfigLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DisplayFps;

public class ConfigLibCompat {
	private const string SettingPrefix = "displayfps:Config.Setting.";
	private readonly FpsText _fpsText;

	public ConfigLibCompat(ICoreAPI api, FpsText fpsText) {
		_fpsText = fpsText;
		api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Lang.Get("displayfps:displayfps_setting"),
			(id, buttons) => EditConfigClient(id, buttons, api));
	}

	private void EditConfigClient(string id, ControlButtons buttons, ICoreAPI api) {
		
		if (buttons.Save) api.StoreModConfig(_fpsText.Config, "DisplayFps.json");
		if (buttons.Restore) api.LoadModConfig<Config>("DisplayFps.json");
		if (buttons.Defaults) _fpsText.Config = new();
		var config = _fpsText.Config;
		config.FontName = OnInputText(id, config.FontName, nameof(config.FontName));
		config.FontWeight = OnInputEnum(id, config.FontWeight, nameof(config.FontWeight));
		config.FontSize = OnInputInt(id, config.FontSize, nameof(config.FontSize));
		config.Alignment = OnInputEnum(id, config.Alignment, nameof(config.Alignment));
		config.Interval = OnInputDouble(id, config.Interval, nameof(config.Interval));
		config.FpsType = OnInputEnum(id, config.FpsType, nameof(config.FpsType));
		config.Detailed = OnInputBool(id, config.Detailed, nameof(config.Detailed));
		OnInputVec2i(id, config.Offset, nameof(config.Offset));
		_fpsText.Config = config;
		_fpsText.UpdateConfig();
	}

	static private string OnInputText(string id, string value, string name) {
		var newValue = value;
		ImGuiNET.ImGui.Text(Lang.Get(SettingPrefix + name));
		ImGuiNET.ImGui.SameLine();
		ImGuiNET.ImGui.InputText($"##{name}-{id}", ref newValue, 64);
		return newValue;
	}

	static private bool OnInputBool(string id, bool value, string name) {
		var newValue = value;
		ImGuiNET.ImGui.Text(Lang.Get(SettingPrefix + name));
		ImGuiNET.ImGui.SameLine();
		ImGuiNET.ImGui.Checkbox($"##{name}-{id}", ref newValue);
		return newValue;
	}

	static private int OnInputInt(string id, int value, string name, int minValue = default) {
		var newValue = value;
		ImGuiNET.ImGui.Text(Lang.Get(SettingPrefix + name));
		ImGuiNET.ImGui.SameLine();
		ImGuiNET.ImGui.InputInt($"##{name}-{id}", ref newValue, step: 1, step_fast: 10);
		return newValue < minValue ? minValue : newValue;
	}

	static private double OnInputDouble(string id, double value, string name, double minValue = default) {
		var newValue = value;
		ImGuiNET.ImGui.Text(Lang.Get(SettingPrefix + name));
		ImGuiNET.ImGui.SameLine();
		ImGuiNET.ImGui.InputDouble($"##{name}-{id}", ref newValue, step: 0.5, step_fast: 1);
		return newValue < minValue ? minValue : newValue;
	}

	static private T OnInputEnum<T>(string id, T value, string name) where T : Enum {
		var enumNames = Enum.GetNames(typeof(T));
		var index = Array.IndexOf(enumNames, value.ToString());

		ImGuiNET.ImGui.Text(Lang.Get(SettingPrefix + name));
		ImGuiNET.ImGui.SameLine();
		if (ImGuiNET.ImGui.Combo($"##{name}-{id}",
			ref index,
			enumNames.Select(
				e => Lang.HasTranslation($"{SettingPrefix}{name}.{e}") ? Lang.Get($"{SettingPrefix}{name}.{e}") : e).ToArray(),
			enumNames.Length)) {
			value = (T)Enum.Parse(typeof(T), enumNames[index]);
		}

		return value;
	}

	static private void OnInputVec2i(string id, Vec2i vec2I, string name) {
		if (!ImGuiNET.ImGui.CollapsingHeader(Lang.Get(SettingPrefix + name) + $"##vec2-{id}")) return;
		ImGuiNET.ImGui.Indent();
		foreach (var pair in ((string, int)[]) [("X", 0), ("Y", 1)]) {
			var value = vec2I[pair.Item2];
			ImGuiNET.ImGui.Text(pair.Item1);
			ImGuiNET.ImGui.SameLine();
			ImGuiNET.ImGui.InputInt($"##{pair.Item1}", ref value, step: 1, step_fast: 10);
			vec2I[pair.Item2] = value;
		}

		ImGuiNET.ImGui.Unindent();
	}
}