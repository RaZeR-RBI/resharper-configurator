<template>
	<fieldset v-if="!isBoolean">
		<legend>
			<span>{{ description }}</span>
			<span class="item-name">&nbsp;({{ item.name }})</span>
		</legend>
		<input type="text" v-if="isString" :value="value" />
		<input type="number" v-else-if="isNumeric" :value="value" />
		<select v-else>
			<option
				v-for="e in enumValues"
				:key="e.value"
				:value="e.value"
			>{{ e.description }} ({{ e.value }})</option>
		</select>
	</fieldset>

	<fieldset v-else style="padding: 0">
		<label class="pure-checkbox pure-checkbox-slim">
			<input type="checkbox" :checked="value" />
			<span class="item-desc">&nbsp;{{ description }}</span>
			<span class="item-name">&nbsp;({{ item.name }})</span>
		</label>
	</fieldset>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { Item, Enumeration } from "../config-types";

@Component
export default class SectionList extends Vue {
	@Prop() private item!: Item;
	@Prop() private enums!: { [key: string]: Enumeration[] };
	value: boolean | number | string | null = null;

	mounted() {
		this.value = this.item.defaultValue;
	}

	get isNumeric() {
		return this.item.type.indexOf("Int") == 0;
	}

	get isString() {
		return this.item.type == "String";
	}

	get isBoolean() {
		return this.item.type == "Boolean";
	}

	get enumValues(): Enumeration[] {
		// if (this.item.type! in this.enums) return [];
		return this.enums[this.item.type];
	}

	get prefixes(): string[] {
		return ["in", "for"];
	}

	get description(): string {
		const wordsInternal = this.item.name.toLowerCase().split("_");
		const wordsVisible = this.item.description.toLowerCase().split(" ");
		let index = wordsInternal.indexOf(wordsVisible[0]);
		const hasPrefix = this.prefixes.indexOf(wordsVisible[0]) >= 0;
		if (hasPrefix) {
			for (const prefix of this.prefixes) {
				const prefixIndex = wordsInternal.indexOf(prefix);
				if (prefixIndex >= 0) {
					index = prefixIndex;
					break;
				}
			}
		}
		if (index > 0) {
			const prefix = this.capitalize(
				wordsInternal.slice(0, index).join(" ") + " "
			);
			const tail = this.item.description;
			return prefix + this.uncapitalize(tail);
		} else if (index < 0 && wordsInternal.length > 1) {
			return this.capitalize(
				this.item.name
					.split("_")
					.join(" ")
					.toLowerCase()
			);
		}
		return this.item.description;
	}

	capitalize(input: string): string {
		return input.charAt(0).toUpperCase() + input.slice(1);
	}

	uncapitalize(input: string): string {
		if (input.length > 1 && input.charAt(1) == input.charAt(1).toUpperCase()) {
			return input;
		}
		return input.charAt(0).toLowerCase() + input.slice(1);
	}
}
</script>

<style>
.item-name {
	color: #bbb;
	font-size: 0.75rem;
}

.pure-form input[type="text"] {
	width: 100%;
}

.pure-form select {
	height: unset;
}

.pure-form input,
.pure-form select {
	padding: 0.1rem;
}

.pure-checkbox {
	margin: 0.25em 0;
}
</style>