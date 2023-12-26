/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css":
/*!********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css ***!
  \********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/***/ ((module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_sourceMaps_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/runtime/sourceMaps.js */ "./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/runtime/sourceMaps.js");
/* harmony import */ var _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_sourceMaps_js__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_sourceMaps_js__WEBPACK_IMPORTED_MODULE_0__);
/* harmony import */ var _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/runtime/api.js */ "./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/runtime/api.js");
/* harmony import */ var _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1__);
// Imports


var ___CSS_LOADER_EXPORT___ = _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1___default()((_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_runtime_sourceMaps_js__WEBPACK_IMPORTED_MODULE_0___default()));
// Module
___CSS_LOADER_EXPORT___.push([module.id, `
botton-area[data-v-ced1ef0a] {
    display: flex;
    margin-bottom: 10px;
}
`, "",{"version":3,"sources":["webpack://./src/Media/ExternalLink/App.vue"],"names":[],"mappings":";AA8CA;IACI,aAAa;IACb,mBAAmB;AACvB","sourcesContent":["<template>\r\n    <div>\r\n        <el-row>\r\n            <div class=\"botton-area\">\r\n                <el-button type=\"primary\" @click=\"getLink\">{{ l('Media:ExternalLinkTitle:GetLink') }}</el-button>\r\n                <el-button type=\"primary\" @click=\"copy\">{{ l('Media:ExternalLinkTitle:Copy') }}</el-button>\r\n                <el-button type=\"primary\" @click=\"move\">{{ l('Media:ExternalLinkTitle:Move') }}</el-button>\r\n            </div>\r\n        </el-row>\r\n        <el-row style=\"margin-top: 10px;\">\r\n            <el-input v-model=\"textarea\" :rows=\"2\" type=\"textarea\" :autosize=\"{ minRows: 2, maxRows: 30 }\"\r\n                placeholder=\"Please input\" />\r\n        </el-row>\r\n    </div>\r\n</template>\r\n\r\n<script setup lang=\"ts\">\r\nimport { ref } from 'vue'\r\n\r\n\r\nconst l: Function = abp.localization.getResource('Telegram') as Function;\r\n\r\nconst textarea = ref('')\r\n\r\nasync function getLink() {\r\n    try {\r\n        let links: string = await dF.telegram.media.mediaInfo.getExternalLinkDownload() as string;\r\n        textarea.value = links;\r\n    } catch (error) {\r\n        alert(error);\r\n    }\r\n}\r\n\r\nfunction copy() {\r\n    navigator.clipboard.writeText(textarea.value);\r\n    alert(l('Media:ExternalLinkTitle:CopySuccessMesage'));\r\n}\r\n\r\nasync function move() {\r\n    let result = await dF.telegram.media.mediaInfo.moveDownloaded() as string;\r\n    textarea.value = result;\r\n}\r\n\r\n</script>\r\n\r\n<style scoped>\r\nbotton-area {\r\n    display: flex;\r\n    margin-bottom: 10px;\r\n}\r\n</style>"],"sourceRoot":""}]);
// Exports
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (___CSS_LOADER_EXPORT___);


/***/ }),

/***/ "./src/Media/ExternalLink/main.ts":
/*!****************************************!*\
  !*** ./src/Media/ExternalLink/main.ts ***!
  \****************************************/
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
const vue_1 = __webpack_require__(/*! vue */ "./node_modules/.pnpm/vue@3.3.4/node_modules/vue/index.js");
const App_vue_1 = __importDefault(__webpack_require__(/*! ./App.vue */ "./src/Media/ExternalLink/App.vue"));
const element_plus_1 = __importDefault(__webpack_require__(/*! element-plus */ "./node_modules/.pnpm/element-plus@2.3.12_vue@3.3.4/node_modules/element-plus/lib/index.js"));
__webpack_require__(/*! element-plus/dist/index.css */ "./node_modules/.pnpm/element-plus@2.3.12_vue@3.3.4/node_modules/element-plus/dist/index.css");
(0, vue_1.createApp)(App_vue_1.default).use(element_plus_1.default).mount('#app');


/***/ }),

/***/ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/cjs.js!./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css":
/*!********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/cjs.js!./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css ***!
  \********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_injectStylesIntoStyleTag_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/injectStylesIntoStyleTag.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/injectStylesIntoStyleTag.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_injectStylesIntoStyleTag_js__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_injectStylesIntoStyleTag_js__WEBPACK_IMPORTED_MODULE_0__);
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleDomAPI_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/styleDomAPI.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/styleDomAPI.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleDomAPI_js__WEBPACK_IMPORTED_MODULE_1___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleDomAPI_js__WEBPACK_IMPORTED_MODULE_1__);
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertBySelector_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/insertBySelector.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/insertBySelector.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertBySelector_js__WEBPACK_IMPORTED_MODULE_2___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertBySelector_js__WEBPACK_IMPORTED_MODULE_2__);
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_setAttributesWithoutAttributes_js__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/setAttributesWithoutAttributes.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/setAttributesWithoutAttributes.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_setAttributesWithoutAttributes_js__WEBPACK_IMPORTED_MODULE_3___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_setAttributesWithoutAttributes_js__WEBPACK_IMPORTED_MODULE_3__);
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertStyleElement_js__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/insertStyleElement.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/insertStyleElement.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertStyleElement_js__WEBPACK_IMPORTED_MODULE_4___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertStyleElement_js__WEBPACK_IMPORTED_MODULE_4__);
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleTagTransform_js__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! !../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/styleTagTransform.js */ "./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/runtime/styleTagTransform.js");
/* harmony import */ var _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleTagTransform_js__WEBPACK_IMPORTED_MODULE_5___default = /*#__PURE__*/__webpack_require__.n(_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleTagTransform_js__WEBPACK_IMPORTED_MODULE_5__);
/* harmony import */ var _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! !!../../../node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css */ "./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css");
/* unplugin-vue-components disabled */
      
      
      
      
      
      
      
      
      

var options = {};

options.styleTagTransform = (_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleTagTransform_js__WEBPACK_IMPORTED_MODULE_5___default());
options.setAttributes = (_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_setAttributesWithoutAttributes_js__WEBPACK_IMPORTED_MODULE_3___default());

      options.insert = _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertBySelector_js__WEBPACK_IMPORTED_MODULE_2___default().bind(null, "head");
    
options.domAPI = (_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_styleDomAPI_js__WEBPACK_IMPORTED_MODULE_1___default());
options.insertStyleElement = (_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_insertStyleElement_js__WEBPACK_IMPORTED_MODULE_4___default());

var update = _node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_runtime_injectStylesIntoStyleTag_js__WEBPACK_IMPORTED_MODULE_0___default()(_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_6__["default"], options);




       /* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_6__["default"] && _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_6__["default"].locals ? _node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_6__["default"].locals : undefined);


/***/ }),

/***/ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts":
/*!**********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts ***!
  \**********************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

/* unplugin-vue-components disabled */
Object.defineProperty(exports, "__esModule", ({ value: true }));
const vue_1 = __webpack_require__(/*! vue */ "./node_modules/.pnpm/vue@3.3.4/node_modules/vue/index.js");
const vue_2 = __webpack_require__(/*! vue */ "./node_modules/.pnpm/vue@3.3.4/node_modules/vue/index.js");
exports["default"] = (0, vue_1.defineComponent)({
    __name: 'App',
    setup(__props, { expose: __expose }) {
        __expose();
        const l = abp.localization.getResource('Telegram');
        const textarea = (0, vue_2.ref)('');
        async function getLink() {
            try {
                let links = await dF.telegram.media.mediaInfo.getExternalLinkDownload();
                textarea.value = links;
            }
            catch (error) {
                alert(error);
            }
        }
        function copy() {
            navigator.clipboard.writeText(textarea.value);
            alert(l('Media:ExternalLinkTitle:CopySuccessMesage'));
        }
        async function move() {
            let result = await dF.telegram.media.mediaInfo.moveDownloaded();
            textarea.value = result;
        }
        const __returned__ = { l, textarea, getLink, copy, move };
        Object.defineProperty(__returned__, '__isScriptSetup', { enumerable: false, value: true });
        return __returned__;
    }
});


/***/ }),

/***/ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/templateLoader.js??ruleSet[1].rules[4]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true":
/*!*************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/templateLoader.js??ruleSet[1].rules[4]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true ***!
  \*************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

/* unplugin-vue-components disabled */
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.render = void 0;
const vue_1 = __webpack_require__(/*! vue */ "./node_modules/.pnpm/vue@3.3.4/node_modules/vue/index.js");
const _withScopeId = n => ((0, vue_1.pushScopeId)("data-v-ced1ef0a"), n = n(), (0, vue_1.popScopeId)(), n);
const _hoisted_1 = { class: "botton-area" };
function render(_ctx, _cache, $props, $setup, $data, $options) {
    const _component_el_button = (0, vue_1.resolveComponent)("el-button");
    const _component_el_row = (0, vue_1.resolveComponent)("el-row");
    const _component_el_input = (0, vue_1.resolveComponent)("el-input");
    return ((0, vue_1.openBlock)(), (0, vue_1.createElementBlock)("div", null, [
        (0, vue_1.createVNode)(_component_el_row, null, {
            default: (0, vue_1.withCtx)(() => [
                (0, vue_1.createElementVNode)("div", _hoisted_1, [
                    (0, vue_1.createVNode)(_component_el_button, {
                        type: "primary",
                        onClick: $setup.getLink
                    }, {
                        default: (0, vue_1.withCtx)(() => [
                            (0, vue_1.createTextVNode)((0, vue_1.toDisplayString)($setup.l('Media:ExternalLinkTitle:GetLink')), 1 /* TEXT */)
                        ]),
                        _: 1 /* STABLE */
                    }),
                    (0, vue_1.createVNode)(_component_el_button, {
                        type: "primary",
                        onClick: $setup.copy
                    }, {
                        default: (0, vue_1.withCtx)(() => [
                            (0, vue_1.createTextVNode)((0, vue_1.toDisplayString)($setup.l('Media:ExternalLinkTitle:Copy')), 1 /* TEXT */)
                        ]),
                        _: 1 /* STABLE */
                    }),
                    (0, vue_1.createVNode)(_component_el_button, {
                        type: "primary",
                        onClick: $setup.move
                    }, {
                        default: (0, vue_1.withCtx)(() => [
                            (0, vue_1.createTextVNode)((0, vue_1.toDisplayString)($setup.l('Media:ExternalLinkTitle:Move')), 1 /* TEXT */)
                        ]),
                        _: 1 /* STABLE */
                    })
                ])
            ]),
            _: 1 /* STABLE */
        }),
        (0, vue_1.createVNode)(_component_el_row, { style: { "margin-top": "10px" } }, {
            default: (0, vue_1.withCtx)(() => [
                (0, vue_1.createVNode)(_component_el_input, {
                    modelValue: $setup.textarea,
                    "onUpdate:modelValue": _cache[0] || (_cache[0] = ($event) => (($setup.textarea) = $event)),
                    rows: 2,
                    type: "textarea",
                    autosize: { minRows: 2, maxRows: 30 },
                    placeholder: "Please input"
                }, null, 8 /* PROPS */, ["modelValue"])
            ]),
            _: 1 /* STABLE */
        })
    ]));
}
exports.render = render;


/***/ }),

/***/ "./src/Media/ExternalLink/App.vue":
/*!****************************************!*\
  !*** ./src/Media/ExternalLink/App.vue ***!
  \****************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   __esModule: () => (/* reexport safe */ _App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_1__.__esModule),
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _App_vue_vue_type_template_id_ced1ef0a_scoped_true_ts_true__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true */ "./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true");
/* harmony import */ var _App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./App.vue?vue&type=script&setup=true&lang=ts */ "./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts");
/* harmony import */ var _App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css */ "./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css");
/* harmony import */ var _node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_exportHelper_js__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/exportHelper.js */ "./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/exportHelper.js");
/* unplugin-vue-components disabled */



;


const __exports__ = /*#__PURE__*/(0,_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_exportHelper_js__WEBPACK_IMPORTED_MODULE_3__["default"])(_App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_1__["default"], [['render',_App_vue_vue_type_template_id_ced1ef0a_scoped_true_ts_true__WEBPACK_IMPORTED_MODULE_0__.render],['__scopeId',"data-v-ced1ef0a"],['__file',"src/Media/ExternalLink/App.vue"]])
/* hot reload */
if (false) {}


/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (__exports__);

/***/ }),

/***/ "./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css":
/*!************************************************************************************************!*\
  !*** ./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css ***!
  \************************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_style_loader_3_3_3_webpack_5_88_2_node_modules_style_loader_dist_cjs_js_node_modules_pnpm_css_loader_6_8_1_webpack_5_88_2_node_modules_css_loader_dist_cjs_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_stylePostLoader_js_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_style_index_0_id_ced1ef0a_scoped_true_lang_css__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! -!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!../../../node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/cjs.js!../../../node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css */ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/style-loader@3.3.3_webpack@5.88.2/node_modules/style-loader/dist/cjs.js!./node_modules/.pnpm/css-loader@6.8.1_webpack@5.88.2/node_modules/css-loader/dist/cjs.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/stylePostLoader.js!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=style&index=0&id=ced1ef0a&scoped=true&lang=css");
/* unplugin-vue-components disabled */

/***/ }),

/***/ "./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts":
/*!***************************************************************************!*\
  !*** ./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts ***!
  \***************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   __esModule: () => (/* reexport safe */ _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_0__.__esModule),
/* harmony export */   "default": () => (/* reexport safe */ _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_0__["default"])
/* harmony export */ });
/* harmony import */ var _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_script_setup_true_lang_ts__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! -!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!../../../node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./App.vue?vue&type=script&setup=true&lang=ts */ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=script&setup=true&lang=ts");
/* unplugin-vue-components disabled */ 

/***/ }),

/***/ "./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true":
/*!******************************************************************************************!*\
  !*** ./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true ***!
  \******************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   __esModule: () => (/* reexport safe */ _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_templateLoader_js_ruleSet_1_rules_4_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_template_id_ced1ef0a_scoped_true_ts_true__WEBPACK_IMPORTED_MODULE_0__.__esModule),
/* harmony export */   render: () => (/* reexport safe */ _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_templateLoader_js_ruleSet_1_rules_4_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_template_id_ced1ef0a_scoped_true_ts_true__WEBPACK_IMPORTED_MODULE_0__.render)
/* harmony export */ });
/* harmony import */ var _node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_vue_components_node_modules_pnpm_unplugin_1_4_0_node_modules_unplugin_dist_webpack_loaders_transform_js_unpluginName_unplugin_auto_import_node_modules_pnpm_ts_loader_9_4_4_typescript_5_2_2_webpack_5_88_2_node_modules_ts_loader_index_js_clonedRuleSet_1_use_0_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_templateLoader_js_ruleSet_1_rules_4_node_modules_pnpm_vue_loader_17_2_2_vue_3_3_4_webpack_5_88_2_node_modules_vue_loader_dist_index_js_ruleSet_1_rules_11_use_0_App_vue_vue_type_template_id_ced1ef0a_scoped_true_ts_true__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! -!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!../../../node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!../../../node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/templateLoader.js??ruleSet[1].rules[4]!../../../node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true */ "./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-vue-components!./node_modules/.pnpm/unplugin@1.4.0/node_modules/unplugin/dist/webpack/loaders/transform.js?unpluginName=unplugin-auto-import!./node_modules/.pnpm/ts-loader@9.4.4_typescript@5.2.2_webpack@5.88.2/node_modules/ts-loader/index.js??clonedRuleSet-1.use[0]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/templateLoader.js??ruleSet[1].rules[4]!./node_modules/.pnpm/vue-loader@17.2.2_vue@3.3.4_webpack@5.88.2/node_modules/vue-loader/dist/index.js??ruleSet[1].rules[11].use[0]!./src/Media/ExternalLink/App.vue?vue&type=template&id=ced1ef0a&scoped=true&ts=true");
/* unplugin-vue-components disabled */

/***/ }),

/***/ "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAIAAADZF8uwAAAAGUlEQVQYV2M4gwH+YwCGIasIUwhT25BVBADtzYNYrHvv4gAAAABJRU5ErkJggg==":
/*!**********************************************************************************************************************************************!*\
  !*** data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAIAAADZF8uwAAAAGUlEQVQYV2M4gwH+YwCGIasIUwhT25BVBADtzYNYrHvv4gAAAABJRU5ErkJggg== ***!
  \**********************************************************************************************************************************************/
/***/ ((module) => {

module.exports = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAIAAADZF8uwAAAAGUlEQVQYV2M4gwH+YwCGIasIUwhT25BVBADtzYNYrHvv4gAAAABJRU5ErkJggg==";

/***/ }),

/***/ "data:image/svg+xml;utf8,%3Csvg class=%27icon%27 width=%27200%27 height=%27200%27 viewBox=%270 0 1024 1024%27 xmlns=%27http://www.w3.org/2000/svg%27%3E%3Cpath fill=%27currentColor%27 d=%27M406.656 706.944L195.84 496.256a32 32 0 10-45.248 45.248l256 256 512-512a32 32 0 00-45.248-45.248L406.592 706.944z%27%3E%3C/path%3E%3C/svg%3E":
/*!************************************************************************************************************************************************************************************************************************************************************************************************************************************************!*\
  !*** data:image/svg+xml;utf8,%3Csvg class=%27icon%27 width=%27200%27 height=%27200%27 viewBox=%270 0 1024 1024%27 xmlns=%27http://www.w3.org/2000/svg%27%3E%3Cpath fill=%27currentColor%27 d=%27M406.656 706.944L195.84 496.256a32 32 0 10-45.248 45.248l256 256 512-512a32 32 0 00-45.248-45.248L406.592 706.944z%27%3E%3C/path%3E%3C/svg%3E ***!
  \************************************************************************************************************************************************************************************************************************************************************************************************************************************************/
/***/ ((module) => {

module.exports = "data:image/svg+xml;utf8,%3Csvg class=%27icon%27 width=%27200%27 height=%27200%27 viewBox=%270 0 1024 1024%27 xmlns=%27http://www.w3.org/2000/svg%27%3E%3Cpath fill=%27currentColor%27 d=%27M406.656 706.944L195.84 496.256a32 32 0 10-45.248 45.248l256 256 512-512a32 32 0 00-45.248-45.248L406.592 706.944z%27%3E%3C/path%3E%3C/svg%3E";

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			id: moduleId,
/******/ 			loaded: false,
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Flag the module as loaded
/******/ 		module.loaded = true;
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = __webpack_modules__;
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/chunk loaded */
/******/ 	(() => {
/******/ 		var deferred = [];
/******/ 		__webpack_require__.O = (result, chunkIds, fn, priority) => {
/******/ 			if(chunkIds) {
/******/ 				priority = priority || 0;
/******/ 				for(var i = deferred.length; i > 0 && deferred[i - 1][2] > priority; i--) deferred[i] = deferred[i - 1];
/******/ 				deferred[i] = [chunkIds, fn, priority];
/******/ 				return;
/******/ 			}
/******/ 			var notFulfilled = Infinity;
/******/ 			for (var i = 0; i < deferred.length; i++) {
/******/ 				var [chunkIds, fn, priority] = deferred[i];
/******/ 				var fulfilled = true;
/******/ 				for (var j = 0; j < chunkIds.length; j++) {
/******/ 					if ((priority & 1 === 0 || notFulfilled >= priority) && Object.keys(__webpack_require__.O).every((key) => (__webpack_require__.O[key](chunkIds[j])))) {
/******/ 						chunkIds.splice(j--, 1);
/******/ 					} else {
/******/ 						fulfilled = false;
/******/ 						if(priority < notFulfilled) notFulfilled = priority;
/******/ 					}
/******/ 				}
/******/ 				if(fulfilled) {
/******/ 					deferred.splice(i--, 1)
/******/ 					var r = fn();
/******/ 					if (r !== undefined) result = r;
/******/ 				}
/******/ 			}
/******/ 			return result;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/compat get default export */
/******/ 	(() => {
/******/ 		// getDefaultExport function for compatibility with non-harmony modules
/******/ 		__webpack_require__.n = (module) => {
/******/ 			var getter = module && module.__esModule ?
/******/ 				() => (module['default']) :
/******/ 				() => (module);
/******/ 			__webpack_require__.d(getter, { a: getter });
/******/ 			return getter;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/global */
/******/ 	(() => {
/******/ 		__webpack_require__.g = (function() {
/******/ 			if (typeof globalThis === 'object') return globalThis;
/******/ 			try {
/******/ 				return this || new Function('return this')();
/******/ 			} catch (e) {
/******/ 				if (typeof window === 'object') return window;
/******/ 			}
/******/ 		})();
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/node module decorator */
/******/ 	(() => {
/******/ 		__webpack_require__.nmd = (module) => {
/******/ 			module.paths = [];
/******/ 			if (!module.children) module.children = [];
/******/ 			return module;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/jsonp chunk loading */
/******/ 	(() => {
/******/ 		__webpack_require__.b = document.baseURI || self.location.href;
/******/ 		
/******/ 		// object to store loaded and loading chunks
/******/ 		// undefined = chunk not loaded, null = chunk preloaded/prefetched
/******/ 		// [resolve, reject, Promise] = chunk loading, 0 = chunk loaded
/******/ 		var installedChunks = {
/******/ 			"MediaExternalLink": 0
/******/ 		};
/******/ 		
/******/ 		// no chunk on demand loading
/******/ 		
/******/ 		// no prefetching
/******/ 		
/******/ 		// no preloaded
/******/ 		
/******/ 		// no HMR
/******/ 		
/******/ 		// no HMR manifest
/******/ 		
/******/ 		__webpack_require__.O.j = (chunkId) => (installedChunks[chunkId] === 0);
/******/ 		
/******/ 		// install a JSONP callback for chunk loading
/******/ 		var webpackJsonpCallback = (parentChunkLoadingFunction, data) => {
/******/ 			var [chunkIds, moreModules, runtime] = data;
/******/ 			// add "moreModules" to the modules object,
/******/ 			// then flag all "chunkIds" as loaded and fire callback
/******/ 			var moduleId, chunkId, i = 0;
/******/ 			if(chunkIds.some((id) => (installedChunks[id] !== 0))) {
/******/ 				for(moduleId in moreModules) {
/******/ 					if(__webpack_require__.o(moreModules, moduleId)) {
/******/ 						__webpack_require__.m[moduleId] = moreModules[moduleId];
/******/ 					}
/******/ 				}
/******/ 				if(runtime) var result = runtime(__webpack_require__);
/******/ 			}
/******/ 			if(parentChunkLoadingFunction) parentChunkLoadingFunction(data);
/******/ 			for(;i < chunkIds.length; i++) {
/******/ 				chunkId = chunkIds[i];
/******/ 				if(__webpack_require__.o(installedChunks, chunkId) && installedChunks[chunkId]) {
/******/ 					installedChunks[chunkId][0]();
/******/ 				}
/******/ 				installedChunks[chunkId] = 0;
/******/ 			}
/******/ 			return __webpack_require__.O(result);
/******/ 		}
/******/ 		
/******/ 		var chunkLoadingGlobal = self["webpackChunkDF_Telegram_VueApp"] = self["webpackChunkDF_Telegram_VueApp"] || [];
/******/ 		chunkLoadingGlobal.forEach(webpackJsonpCallback.bind(null, 0));
/******/ 		chunkLoadingGlobal.push = webpackJsonpCallback.bind(null, chunkLoadingGlobal.push.bind(chunkLoadingGlobal));
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/nonce */
/******/ 	(() => {
/******/ 		__webpack_require__.nc = undefined;
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module depends on other loaded chunks and execution need to be delayed
/******/ 	var __webpack_exports__ = __webpack_require__.O(undefined, ["vendors-node_modules_pnpm_ctrl_tinycolor_3_6_1_node_modules_ctrl_tinycolor_dist_module_public-a7e4d6","vendors-node_modules_pnpm_dayjs_1_11_9_node_modules_dayjs_dayjs_min_js-node_modules_pnpm_dayj-a8c611","vendors-node_modules_pnpm_css-loader_6_8_1_webpack_5_88_2_node_modules_css-loader_dist_cjs_js-bdb3e9","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-09dc22","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-87ea0b","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-342fa4","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-dfddbe","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-53527c","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-7e5e52","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-fe17a3","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-9a474c","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_compone-4a10fc","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_index_js","vendors-node_modules_pnpm_element-plus_2_3_12_vue_3_3_4_node_modules_element-plus_lib_plugin_-031b74","vendors-node_modules_pnpm_element-plus_icons-vue_2_1_0_vue_3_3_4_node_modules_element-plus_ic-169417","vendors-node_modules_pnpm_floating-ui_dom_1_5_1_node_modules_floating-ui_dom_dist_floating-ui-a31027","vendors-node_modules_pnpm_lodash_4_17_21_node_modules_lodash_lodash_js","vendors-node_modules_pnpm_vue_compiler-dom_3_3_4_node_modules_vue_compiler-dom_dist_compiler--b20598","vendors-node_modules_pnpm_vue_runtime-dom_3_3_4_node_modules_vue_runtime-dom_dist_runtime-dom-766408","vendors-node_modules_pnpm_vue_shared_3_3_4_node_modules_vue_shared_dist_shared_esm-bundler_js-f0e52f"], () => (__webpack_require__("./src/Media/ExternalLink/main.ts")))
/******/ 	__webpack_exports__ = __webpack_require__.O(__webpack_exports__);
/******/ 	
/******/ })()
;
//# sourceMappingURL=MediaExternalLink.entry.js.map