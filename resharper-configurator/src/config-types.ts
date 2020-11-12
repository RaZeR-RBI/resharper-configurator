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
	defaultValue: boolean | number | null | string;
}
