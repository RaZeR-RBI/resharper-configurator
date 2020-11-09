using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Extractor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1) PrintUsage();

			var dir = string.Join(' ', args);
			LoadAssembly(Path.Join(dir, "JetBrains.Platform.Core.dll"));
			LoadAssembly(Path.Join(dir, "JetBrains.Platform.ComponentModel.dll"));
			var shell = LoadAssembly(Path.Join(dir, "JetBrains.Platform.Shell.dll"));
			LoadAssembly(Path.Join(dir, "JetBrains.ReSharper.Psi.dll"));
			var csharp = LoadAssembly(Path.Join(dir, "JetBrains.ReSharper.Psi.CSharp.dll"));

			var settingsTypeName = "JetBrains.ReSharper.Psi.CSharp.CodeStyle.FormatSettings.CSharpFormatSettingsKey";
			var settingsType = csharp.GetType(settingsTypeName);
			var settingAttributeType = shell.GetType("JetBrains.Application.Settings.SettingsEntryAttribute");
			if (settingsType == null || settingAttributeType == null)
			{
				Console.WriteLine("Unable to locate setting types in loaded assemblies");
				Environment.Exit(2);
			}
			var settingsData = new SettingsData(settingsType, settingAttributeType);
			var json = AsJson(settingsData);
			Console.WriteLine(json);
		}

		static string AsJson(object? data) =>
			JsonConvert.SerializeObject(data, new JsonSerializerSettings()
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.Indented
			});

		static void PrintUsage()
		{
			Console.WriteLine("Please specify folder where ReSharper assemblies are located.");
			Environment.Exit(1);
		}

		static Assembly LoadAssembly(string name) =>
			AssemblyLoadContext.Default.LoadFromAssemblyPath(name);
	}

	public readonly struct SettingValue
	{
		public readonly string Value, Description;

		public SettingValue(string value, string description)
		{
			Value = value;
			Description = description;
		}

		public override string ToString() => $"{Value} - {Description}";
	}

	public readonly struct SettingItem
	{
		public readonly string Name, Description, Type;
		public readonly bool IsObsolete;
		public readonly object? DefaultValue;

		public SettingItem(
			string name,
			string description,
			string type,
			bool isObsolete,
			object? defaultValue)
		{
			Name = name;
			Description = description;
			Type = type;
			IsObsolete = isObsolete;
			DefaultValue = defaultValue;
		}
	}

	public class SettingsData
	{
		private readonly Dictionary<string, List<SettingValue>> _enums =
			new Dictionary<string, List<SettingValue>>();

		private readonly List<SettingItem> _items = new List<SettingItem>();

		public IReadOnlyDictionary<string, List<SettingValue>> Enumerations => _enums;
		public IEnumerable<SettingItem> Items => _items;

		public SettingsData(Type type, Type settingAttributeType)
		{
			var flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
			var fields = type.GetFields(flags)
				.Where(t => t.IsDefined(settingAttributeType, true))
				.OrderBy(t => t.Name)
				.ToList();

			var enums = fields
				.Select(f => f.FieldType)
				.Where(t => t.IsEnum)
				.Distinct()
				.OrderBy(t => t.Name)
				.ToList();

			foreach (var e in enums)
			{
				var name = e.Name;
				var values = e.GetMembers(BindingFlags.Public | BindingFlags.Static)
					.Select(m => new SettingValue(m.Name, GetDescription(m) ?? m.Name))
					.ToList();
				_enums.Add(name, values);
			}

			foreach (var f in fields)
			{
				var item = LoadSettingItem(f, settingAttributeType);
				if (item != null)
					_items.Add(item.Value);
			}
		}

		private string? GetDescription(MemberInfo member)
		{
			var attr = typeof(DescriptionAttribute);
			if (!member.IsDefined(attr))
				return null;
			return (member.GetCustomAttribute(attr) as DescriptionAttribute)?.Description;
		}

		private SettingItem? LoadSettingItem(FieldInfo field, Type settingsAttributeType)
		{
			var name = field.Name;
			var typeName = field.FieldType.Name;
			if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
			{
				Console.Error.WriteLine("{name} - unsupported field type");
				return null;
			}
			var attr = field.GetCustomAttribute(settingsAttributeType);
			var isObsolete = field.IsDefined(typeof(ObsoleteAttribute), true);
			var description = (settingsAttributeType.GetProperty("Description")?.GetValue(attr) as string) ?? name;
			var defaultValue = settingsAttributeType.GetProperty("DefaultValue")?.GetValue(attr) ?? new object();
			if (field.FieldType.IsEnum && defaultValue != null)
				defaultValue = Enum.GetName(field.FieldType, defaultValue);
			return new SettingItem(name, description, typeName, isObsolete, defaultValue);
		}
	}
}
