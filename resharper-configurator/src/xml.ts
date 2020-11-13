import { jsToXml, xmlToString, stringToXml, xmlToJs } from 'jxon';
import { Section, ConfigValue, isNumeric, isBoolean } from './config-types';

interface XmlOption {
	'$x:Key': string;
	'_': ConfigValue;
}

interface XmlConfig {
	'wpf:ResourceDictionary': {
		'$xml:space': string;
		'$xmlns:wpf': string;
		'$xmlns:x': string;
		'$xmlns:s': string;
		'$xmlns:ss': string;
		's:String'?: XmlOption[] | XmlOption;
		's:Boolean'?: XmlOption[] | XmlOption;
		's:Int64'?: XmlOption[] | XmlOption;
	};
}

export function toXml(sections: Section[], values: Map<number, Map<number, ConfigValue>>): string | null {
	const cfg: XmlConfig = {
		'wpf:ResourceDictionary': {
			'$xml:space': "preserve",
			'$xmlns:wpf': "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
			'$xmlns:x': "http://schemas.microsoft.com/winfx/2006/xaml",
			'$xmlns:s': "clr-namespace:System;assembly=mscorlib",
			'$xmlns:ss': "urn:shemas-jetbrains-com:settings-storage-xaml",
			's:String': [],
			's:Boolean': [],
			's:Int64': []
		}
	};
	const strings: XmlOption[] = [];
	const numbers: XmlOption[] = [];
	const booleans: XmlOption[] = [];

	for (const [sectionId, options] of values) {
		const section = sections[sectionId];
		for (const [optionId, value] of options) {
			const option = section.settings.items[optionId];
			const key = "/" + ["Default", ...section.subtree, option.name, "@EntryValue"].join("/");
			const target = isNumeric(option) ? numbers : isBoolean(option) ? booleans : strings;
			const xmlValue = isBoolean(option) ? '' + value : value;
			target.push({ '$x:Key': key, '_': xmlValue });
		}
	}

	cfg['wpf:ResourceDictionary']["s:String"] = strings;
	cfg['wpf:ResourceDictionary']["s:Boolean"] = booleans;
	cfg['wpf:ResourceDictionary']["s:Int64"] = numbers;

	try {
		const xmlDoc = jsToXml(cfg);
		return '<?xml version="1.0" encoding="utf-8"?>' + xmlToString(xmlDoc);
	} catch (error) {
		console.error(error);
		return null;
	}
}

function findSectionId(key: string, sections: Section[]) {
	const subtree = key.slice(1, key.length - '/@EntryValue'.length).split('/').slice(1, -1);
	const expected = JSON.stringify(subtree);
	return sections.findIndex(function (section, i, a) {
		return JSON.stringify(section.subtree) == expected;
	});
}

function findOptionId(key: string, section: Section) {
	const name = key.slice(1, key.length - '/@EntryValue'.length).split('/').pop();
	return section.settings.items.findIndex(function (item, i, a) {
		return item.name == name;
	});
}

export function fromXml(sections: Section[], input: string): Map<number, Map<number, ConfigValue>> {
	const result = new Map<number, Map<number, ConfigValue>>();
	try {
		const xml = stringToXml(input);
		const obj = xmlToJs(xml) as XmlConfig;
		const root = obj["wpf:ResourceDictionary"];
		const allItems = ([root["s:String"] || []]).concat([root["s:Boolean"] || []]).concat([root["s:Int64"] || []]).flat();

		let hasErrors = false;
		for (const option of allItems) {
			const key = option["$x:Key"];
			const sectionId = findSectionId(key, sections);
			if (sectionId < 0) {
				console.warn("Unable to find section for '" + key + "'");
				hasErrors = true;
				continue;
			}
			const optionId = findOptionId(key, sections[sectionId]);
			if (optionId < 0) {
				console.warn("Unable to find option for '" + key + "'");
				hasErrors = true;
				continue;
			}
			if (!result.has(sectionId)) {
				result.set(sectionId, new Map());
			}
			const item = sections[sectionId].settings.items[optionId];
			const foundValue = '' + option._;
			const value: ConfigValue = isBoolean(item) ? foundValue == 'true' :
				isNumeric(item) ? parseInt(foundValue) : foundValue;
			result.get(sectionId)!.set(optionId, value);
		}
		if (hasErrors) {
			alert('Could not successfully import all settings - check the console');
		}
	} catch (error) {
		console.error(error);
		alert(error);
	}
	return result;
}