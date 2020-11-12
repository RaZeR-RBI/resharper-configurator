
<template>
	<div class="editor">
		<!--
		<ul class="breadcrumbs">
			<li class="breadcrumb">&nbsp;</li>
			<li class="breadcrumb">Default</li>
			<li class="breadcrumb" v-for="item in section.subtree" :key="item">{{ item }}</li>
		</ul>
		-->
		<form class="pure-form">
			<EditorOption
				v-for="(item, index) in section.settings.items"
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

import { Section, ConfigValue, ChangeEvent } from "../config-types";

@Component({
	components: {
		EditorOption
	}
})
export default class Editor extends Vue {
	@Prop() private currentOptions!: Map<number, Map<number, ConfigValue>>;
	@Prop() private sectionId!: number;
	@Prop() private section!: Section;

	onChanged(e: ChangeEvent) {
		this.$emit("changed", e);
	}

	onReset(e: ChangeEvent) {
		this.$emit("reset", e);
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
