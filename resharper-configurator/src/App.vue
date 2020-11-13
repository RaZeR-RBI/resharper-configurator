<template>
	<div id="app">
		<div class="content-sidebar">
			<div>
				<SectionList
					:sections="sections"
					:selectedSectionId="selectedSectionId"
					:changes="changedSections"
					v-on:change="onSectionChanged"
				/>
				<hr />
				<a href="#" @click="importXml()" class="pure-button">Import</a>
				<a href="#" @click="exportXml()" class="pure-button pure-button-primary">Export</a>
				<hr />
				<div style="text-align: center">
					<a href="https://github.com/razer-rbi/resharper-configurator" target="_blank">Fork me on GitHub</a>
				</div>
				<hr />
				<div class="disclaimer">
					<p>
						Made by
						<a href="https://github.com/razer-rbi/" target="_blank">Adel Vilkov</a>
					</p>
					<p>
						<mark>This is not an official tool.</mark>
					</p>
					<p>
						This tool's sole purpose is to aid in configuring
						<a
							href="https://www.jetbrains.com/resharper/"
							target="_blank"
						>ReSharper</a> and
						<a
							href="https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html"
							target="_blank"
						>ReSharper CLI</a>.
						The options list was created by using the ReSharper SDK.
					</p>
					<p>ReSharper is a registered trademark of JetBrains s.r.o.</p>
				</div>
			</div>
		</div>
		<div class="content-main">
			<Editor
				v-if="section && showEditor"
				:section="section"
				:sectionId="selectedSectionId"
				:currentOptions="currentOptions"
				@changed="onOptionChanged"
				@reset="onOptionReset"
			/>
			<ImportView v-else-if="showImport" v-on:import="onDoImport" />
			<ExportView v-else-if="showExport" :contents="exportedData" />
		</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Watch } from "vue-property-decorator";
import SectionList from "./components/SectionList.vue";
import Editor from "./components/Editor.vue";
import ExportView from "./components/ExportView.vue";
import ImportView from "./components/ImportView.vue";
import * as config from "./config.json";
import { Section, ConfigValue, ChangeEvent } from "./config-types";
import { toXml, fromXml } from "./xml";

@Component({
	components: {
		SectionList,
		Editor,
		ExportView,
		ImportView
	}
})
export default class App extends Vue {
	selectedSectionId = -1;
	section: Section | null = null;
	changedItems: Map<number, Map<number, ConfigValue>> = new Map();
	changedSections: number[] = [];
	exportedData: string | null = null;
	showImport = false;

	mounted() {
		this.goToEditor();
	}

	goToEditor() {
		this.onSectionChanged(this.sections.length - 1);
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
		this.exportedData = null;
		this.showImport = false;
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

	exportXml() {
		this.exportedData = toXml(this.sections, this.changedItems);
		this.section = null;
		this.showImport = false;
	}

	importXml() {
		this.section = null;
		this.exportedData = null;
		this.showImport = true;
	}

	showEditor() {
		return !this.showImport && !this.showExport && this.section != null;
	}

	showExport() {
		return this.exportedData != null;
	}

	onDoImport(contents: string) {
		this.changedItems = fromXml(this.sections, contents);
		this.updateChangedSections();
		this.goToEditor();
	}
}
</script>

<style>
html,
body {
	height: 100%;
}

body {
	margin: 0;
	background: rgb(197, 71, 139);
	background: linear-gradient(
		135deg,
		rgba(197, 71, 139, 1) 0%,
		rgba(243, 122, 62, 1) 30%,
		rgba(252, 237, 57, 1) 100%
	);
	background-repeat: no-repeat;
	background-attachment: fixed;
}

#app {
	background-color: white;
	font-family: Avenir, Helvetica, Arial, sans-serif;
	-webkit-font-smoothing: antialiased;
	-moz-osx-font-smoothing: grayscale;
	display: flex;
	max-width: 1000px;
	margin: 0 auto;
}

#app > .content-sidebar {
	flex: 0 0 250px;
	background: white;
	min-height: 100vh;
}

#app > .content-sidebar > div {
	position: fixed;
	top: 0;
	width: 250px;
}

#app > .content-main {
	padding: 0.5em;
	background: white;
	width: 100%;
	max-width: 750px;
}

.content-sidebar .disclaimer {
	text-align: center;
	padding: 0.5em;
	font-size: 0.75rem;
	color: #999;
}

.content-sidebar .pure-button {
	display: block;
}

.disclaimer mark {
	background-color: transparent;
	border-bottom: 1px solid red;
}

hr {
	border-color: #fefefe;
}
</style>
