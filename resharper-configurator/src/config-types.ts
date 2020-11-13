export interface Section {
	description: string;
	subtree: string[];
	settings: Settings;
}

export interface Settings {
	enumerations: { [key: string]: Enumeration[] };
	items: Item[];
}

export interface Enumeration {
	value: string;
	description: string;
}

export interface Item {
	name: string;
	description: string;
	type: string;
	isObsolete: boolean;
	defaultValue: ConfigValue;
}

export type ConfigValue = boolean | number | string | null;

export interface ChangeEvent {
	sectionId: number;
	optionId: number;
	value: ConfigValue;
}

function capitalize(input: string): string {
	return input.charAt(0).toUpperCase() + input.slice(1);
}

function uncapitalize(input: string): string {
	if (input.length > 1 && input.charAt(1) == input.charAt(1).toUpperCase()) {
		return input;
	}
	return input.charAt(0).toLowerCase() + input.slice(1);
}

function removePrefix(input: string, prefix: string) {
	if (input.indexOf(prefix) != 0) {
		return input;
	}
	return input.slice(prefix.length);
}

export function formatOption(item: Item): string {
	const name = removePrefix(item.name, "INT_");
	const wordsInternal = name.toLowerCase().split("_");
	const wordsVisible = item.description.toLowerCase().split(" ");
	const prefixes = ["in", "for"];
	let index = wordsInternal.indexOf(wordsVisible[0]);
	const hasPrefix = prefixes.indexOf(wordsVisible[0]) >= 0;
	if (hasPrefix) {
		for (const prefix of prefixes) {
			const prefixIndex = wordsInternal.indexOf(prefix);
			if (prefixIndex >= 0) {
				index = prefixIndex;
				break;
			}
		}
	}
	if (index > 0) {
		const prefix = capitalize(
			wordsInternal.slice(0, index).join(" ") + " "
		);
		const tail = item.description;
		return prefix + uncapitalize(tail);
	} else if (index < 0 && wordsInternal.length > 1) {
		return capitalize(
			name
				.split("_")
				.join(" ")
				.toLowerCase()
		);
	}
	return item.description;
}