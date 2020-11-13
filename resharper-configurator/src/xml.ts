import { jsToXml, xmlToString } from 'jxon';
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
		's:String': XmlOption[];
		's:Boolean': XmlOption[];
		's:Int64': XmlOption[];
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