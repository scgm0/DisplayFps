using System;
using System.Linq;
using ConfigLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DisplayFps;

public class ConfigLibCompat {
	private const string SettingPrefix = "displayfps:Config.Setting.";
	private ICoreAPI _api;
	private FpsText _fpsText;

	public ConfigLibCompat(ICoreAPI api, FpsText fpsText) {
		_api = api;
		_fpsText = fpsText;
		api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Lang.Get("displayfps:displayfps_setting"),
			(id, buttons) => EditConfigClient(id, buttons, api));
	}

	private void EditConfigClient(string id, ConfigLib.ControlButtons buttons, ICoreAPI api) {
		if (buttons.Save) api.StoreModConfig(_fpsText.Config, "DisplayFps.json");
		if (buttons.Restore) api.LoadModConfig<Config>("DisplayFps.json");
		if (buttons.Defaults) _fpsText.Config = new();
		_fpsText.Config.FontName = OnInputText(id, _fpsText.Config.FontName, nameof(_fpsText.Config.FontName));
		_fpsText.Config.FontWeight = OnInputEnum(id, _fpsText.Config.FontWeight, nameof(_fpsText.Config.FontWeight));
		_fpsText.Config.FontSize = OnInputInt(id, _fpsText.Config.FontSize, nameof(_fpsText.Config.FontSize));
		_fpsText.Config.Alignment = OnInputEnum(id, _fpsText.Config.Alignment, nameof(_fpsText.Config.Alignment));
		_fpsText.Config.Interval = OnInputDouble(id, _fpsText.Config.Interval, nameof(_fpsText.Config.Interval));
		_fpsText.Config.FpsType = OnInputEnum(id, _fpsText.Config.FpsType, nameof(_fpsText.Config.FpsType));
		_fpsText.Config.Detailed = OnInputBool(id, _fpsText.Config.Detailed, nameof(_fpsText.Config.Detailed));
		OnInputVec2i(id, _fpsText.Config.Offset, nameof(_fpsText.Config.Offset));
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