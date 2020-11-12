<template>
	<div class="pure-menu pure-menu-horizontal">
		<span class="menu-brand">R# Configurator</span>
		<ul class="pure-menu-list">
			<li
				class="pure-menu-item"
				v-for="(section, index) in sections"
				:key="section.description"
				:class="{ 'pure-menu-selected': index == selectedSectionId}"
			>
				<a href="#" class="pure-menu-link" @click="onClick(index)">{{ section.description }}</a>
			</li>
		</ul>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { Section } from "../config-types";

@Component
export default class SectionList extends Vue {
	@Prop() private sections!: Section[];
	@Prop() private selectedSectionId!: number;

	onClick(id: number) {
		this.$emit("change", id);
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
.pure-menu.pure-menu-horizontal {
	white-space: unset;
}
</style>
