import { ref, onMounted } from 'vue';
import { marked, setOptions } from '../../lib/marked/marked.esm.js';
import mermaid from '../../lib/mermaid/mermaid.esm.min.mjs';
import hljs from '../../lib/highlightjs/highlight.min.js';

export default {
  template: `<div ref="tplRef"><div class="source" style="display:none;"><slot /></div><div class="markdown-body"></div></template>`,
  props: {
    name: {
      default: null
    }
  },
  setup(props) {
    const tplRef = ref(null);
    mermaid.initialize({ startOnLoad: false });
    let id = 0;
    onMounted(async () => {
      setOptions({
        highlight: function (code, lang) {
          if (lang === 'mermaid') {
            return mermaid.mermaidAPI.render(`mermaid${id++}`, code, undefined);
          } else {
            const language = hljs.getLanguage(lang) ? lang : 'plaintext';
            return hljs.highlight(code, { language }).value;
          }
        },
        langPrefix: 'hljs language-',
      });
      let mdText = tplRef.value.querySelector('.source').innerText;
      if (props.name !== null) {
        const response = await fetch(`./assets/docs/${props.name}.md`);
        mdText = await response.text();
      }
      tplRef.value.querySelector('.markdown-body').innerHTML = marked(mdText);
      tplRef.value.querySelector('.source').remove();
    });
    return {
      tplRef
    };
  }
}
