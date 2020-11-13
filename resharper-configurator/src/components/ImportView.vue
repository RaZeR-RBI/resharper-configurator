<template>
	<div class="import">
		<h3 style="margin-top: 0">Import .DotSettings</h3>
		<textarea ref="box" v-model="contents" placeholder="Paste .DotSettings XML contents here"></textarea>
		<div style="margin-top: 1em">
			<a href="#" @click="doImport()" class="pure-button pure-button-primary">Import</a>
		</div>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Prop, Watch } from "vue-property-decorator";

@Component({})
export default class ImportView extends Vue {
	contents = "";

	mounted() {
		this.onContentsChanged();
	}

	doImport() {
		this.$emit("import", this.contents);
	}

	@Watch("contents")
	onContentsChanged() {
		const refs = this.$refs;
		this.$nextTick(function() {
			const el = refs.box as HTMLTextAreaElement;
			el.style.height = "5px";
			el.style.height = el.scrollHeight + "px";
		});
	}
}
</script>

<style>
.import textarea {
	width: 100%;
	overflow-x: auto;
	font-size: 0.75rem;
	border: 1px solid gray;
	resize: none;
	min-height: 50px;
}
</style>