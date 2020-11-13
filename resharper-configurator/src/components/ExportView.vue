<template>
	<div class="export">
		<h3 style="margin-top: 0">Exported .DotSettings</h3>
		<textarea ref="box" readonly v-model="formattedContents"></textarea>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";

@Component({})
export default class ExportView extends Vue {
	@Prop() contents!: string;

	formatXml(xml: string | null) {
		if (xml == null) {
			return "";
		}
		const PADDING = " ".repeat(2); // set desired indent size here
		const reg = /(>)(<)(\/*)/g;
		let pad = 0;

		xml = xml.replace(reg, "$1\r\n$2$3");

		return xml
			.split("\r\n")
			.map((node, index) => {
				let indent = 0;
				if (node.match(/.+<\/\w[^>]*>$/)) {
					indent = 0;
				} else if (node.match(/^<\/\w/) && pad > 0) {
					pad -= 1;
				} else if (node.match(/^<\w[^>]*[^/]>.*$/)) {
					indent = 1;
				} else {
					indent = 0;
				}

				pad += indent;

				return PADDING.repeat(pad - indent) + node;
			})
			.join("\r\n");
	}

	get formattedContents() {
		const s = this.formatXml(this.contents);
		const refs = this.$refs;
		this.$nextTick(function() {
			const el = refs.box as HTMLTextAreaElement;
			if (!el) {
				return;
			}
			el.style.height = "5px";
			el.style.height = el.scrollHeight + "px";
		});
		return s;
	}
}
</script>

<style>
.export textarea {
	width: 100%;
	overflow-x: auto;
	font-size: 0.75rem;
	background: #eee;
	border: 1px solid gray;
	resize: none;
}
</style>