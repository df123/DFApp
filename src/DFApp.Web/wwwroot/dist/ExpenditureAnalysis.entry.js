(()=>{"use strict";var e,a={52717:(e,a,l)=>{l.d(a,{Z:()=>u});var t=l(8081),n=l.n(t),o=l(23645),r=l.n(o)()(n());r.push([e.id,"\nbutton[data-v-1e16ffc3] {\n    font-weight: bold;\n}\nh2#card-usage ~ .example .example-showcase {\n    background-color: var(--el-fill-color) !important;\n}\n.el-statistic[data-v-1e16ffc3] {\n    --el-statistic-content-font-size: 28px;\n}\n.statistic-card[data-v-1e16ffc3] {\n    height: 100%;\n    padding: 20px;\n    border-radius: 4px;\n    background-color: var(--el-bg-color-overlay);\n}\n.statistic-footer[data-v-1e16ffc3] {\n    display: flex;\n    justify-content: space-between;\n    align-items: center;\n    flex-wrap: wrap;\n    font-size: 12px;\n    color: var(--el-text-color-regular);\n    margin-top: 16px;\n}\n.statistic-footer .footer-item[data-v-1e16ffc3] {\n    display: flex;\n    justify-content: space-between;\n    align-items: center;\n}\n.statistic-footer .footer-item span[data-v-1e16ffc3]:last-child {\n    display: inline-flex;\n    align-items: center;\n    margin-left: 4px;\n}\n.green[data-v-1e16ffc3] {\n    color: var(--el-color-success);\n}\n.red[data-v-1e16ffc3] {\n    color: var(--el-color-error);\n}\n",""]);const u=r},84537:function(e,a,l){var t=this&&this.__importDefault||function(e){return e&&e.__esModule?e:{default:e}};Object.defineProperty(a,"__esModule",{value:!0});const n=l(76765),o=t(l(9881)),r=t(l(49666));l(96671),(0,n.createApp)(o.default).use(r.default).mount("#app")},77729:(e,a,l)=>{Object.defineProperty(a,"X",{value:!0});const t=l(76765),n=l(76765),o=e=>((0,n.pushScopeId)("data-v-1e16ffc3"),e=e(),(0,n.popScopeId)(),e),r=o((()=>(0,n.createElementVNode)("div",null,[(0,n.createElementVNode)("canvas",{id:"chart"})],-1))),u={class:"statistic-card"},c=o((()=>(0,n.createElementVNode)("div",{style:{display:"inline-flex","align-items":"center"}}," 合计 ",-1))),i={class:"statistic-footer"},d={class:"footer-item"},s={class:"green"},v=l(76765),p=l(45739),f=l(98097);a.Z=(0,t.defineComponent)({__name:"App",setup(e){abp.localization.getResource("DFApp");const a=(0,v.ref)([]),l=(0,v.ref)(!1),t=(0,v.ref)(1),o=[{label:"天",value:0},{label:"月",value:1},{label:"年",value:2}],m=(0,v.ref)("bar"),g=[{value:"bar",label:"柱状图"},{value:"pie",label:"饼状图"}],h=(0,v.ref)("0"),b=[{label:"数值",value:"0"},{label:"百分数",value:"1"}],w=(0,v.ref)(void 0),V=(0,v.ref)(0),y=(0,v.ref)(""),x=(0,v.ref)("");async function C(e){await E(m.value,e,l.value,t.value,h.value)}async function A(e){await E(e,a.value,l.value,t.value,h.value)}async function N(e){await E(m.value,a.value,e,t.value,h.value)}async function _(e){await E(m.value,a.value,l.value,e,h.value),S()}async function k(e){await E(m.value,a.value,l.value,t.value,e),S()}async function E(e,a,l,t,n){let o=await dFApp.bookkeeping.expenditure.bookkeepingExpenditure.getChartJSDto(O(a[0]),O(a[1]),l,t,n);void 0!==w.value&&null!==w.value&&w.value.destroy(),w.value=new p.Chart("chart",{type:e,data:o}),V.value=o.total,S(),x.value=o.differenceTotal.toString()}function S(){let e=o.find((e=>e.value==t.value));0===t.value?y.value="对比昨"+e.label:1===t.value?y.value="对比上"+e.label:2===t.value&&(y.value="对比去"+e.label)}function O(e){return`${e.getFullYear()}-${(e.getMonth()+1).toString().padStart(2,"0")}-${e.getDate().toString().padStart(2,"0")}`}return(0,v.onMounted)((async()=>{a.value=function(){let e=new Date;return[new Date(e.getFullYear(),e.getMonth(),1),new Date(e.getFullYear(),e.getMonth()+1,0)]}(),await E("bar",a.value,l.value,t.value,h.value)})),(e,v)=>{const p=(0,n.resolveComponent)("el-checkbox"),w=(0,n.resolveComponent)("el-col"),E=(0,n.resolveComponent)("el-option"),S=(0,n.resolveComponent)("el-select"),O=(0,n.resolveComponent)("el-date-picker"),B=(0,n.resolveComponent)("el-row"),D=(0,n.resolveComponent)("el-statistic"),F=(0,n.resolveComponent)("el-icon");return(0,n.openBlock)(),(0,n.createElementBlock)(n.Fragment,null,[(0,n.createVNode)(B,null,{default:(0,n.withCtx)((()=>[(0,n.createVNode)(w,{span:2},{default:(0,n.withCtx)((()=>[(0,n.createVNode)(p,{modelValue:l.value,"onUpdate:modelValue":v[0]||(v[0]=e=>l.value=e),label:"开启对比",onChange:N},null,8,["modelValue"])])),_:1}),l.value?((0,n.openBlock)(),(0,n.createBlock)(w,{key:0,span:3},{default:(0,n.withCtx)((()=>[(0,n.createVNode)(S,{modelValue:t.value,"onUpdate:modelValue":v[1]||(v[1]=e=>t.value=e),class:"m-2",placeholder:"对比模式",onChange:_},{default:(0,n.withCtx)((()=>[((0,n.openBlock)(),(0,n.createElementBlock)(n.Fragment,null,(0,n.renderList)(o,(e=>(0,n.createVNode)(E,{key:e.value,label:e.label,value:e.value},null,8,["label","value"]))),64))])),_:1},8,["modelValue"])])),_:1})):(0,n.createCommentVNode)("v-if",!0),(0,n.createVNode)(w,{span:3},{default:(0,n.withCtx)((()=>[(0,n.createVNode)(S,{modelValue:m.value,"onUpdate:modelValue":v[2]||(v[2]=e=>m.value=e),class:"m-2",placeholder:"图类型",onChange:A},{default:(0,n.withCtx)((()=>[((0,n.openBlock)(),(0,n.createElementBlock)(n.Fragment,null,(0,n.renderList)(g,(e=>(0,n.createVNode)(E,{key:e.value,label:e.label,value:e.value},null,8,["label","value"]))),64))])),_:1},8,["modelValue"])])),_:1}),(0,n.createVNode)(w,{span:3},{default:(0,n.withCtx)((()=>[(0,n.createVNode)(S,{modelValue:h.value,"onUpdate:modelValue":v[3]||(v[3]=e=>h.value=e),class:"m-2",placeholder:"数字模式",onChange:k},{default:(0,n.withCtx)((()=>[((0,n.openBlock)(),(0,n.createElementBlock)(n.Fragment,null,(0,n.renderList)(b,(e=>(0,n.createVNode)(E,{key:e.value,label:e.label,value:e.value},null,8,["label","value"]))),64))])),_:1},8,["modelValue"])])),_:1}),(0,n.createVNode)(w,{span:8},{default:(0,n.withCtx)((()=>[(0,n.createVNode)(O,{class:"m-2",modelValue:a.value,"onUpdate:modelValue":v[4]||(v[4]=e=>a.value=e),type:"daterange","range-separator":"To","start-placeholder":"开始时间","end-placeholder":"结束时间",onChange:C},null,8,["modelValue"])])),_:1})])),_:1}),r,(0,n.createVNode)(B,null,{default:(0,n.withCtx)((()=>[(0,n.createElementVNode)("div",u,[(0,n.createVNode)(D,{value:V.value},{title:(0,n.withCtx)((()=>[c])),_:1},8,["value"]),(0,n.createElementVNode)("div",i,[(0,n.createElementVNode)("div",d,[(0,n.createElementVNode)("span",null,(0,n.toDisplayString)(y.value),1),(0,n.createElementVNode)("span",s,[(0,n.createTextVNode)((0,n.toDisplayString)(x.value)+" ",1),(0,n.createVNode)(F,null,{default:(0,n.withCtx)((()=>[(0,n.createVNode)((0,n.unref)(f.CaretTop))])),_:1})])])])])])),_:1}),(0,n.createCommentVNode)(' <el-row>\r\n        <div>\r\n            <canvas id="chart2"></canvas>\r\n        </div>\r\n    </el-row> ')],64)}}})},9881:(e,a,l)=>{l.r(a),l.d(a,{__esModule:()=>t.X,default:()=>b});var t=l(77729),n=l(93379),o=l.n(n),r=l(7795),u=l.n(r),c=l(90569),i=l.n(c),d=l(3565),s=l.n(d),v=l(19216),p=l.n(v),f=l(44589),m=l.n(f),g=l(52717),h={};h.styleTagTransform=m(),h.setAttributes=s(),h.insert=i().bind(null,"head"),h.domAPI=u(),h.insertStyleElement=p(),o()(g.Z,h),g.Z&&g.Z.locals&&g.Z.locals;const b=(0,l(83744).Z)(t.Z,[["__scopeId","data-v-1e16ffc3"]])},66270:e=>{e.exports="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAIAAADZF8uwAAAAGUlEQVQYV2M4gwH+YwCGIasIUwhT25BVBADtzYNYrHvv4gAAAABJRU5ErkJggg=="},9235:e=>{e.exports="data:image/svg+xml;utf8,%3Csvg class=%27icon%27 width=%27200%27 height=%27200%27 viewBox=%270 0 1024 1024%27 xmlns=%27http://www.w3.org/2000/svg%27%3E%3Cpath fill=%27currentColor%27 d=%27M406.656 706.944L195.84 496.256a32 32 0 10-45.248 45.248l256 256 512-512a32 32 0 00-45.248-45.248L406.592 706.944z%27%3E%3C/path%3E%3C/svg%3E"}},l={};function t(e){var n=l[e];if(void 0!==n)return n.exports;var o=l[e]={id:e,loaded:!1,exports:{}};return a[e].call(o.exports,o,o.exports,t),o.loaded=!0,o.exports}t.m=a,e=[],t.O=(a,l,n,o)=>{if(!l){var r=1/0;for(d=0;d<e.length;d++){for(var[l,n,o]=e[d],u=!0,c=0;c<l.length;c++)(!1&o||r>=o)&&Object.keys(t.O).every((e=>t.O[e](l[c])))?l.splice(c--,1):(u=!1,o<r&&(r=o));if(u){e.splice(d--,1);var i=n();void 0!==i&&(a=i)}}return a}o=o||0;for(var d=e.length;d>0&&e[d-1][2]>o;d--)e[d]=e[d-1];e[d]=[l,n,o]},t.n=e=>{var a=e&&e.__esModule?()=>e.default:()=>e;return t.d(a,{a}),a},t.d=(e,a)=>{for(var l in a)t.o(a,l)&&!t.o(e,l)&&Object.defineProperty(e,l,{enumerable:!0,get:a[l]})},t.g=function(){if("object"==typeof globalThis)return globalThis;try{return this||new Function("return this")()}catch(e){if("object"==typeof window)return window}}(),t.o=(e,a)=>Object.prototype.hasOwnProperty.call(e,a),t.r=e=>{"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},t.nmd=e=>(e.paths=[],e.children||(e.children=[]),e),(()=>{t.b=document.baseURI||self.location.href;var e={245:0};t.O.j=a=>0===e[a];var a=(a,l)=>{var n,o,[r,u,c]=l,i=0;if(r.some((a=>0!==e[a]))){for(n in u)t.o(u,n)&&(t.m[n]=u[n]);if(c)var d=c(t)}for(a&&a(l);i<r.length;i++)o=r[i],t.o(e,o)&&e[o]&&e[o][0](),e[o]=0;return t.O(d)},l=self.webpackChunkDFApp_VueApp=self.webpackChunkDFApp_VueApp||[];l.forEach(a.bind(null,0)),l.push=a.bind(null,l.push.bind(l))})(),t.nc=void 0;var n=t.O(void 0,[250,963],(()=>t(84537)));n=t.O(n)})();