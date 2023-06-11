var uploadUrl = "https://{{{ AWS_BUCKET_NAME }}}.s3-accelerate.dualstack.amazonaws.com/Web/{{{ BUILD_CONFIG_NAME }}}/{{{ PRODUCT_NAME }}}";

var buildUrl = uploadUrl + "/Build";
var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
var unityConfig = {
  dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
  frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
#if USE_WASM
  codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
#endif
#if MEMORY_FILENAME
  memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
#endif
#if SYMBOLS_FILENAME
  symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
#endif
  streamingAssetsUrl: uploadUrl + "/StreamingAssets",
  companyName: "{{{ COMPANY_NAME }}}",
  productName: "{{{ PRODUCT_NAME }}}",
  productVersion: "{{{ PRODUCT_VERSION }}}",
};