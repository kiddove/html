/*
* Fingerprintjs2 0.0.5 - Modern & flexible browser fingerprint library v2
* https://github.com/Valve/fingerprintjs2
* Copyright (c) 2015 Valentin Vasilyev (valentin.vasilyev@outlook.com)
* Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php) license.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
* THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
(function (name, context, definition) {
  "use strict";
  if (typeof module !== "undefined" && module.exports) { module.exports = definition(); }
  else if (typeof define === "function" && define.amd) { define(definition); }
  else { context[name] = definition(); }
})("Fingerprint2", this, function() {
  "use strict";
  var DEBUG = true;
  var Fingerprint2 = function(options) {
    var defaultOptions = {
      swfContainerId: "fingerprintjs2",
      swfPath: "flash/compiled/FontList.swf"
    };
    this.options = this.extend(options, defaultOptions);
    this.nativeForEach = Array.prototype.forEach;
    this.nativeMap = Array.prototype.map;
  };
  Fingerprint2.prototype = {
    extend: function(source, target) {
      if (source == null) { return target; }
      for (var k in source) {
        if(source[k] != null && target[k] !== source[k]) {
          target[k] = source[k];
        }
      }
      return target;
    },
    log: function(msg){
      if(window.console){
        console.log(msg);
      }
    },
    get: function(done){
      var keys = [];
      keys = this.userAgentKey(keys);
      keys = this.languageKey(keys);
      keys = this.colorDepthKey(keys);
      keys = this.screenResolutionKey(keys);
      keys = this.timezoneOffsetKey(keys);
      keys = this.sessionStorageKey(keys);
      keys = this.localStorageKey(keys);
      keys = this.indexedDbKey(keys);
      keys = this.addBehaviorKey(keys);
      keys = this.openDatabaseKey(keys);
      keys = this.cpuClassKey(keys);
      keys = this.platformKey(keys);
      keys = this.doNotTrackKey(keys);
      keys = this.canvasKey(keys);
      keys = this.webglKey(keys);
      var that = this;
      this.fontsKey(keys, function(newKeys){
        var murmur = that.x64hash128(newKeys.join("~~~"), 31);
        return done(murmur);
      });
    },

    userAgentKey: function(keys) {
        if (!this.options.excludeUserAgent) {
            if (keys !== undefined) {
                keys.push(navigator.userAgent);
                return keys;
            }
            else return navigator.userAgent;
      }
      return null;
    },
    languageKey: function(keys) {
        if (!this.options.excludeLanguage) {
            if (keys !== undefined) {
                keys.push(navigator.language);
                return keys;
            }
            else return navigator.language;
      }
      return null;
    },
    colorDepthKey: function(keys) {
        if (!this.options.excludeColorDepth) {
            if (keys !== undefined) {
                keys.push(screen.colorDepth);
                return keys;
            }
            else
                return screen.colorDepth;
      }
      return null;
    },
    screenResolutionKey: function(keys) {
      if(!this.options.excludeScreenResolution) {
        var resolution = this.getScreenResolution();
        if (typeof resolution !== "undefined") { // headless browsers, such as phantomjs
            if (keys !== undefined) {
                keys.push(resolution.join("x"));
                return keys;
            }
            else
                return resolution.join("x");
        }
      }
      return null;
    },
    getScreenResolution: function () {
      var resolution;
      if(this.options.detectScreenOrientation) {
        resolution = (screen.height > screen.width) ? [screen.height, screen.width] : [screen.width, screen.height];
      } else {
        resolution = [screen.height, screen.width];
      }
      return resolution;
    },
    timezoneOffsetKey: function(keys) {
        if (!this.options.excludeTimezoneOffset) {
            if (keys !== undefined) {
                keys.push(new Date().getTimezoneOffset());
                return keys;
            }
            else
                return new Date().getTimezoneOffset();
      }
      return null;
    },
    sessionStorageKey: function(keys) {
      if(!this.options.excludeSessionStorage && this.hasSessionStorage()) {
        keys.push("sessionStorageKey");
      }
      return keys;
    },
    localStorageKey: function(keys) {
      if(!this.options.excludeSessionStorage && this.hasLocalStorage()) {
        keys.push("localStorageKey");
      }
      return keys;
    },
    indexedDbKey: function(keys) {
      if(!this.options.excludeIndexedDB && this.hasIndexedDB()) {
        keys.push("indexedDbKey");
      }
      return keys;
    },
    addBehaviorKey: function(keys) {
      //body might not be defined at this point or removed programmatically
      if(document.body && !this.options.excludeAddBehavior && document.body.addBehavior) {
        keys.push("addBehaviorKey");
      }
      return keys;
    },
    openDatabaseKey: function(keys) {
      if(!this.options.excludeOpenDatabase && window.openDatabase) {
        keys.push("openDatabase");
      }
      return keys;
    },
    cpuClassKey: function(keys) {
        if (!this.options.excludeCpuClass) {
            if (keys !== undefined) {
                keys.push(this.getNavigatorCpuClass());
                return keys;
            }
            else
                return this.getNavigatorCpuClass();
      }
      return null;
    },
    platformKey: function(keys) {
        if (!this.options.excludePlatform) {
            if (keys !== undefined) {
                keys.push(this.getNavigatorPlatform());
                return keys;
            }
            else
                return this.getNavigatorPlatform();
      }
      return null;
    },
    doNotTrackKey: function(keys) {
        if (!this.options.excludeDoNotTrack) {
            if (keys !== undefined) {
                keys.push(this.getDoNotTrack());
                return keys;
            }
            else
                return this.getDoNotTrack();
      }
      return null;
    },
    canvasKey: function(keys) {
      if(!this.options.excludeCanvas && this.isCanvasSupported()) {
        keys.push(this.getCanvasFp());
      }
      return keys;
    },
    webglKey: function(keys) {
      if(!this.options.excludeWebGL && this.isCanvasSupported()) {
        keys.push(this.getWebglFp());
      }
      return keys;
    },
    fontsKey: function(keys, done) {
      if(this.options.excludeFlashFonts) {
        if(DEBUG){
          this.log("Skipping flash fonts detection per excludeFlashFonts configuration option");
        }
        if(this.options.excludeJsFonts) {
          if(DEBUG) {
            this.log("Skipping js fonts detection per excludeJsFonts configuration option");
          }
          return done(keys);
        }
        return done(this.jsFontsKey(keys));
      }
      // we do flash if swfobject is loaded
      if(!this.hasSwfObjectLoaded()){
        if(DEBUG){
          this.log("Swfobject is not detected, Flash fonts enumeration is skipped");
        }
        return done(this.jsFontsKey(keys));
      }
      if(!this.hasMinFlashInstalled()){
        if(DEBUG){
          this.log("Flash is not installed, skipping Flash fonts enumeration");
        }
        return done(this.jsFontsKey(keys));
      }
      if(typeof this.options.swfPath === "undefined"){
        if(DEBUG){
          this.log("To use Flash fonts detection, you must pass a valid swfPath option, skipping Flash fonts enumeration");
        }
        return done(this.jsFontsKey(keys));
      }
      return this.flashFontsKey(keys, done);
    },
    // flash fonts (will increase fingerprinting time 20X to ~ 130-150ms)
    flashFontsKey: function(keys, done) {
      this.loadSwfAndDetectFonts(function(fonts){
        keys.push(fonts.join(";"));
        done(keys);
      });
    },
    // kudos to http://www.lalit.org/lab/javascript-css-font-detect/
    jsFontsKey: function(keys) {
      // a font will be compared against all the three default fonts.
      // and if it doesn't match all 3 then that font is not available.
      var baseFonts = ["monospace", "sans-serif", "serif"];

      //we use m or w because these two characters take up the maximum width.
      // And we use a LLi so that the same matching fonts can get separated
      var testString = "mmmmmmmmmmlli";

      //we test using 72px font size, we may use any size. I guess larger the better.
      var testSize = "72px";

      var h = document.getElementsByTagName("body")[0];

      // create a SPAN in the document to get the width of the text we use to test
      var s = document.createElement("span");
      s.style.fontSize = testSize;
      s.innerHTML = testString;
      var defaultWidth = {};
      var defaultHeight = {};
      for (var index in baseFonts) {
          //get the default width for the three base fonts
          s.style.fontFamily = baseFonts[index];
          h.appendChild(s);
          defaultWidth[baseFonts[index]] = s.offsetWidth; //width for the default font
          defaultHeight[baseFonts[index]] = s.offsetHeight; //height for the defualt font
          h.removeChild(s);
      }
      var detect = function (font) {
          var detected = false;
          for (var index in baseFonts) {
              s.style.fontFamily = font + "," + baseFonts[index]; // name of the font along with the base font for fallback.
              h.appendChild(s);
              var matched = (s.offsetWidth !== defaultWidth[baseFonts[index]] || s.offsetHeight !== defaultHeight[baseFonts[index]]);
              h.removeChild(s);
              detected = detected || matched;
          }
          return detected;
      };

      var fontList = ["Arial Black", "Arial Narrow", "Arial Rounded MT Bold", "Arial", "Bookman Old Style", "Bradley Hand ITC", "Century Gothic", "Century", "Comic Sans MS", "Courier New", "Courier", "Cursive", "Fantasy", "Gentium", "Georgia", "Impact", "King", "Lalit", "Lucida Console", "Modena", "Monospace", "Monotype Corsiva", "Papyrus", "Sans-Serif", "Serif", "Tahoma", "TeX", "Times New Roman", "Times", "Trebuchet MS", "Verdana", "Verona"];
      var available = [];
      for (var i = 0, l = fontList.length; i < l; i++) {
        if(detect(fontList[i])) {
          available.push(fontList[i]);
        }
      }
      keys.push(available.join(";"));
      return keys;
    },
    hasSessionStorage: function () {
      try {
        return !!window.sessionStorage;
      } catch(e) {
        return true; // SecurityError when referencing it means it exists
      }
    },
    // https://bugzilla.mozilla.org/show_bug.cgi?id=781447
    hasLocalStorage: function () {
      try {
        return !!window.localStorage;
      } catch(e) {
        return true; // SecurityError when referencing it means it exists
      }
    },
    hasIndexedDB: function (){
      return !!window.indexedDB;
    },
    getNavigatorCpuClass: function () {
      if(navigator.cpuClass){
        return "navigatorCpuClass: " + navigator.cpuClass;
      } else {
        return "navigatorCpuClass: unknown";
      }
    },
    getNavigatorPlatform: function () {
      if(navigator.platform) {
        return "navigatorPlatform: " + navigator.platform;
      } else {
        return "navigatorPlatform: unknown";
      }
    },
    getDoNotTrack: function () {
      if(navigator.doNotTrack) {
        return "doNotTrack: " + navigator.doNotTrack;
      } else {
        return "doNotTrack: unknown";
      }
    },
    getCanvasFp: function() {
      // Very simple now, need to make it more complex (geo shapes etc)
      var canvas = document.createElement("canvas");
      var ctx = canvas.getContext("2d");
      // https://www.browserleaks.com/canvas#how-does-it-work
      var txt = "Cwm fjordbank glyphs vext quiz, https://github.com/valve ὠ";
      ctx.textBaseline = "top";
      ctx.font = "70px 'Arial'";
      ctx.textBaseline = "alphabetic";
      ctx.fillStyle = "#f60";
      ctx.fillRect(125, 1, 62, 20);
      ctx.fillStyle = "#069";
      ctx.fillText(txt, 2, 15);
      ctx.fillStyle = "rgba(102, 204, 0, 0.7)";
      ctx.fillText(txt, 4, 17);
      return canvas.toDataURL();
    },

    getWebglFp: function() {
      var gl;
      var fa2s = function(fa) {
      gl.clearColor(0.0, 0.0, 0.0, 1.0);
      gl.enable(gl.DEPTH_TEST);
      gl.depthFunc(gl.LEQUAL);
      gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        return "[" + fa[0] + ", " + fa[1] + "]";
      };
      var maxAnisotropy = function(gl) {
        var anisotropy, ext = gl.getExtension("EXT_texture_filter_anisotropic") || gl.getExtension("WEBKIT_EXT_texture_filter_anisotropic") || gl.getExtension("MOZ_EXT_texture_filter_anisotropic");
        return ext ? (anisotropy = gl.getParameter(ext.MAX_TEXTURE_MAX_ANISOTROPY_EXT), 0 === anisotropy && (anisotropy = 2), anisotropy) : null;
      };
      gl = this.getWebglCanvas();
      if(!gl) { return null; }
      // WebGL fingerprinting is a combination of techniques, found in MaxMind antifraud script & Augur fingerprinting.
      // First it draws a gradient object with shaders and convers the image to the Base64 string.
      // Then it enumerates all WebGL extensions & capabilities and appends them to the Base64 string, resulting in a huge WebGL string, potentially very unique on each device
      // Since iOS supports webgl starting from version 8.1 and 8.1 runs on several graphics chips, the results may be different across ios devices, but we need to verify it.
      var result = [];
      var canvas = document.createElement("canvas");
      var vShaderTemplate = "attribute vec2 attrVertex;varying vec2 varyinTexCoordinate;uniform vec2 uniformOffset;void main(){varyinTexCoordinate=attrVertex+uniformOffset;gl_Position=vec4(attrVertex,0,1);}";
      var fShaderTemplate = "precision mediump float;varying vec2 varyinTexCoordinate;void main() {gl_FragColor=vec4(varyinTexCoordinate,0,1);}";
      var vertexPosBuffer = gl.createBuffer();
      gl.bindBuffer(gl.ARRAY_BUFFER, vertexPosBuffer);
      var vertices = new Float32Array([-.2, -.9, 0, .4, -.26, 0, 0, .732134444, 0]);
      gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);
      vertexPosBuffer.itemSize = 3;
      vertexPosBuffer.numItems = 3;
      var program = gl.createProgram(), vshader = gl.createShader(gl.VERTEX_SHADER);
      gl.shaderSource(vshader, vShaderTemplate);
      gl.compileShader(vshader);
      var fshader = gl.createShader(gl.FRAGMENT_SHADER);
      gl.shaderSource(fshader, fShaderTemplate);
      gl.compileShader(fshader);
      gl.attachShader(program, vshader);
      gl.attachShader(program, fshader);
      gl.linkProgram(program);
      gl.useProgram(program);
      program.vertexPosAttrib = gl.getAttribLocation(program, "attrVertex");
      program.offsetUniform = gl.getUniformLocation(program, "uniformOffset");
      gl.enableVertexAttribArray(program.vertexPosArray);
      gl.vertexAttribPointer(program.vertexPosAttrib, vertexPosBuffer.itemSize, gl.FLOAT, !1, 0, 0);
      gl.uniform2f(program.offsetUniform, 1, 1);
      gl.drawArrays(gl.TRIANGLE_STRIP, 0, vertexPosBuffer.numItems);
      result.push(canvas.toDataURL());
      result.push("extensions:" + gl.getSupportedExtensions().join(";"));
      result.push("webgl aliased line width range:" + fa2s(gl.getParameter(gl.ALIASED_LINE_WIDTH_RANGE)));
      result.push("webgl aliased point size range:" + fa2s(gl.getParameter(gl.ALIASED_POINT_SIZE_RANGE)));
      result.push("webgl alpha bits:" + gl.getParameter(gl.ALPHA_BITS));
      result.push("webgl antialiasing:" + (gl.getContextAttributes().antialias ? "yes" : "no"));
      result.push("webgl blue bits:" + gl.getParameter(gl.BLUE_BITS));
      result.push("webgl depth bits:" + gl.getParameter(gl.DEPTH_BITS));
      result.push("webgl green bits:" + gl.getParameter(gl.GREEN_BITS));
      result.push("webgl max anisotropy:" + maxAnisotropy(gl));
      result.push("webgl max combined texture image units:" + gl.getParameter(gl.MAX_COMBINED_TEXTURE_IMAGE_UNITS));
      result.push("webgl max cube map texture size:" + gl.getParameter(gl.MAX_CUBE_MAP_TEXTURE_SIZE));
      result.push("webgl max fragment uniform vectors:" + gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS));
      result.push("webgl max render buffer size:" + gl.getParameter(gl.MAX_RENDERBUFFER_SIZE));
      result.push("webgl max texture image units:" + gl.getParameter(gl.MAX_TEXTURE_IMAGE_UNITS));
      result.push("webgl max texture size:" + gl.getParameter(gl.MAX_TEXTURE_SIZE));
      result.push("webgl max varying vectors:" + gl.getParameter(gl.MAX_VARYING_VECTORS));
      result.push("webgl max vertex attribs:" + gl.getParameter(gl.MAX_VERTEX_ATTRIBS));
      result.push("webgl max vertex texture image units:" + gl.getParameter(gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS));
      result.push("webgl max vertex uniform vectors:" + gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS));
      result.push("webgl max viewport dims:" + fa2s(gl.getParameter(gl.MAX_VIEWPORT_DIMS)));
      result.push("webgl red bits:" + gl.getParameter(gl.RED_BITS));
      result.push("webgl renderer:" + gl.getParameter(gl.RENDERER));
      result.push("webgl shading language version:" + gl.getParameter(gl.SHADING_LANGUAGE_VERSION));
      result.push("webgl stencil bits:" + gl.getParameter(gl.STENCIL_BITS));
      result.push("webgl vendor:" + gl.getParameter(gl.VENDOR));
      result.push("webgl version:" + gl.getParameter(gl.VERSION));
      //TODO: implement vertex shader & fragment shader precision
      return result.join("§");
    },
    isCanvasSupported: function () {
      var elem = document.createElement("canvas");
      return !!(elem.getContext && elem.getContext("2d"));
    },
    hasSwfObjectLoaded: function(){
      return typeof window.swfobject !== "undefined";
    },
    hasMinFlashInstalled: function () {
      return swfobject.hasFlashPlayerVersion("9.0.0");
    },
    addFlashDivNode: function() {
      var node = document.createElement("div");
      node.setAttribute("id", this.options.swfContainerId);
      document.body.appendChild(node);
    },
    loadSwfAndDetectFonts: function(done) {
      var hiddenCallback = "___fp_swf_loaded";
      window[hiddenCallback] = function(fonts) {
        done(fonts);
      };
      var id = this.options.swfContainerId;
      this.addFlashDivNode();
      var flashvars = { onReady: hiddenCallback};
      var flashparams = { allowScriptAccess: "always", menu: "false" };
      swfobject.embedSWF(this.options.swfPath, id, "1", "1", "9.0.0", false, flashvars, flashparams, {});
    },
    getWebglCanvas: function() {
      var canvas = document.createElement("canvas");
      var gl = null;
      try {
        gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
      } catch(e) {}
      if(!gl){gl = null;}
      return gl;
    },
    each: function (obj, iterator, context) {
      if (obj === null) {
        return;
      }
      if (this.nativeForEach && obj.forEach === this.nativeForEach) {
        obj.forEach(iterator, context);
      } else if (obj.length === +obj.length) {
        for (var i = 0, l = obj.length; i < l; i++) {
          if (iterator.call(context, obj[i], i, obj) === {}) { return; }
        }
      } else {
        for (var key in obj) {
          if (obj.hasOwnProperty(key)) {
            if (iterator.call(context, obj[key], key, obj) === {}) { return; }
          }
        }
      }
    },

    map: function(obj, iterator, context) {
      var results = [];
      // Not using strict equality so that this acts as a
      // shortcut to checking for `null` and `undefined`.
      if (obj == null) { return results; }
      if (this.nativeMap && obj.map === this.nativeMap) { return obj.map(iterator, context); }
      this.each(obj, function(value, index, list) {
        results[results.length] = iterator.call(context, value, index, list);
      });
      return results;
    },

    /// MurmurHash3 related functions

    //
    // Given two 64bit ints (as an array of two 32bit ints) returns the two
    // added together as a 64bit int (as an array of two 32bit ints).
    //
    x64Add: function(m, n) {
      m = [m[0] >>> 16, m[0] & 0xffff, m[1] >>> 16, m[1] & 0xffff];
      n = [n[0] >>> 16, n[0] & 0xffff, n[1] >>> 16, n[1] & 0xffff];
      var o = [0, 0, 0, 0];
      o[3] += m[3] + n[3];
      o[2] += o[3] >>> 16;
      o[3] &= 0xffff;
      o[2] += m[2] + n[2];
      o[1] += o[2] >>> 16;
      o[2] &= 0xffff;
      o[1] += m[1] + n[1];
      o[0] += o[1] >>> 16;
      o[1] &= 0xffff;
      o[0] += m[0] + n[0];
      o[0] &= 0xffff;
      return [(o[0] << 16) | o[1], (o[2] << 16) | o[3]];
    },

    //
    // Given two 64bit ints (as an array of two 32bit ints) returns the two
    // multiplied together as a 64bit int (as an array of two 32bit ints).
    //
    x64Multiply: function(m, n) {
      m = [m[0] >>> 16, m[0] & 0xffff, m[1] >>> 16, m[1] & 0xffff];
      n = [n[0] >>> 16, n[0] & 0xffff, n[1] >>> 16, n[1] & 0xffff];
      var o = [0, 0, 0, 0];
      o[3] += m[3] * n[3];
      o[2] += o[3] >>> 16;
      o[3] &= 0xffff;
      o[2] += m[2] * n[3];
      o[1] += o[2] >>> 16;
      o[2] &= 0xffff;
      o[2] += m[3] * n[2];
      o[1] += o[2] >>> 16;
      o[2] &= 0xffff;
      o[1] += m[1] * n[3];
      o[0] += o[1] >>> 16;
      o[1] &= 0xffff;
      o[1] += m[2] * n[2];
      o[0] += o[1] >>> 16;
      o[1] &= 0xffff;
      o[1] += m[3] * n[1];
      o[0] += o[1] >>> 16;
      o[1] &= 0xffff;
      o[0] += (m[0] * n[3]) + (m[1] * n[2]) + (m[2] * n[1]) + (m[3] * n[0]);
      o[0] &= 0xffff;
      return [(o[0] << 16) | o[1], (o[2] << 16) | o[3]];
    },
    //
    // Given a 64bit int (as an array of two 32bit ints) and an int
    // representing a number of bit positions, returns the 64bit int (as an
    // array of two 32bit ints) rotated left by that number of positions.
    //
    x64Rotl: function(m, n) {
      n %= 64;
      if (n === 32) {
        return [m[1], m[0]];
      }
      else if (n < 32) {
        return [(m[0] << n) | (m[1] >>> (32 - n)), (m[1] << n) | (m[0] >>> (32 - n))];
      }
      else {
        n -= 32;
        return [(m[1] << n) | (m[0] >>> (32 - n)), (m[0] << n) | (m[1] >>> (32 - n))];
      }
    },
    //
    // Given a 64bit int (as an array of two 32bit ints) and an int
    // representing a number of bit positions, returns the 64bit int (as an
    // array of two 32bit ints) shifted left by that number of positions.
    //
    x64LeftShift: function(m, n) {
      n %= 64;
      if (n === 0) {
        return m;
      }
      else if (n < 32) {
        return [(m[0] << n) | (m[1] >>> (32 - n)), m[1] << n];
      }
      else {
        return [m[1] << (n - 32), 0];
      }
    },
    //
    // Given two 64bit ints (as an array of two 32bit ints) returns the two
    // xored together as a 64bit int (as an array of two 32bit ints).
    //
    x64Xor: function(m, n) {
      return [m[0] ^ n[0], m[1] ^ n[1]];
    },
    //
    // Given a block, returns murmurHash3's final x64 mix of that block.
    // (`[0, h[0] >>> 1]` is a 33 bit unsigned right shift. This is the
    // only place where we need to right shift 64bit ints.)
    //
    x64Fmix: function(h) {
      h = this.x64Xor(h, [0, h[0] >>> 1]);
      h = this.x64Multiply(h, [0xff51afd7, 0xed558ccd]);
      h = this.x64Xor(h, [0, h[0] >>> 1]);
      h = this.x64Multiply(h, [0xc4ceb9fe, 0x1a85ec53]);
      h = this.x64Xor(h, [0, h[0] >>> 1]);
      return h;
    },

    //
    // Given a string and an optional seed as an int, returns a 128 bit
    // hash using the x64 flavor of MurmurHash3, as an unsigned hex.
    //
    x64hash128: function (key, seed) {
      key = key || "";
      seed = seed || 0;
      var remainder = key.length % 16;
      var bytes = key.length - remainder;
      var h1 = [0, seed];
      var h2 = [0, seed];
      var k1 = [0, 0];
      var k2 = [0, 0];
      var c1 = [0x87c37b91, 0x114253d5];
      var c2 = [0x4cf5ad43, 0x2745937f];
      for (var i = 0; i < bytes; i = i + 16) {
        k1 = [((key.charCodeAt(i + 4) & 0xff)) | ((key.charCodeAt(i + 5) & 0xff) << 8) | ((key.charCodeAt(i + 6) & 0xff) << 16) | ((key.charCodeAt(i + 7) & 0xff) << 24), ((key.charCodeAt(i) & 0xff)) | ((key.charCodeAt(i + 1) & 0xff) << 8) | ((key.charCodeAt(i + 2) & 0xff) << 16) | ((key.charCodeAt(i + 3) & 0xff) << 24)];
        k2 = [((key.charCodeAt(i + 12) & 0xff)) | ((key.charCodeAt(i + 13) & 0xff) << 8) | ((key.charCodeAt(i + 14) & 0xff) << 16) | ((key.charCodeAt(i + 15) & 0xff) << 24), ((key.charCodeAt(i + 8) & 0xff)) | ((key.charCodeAt(i + 9) & 0xff) << 8) | ((key.charCodeAt(i + 10) & 0xff) << 16) | ((key.charCodeAt(i + 11) & 0xff) << 24)];
        k1 = this.x64Multiply(k1, c1);
        k1 = this.x64Rotl(k1, 31);
        k1 = this.x64Multiply(k1, c2);
        h1 = this.x64Xor(h1, k1);
        h1 = this.x64Rotl(h1, 27);
        h1 = this.x64Add(h1, h2);
        h1 = this.x64Add(this.x64Multiply(h1, [0, 5]), [0, 0x52dce729]);
        k2 = this.x64Multiply(k2, c2);
        k2 = this.x64Rotl(k2, 33);
        k2 = this.x64Multiply(k2, c1);
        h2 = this.x64Xor(h2, k2);
        h2 = this.x64Rotl(h2, 31);
        h2 = this.x64Add(h2, h1);
        h2 = this.x64Add(this.x64Multiply(h2, [0, 5]), [0, 0x38495ab5]);
      }
      k1 = [0, 0];
      k2 = [0, 0];
      switch(remainder) {
        case 15:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 14)], 48));
        case 14:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 13)], 40));
        case 13:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 12)], 32));
        case 12:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 11)], 24));
        case 11:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 10)], 16));
        case 10:
          k2 = this.x64Xor(k2, this.x64LeftShift([0, key.charCodeAt(i + 9)], 8));
        case 9:
          k2 = this.x64Xor(k2, [0, key.charCodeAt(i + 8)]);
          k2 = this.x64Multiply(k2, c2);
          k2 = this.x64Rotl(k2, 33);
          k2 = this.x64Multiply(k2, c1);
          h2 = this.x64Xor(h2, k2);
        case 8:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 7)], 56));
        case 7:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 6)], 48));
        case 6:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 5)], 40));
        case 5:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 4)], 32));
        case 4:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 3)], 24));
        case 3:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 2)], 16));
        case 2:
          k1 = this.x64Xor(k1, this.x64LeftShift([0, key.charCodeAt(i + 1)], 8));
        case 1:
          k1 = this.x64Xor(k1, [0, key.charCodeAt(i)]);
          k1 = this.x64Multiply(k1, c1);
          k1 = this.x64Rotl(k1, 31);
          k1 = this.x64Multiply(k1, c2);
          h1 = this.x64Xor(h1, k1);
      }
      h1 = this.x64Xor(h1, [0, key.length]);
      h2 = this.x64Xor(h2, [0, key.length]);
      h1 = this.x64Add(h1, h2);
      h2 = this.x64Add(h2, h1);
      h1 = this.x64Fmix(h1);
      h2 = this.x64Fmix(h2);
      h1 = this.x64Add(h1, h2);
      h2 = this.x64Add(h2, h1);
      return ("00000000" + (h1[0] >>> 0).toString(16)).slice(-8) + ("00000000" + (h1[1] >>> 0).toString(16)).slice(-8) + ("00000000" + (h2[0] >>> 0).toString(16)).slice(-8) + ("00000000" + (h2[1] >>> 0).toString(16)).slice(-8);
    }
  };
  return Fingerprint2;
});

//var WhichBrowser = (function () {

//    var WhichBrowser = function () { this.initialize.apply(this, arguments) };
//    WhichBrowser.prototype = {
//        initialize: function (options) {
//            this.options = {
//            }

//            this.browser = new Browser({ stock: false, hidden: false, channel: "", mode: "", name: "Chrome", version: new Version({ value: "42.0.2311.135", details: 1 }) });
//            this.engine = new Engine({ name: "Blink", version: new Version({ value: "537.36" }) });
//            this.os = new Os({ name: "Windows", version: new Version({ value: "6.1", alias: "7" }) });
//            this.device = new Device({ type: "desktop", identified: 0, generic: true });
//            this.camouflage = false;
//            this.features = ["webkit", "chrome"];
//        },

//        a: function (s) {
//            return (/^[aeiou]/i.test(s) ? 'an ' : 'a ') + s;
//        },

//        isX: function () {
//            var valid = true;
//            var x = arguments[0];

//            if (arguments.length >= 2) {
//                valid = valid && this[x].name == arguments[1];
//            }

//            if (arguments.length >= 4 && this[x].version && valid) {
//                valid = valid && this[x].version.is(arguments[2], arguments[3]);
//            }

//            return valid;
//        },

//        isBrowser: function () { var a = Array.prototype.slice.call(arguments); a.unshift('browser'); return this.isX.apply(this, a); },
//        isEngine: function () { var a = Array.prototype.slice.call(arguments); a.unshift('engine'); return this.isX.apply(this, a); },
//        isOs: function () { var a = Array.prototype.slice.call(arguments); a.unshift('os'); return this.isX.apply(this, a); },

//        isDevice: function (d) {
//            return this.device.series == d || this.device.model == d;
//        },

//        isType: function () {
//            var valid = false;
//            for (var a = 0; a < arguments.length; a++) valid = valid || arguments[a] == this.device.type;
//            return valid;
//        },

//        toJSON: function () {
//            return {
//                browser: this.browser.toJSON(),
//                os: this.os.toJSON(),
//                engine: this.engine.toJSON(),
//                device: this.device.toJSON()
//            };
//        },

//        toString: function () {
//            var prefix = this.camouflage ? 'an unknown browser that imitates ' : '';
//            var browser = this.browser.toString();
//            var os = this.os.toString();
//            var engine = this.engine.toString();
//            var device = this.device.toString();

//            if (!device && !os && this.device.type == 'television') device = 'television';
//            if (!device && this.device.type == 'emulator') device = 'emulator';

//            if (browser && os && device) return prefix + browser + ' on ' + this.a(device) + ' running ' + os;
//            if (browser && !os && device) return prefix + browser + ' on ' + this.a(device);
//            if (browser && os && !device) return prefix + browser + ' on ' + os;
//            if (!browser && os && device) return prefix + this.a(device) + ' running ' + os;
//            if (browser && !os && !device) return prefix + browser;
//            if (!browser && !os && device) return prefix + this.a(device);
//            if (this.device.type == 'desktop' && os && engine != '' && !device) return 'an unknown browser based on ' + engine + ' running on ' + os;
//            if (this.browser.stock && os && !device) return os;
//            if (this.browser.stock && engine != '' && !device) return 'an unknown browser based on ' + engine;

//            return 'an unknown browser';
//        }
//    };

//    var Browser = function () { this.initialize.apply(this, Array.prototype.slice.call(arguments)) };
//    Browser.prototype = {
//        initialize: function (v) {
//            this.name = v.name || null;
//            this.alias = v.alias || null;
//            this.version = v.version || null;

//            this.stock = v.stock || false;
//            this.channel = v.channel || null;
//            this.mode = v.mode || null;
//            this.hidden = v.hidden || false;
//        },

//        toJSON: function () {
//            return {
//                name: this.name,
//                version: this.version.toJSON(),
//                stock: this.stock,
//                channel: this.channel,
//                mode: this.mode,
//                hidden: this.hidden
//            }
//        },

//        toString: function () {
//            var name = this.alias ? this.alias : (this.name ? this.name : '');
//            return (name ? name + (this.channel ? ' ' + this.channel : '') + (this.version && !this.version.hidden ? ' ' + this.version.toString() : '') : '');
//        }
//    }

//    var Engine = function () { this.initialize.apply(this, Array.prototype.slice.call(arguments)) };
//    Engine.prototype = {
//        initialize: function (v) {
//            this.name = v.name || null;
//            this.alias = v.alias || null;
//            this.version = v.version || null;
//        },

//        toJSON: function () {
//            return {
//                name: this.name,
//                version: this.version.toJSON()
//            }
//        },

//        toString: function () {
//            var name = this.alias ? this.alias : (this.name ? this.name : '');
//            return name;
//        }
//    }

//    var Os = function () { this.initialize.apply(this, Array.prototype.slice.call(arguments)) };
//    Os.prototype = {
//        initialize: function (v) {
//            this.name = v.name || null;
//            this.alias = v.alias || null;
//            this.version = v.version || null;
//        },

//        toJSON: function () {
//            return {
//                name: this.name,
//                version: this.version.toJSON()
//            }
//        },

//        toString: function () {
//            var name = this.alias ? this.alias : (this.name ? this.name : '');
//            return (name ? name + (this.version && !this.version.hidden ? ' ' + this.version.toString() : '') : '');
//        }
//    }

//    var Device = function () { this.initialize.apply(this, Array.prototype.slice.call(arguments)) };
//    Device.prototype = {
//        initialize: function (v) {
//            this.type = v.type || null;
//            this.identified = v.identified || false;
//            this.manufacturer = v.manufacturer || null;
//            this.model = v.model || null;
//            this.series = v.series || null;
//        },

//        toJSON: function () {
//            return {
//                type: this.type,
//                identified: this.identified,
//                manufacturer: this.manufacturer,
//                model: this.model,
//                series: this.series
//            };
//        },

//        toString: function () {
//            if (this.identified)
//                return ((this.manufacturer || '') + ' ' + (this.model || '') + ' ' + (this.series || '')).replace(/^\s+|\s+$/g, '');
//            else
//                return (this.model ? 'unrecognized device (' + this.model + ')' : '');
//        }
//    }

//    var Version = function () { this.initialize.apply(this, Array.prototype.slice.call(arguments)) };
//    Version.prototype = {
//        initialize: function (v) {
//            this.original = v.value || null;
//            this.alias = v.alias || null;
//            this.nickname = v.nickname || null;
//            this.details = v.details || null;
//            this.hidden = v.hidden || null;
//            this.builds = typeof v.builds != 'undefined' ? v.builds : true;

//            this.major = 0;
//            this.minor = null;
//            this.revision = null;
//            this.build = null;
//            this.type = '';

//            var match;
//            if (match = /([0-9]+)(?:\.([0-9]+))?(?:\.([0-9]+))?(?:\.([0-9]+))?(?:([ab])([0-9]+))?/.exec(this.original)) {
//                if (typeof match[1] != 'undefined') {
//                    this.major = match[1];
//                }

//                if (typeof match[2] != 'undefined') {
//                    this.minor = match[2];
//                }

//                if (typeof match[3] != 'undefined') {
//                    this.revision = match[3];
//                }

//                if (typeof match[4] != 'undefined') {
//                    this.build = match[4];
//                }

//                if (typeof match[5] != 'undefined') {
//                    switch (match[5]) {
//                        case 'a': this.type = 'alpha'; break;
//                        case 'b': this.type = 'beta'; break;
//                    }

//                    if (typeof match[6] != 'undefined') {
//                        this.build = match[6];
//                    }
//                }
//            }
//        },

//        is: function (v) {
//            var valid = false;

//            if (arguments.length > 0) {
//                var operator = '=';
//                var compare = null;

//                if (arguments.length == 1) {
//                    compare = new Version({ value: arguments[0] });
//                }

//                if (arguments.length >= 2) {
//                    operator = arguments[0];
//                    compare = new Version({ value: arguments[1] });
//                }

//                if (compare) {
//                    var v1 = v2 = '';

//                    if (compare.major && this.major) {
//                        v1 += this.major;
//                        v2 += compare.major;

//                        if (compare.minor && this.minor) {
//                            v1 += '.' + ('0000' + this.minor).slice(-4);
//                            v2 += '.' + ('0000' + compare.minor).slice(-4);

//                            if (compare.revision && this.revision) {
//                                v1 += ('0000' + this.revision).slice(-4);
//                                v2 += ('0000' + compare.revision).slice(-4);
//                            }
//                        }
//                    }

//                    v1 = parseFloat(v1);
//                    v2 = parseFloat(v2);

//                    switch (operator) {
//                        case '<': valid = v1 < v2; break;
//                        case '<=': valid = v1 <= v2; break;
//                        case '=': valid = v1 == v2; break;
//                        case '>': valid = v1 > v2; break;
//                        case '>=': valid = v1 >= v2; break;
//                    }
//                }

//                return valid;
//            }

//            return false;
//        },

//        valueOf: function () {
//            return parseFloat('' + this.major + '.' + ('0000' + this.minor).slice(-4) + ('0000' + this.revision).slice(-4));
//        },

//        toJSON: function () {
//            var o = {
//                value: this.toString(),
//                details: this.details,
//                hidden: this.hidden,
//                original: this.original,
//                major: this.major,
//                minor: this.minor,
//                build: this.build,
//                revision: this.revision
//            };

//            if (this.type) o.type = this.type;
//            if (this.alias) o.alias = this.alias;
//            if (this.nickname) o.nickname = this.nickname;

//            return o;
//        },

//        toString: function () {
//            if (this.alias)
//                return this.alias;

//            var version = '';

//            if (this.nickname) {
//                version += this.nickname + ' ';
//            }

//            if (this.major || this.minor) {
//                var v = [];
//                v.push(this.major);
//                if (this.minor != '' && this.minor != null) v.push(this.minor);
//                if (this.revision != '' && this.revision != null) v.push(this.revision);
//                if (this.type == '' && this.build) v.push(this.build);
//                if (this.details < 0) v.splice(this.details, 0 - this.details);
//                if (this.details > 0) v.splice(this.details, v.length - this.details);

//                if (!this.builds) {
//                    for (var i = 0; i < v.length; i++) {
//                        if (v[i] > 999) {
//                            v.splice(i, 1);
//                            i--;
//                        }
//                    }
//                }

//                version += v.join('.');

//                if (this.type != '') version += this.type[0] + (this.build ? this.build : '');
//            }

//            return version;
//        }
//    };

//    return WhichBrowser;
//})();
/**
 * Detect.js: User-Agent Parser
 * https://github.com/darcyclarke/Detect.js
 * Dual licensed under the MIT and GPL licenses.
 *
 * @version 2.2.1
 * @author Darcy Clarke
 * @url http://darcyclarke.me
 * @createdat Thu Feb 13 2014 11:36:42 GMT+0000 (WET)
 *
 * Based on UA-Parser (https://github.com/tobie/ua-parser) by Tobie Langel
 *
 * Example Usage:
 * var agentInfo = detect.parse(navigator.userAgent);
 * console.log(agentInfo.browser.family); // Chrome
 *
 */
(function (root, undefined) {
    // Shim Array.prototype.map if necessary
    // Production steps of ECMA-262, Edition 5, 15.4.4.19
    // Reference: http://es5.github.com/#x15.4.4.19
    if (!Array.prototype.map) {
        Array.prototype.map = function (callback, thisArg) {
            var T, A, k;
            if (this == null) {
                throw new TypeError(" this is null or not defined");
            }
            // 1. Let O be the result of calling ToObject passing the |this| value as the argument.
            var O = Object(this);
            // 2. Let lenValue be the result of calling the Get internal method of O with the argument "length".
            // 3. Let len be ToUint32(lenValue).
            var len = O.length >>> 0;
            // 4. If IsCallable(callback) is false, throw a TypeError exception.
            // See: http://es5.github.com/#x9.11
            if (typeof callback !== "function") {
                throw new TypeError(callback + " is not a function");
            }
            // 5. If thisArg was supplied, let T be thisArg; else let T be undefined.
            if (thisArg) {
                T = thisArg;
            }
            // 6. Let A be a new array created as if by the expression new Array(len) where Array is
            // the standard built-in constructor with that name and len is the value of len.
            A = new Array(len);
            // 7. Let k be 0
            k = 0;
            // 8. Repeat, while k < len
            while (k < len) {
                var kValue, mappedValue;
                // a. Let Pk be ToString(k).
                //   This is implicit for LHS operands of the in operator
                // b. Let kPresent be the result of calling the HasProperty internal method of O with argument Pk.
                //   This step can be combined with c
                // c. If kPresent is true, then
                if (k in O) {
                    // i. Let kValue be the result of calling the Get internal method of O with argument Pk.
                    kValue = O[k];
                    // ii. Let mappedValue be the result of calling the Call internal method of callback
                    // with T as the this value and argument list containing kValue, k, and O.
                    mappedValue = callback.call(T, kValue, k, O);
                    // iii. Call the DefineOwnProperty internal method of A with arguments
                    // Pk, Property Descriptor {Value: mappedValue, : true, Enumerable: true, Configurable: true},
                    // and false.
                    // In browsers that support Object.defineProperty, use the following:
                    // Object.defineProperty(A, Pk, { value: mappedValue, writable: true, enumerable: true, configurable: true });
                    // For best browser support, use the following:
                    A[k] = mappedValue;
                }
                // d. Increase k by 1.
                k++;
            }
            // 9. return A
            return A;
        };
    }
    // Detect
    var detect = root.detect = function () {
        // Context
        var _this = function () { };
        // Regexes
        var regexes = {
            browser_parsers: [{
                regex: "^(Opera)/(\\d+)\\.(\\d+) \\(Nintendo Wii",
                family_replacement: "Wii",
                manufacturer: "Nintendo"
            }, {
                regex: "(SeaMonkey|Camino)/(\\d+)\\.(\\d+)\\.?([ab]?\\d+[a-z]*)",
                family_replacement: "Camino",
                other: true
            }, {
                regex: "(Pale[Mm]oon)/(\\d+)\\.(\\d+)\\.?(\\d+)?",
                family_replacement: "Pale Moon (Firefox Variant)",
                other: true
            }, {
                regex: "(Fennec)/(\\d+)\\.(\\d+)\\.?([ab]?\\d+[a-z]*)",
                family_replacement: "Firefox Mobile"
            }, {
                regex: "(Fennec)/(\\d+)\\.(\\d+)(pre)",
                family_replacment: "Firefox Mobile"
            }, {
                regex: "(Fennec)/(\\d+)\\.(\\d+)",
                family_replacement: "Firefox Mobile"
            }, {
                regex: "Mobile.*(Firefox)/(\\d+)\\.(\\d+)",
                family_replacement: "Firefox Mobile"
            }, {
                regex: "(Namoroka|Shiretoko|Minefield)/(\\d+)\\.(\\d+)\\.(\\d+(?:pre)?)",
                family_replacement: "Firefox ($1)"
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)(a\\d+[a-z]*)",
                family_replacement: "Firefox Alpha"
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)(b\\d+[a-z]*)",
                family_replacement: "Firefox Beta"
            }, {
                regex: "(Firefox)-(?:\\d+\\.\\d+)?/(\\d+)\\.(\\d+)(a\\d+[a-z]*)",
                family_replacement: "Firefox Alpha"
            }, {
                regex: "(Firefox)-(?:\\d+\\.\\d+)?/(\\d+)\\.(\\d+)(b\\d+[a-z]*)",
                family_replacement: "Firefox Beta"
            }, {
                regex: "(Namoroka|Shiretoko|Minefield)/(\\d+)\\.(\\d+)([ab]\\d+[a-z]*)?",
                family_replacement: "Firefox ($1)"
            }, {
                regex: "(Firefox).*Tablet browser (\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "MicroB",
                tablet: true
            }, {
                regex: "(MozillaDeveloperPreview)/(\\d+)\\.(\\d+)([ab]\\d+[a-z]*)?"
            }, {
                regex: "(Flock)/(\\d+)\\.(\\d+)(b\\d+?)",
                family_replacement: "Flock",
                other: true
            }, {
                regex: "(RockMelt)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Rockmelt",
                other: true
            }, {
                regex: "(Navigator)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Netscape"
            }, {
                regex: "(Navigator)/(\\d+)\\.(\\d+)([ab]\\d+)",
                family_replacement: "Netscape"
            }, {
                regex: "(Netscape6)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Netscape"
            }, {
                regex: "(MyIBrow)/(\\d+)\\.(\\d+)",
                family_replacement: "My Internet Browser",
                other: true
            }, {
                regex: "(Opera Tablet).*Version/(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                family_replacement: "Opera Tablet",
                tablet: true
            }, {
                regex: "(Opera)/.+Opera Mobi.+Version/(\\d+)\\.(\\d+)",
                family_replacement: "Opera Mobile"
            }, {
                regex: "Opera Mobi",
                family_replacement: "Opera Mobile"
            }, {
                regex: "(Opera Mini)/(\\d+)\\.(\\d+)",
                family_replacement: "Opera Mini"
            }, {
                regex: "(Opera Mini)/att/(\\d+)\\.(\\d+)",
                family_replacement: "Opera Mini"
            }, {
                regex: "(Opera)/9.80.*Version/(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                family_replacement: "Opera"
            }, {
                regex: "(webOSBrowser)/(\\d+)\\.(\\d+)",
                family_replacement: "webOS"
            }, {
                regex: "(webOS)/(\\d+)\\.(\\d+)",
                family_replacement: "webOS"
            }, {
                regex: "(wOSBrowser).+TouchPad/(\\d+)\\.(\\d+)",
                family_replacement: "webOS TouchPad"
            }, {
                regex: "(luakit)",
                family_replacement: "LuaKit",
                other: true
            }, {
                regex: "(Lightning)/(\\d+)\\.(\\d+)([ab]?\\d+[a-z]*)",
                family_replacement: "Lightning",
                other: true
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)\\.(\\d+(?:pre)?) \\(Swiftfox\\)",
                family_replacement: "Swiftfox",
                other: true
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)([ab]\\d+[a-z]*)? \\(Swiftfox\\)",
                family_replacement: "Swiftfox",
                other: true
            }, {
                regex: "rekonq",
                family_replacement: "Rekonq",
                other: true
            }, {
                regex: "(conkeror|Conkeror)/(\\d+)\\.(\\d+)\\.?(\\d+)?",
                family_replacement: "Conkeror",
                other: true
            }, {
                regex: "(konqueror)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Konqueror",
                other: true
            }, {
                regex: "(WeTab)-Browser",
                family_replacement: "WeTab",
                other: true
            }, {
                regex: "(Comodo_Dragon)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Comodo Dragon",
                other: true
            }, {
                regex: "(YottaaMonitor)",
                family_replacement: "Yottaa Monitor",
                other: true
            }, {
                regex: "(Kindle)/(\\d+)\\.(\\d+)",
                family_replacement: "Kindle"
            }, {
                regex: "(Symphony) (\\d+).(\\d+)",
                family_replacement: "Symphony",
                other: true
            }, {
                regex: "Minimo",
                family_replacement: "Minimo",
                other: true
            }, {
                regex: "(CrMo)/(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Chrome Mobile"
            }, {
                regex: "(CriOS)/(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Chrome Mobile iOS"
            }, {
                regex: "(Chrome)/(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+) Mobile",
                family_replacement: "Chrome Mobile"
            }, {
                regex: "(chromeframe)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Chrome Frame"
            }, {
                regex: "(UC Browser)(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "UC Browser",
                other: true
            }, {
                regex: "(SLP Browser)/(\\d+)\\.(\\d+)",
                family_replacement: "Tizen Browser",
                other: true
            }, {
                regex: "(Epiphany)/(\\d+)\\.(\\d+).(\\d+)",
                family_replacement: "Epiphany",
                other: true
            }, {
                regex: "(SE 2\\.X) MetaSr (\\d+)\\.(\\d+)",
                family_replacement: "Sogou Explorer",
                other: true
            }, {
                regex: "(Pingdom.com_bot_version_)(\\d+)\\.(\\d+)",
                family_replacement: "PingdomBot",
                other: true
            }, {
                regex: "(facebookexternalhit)/(\\d+)\\.(\\d+)",
                family_replacement: "FacebookBot"
            }, {
                regex: "Facebot",
                family_replacement: "FacebookBot"
            }, {
                regex: "(Twitterbot)/(\\d+)\\.(\\d+)",
                family_replacement: "TwitterBot"
            }, {
                regex: "(AdobeAIR|Chromium|FireWeb|Jasmine|ANTGalio|Midori|Fresco|Lobo|PaleMoon|Maxthon|Lynx|OmniWeb|Dillo|Camino|Demeter|Fluid|Fennec|Shiira|Sunrise|Chrome|Flock|Netscape|Lunascape|WebPilot|NetFront|Netfront|Konqueror|SeaMonkey|Kazehakase|Vienna|Iceape|Iceweasel|IceWeasel|Iron|K-Meleon|Sleipnir|Galeon|GranParadiso|Opera Mini|iCab|NetNewsWire|ThunderBrowse|Iron|Iris|UP\\.Browser|Bunjaloo|Google Earth|Raven for Mac)/(\\d+)\\.(\\d+)\\.(\\d+)"
            }, {
                regex: "(Bolt|Jasmine|IceCat|Skyfire|Midori|Maxthon|Lynx|Arora|IBrowse|Dillo|Camino|Shiira|Fennec|Phoenix|Chrome|Flock|Netscape|Lunascape|Epiphany|WebPilot|Opera Mini|Opera|NetFront|Netfront|Konqueror|Googlebot|SeaMonkey|Kazehakase|Vienna|Iceape|Iceweasel|IceWeasel|Iron|K-Meleon|Sleipnir|Galeon|GranParadiso|iCab|NetNewsWire|Iron|Space Bison|Stainless|Orca|Dolfin|BOLT|Minimo|Tizen Browser|Polaris)/(\\d+)\\.(\\d+)"
            }, {
                regex: "(iRider|Crazy Browser|SkipStone|iCab|Lunascape|Sleipnir|Maemo Browser) (\\d+)\\.(\\d+)\\.(\\d+)"
            }, {
                regex: "(iCab|Lunascape|Opera|Android|Jasmine|Polaris|BREW) (\\d+)\\.(\\d+)\\.?(\\d+)?"
            }, {
                regex: "(Android) Donut",
                v2_replacement: "2",
                v1_replacement: "1"
            }, {
                regex: "(Android) Eclair",
                v2_replacement: "1",
                v1_replacement: "2"
            }, {
                regex: "(Android) Froyo",
                v2_replacement: "2",
                v1_replacement: "2"
            }, {
                regex: "(Android) Gingerbread",
                v2_replacement: "3",
                v1_replacement: "2"
            }, {
                regex: "(Android) Honeycomb",
                v1_replacement: "3"
            }, {
                regex: "(IEMobile)[ /](\\d+)\\.(\\d+)",
                family_replacement: "IE Mobile"
            }, {
                regex: "(MSIE) (\\d+)\\.(\\d+).*XBLWP7",
                family_replacement: "IE Large Screen"
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)\\.(\\d+)"
            }, {
                regex: "(Firefox)/(\\d+)\\.(\\d+)(pre|[ab]\\d+[a-z]*)?"
            }, {
                regex: "(Obigo)InternetBrowser",
                other: true
            }, {
                regex: "(Obigo)\\-Browser",
                other: true
            }, {
                regex: "(Obigo|OBIGO)[^\\d]*(\\d+)(?:.(\\d+))?",
                other: true
            }, {
                regex: "(MAXTHON|Maxthon) (\\d+)\\.(\\d+)",
                family_replacement: "Maxthon",
                other: true
            }, {
                regex: "(Maxthon|MyIE2|Uzbl|Shiira)",
                v1_replacement: "0",
                other: true
            }, {
                regex: "(PLAYSTATION) (\\d+)",
                family_replacement: "PlayStation",
                manufacturer: "Sony"
            }, {
                regex: "(PlayStation Portable)[^\\d]+(\\d+).(\\d+)",
                manufacturer: "Sony"
            }, {
                regex: "(BrowseX) \\((\\d+)\\.(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(POLARIS)/(\\d+)\\.(\\d+)",
                family_replacement: "Polaris",
                other: true
            }, {
                regex: "(Embider)/(\\d+)\\.(\\d+)",
                family_replacement: "Polaris",
                other: true
            }, {
                regex: "(BonEcho)/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Bon Echo",
                other: true
            }, {
                regex: "(iPod).+Version/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPod).*Version/(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPod)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone).*Version/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone).*Version/(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone)",
                family_replacement: "Mobile Safari",
                manufacturer: "Apple"
            }, {
                regex: "(iPad).*Version/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                tablet: true,
                manufacturer: "Apple"
            }, {
                regex: "(iPad).*Version/(\\d+)\\.(\\d+)",
                family_replacement: "Mobile Safari",
                tablet: true,
                manufacturer: "Apple"
            }, {
                regex: "(iPad)",
                family_replacement: "Mobile Safari",
                tablet: true,
                manufacturer: "Apple"
            }, {
                regex: "(AvantGo) (\\d+).(\\d+)",
                other: true
            }, {
                regex: "(Avant)",
                v1_replacement: "1",
                other: true
            }, {
                regex: "^(Nokia)",
                family_replacement: "Nokia Services (WAP) Browser",
                manufacturer: "Nokia"
            }, {
                regex: "(NokiaBrowser)/(\\d+)\\.(\\d+).(\\d+)\\.(\\d+)",
                manufacturer: "Nokia"
            }, {
                regex: "(NokiaBrowser)/(\\d+)\\.(\\d+).(\\d+)",
                manufacturer: "Nokia"
            }, {
                regex: "(NokiaBrowser)/(\\d+)\\.(\\d+)",
                manufacturer: "Nokia"
            }, {
                regex: "(BrowserNG)/(\\d+)\\.(\\d+).(\\d+)",
                family_replacement: "NokiaBrowser",
                manufacturer: "Nokia"
            }, {
                regex: "(Series60)/5\\.0",
                v2_replacement: "0",
                v1_replacement: "7",
                family_replacement: "NokiaBrowser",
                manufacturer: "Nokia"
            }, {
                regex: "(Series60)/(\\d+)\\.(\\d+)",
                family_replacement: "Nokia OSS Browser",
                manufacturer: "Nokia"
            }, {
                regex: "(S40OviBrowser)/(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Nokia Series 40 Ovi Browser",
                manufacturer: "Nokia"
            }, {
                regex: "(Nokia)[EN]?(\\d+)",
                manufacturer: "Nokia"
            }, {
                regex: "(PlayBook).+RIM Tablet OS (\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Blackberry WebKit",
                tablet: true,
                manufacturer: "Nokia"
            }, {
                regex: "(Black[bB]erry).+Version/(\\d+)\\.(\\d+)\\.(\\d+)",
                family_replacement: "Blackberry WebKit",
                manufacturer: "RIM"
            }, {
                regex: "(Black[bB]erry)\\s?(\\d+)",
                family_replacement: "Blackberry",
                manufacturer: "RIM"
            }, {
                regex: "(OmniWeb)/v(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(Blazer)/(\\d+)\\.(\\d+)",
                family_replacement: "Palm Blazer",
                manufacturer: "Palm"
            }, {
                regex: "(Pre)/(\\d+)\\.(\\d+)",
                family_replacement: "Palm Pre",
                manufacturer: "Palm"
            }, {
                regex: "(Links) \\((\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(QtWeb) Internet Browser/(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(Silk)/(\\d+)\\.(\\d+)(?:\\.([0-9\\-]+))?",
                other: true,
                tablet: true
            }, {
                regex: "(AppleWebKit)/(\\d+)\\.?(\\d+)?\\+ .* Version/\\d+\\.\\d+.\\d+ Safari/",
                family_replacement: "WebKit Nightly"
            }, {
                regex: "(Version)/(\\d+)\\.(\\d+)(?:\\.(\\d+))?.*Safari/",
                family_replacement: "Safari"
            }, {
                regex: "(Safari)/\\d+"
            }, {
                regex: "(OLPC)/Update(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(OLPC)/Update()\\.(\\d+)",
                v1_replacement: "0",
                other: true
            }, {
                regex: "(SEMC\\-Browser)/(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(Teleca)",
                family_replacement: "Teleca Browser",
                other: true
            }, {
                regex: "Trident(.*)rv.(\\d+)\\.(\\d+)",
                family_replacement: "IE"
            }, {
                regex: "(MSIE) (\\d+)\\.(\\d+)",
                family_replacement: "IE"
            }],
            os_parsers: [{
                regex: "(Android) (\\d+)\\.(\\d+)(?:[.\\-]([a-z0-9]+))?"
            }, {
                regex: "(Android)\\-(\\d+)\\.(\\d+)(?:[.\\-]([a-z0-9]+))?"
            }, {
                regex: "(Android) Donut",
                os_v2_replacement: "2",
                os_v1_replacement: "1"
            }, {
                regex: "(Android) Eclair",
                os_v2_replacement: "1",
                os_v1_replacement: "2"
            }, {
                regex: "(Android) Froyo",
                os_v2_replacement: "2",
                os_v1_replacement: "2"
            }, {
                regex: "(Android) Gingerbread",
                os_v2_replacement: "3",
                os_v1_replacement: "2"
            }, {
                regex: "(Android) Honeycomb",
                os_v1_replacement: "3"
            }, {
                regex: "(Silk-Accelerated=[a-z]{4,5})",
                os_replacement: "Android"
            }, {
                regex: "(Windows Phone 6\\.5)"
            }, {
                regex: "(Windows (?:NT 5\\.2|NT 5\\.1))",
                os_replacement: "Windows XP"
            }, {
                regex: "(XBLWP7)",
                os_replacement: "Windows Phone OS"
            }, {
                regex: "(Windows NT 6\\.1)",
                os_replacement: "Windows 7"
            }, {
                regex: "(Windows NT 6\\.0)",
                os_replacement: "Windows Vista"
            }, {
                regex: "(Windows 98|Windows XP|Windows ME|Windows 95|Windows CE|Windows 7|Windows NT 4\\.0|Windows Vista|Windows 2000)"
            }, {
                regex: "(Windows NT 6\\.2)",
                os_replacement: "Windows 8"
            }, {
                regex: "(Windows Phone 8)",
                os_replacement: "Windows Phone 8"
            }, {
                regex: "(Windows NT 5\\.0)",
                os_replacement: "Windows 2000"
            }, {
                regex: "(Windows Phone OS) (\\d+)\\.(\\d+)"
            }, {
                regex: "(Windows ?Mobile)",
                os_replacement: "Windows Mobile"
            }, {
                regex: "(WinNT4.0)",
                os_replacement: "Windows NT 4.0"
            }, {
                regex: "(Win98)",
                os_replacement: "Windows 98"
            }, {
                regex: "(Tizen)/(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(Mac OS X) (\\d+)[_.](\\d+)(?:[_.](\\d+))?",
                manufacturer: "Apple"
            }, {
                regex: "(?:PPC|Intel) (Mac OS X)",
                manufacturer: "Apple"
            }, {
                regex: "(CPU OS|iPhone OS) (\\d+)_(\\d+)(?:_(\\d+))?",
                os_replacement: "iOS",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone|iPad|iPod); Opera",
                os_replacement: "iOS",
                manufacturer: "Apple"
            }, {
                regex: "(iPad); Opera",
                tablet: true,
                manufacturer: "Apple"
            }, {
                regex: "(iPhone|iPad|iPod).*Mac OS X.*Version/(\\d+)\\.(\\d+)",
                os_replacement: "iOS",
                manufacturer: "Apple"
            }, {
                regex: "(CrOS) [a-z0-9_]+ (\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                os_replacement: "Chrome OS"
            }, {
                regex: "(Debian)-(\\d+)\\.(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                other: true
            }, {
                regex: "(Linux Mint)(?:/(\\d+))?",
                other: true
            }, {
                regex: "(Mandriva)(?: Linux)?/(\\d+)\\.(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                other: true
            }, {
                regex: "(Symbian[Oo][Ss])/(\\d+)\\.(\\d+)",
                os_replacement: "Symbian OS"
            }, {
                regex: "(Symbian/3).+NokiaBrowser/7\\.3",
                os_replacement: "Symbian^3 Anna"
            }, {
                regex: "(Symbian/3).+NokiaBrowser/7\\.4",
                os_replacement: "Symbian^3 Belle"
            }, {
                regex: "(Symbian/3)",
                os_replacement: "Symbian^3"
            }, {
                regex: "(Series 60|SymbOS|S60)",
                os_replacement: "Symbian OS"
            }, {
                regex: "(MeeGo)",
                other: true
            }, {
                regex: "Symbian [Oo][Ss]",
                os_replacement: "Symbian OS"
            }, {
                regex: "(Black[Bb]erry)[0-9a-z]+/(\\d+)\\.(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                os_replacement: "BlackBerry OS",
                manufacturer: "RIM"
            }, {
                regex: "(Black[Bb]erry).+Version/(\\d+)\\.(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                os_replacement: "BlackBerry OS",
                manufacturer: "RIM"
            }, {
                regex: "(RIM Tablet OS) (\\d+)\\.(\\d+)\\.(\\d+)",
                os_replacement: "BlackBerry Tablet OS",
                tablet: true,
                manufacturer: "RIM"
            }, {
                regex: "(Play[Bb]ook)",
                os_replacement: "BlackBerry Tablet OS",
                tablet: true,
                manufacturer: "RIM"
            }, {
                regex: "(Black[Bb]erry)",
                os_replacement: "Blackberry OS",
                manufacturer: "RIM"
            }, {
                regex: "(webOS|hpwOS)/(\\d+)\\.(\\d+)(?:\\.(\\d+))?",
                os_replacement: "webOS"
            }, {
                regex: "(SUSE|Fedora|Red Hat|PCLinuxOS)/(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(SUSE|Fedora|Red Hat|Puppy|PCLinuxOS|CentOS)/(\\d+)\\.(\\d+)\\.(\\d+)",
                other: true
            }, {
                regex: "(Ubuntu|Kindle|Bada|Lubuntu|BackTrack|Red Hat|Slackware)/(\\d+)\\.(\\d+)"
            }, {
                regex: "(Windows|OpenBSD|FreeBSD|NetBSD|Ubuntu|Kubuntu|Android|Arch Linux|CentOS|WeTab|Slackware)"
            }, {
                regex: "(Linux|BSD)",
                other: true
            }],
            mobile_os_families: ["Windows Phone 6.5", "Windows CE", "Symbian OS"],
            device_parsers: [{
                regex: "HTC ([A-Z][a-z0-9]+) Build",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "HTC ([A-Z][a-z0-9 ]+) \\d+\\.\\d+\\.\\d+\\.\\d+",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "HTC_Touch_([A-Za-z0-9]+)",
                device_replacement: "HTC Touch ($1)",
                manufacturer: "HTC"
            }, {
                regex: "USCCHTC(\\d+)",
                device_replacement: "HTC $1 (US Cellular)",
                manufacturer: "HTC"
            }, {
                regex: "Sprint APA(9292)",
                device_replacement: "HTC $1 (Sprint)",
                manufacturer: "HTC"
            }, {
                regex: "HTC ([A-Za-z0-9]+ [A-Z])",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "HTC-([A-Za-z0-9]+)",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "HTC_([A-Za-z0-9]+)",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "HTC ([A-Za-z0-9]+)",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "(ADR[A-Za-z0-9]+)",
                device_replacement: "HTC $1",
                manufacturer: "HTC"
            }, {
                regex: "(HTC)",
                manufacturer: "HTC"
            }, {
                regex: "SonyEricsson([A-Za-z0-9]+)/",
                device_replacement: "Ericsson $1",
                other: true,
                manufacturer: "Sony"
            }, {
                regex: "Android[\\- ][\\d]+\\.[\\d]+\\; [A-Za-z]{2}\\-[A-Za-z]{2}\\; WOWMobile (.+) Build"
            }, {
                regex: "Android[\\- ][\\d]+\\.[\\d]+\\.[\\d]+; [A-Za-z]{2}\\-[A-Za-z]{2}\\; (.+) Build"
            }, {
                regex: "Android[\\- ][\\d]+\\.[\\d]+\\-update1\\; [A-Za-z]{2}\\-[A-Za-z]{2}\\; (.+) Build"
            }, {
                regex: "Android[\\- ][\\d]+\\.[\\d]+\\; [A-Za-z]{2}\\-[A-Za-z]{2}\\; (.+) Build"
            }, {
                regex: "Android[\\- ][\\d]+\\.[\\d]+\\.[\\d]+; (.+) Build"
            }, {
                regex: "NokiaN([0-9]+)",
                device_replacement: "Nokia N$1",
                manufacturer: "Nokia"
            }, {
                regex: "Nokia([A-Za-z0-9\\v-]+)",
                device_replacement: "Nokia $1",
                manufacturer: "Nokia"
            }, {
                regex: "NOKIA ([A-Za-z0-9\\-]+)",
                device_replacement: "Nokia $1",
                manufacturer: "Nokia"
            }, {
                regex: "Nokia ([A-Za-z0-9\\-]+)",
                device_replacement: "Nokia $1",
                manufacturer: "Nokia"
            }, {
                regex: "Lumia ([A-Za-z0-9\\-]+)",
                device_replacement: "Lumia $1",
                manufacturer: "Nokia"
            }, {
                regex: "Symbian",
                device_replacement: "Nokia",
                manufacturer: "Nokia"
            }, {
                regex: "(PlayBook).+RIM Tablet OS",
                device_replacement: "Blackberry Playbook",
                tablet: true,
                manufacturer: "RIM"
            }, {
                regex: "(Black[Bb]erry [0-9]+);",
                manufacturer: "RIM"
            }, {
                regex: "Black[Bb]erry([0-9]+)",
                device_replacement: "BlackBerry $1",
                manufacturer: "RIM"
            }, {
                regex: "(Pre)/(\\d+)\\.(\\d+)",
                device_replacement: "Palm Pre",
                manufacturer: "Palm"
            }, {
                regex: "(Pixi)/(\\d+)\\.(\\d+)",
                device_replacement: "Palm Pixi",
                manufacturer: "Palm"
            }, {
                regex: "(Touchpad)/(\\d+)\\.(\\d+)",
                device_replacement: "HP Touchpad",
                manufacturer: "HP"
            }, {
                regex: "HPiPAQ([A-Za-z0-9]+)/(\\d+).(\\d+)",
                device_replacement: "HP iPAQ $1",
                manufacturer: "HP"
            }, {
                regex: "Palm([A-Za-z0-9]+)",
                device_replacement: "Palm $1",
                manufacturer: "Palm"
            }, {
                regex: "Treo([A-Za-z0-9]+)",
                device_replacement: "Palm Treo $1",
                manufacturer: "Palm"
            }, {
                regex: "webOS.*(P160UNA)/(\\d+).(\\d+)",
                device_replacement: "HP Veer",
                manufacturer: "HP"
            }, {
                regex: "(Kindle Fire)",
                manufacturer: "Amazon"
            }, {
                regex: "(Kindle)",
                manufacturer: "Amazon"
            }, {
                regex: "(Silk)/(\\d+)\\.(\\d+)(?:\\.([0-9\\-]+))?",
                device_replacement: "Kindle Fire",
                tablet: true,
                manufacturer: "Amazon"
            }, {
                regex: "(iPad) Simulator;",
                manufacturer: "Apple"
            }, {
                regex: "(iPad);",
                manufacturer: "Apple"
            }, {
                regex: "(iPod);",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone) Simulator;",
                manufacturer: "Apple"
            }, {
                regex: "(iPhone);",
                manufacturer: "Apple"
            }, {
                regex: "Nexus\\ ([A-Za-z0-9\\-]+)",
                device_replacement: "Nexus $1"
            }, {
                regex: "acer_([A-Za-z0-9]+)_",
                device_replacement: "Acer $1",
                manufacturer: "Acer"
            }, {
                regex: "acer_([A-Za-z0-9]+)_",
                device_replacement: "Acer $1",
                manufacturer: "Acer"
            }, {
                regex: "Amoi\\-([A-Za-z0-9]+)",
                device_replacement: "Amoi $1",
                other: true,
                manufacturer: "Amoi"
            }, {
                regex: "AMOI\\-([A-Za-z0-9]+)",
                device_replacement: "Amoi $1",
                other: true,
                manufacturer: "Amoi"
            }, {
                regex: "Asus\\-([A-Za-z0-9]+)",
                device_replacement: "Asus $1",
                manufacturer: "Asus"
            }, {
                regex: "ASUS\\-([A-Za-z0-9]+)",
                device_replacement: "Asus $1",
                manufacturer: "Asus"
            }, {
                regex: "BIRD\\-([A-Za-z0-9]+)",
                device_replacement: "Bird $1",
                other: true
            }, {
                regex: "BIRD\\.([A-Za-z0-9]+)",
                device_replacement: "Bird $1",
                other: true
            }, {
                regex: "BIRD ([A-Za-z0-9]+)",
                device_replacement: "Bird $1",
                other: true
            }, {
                regex: "Dell ([A-Za-z0-9]+)",
                device_replacement: "Dell $1",
                manufacturer: "Dell"
            }, {
                regex: "DoCoMo/2\\.0 ([A-Za-z0-9]+)",
                device_replacement: "DoCoMo $1",
                other: true
            }, {
                regex: "([A-Za-z0-9]+)\\_W\\;FOMA",
                device_replacement: "DoCoMo $1",
                other: true
            }, {
                regex: "([A-Za-z0-9]+)\\;FOMA",
                device_replacement: "DoCoMo $1",
                other: true
            }, {
                regex: "vodafone([A-Za-z0-9]+)",
                device_replacement: "Huawei Vodafone $1",
                other: true
            }, {
                regex: "i\\-mate ([A-Za-z0-9]+)",
                device_replacement: "i-mate $1",
                other: true
            }, {
                regex: "Kyocera\\-([A-Za-z0-9]+)",
                device_replacement: "Kyocera $1",
                other: true
            }, {
                regex: "KWC\\-([A-Za-z0-9]+)",
                device_replacement: "Kyocera $1",
                other: true
            }, {
                regex: "Lenovo\\-([A-Za-z0-9]+)",
                device_replacement: "Lenovo $1",
                manufacturer: "Lenovo"
            }, {
                regex: "Lenovo\\_([A-Za-z0-9]+)",
                device_replacement: "Lenovo $1",
                manufacturer: "Levovo"
            }, {
                regex: "LG/([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LG-LG([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LGE-LG([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LGE VX([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LG ([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LGE LG\\-AX([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LG\\-([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LGE\\-([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "LG([A-Za-z0-9]+)",
                device_replacement: "LG $1",
                manufacturer: "LG"
            }, {
                regex: "(KIN)\\.One (\\d+)\\.(\\d+)",
                device_replacement: "Microsoft $1"
            }, {
                regex: "(KIN)\\.Two (\\d+)\\.(\\d+)",
                device_replacement: "Microsoft $1"
            }, {
                regex: "(Motorola)\\-([A-Za-z0-9]+)",
                manufacturer: "Motorola"
            }, {
                regex: "MOTO\\-([A-Za-z0-9]+)",
                device_replacement: "Motorola $1",
                manufacturer: "Motorola"
            }, {
                regex: "MOT\\-([A-Za-z0-9]+)",
                device_replacement: "Motorola $1",
                manufacturer: "Motorola"
            }, {
                regex: "Philips([A-Za-z0-9]+)",
                device_replacement: "Philips $1",
                manufacturer: "Philips"
            }, {
                regex: "Philips ([A-Za-z0-9]+)",
                device_replacement: "Philips $1",
                manufacturer: "Philips"
            }, {
                regex: "SAMSUNG-([A-Za-z0-9\\-]+)",
                device_replacement: "Samsung $1",
                manufacturer: "Samsung"
            }, {
                regex: "SAMSUNG\\; ([A-Za-z0-9\\-]+)",
                device_replacement: "Samsung $1",
                manufacturer: "Samsung"
            }, {
                regex: "Softbank/1\\.0/([A-Za-z0-9]+)",
                device_replacement: "Softbank $1",
                other: true
            }, {
                regex: "Softbank/2\\.0/([A-Za-z0-9]+)",
                device_replacement: "Softbank $1",
                other: true
            }, {
                regex: "(hiptop|avantgo|plucker|xiino|blazer|elaine|up.browser|up.link|mmp|smartphone|midp|wap|vodafone|o2|pocket|mobile|pda)",
                device_replacement: "Generic Smartphone"
            }, {
                regex: "^(1207|3gso|4thp|501i|502i|503i|504i|505i|506i|6310|6590|770s|802s|a wa|acer|acs\\-|airn|alav|asus|attw|au\\-m|aur |aus |abac|acoo|aiko|alco|alca|amoi|anex|anny|anyw|aptu|arch|argo|bell|bird|bw\\-n|bw\\-u|beck|benq|bilb|blac|c55/|cdm\\-|chtm|capi|comp|cond|craw|dall|dbte|dc\\-s|dica|ds\\-d|ds12|dait|devi|dmob|doco|dopo|el49|erk0|esl8|ez40|ez60|ez70|ezos|ezze|elai|emul|eric|ezwa|fake|fly\\-|fly\\_|g\\-mo|g1 u|g560|gf\\-5|grun|gene|go.w|good|grad|hcit|hd\\-m|hd\\-p|hd\\-t|hei\\-|hp i|hpip|hs\\-c|htc |htc\\-|htca|htcg)",
                device_replacement: "Generic Feature Phone"
            }, {
                regex: "^(htcp|htcs|htct|htc\\_|haie|hita|huaw|hutc|i\\-20|i\\-go|i\\-ma|i230|iac|iac\\-|iac/|ig01|im1k|inno|iris|jata|java|kddi|kgt|kgt/|kpt |kwc\\-|klon|lexi|lg g|lg\\-a|lg\\-b|lg\\-c|lg\\-d|lg\\-f|lg\\-g|lg\\-k|lg\\-l|lg\\-m|lg\\-o|lg\\-p|lg\\-s|lg\\-t|lg\\-u|lg\\-w|lg/k|lg/l|lg/u|lg50|lg54|lge\\-|lge/|lynx|leno|m1\\-w|m3ga|m50/|maui|mc01|mc21|mcca|medi|meri|mio8|mioa|mo01|mo02|mode|modo|mot |mot\\-|mt50|mtp1|mtv |mate|maxo|merc|mits|mobi|motv|mozz|n100|n101|n102|n202|n203|n300|n302|n500|n502|n505|n700|n701|n710|nec\\-|nem\\-|newg|neon)",
                device_replacement: "Generic Feature Phone"
            }, {
                regex: "^(netf|noki|nzph|o2 x|o2\\-x|opwv|owg1|opti|oran|ot\\-s|p800|pand|pg\\-1|pg\\-2|pg\\-3|pg\\-6|pg\\-8|pg\\-c|pg13|phil|pn\\-2|pt\\-g|palm|pana|pire|pock|pose|psio|qa\\-a|qc\\-2|qc\\-3|qc\\-5|qc\\-7|qc07|qc12|qc21|qc32|qc60|qci\\-|qwap|qtek|r380|r600|raks|rim9|rove|s55/|sage|sams|sc01|sch\\-|scp\\-|sdk/|se47|sec\\-|sec0|sec1|semc|sgh\\-|shar|sie\\-|sk\\-0|sl45|slid|smb3|smt5|sp01|sph\\-|spv |spv\\-|sy01|samm|sany|sava|scoo|send|siem|smar|smit|soft|sony|t\\-mo|t218|t250|t600|t610|t618|tcl\\-|tdg\\-|telm|tim\\-|ts70|tsm\\-|tsm3|tsm5|tx\\-9|tagt)",
                device_replacement: "Generic Feature Phone"
            }, {
                regex: "^(talk|teli|topl|tosh|up.b|upg1|utst|v400|v750|veri|vk\\-v|vk40|vk50|vk52|vk53|vm40|vx98|virg|vite|voda|vulc|w3c |w3c\\-|wapj|wapp|wapu|wapm|wig |wapi|wapr|wapv|wapy|wapa|waps|wapt|winc|winw|wonu|x700|xda2|xdag|yas\\-|your|zte\\-|zeto|aste|audi|avan|blaz|brew|brvw|bumb|ccwa|cell|cldc|cmd\\-|dang|eml2|fetc|hipt|http|ibro|idea|ikom|ipaq|jbro|jemu|jigs|keji|kyoc|kyok|libw|m\\-cr|midp|mmef|moto|mwbp|mywa|newt|nok6|o2im|pant|pdxg|play|pluc|port|prox|rozo|sama|seri|smal|symb|treo|upsi|vx52|vx53|vx60|vx61|vx70|vx80|vx81|vx83|vx85|wap\\-|webc|whit|wmlb|xda\\-|xda\\_)",
                device_replacement: "Generic Feature Phone"
            }, {
                regex: "(bot|borg|google(^tv)|yahoo|slurp|msnbot|msrbot|openbot|archiver|netresearch|lycos|scooter|altavista|teoma|gigabot|baiduspider|blitzbot|oegp|charlotte|furlbot|http%20client|polybot|htdig|ichiro|mogimogi|larbin|pompos|scrubby|searchsight|seekbot|semanticdiscovery|silk|snappy|speedy|spider|voila|vortex|voyager|zao|zeal|fast\\-webcrawler|converacrawler|dataparksearch|findlinks)",
                device_replacement: "Spider"
            }],
            mobile_browser_families: ["Firefox Mobile", "Opera Mobile", "Opera Mini", "Mobile Safari", "webOS", "IE Mobile", "Playstation Portable", "Nokia", "Blackberry", "Palm", "Silk", "Android", "Maemo", "Obigo", "Netfront", "AvantGo", "Teleca", "SEMC-Browser", "Bolt", "Iris", "UP.Browser", "Symphony", "Minimo", "Bunjaloo", "Jasmine", "Dolfin", "Polaris", "BREW", "Chrome Mobile", "Chrome Mobile iOS", "UC Browser", "Tizen Browser"]
        };
        // Parsers
        _this.parsers = ["device_parsers", "browser_parsers", "os_parsers", "mobile_os_families", "mobile_browser_families"];
        // Types
        _this.types = ["browser", "os", "device"];
        // Regular Expressions
        _this.regexes = regexes || function () {
            var results = {};
            _this.parsers.map(function (parser) {
                results[parser] = [];
            });
            return results;
        }();
        // Families
        _this.families = function () {
            var results = {};
            _this.types.map(function (type) {
                results[type] = [];
            });
            return results;
        }();
        // Utility Variables
        var ArrayProto = Array.prototype, ObjProto = Object.prototype, FuncProto = Function.prototype, nativeForEach = ArrayProto.forEach, nativeIndexOf = ArrayProto.indexOf;
        // Find Utility
        var find = function (ua, obj) {
            var ret = {};
            for (var i = 0; i < obj.length; i++) {
                ret = obj[i](ua);
                if (ret) {
                    break;
                }
            }
            return ret;
        };
        // Remove Utility
        var remove = function (arr, props) {
            each(arr, function (obj) {
                each(props, function (prop) {
                    delete obj[prop];
                });
            });
        };
        // Contains Utility
        var contains = function (obj, target) {
            var found = false;
            if (obj == null) return found;
            if (nativeIndexOf && obj.indexOf === nativeIndexOf) return obj.indexOf(target) != -1;
            found = any(obj, function (value) {
                return value === target;
            });
            return found;
        };
        // Each Utility
        var each = forEach = function (obj, iterator, context) {
            if (obj == null) return;
            if (nativeForEach && obj.forEach === nativeForEach) {
                obj.forEach(iterator, context);
            } else if (obj.length === +obj.length) {
                for (var i = 0, l = obj.length; i < l; i++) {
                    iterator.call(context, obj[i], i, obj);
                }
            } else {
                for (var key in obj) {
                    if (_.has(obj, key)) {
                        iterator.call(context, obj[key], key, obj);
                    }
                }
            }
        };
        // Extend Utiltiy
        var extend = function (obj) {
            each(slice.call(arguments, 1), function (source) {
                for (var prop in source) {
                    obj[prop] = source[prop];
                }
            });
            return obj;
        };
        // Check String Utility
        var check = function (str) {
            return !!(str && typeof str != "undefined" && str != null);
        };
        // To Version String Utility
        var toVersionString = function (obj) {
            var output = "";
            obj = obj || {};
            if (check(obj)) {
                if (check(obj.major)) {
                    output += obj.major;
                    if (check(obj.minor)) {
                        output += "." + obj.minor;
                        if (check(obj.patch)) {
                            output += "." + obj.patch;
                        }
                    }
                }
            }
            return output;
        };
        // To String Utility
        var toString = function (obj) {
            obj = obj || {};
            var suffix = toVersionString(obj);
            if (suffix) suffix = " " + suffix;
            return obj && check(obj.family) ? obj.family + suffix : "";
        };
        // Parse User-Agent String
        _this.parse = function (ua) {
            // Parsers Utility
            var parsers = function (type) {
                return _this.regexes[type + "_parsers"].map(function (obj) {
                    var regexp = new RegExp(obj.regex), rep = obj[(type === "browser" ? "family" : type) + "_replacement"], major_rep = obj.major_version_replacement;
                    function parser(ua) {
                        var m = ua.match(regexp);
                        if (!m) return null;
                        var ret = {};
                        ret.family = (rep ? rep.replace("$1", m[1]) : m[1]) || "other";
                        ret.major = parseInt(major_rep ? major_rep : m[2]) || null;
                        ret.minor = m[3] ? parseInt(m[3]) : null;
                        ret.patch = m[4] ? parseInt(m[4]) : null;
                        ret.tablet = obj.tablet;
                        ret.man = obj.manufacturer || null;
                        return ret;
                    }
                    return parser;
                });
            };
            // User Agent
            var UserAgent = function () { };
            // Browsers Parsed
            var browser_parsers = parsers("browser");
            // Operating Systems Parsed
            var os_parsers = parsers("os");
            // Devices Parsed
            var device_parsers = parsers("device");
            // Set Agent
            var a = new UserAgent();
            // Remember the original user agent string
            a.source = ua;
            // Set Browser
            a.browser = find(ua, browser_parsers);
            if (check(a.browser)) {
                a.browser.name = toString(a.browser);
                a.browser.version = toVersionString(a.browser);
            } else {
                a.browser = {};
            }
            // Set OS
            a.os = find(ua, os_parsers);
            if (check(a.os)) {
                a.os.name = toString(a.os);
                a.os.version = toVersionString(a.os);
            } else {
                a.os = {};
            }
            // Set Device
            a.device = find(ua, device_parsers);
            if (check(a.device)) {
                a.device.name = toString(a.device);
                a.device.version = toVersionString(a.device);
            } else {
                a.device = {
                    tablet: false,
                    family: "Other"
                };
            }
            // Determine Device Type
            var mobile_agents = {};
            var mobile_browser_families = _this.regexes.mobile_browser_families.map(function (str) {
                mobile_agents[str] = true;
            });
            var mobile_os_families = _this.regexes.mobile_os_families.map(function (str) {
                mobile_agents[str] = true;
            });
            // Is Spider
            if (a.browser.family === "Spider") {
                a.device.type = "Spider";
            } else if (a.browser.tablet || a.os.tablet || a.device.tablet) {
                a.device.type = "Tablet";
            } else if (mobile_agents.hasOwnProperty(a.browser.family)) {
                a.device.type = "Mobile";
            } else {
                a.device.type = "Desktop";
            }
            // Determine Device Manufacturer
            a.device.manufacturer = a.browser.man || a.os.man || a.device.man || null;
            // Cleanup Objects
            remove([a.browser, a.os, a.device], ["tablet", "man"]);
            // Return Agent
            return a;
        };
        // Return context
        return _this;
    }();
    // Export the Underscore object for **Node.js** and **"CommonJS"**,
    // backwards-compatibility for the old `require()` API. If we're not
    // CommonJS, add `_` to the global object via a string identifier
    // the Closure Compiler "advanced" mode. Registration as an AMD
    // via define() happens at the end of this file
    if (typeof exports !== "undefined") {
        if (typeof module !== "undefined" && module.exports) {
            exports = module.exports = detect;
        }
        exports.detect = detect;
    } else {
        root["detect"] = detect;
    }
    // AMD define happens at the end for compatibility with AMD
    // that don't enforce next-turn semantics on modules
    if (typeof define === "function" && define.amd) {
        define(function (require) {
            return detect;
        });
    }
})(window);