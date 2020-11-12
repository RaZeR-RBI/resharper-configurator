<template>
	<div id="app">
		<SectionList
			:sections="sections"
			:selectedSectionId="selectedSectionId"
			v-on:change="onSectionChanged"
		/>
		<hr />
		<Editor v-if="section" :section="section" />
	</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import SectionList from "./components/SectionList.vue";
import Editor from "./components/Editor.vue";
import * as config from "./config.json";
import { Section } from "./config-types";

@Component({
	components: {
		SectionList,
		Editor
	}
})
export default class App extends Vue {
	selectedSectionId = -1;
	section?: Section = null;

	mounted() {
		this.onSectionChanged(this.sections.length - 1);
	}

	get sections(): Section[] {
		return (config as any).default as Section[];
	}

	onSectionChanged(id: number) {
		this.selectedSectionId = id;
		this.section = this.sections[id];
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
