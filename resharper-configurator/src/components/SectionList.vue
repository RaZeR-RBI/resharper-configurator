<template>
	<div class="sections pure-menu">
		<span class="menu-brand">R# Configurator</span>
		<ul class="pure-menu-list">
			<li
				class="pure-menu-item"
				v-for="(section, index) in sections"
				:key="section.description"
				:class="{ 'pure-menu-selected pure-button-primary': index == selectedSectionId}"
			>
				<a
					v-if="!hasChangedOptions(index)"
					href="#"
					class="pure-menu-link"
					@click="onClick(index)"
				>{{ formatDescription(section.description) }}</a>
				<a v-else href="#" class="pure-menu-link" @click="onClick(index)">
					<i>{{ formatDescription(section.description) }}*</i>
				</a>
			</li>
		</ul>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { Section, ConfigValue } from "../config-types";

@Component
export default class SectionList extends Vue {
	@Prop() private changes!: number[];
	@Prop() private sections!: Section[];
	@Prop() private selectedSectionId!: number;

	onClick(id: number) {
		this.$emit("change", id);
	}

	hasChangedOptions(sectionId: number) {
		return this.changes.indexOf(sectionId) >= 0;
	}

	formatDescription(input: string): string {
		return input
			.split("CSharp")
			.join("C#")
			.split("settings")
			.join(" ")
			.trim();
	}
}
</script>

<style>
.menu-brand {
	display: block;
	padding: 0.5em 1em;
	background-color: #ffc723;
	font-weight: bold;
}

.sections.pure-menu .pure-menu-item {
	font-size: 0.9rem;
}

.pure-menu.pure-menu-horizontal {
	white-space: unset;
}
</style>
