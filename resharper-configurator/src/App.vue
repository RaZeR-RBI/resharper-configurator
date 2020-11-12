<template>
	<div id="app">
		<SectionList
			:sections="sections"
			:selectedSectionId="selectedSectionId"
			:changes="changedSections"
			v-on:change="onSectionChanged"
		/>
		<hr />
		<Editor
			v-if="section"
			:section="section"
			:sectionId="selectedSectionId"
			:currentOptions="currentOptions"
			@changed="onOptionChanged"
			@reset="onOptionReset"
		/>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import SectionList from "./components/SectionList.vue";
import Editor from "./components/Editor.vue";
import * as config from "./config.json";
import { Section, ConfigValue, ChangeEvent } from "./config-types";

@Component({
	components: {
		SectionList,
		Editor
	}
})
export default class App extends Vue {
	selectedSectionId = -1;
	section: Section | null = null;
	changedItems: Map<number, Map<number, ConfigValue>> = new Map();
	changedSections: number[] = [];

	mounted() {
		// this.onSectionChanged(this.sections.length - 1);
		this.onSectionChanged(0);
	}

	get sections(): Section[] {
		return (config as any).default as Section[];
	}

	get currentOptions(): Map<number, Map<number, ConfigValue>> {
		return this.changedItems;
	}

	updateChangedSections() {
		const result: number[] = [];
		for (const [sectionId, options] of this.changedItems) {
			if (options.size > 0) {
				result.push(sectionId);
			}
		}
		this.changedSections = result;
	}

	onSectionChanged(id: number) {
		this.selectedSectionId = id;
		this.section = this.sections[id];
	}

	onOptionChanged(e: ChangeEvent) {
		const items = this.changedItems;
		if (!items.has(e.sectionId)) {
			items.set(e.sectionId, new Map());
		}
		const section = items.get(e.sectionId);
		section!.set(e.optionId, e.value);
		this.updateChangedSections();
	}

	onOptionReset(e: ChangeEvent) {
		const items = this.changedItems;
		if (!items.has(e.sectionId)) {
			return;
		}
		items.get(e.sectionId)!.delete(e.optionId);
		if (items.get(e.sectionId)!.size <= 0) {
			items.delete(e.sectionId);
		}
		this.updateChangedSections();
	}
}
</script>

<style>
#app {
	font-family: Avenir, Helvetica, Arial, sans-serif;
	-webkit-font-smoothing: antialiased;
	-moz-osx-font-smoothing: grayscale;
}
</style>
