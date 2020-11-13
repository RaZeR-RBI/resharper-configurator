
<template>
	<div class="editor">
		<ul class="breadcrumbs">
			<li class="breadcrumb">Section:&nbsp;</li>
			<li class="breadcrumb">Default</li>
			<li class="breadcrumb" v-for="item in section.subtree" :key="item">{{ item }}</li>
		</ul>
		<form class="pure-form pure-form-aligned">
			<fieldset>
				<div class="pure-control-group">
					<input type="text" placeholder="Search" v-model="search" />
				</div>
			</fieldset>
		</form>
		<hr />
		<form class="pure-form">
			<EditorOption
				v-for="(item, index) in section.settings.items"
				:class="{'hidden': !isVisible(item)}"
				:currentOptions="currentOptions"
				:sectionId="sectionId"
				:optionId="index"
				:key="item.name"
				:item="item"
				:enums="section.settings.enumerations"
				@changed="onChanged"
				@reset="onReset"
			/>
		</form>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import EditorOption from "./EditorOption.vue";

import {
	Section,
	ConfigValue,
	ChangeEvent,
	Item,
	formatOption
} from "../config-types";

@Component({
	components: {
		EditorOption
	}
})
export default class Editor extends Vue {
	@Prop() private currentOptions!: Map<number, Map<number, ConfigValue>>;
	@Prop() private sectionId!: number;
	@Prop() private section!: Section;
	search: string | null = null;

	onChanged(e: ChangeEvent) {
		this.$emit("changed", e);
	}

	onReset(e: ChangeEvent) {
		this.$emit("reset", e);
	}

	get hasSearchTerm() {
		return !(this.search == null || this.search.length == 0);
	}

	isVisible(item: Item): boolean {
		if (!this.hasSearchTerm) {
			return true;
		}
		const search = this.search.toLowerCase();
		return (
			item.name.toLowerCase().indexOf(search) >= 0 ||
			formatOption(item)
				.toLowerCase()
				.indexOf(search) >= 0
		);
	}
}
</script>

<style>
.breadcrumbs {
	padding: 0;
	margin: 0 0.5em;
	font-size: 0.75rem;
	color: #888;
}

.breadcrumb {
	display: inline-block;
}

.breadcrumb::after {
	content: "/";
	color: #bbb;
}

.editor {
	padding: 0 0.5em;
	font-size: 0.9rem;
}
</style>
