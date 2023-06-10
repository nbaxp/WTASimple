/**
 * Bundled by jsDelivr using Rollup v2.79.1 and Terser v5.17.1.
 * Original file: /npm/detect-it@4.0.1/dist/detect-it.esm.js
 *
 * Do NOT use SRI with dynamically generated files! More information: https://www.jsdelivr.com/using-sri-with-dynamic-files
 */
var e="undefined"!=typeof window?window:{screen:{},navigator:{}},n=(e.matchMedia||function(){return{matches:!1}}).bind(e),t=!1,o={get passive(){return t=!0}},i=function(){};e.addEventListener&&e.addEventListener("p",i,o),e.removeEventListener&&e.removeEventListener("p",i,!1);var r=t,a="PointerEvent"in e,s="ontouchstart"in e,c=s||"TouchEvent"in e&&n("(any-pointer: coarse)").matches,h=(e.navigator.maxTouchPoints||0)>0||c,u=e.navigator.userAgent||"",m=n("(pointer: coarse)").matches&&/iPad|Macintosh/.test(u)&&Math.min(e.screen.width||0,e.screen.height||0)>=768,v=(n("(pointer: coarse)").matches||!n("(pointer: fine)").matches&&s)&&!/Windows.*Firefox/.test(u),d=n("(any-pointer: fine)").matches||n("(any-hover: hover)").matches||m||!s,p=!h||!d&&v?h?"touchOnly":"mouseOnly":"hybrid",y="mouseOnly"===p?"mouse":"touchOnly"===p||v?"touch":"mouse";export{p as deviceType,y as primaryInput,r as supportsPassiveEvents,a as supportsPointerEvents,c as supportsTouchEvents};export default null;
//# sourceMappingURL=/sm/4280e12bc6c9669987adcde477eb6347c379a3a324091a23187d02e5284db1bb.map