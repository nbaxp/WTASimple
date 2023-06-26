const styleCounterName = 'data-vue-component-instances';

function getStyleList(name) {
  return document.querySelectorAll(`head style.${name},head link[rel='stylesheet'].${name}`);
}

function append(parent, html) {
  parent.insertAdjacentHTML('beforeend', html);
}

function addStyles(name, styleSource) {
  var styleList = getStyleList(name);
  if (styleList.length > 0) {
    for (let i = 0; i < styleList.length; i++) {
      const style = styleList[i];
      var counter = parseInt(style.getAttribute(styleCounterName));
      style.setAttribute(styleCounterName, counter + 1);
    }
  } else {
    const doc = new DOMParser().parseFromString(styleSource, 'text/html');
    const styles = doc.querySelectorAll("style,link[rel='stylesheet']");
    for (let i = 0; i < styles.length; i++) {
      const style = styles[i];
      style.setAttribute('class', name);
      style.setAttribute(styleCounterName, 1);
      append(document.head, style.outerHTML);
    }
  }
}

function removeStyles(name) {
  var styleList = getStyleList(name);
  if (styleList.length > 0) {
    for (var i = 0; i < styleList.length; i++) {
      var style = styleList[i];
      var counter = parseInt(style.getAttribute(styleCounterName));
      if (counter - 1 > 0) {
        style.setAttribute(styleCounterName, counter - 1);
      } else {
        document.head.removeChild(style);
      }
    }
  }
}

let id = 0;

export default {
  created() {
    if (this._.type.styles) {
      if (!this._.type.uid) {
        this._.type.uid = `uid_${id++}`;
      }
      addStyles(this._.type.uid, this._.type.styles);
    }
  },
  unmounted() {
    removeStyles(this._.type.uid);
  },
};
