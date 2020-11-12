using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
		static IReadOnlyList<string> s_dependencies = new List<string>() {
			"JetBrains.Annotations.dll",
			"JetBrains.Lifetimes.dll",
			"JetBrains.Platform.Core.dll",
			"JetBrains.Platform.ComponentModel.dll",
			"JetBrains.Platform.DocumentModel.dll",
			"JetBrains.Platform.Metadata.dll",
			"JetBrains.Platform.ProjectModel.dll",
			"JetBrains.Platform.TextControl.dll",
			"JetBrains.Platform.Util.dll",
			"JetBrains.ReSharper.Psi.dll",
			"JetBrains.ReSharper.Psi.Xml.dll",
			"JetBrains.ReSharper.Resources.dll",
		};
		static void Main(string[] args)
		{
			if (args.Length < 1) PrintUsage();

			var dir = string.Join(' ', args);
			foreach (var dependency in s_dependencies)
				LoadAssembly(Path.Join(dir, dependency));

			var shell = LoadAssembly(Path.Join(dir, "JetBrains.Platform.Shell.dll"));
			var csharp = LoadAssembly(Path.Join(dir, "JetBrains.ReSharper.Psi.CSharp.dll"));

			var keyType = shell.GetType("JetBrains.Application.Settings.SettingsKeyAttribute");
			var entryType = shell.GetType("JetBrains.Application.Settings.SettingsEntryAttribute");
			if (keyType == null || entryType == null)
			{
				Console.Error.WriteLine("Unable to locate setting types in loaded assemblies");
				Environment.Exit(2);
			}
			var types = GetTypesWithAttribute(csharp, keyType);
			var sections = types.Select(t => new SettingsSection(t, keyType, entryType)).ToList();
			var json = AsJson(sections);
			Console.WriteLine(json);
		}

		static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attribute)
		{
			var result = new List<Type>();
			foreach (var type in assembly.GetTypes())
				try
				{
					if (type.IsDefined(attribute, true))
						result.Add(type);
				}
				catch (FileNotFoundException ex)
				{
					// skip WPF stuff
					if (!ex.Message.Contains("System.Xaml"))
						throw;
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Skipping {type.FullName}, reason: {ex}");
				}
			return result;
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

		public override string ToString() =>
			string.Format("[{0}] {1} - {2} = {3}{4}",
				Type,
				Name,
				Description,
				DefaultValue,
				IsObsolete ? " (obsolete)" : "");
	}

	public class SettingsSection
	{
		public string Description { get; private set; }
		public IReadOnlyList<string> Subtree { get; private set; }
		public SettingsData Settings { get; private set; }

		public SettingsSection(Type type, Type keyType, Type entryType)
		{
			var attr = type.GetCustomAttribute(keyType, true);
			Debug.Assert(attr != null);
			Description = keyType.GetProperty("Description")?.GetValue(attr) as string ?? type.Name;
			Settings = new SettingsData(type, entryType);
			var subtreeTypes = ResolveSettingsSubtree(type, keyType);
			Subtree = subtreeTypes.Reverse().Select(GetSettingsKeyName).ToList();
		}

		private string GetSettingsKeyName(Type type) =>
			type.Name.Replace("Settings", "").Replace("Key", "");

		private IReadOnlyList<Type> ResolveSettingsSubtree(Type item, Type keyType)
		{
			var result = new List<Type>() { item };
			do
			{
				var current = result.Last();
				if (!current.IsDefined(keyType, inherit: false))
					break;
				var attr = current.GetCustomAttribute(keyType, inherit: false);
				var parentType = keyType.GetProperty("Parent")?.GetValue(attr) as Type;
				if (parentType == null || parentType.Name.Contains("Missing"))
					break;
				result.Add(parentType);
			} while (true);
			return result;
		}

		public override string ToString() =>
			$"{Description} - {Settings.Items.Count} items ({string.Join('/', Subtree)})";
	}


	public class SettingsData
	{
		private readonly Dictionary<string, List<SettingValue>> _enums =
			new Dictionary<string, List<SettingValue>>();

		private readonly List<SettingItem> _items = new List<SettingItem>();

		public IReadOnlyDictionary<string, List<SettingValue>> Enumerations => _enums;
		public IReadOnlyList<SettingItem> Items => _items;

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
			var type = field.FieldType;
			var typeName = type.Name.Split('.').Last();
			if (!type.IsPrimitive && !type.IsEnum && type != typeof(string))
			{
				Console.Error.WriteLine($"{field.DeclaringType?.FullName}.{name} - unsupported field type {type}");
				return null;
			}
			var attr = field.GetCustomAttribute(settingsAttributeType);
			var isObsolete = field.IsDefined(typeof(ObsoleteAttribute), true);
			var description = (settingsAttributeType.GetProperty("Description")?.GetValue(attr) as string) ?? name;
			var defaultValue = settingsAttributeType.GetProperty("DefaultValue")?.GetValue(attr) ?? new object();
			if (type.IsEnum && defaultValue != null)
				defaultValue = Enum.GetName(type, defaultValue);
			return new SettingItem(name, description, typeName, isObsolete, defaultValue);
		}
	}
}
