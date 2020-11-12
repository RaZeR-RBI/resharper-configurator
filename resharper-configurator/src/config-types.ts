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
	defaultValue: ConfigValue
}

export type ConfigValue = boolean | number | string | null;

export interface ChangeEvent {
	sectionId: number,
	optionId: number,
	value: ConfigValue
}