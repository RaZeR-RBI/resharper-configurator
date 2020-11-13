<template>
	<fieldset v-if="!isBoolean" :class="{'changed': isChanged}">
		<legend>
			<span>{{ description }}</span>
			<span class="item-name">&nbsp;({{ item.name }})&nbsp;</span>
			<a href="#" v-if="isChanged" @click="reset()">
				<small>Reset</small>
			</a>
		</legend>
		<input type="text" v-if="isString" v-model="value" />
		<input type="number" v-else-if="isNumeric" v-model="value" />
		<select v-else v-model="value">
			<option
				v-for="e in enumValues"
				:key="e.value"
				:value="e.value"
			>{{ e.description }} ({{ e.value }})</option>
		</select>
	</fieldset>

	<fieldset v-else style="padding: 0" :class="{'changed': isChanged}">
		<label class="pure-checkbox pure-checkbox-slim">
			<input type="checkbox" v-model="value" />
			<span class="item-desc">&nbsp;{{ description }}</span>
			<span class="item-name">&nbsp;({{ item.name }})</span>
		</label>
	</fieldset>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from "vue-property-decorator";

import {
	Item,
	Enumeration,
	ConfigValue,
	ChangeEvent,
	formatOption,
	isNumeric,
	isString,
	isBoolean
} from "../config-types";

@Component
export default class EditorOption extends Vue {
	@Prop() private currentOptions!: Map<number, Map<number, ConfigValue>>;
	@Prop() private sectionId!: number;
	@Prop() private optionId!: number;
	@Prop() private item!: Item;
	@Prop() private enums!: { [key: string]: Enumeration[] };
	private value: ConfigValue = null;

	mounted() {
		if (
			!this.currentOptions.has(this.sectionId) ||
			(!this.currentOptions.get(this.sectionId)?.has(this.optionId) ?? true)
		) {
			this.reset();
			return;
		}
		this.value =
			this.currentOptions.get(this.sectionId)?.get(this.optionId) ??
			this.item.defaultValue;
	}

	@Watch("value")
	onValueChanged(val: ConfigValue) {
		if (this.isString && val == "" && this.item.defaultValue == null) {
			this.value = null;
			return;
		}
		const result: ChangeEvent = {
			sectionId: this.sectionId,
			optionId: this.optionId,
			value: val
		};
		if (this.value == this.item.defaultValue) {
			this.$emit("reset", result);
			return;
		}
		this.$emit("changed", result);
	}

	reset() {
		this.value = this.item.defaultValue;
	}

	get isChanged() {
		return this.value != this.item.defaultValue;
	}

	get isNumeric() {
		return isNumeric(this.item);
	}

	get isString() {
		return isString(this.item);
	}

	get isBoolean() {
		return isBoolean(this.item);
	}

	get enumValues(): Enumeration[] {
		return this.enums[this.item.type];
	}

	get description(): string {
		return formatOption(this.item);
	}
}
</script>

<style>
.item-name {
	color: #bbb;
	font-size: 0.7rem;
}

.pure-form input[type="text"],
.pure-form input[type="number"],
.pure-form select {
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

fieldset.changed,
fieldset.changed legend {
	background-color: #ffffbb;
}
</style>