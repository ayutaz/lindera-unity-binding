var LinderaWebGLPlugin = {
    $LinderaState: {
        initialized: false,
        initializing: false,
        wasm: null,
        pendingCallbacks: [],

        // Memory helpers
        cachedTextDecoder: null,
        cachedTextEncoder: null,
        cachedUint8ArrayMemory: null,
        cachedDataViewMemory: null,
        WASM_VECTOR_LEN: 0,

        // Externref table management
        externrefTable: null,
        externrefNextIdx: 4,
        externrefFreeList: [],

        getUint8ArrayMemory: function() {
            if (LinderaState.cachedUint8ArrayMemory === null || LinderaState.cachedUint8ArrayMemory.byteLength === 0) {
                LinderaState.cachedUint8ArrayMemory = new Uint8Array(LinderaState.wasm.memory.buffer);
            }
            return LinderaState.cachedUint8ArrayMemory;
        },

        getDataViewMemory: function() {
            if (LinderaState.cachedDataViewMemory === null ||
                LinderaState.cachedDataViewMemory.buffer.detached === true ||
                (LinderaState.cachedDataViewMemory.buffer.detached === undefined &&
                 LinderaState.cachedDataViewMemory.buffer !== LinderaState.wasm.memory.buffer)) {
                LinderaState.cachedDataViewMemory = new DataView(LinderaState.wasm.memory.buffer);
            }
            return LinderaState.cachedDataViewMemory;
        },

        getStringFromWasm: function(ptr, len) {
            ptr = ptr >>> 0;
            if (LinderaState.cachedTextDecoder === null) {
                LinderaState.cachedTextDecoder = new TextDecoder('utf-8', { ignoreBOM: true, fatal: true });
                LinderaState.cachedTextDecoder.decode();
            }
            return LinderaState.cachedTextDecoder.decode(LinderaState.getUint8ArrayMemory().subarray(ptr, ptr + len));
        },

        passStringToWasm: function(arg, malloc, realloc) {
            if (LinderaState.cachedTextEncoder === null) {
                LinderaState.cachedTextEncoder = new TextEncoder();
            }

            if (realloc === undefined) {
                var buf = LinderaState.cachedTextEncoder.encode(arg);
                var ptr = malloc(buf.length, 1) >>> 0;
                LinderaState.getUint8ArrayMemory().subarray(ptr, ptr + buf.length).set(buf);
                LinderaState.WASM_VECTOR_LEN = buf.length;
                return ptr;
            }

            var len = arg.length;
            var ptr = malloc(len, 1) >>> 0;
            var mem = LinderaState.getUint8ArrayMemory();
            var offset = 0;

            for (; offset < len; offset++) {
                var code = arg.charCodeAt(offset);
                if (code > 0x7F) break;
                mem[ptr + offset] = code;
            }

            if (offset !== len) {
                if (offset !== 0) {
                    arg = arg.slice(offset);
                }
                ptr = realloc(ptr, len, len = offset + arg.length * 3, 1) >>> 0;
                var view = LinderaState.getUint8ArrayMemory().subarray(ptr + offset, ptr + len);
                var ret = LinderaState.cachedTextEncoder.encodeInto(arg, view);
                offset += ret.written;
                ptr = realloc(ptr, len, offset, 1) >>> 0;
            }

            LinderaState.WASM_VECTOR_LEN = offset;
            return ptr;
        },

        addToExternrefTable: function(obj) {
            var idx;
            if (LinderaState.externrefFreeList.length > 0) {
                idx = LinderaState.externrefFreeList.pop();
            } else {
                idx = LinderaState.externrefNextIdx++;
            }
            LinderaState.externrefTable.set(idx, obj);
            return idx;
        },

        takeFromExternrefTable: function(idx) {
            var value = LinderaState.externrefTable.get(idx);
            LinderaState.externrefTable.set(idx, undefined);
            LinderaState.externrefFreeList.push(idx);
            return value;
        },

        getArrayU8FromWasm: function(ptr, len) {
            ptr = ptr >>> 0;
            return LinderaState.getUint8ArrayMemory().subarray(ptr / 1, ptr / 1 + len);
        },

        isLikeNone: function(x) {
            return x === undefined || x === null;
        },

        debugString: function(val) {
            var type = typeof val;
            if (type == 'number' || type == 'boolean' || val == null) {
                return '' + val;
            }
            if (type == 'string') {
                return '"' + val + '"';
            }
            if (type == 'function') {
                return 'Function';
            }
            if (Array.isArray(val)) {
                return '[Array]';
            }
            try {
                return 'Object(' + JSON.stringify(val) + ')';
            } catch (_) {
                return 'Object';
            }
        },

        handleError: function(f, args) {
            try {
                return f.apply(this, args);
            } catch (e) {
                var idx = LinderaState.addToExternrefTable(e);
                LinderaState.wasm.__wbindgen_exn_store(idx);
            }
        },

        buildImports: function() {
            var imports = {};
            imports.wbg = {};

            imports.wbg.__wbg_Error_e17e777aac105295 = function(arg0, arg1) {
                return new Error(LinderaState.getStringFromWasm(arg0, arg1));
            };

            imports.wbg.__wbg_String_8f0eb39a4a4c2f66 = function(arg0, arg1) {
                var ret = String(arg1);
                var ptr1 = LinderaState.passStringToWasm(ret, LinderaState.wasm.__wbindgen_malloc, LinderaState.wasm.__wbindgen_realloc);
                var len1 = LinderaState.WASM_VECTOR_LEN;
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 1, len1, true);
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 0, ptr1, true);
            };

            imports.wbg.__wbg_call_13410aac570ffff7 = function() {
                return LinderaState.handleError(function(arg0, arg1) {
                    return arg0.call(arg1);
                }, arguments);
            };

            imports.wbg.__wbg_done_75ed0ee6dd243d9d = function(arg0) {
                return arg0.done;
            };

            imports.wbg.__wbg_entries_2be2f15bd5554996 = function(arg0) {
                return Object.entries(arg0);
            };

            imports.wbg.__wbg_get_0da715ceaecea5c8 = function(arg0, arg1) {
                return arg0[arg1 >>> 0];
            };

            imports.wbg.__wbg_get_458e874b43b18b25 = function() {
                return LinderaState.handleError(function(arg0, arg1) {
                    return Reflect.get(arg0, arg1);
                }, arguments);
            };

            imports.wbg.__wbg_instanceof_ArrayBuffer_67f3012529f6a2dd = function(arg0) {
                try { return arg0 instanceof ArrayBuffer; } catch (_) { return false; }
            };

            imports.wbg.__wbg_instanceof_Map_ebb01a5b6b5ffd0b = function(arg0) {
                try { return arg0 instanceof Map; } catch (_) { return false; }
            };

            imports.wbg.__wbg_instanceof_Uint8Array_9a8378d955933db7 = function(arg0) {
                try { return arg0 instanceof Uint8Array; } catch (_) { return false; }
            };

            imports.wbg.__wbg_isArray_030cce220591fb41 = function(arg0) {
                return Array.isArray(arg0);
            };

            imports.wbg.__wbg_isSafeInteger_1c0d1af5542e102a = function(arg0) {
                return Number.isSafeInteger(arg0);
            };

            imports.wbg.__wbg_iterator_f370b34483c71a1c = function() {
                return Symbol.iterator;
            };

            imports.wbg.__wbg_length_186546c51cd61acd = function(arg0) {
                return arg0.length;
            };

            imports.wbg.__wbg_length_6bb7e81f9d7713e4 = function(arg0) {
                return arg0.length;
            };

            imports.wbg.__wbg_new_19c25a3f2fa63a02 = function() {
                return new Object();
            };

            imports.wbg.__wbg_new_1f3a344cf3123716 = function() {
                return new Array();
            };

            imports.wbg.__wbg_new_638ebfaedbf32a5e = function(arg0) {
                return new Uint8Array(arg0);
            };

            imports.wbg.__wbg_next_5b3530e612fde77d = function(arg0) {
                return arg0.next;
            };

            imports.wbg.__wbg_next_692e82279131b03c = function() {
                return LinderaState.handleError(function(arg0) {
                    return arg0.next();
                }, arguments);
            };

            imports.wbg.__wbg_prototypesetcall_3d4a26c1ed734349 = function(arg0, arg1, arg2) {
                Uint8Array.prototype.set.call(LinderaState.getArrayU8FromWasm(arg0, arg1), arg2);
            };

            imports.wbg.__wbg_push_330b2eb93e4e1212 = function(arg0, arg1) {
                return arg0.push(arg1);
            };

            imports.wbg.__wbg_set_453345bcda80b89a = function() {
                return LinderaState.handleError(function(arg0, arg1, arg2) {
                    return Reflect.set(arg0, arg1, arg2);
                }, arguments);
            };

            imports.wbg.__wbg_value_dd9372230531eade = function(arg0) {
                return arg0.value;
            };

            imports.wbg.__wbg_wbindgenbigintgetasi64_ac743ece6ab9bba1 = function(arg0, arg1) {
                var v = arg1;
                var ret = typeof(v) === 'bigint' ? v : undefined;
                LinderaState.getDataViewMemory().setBigInt64(arg0 + 8 * 1, LinderaState.isLikeNone(ret) ? BigInt(0) : ret, true);
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 0, !LinderaState.isLikeNone(ret), true);
            };

            imports.wbg.__wbg_wbindgenbooleanget_3fe6f642c7d97746 = function(arg0) {
                var v = arg0;
                var ret = typeof(v) === 'boolean' ? v : undefined;
                return LinderaState.isLikeNone(ret) ? 0xFFFFFF : ret ? 1 : 0;
            };

            imports.wbg.__wbg_wbindgendebugstring_99ef257a3ddda34d = function(arg0, arg1) {
                var ret = LinderaState.debugString(arg1);
                var ptr1 = LinderaState.passStringToWasm(ret, LinderaState.wasm.__wbindgen_malloc, LinderaState.wasm.__wbindgen_realloc);
                var len1 = LinderaState.WASM_VECTOR_LEN;
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 1, len1, true);
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 0, ptr1, true);
            };

            imports.wbg.__wbg_wbindgenin_d7a1ee10933d2d55 = function(arg0, arg1) {
                return arg0 in arg1;
            };

            imports.wbg.__wbg_wbindgenisbigint_ecb90cc08a5a9154 = function(arg0) {
                return typeof(arg0) === 'bigint';
            };

            imports.wbg.__wbg_wbindgenisfunction_8cee7dce3725ae74 = function(arg0) {
                return typeof(arg0) === 'function';
            };

            imports.wbg.__wbg_wbindgenisobject_307a53c6bd97fbf8 = function(arg0) {
                var val = arg0;
                return typeof(val) === 'object' && val !== null;
            };

            imports.wbg.__wbg_wbindgenjsvaleq_e6f2ad59ccae1b58 = function(arg0, arg1) {
                return arg0 === arg1;
            };

            imports.wbg.__wbg_wbindgenjsvallooseeq_9bec8c9be826bed1 = function(arg0, arg1) {
                return arg0 == arg1;
            };

            imports.wbg.__wbg_wbindgennumberget_f74b4c7525ac05cb = function(arg0, arg1) {
                var obj = arg1;
                var ret = typeof(obj) === 'number' ? obj : undefined;
                LinderaState.getDataViewMemory().setFloat64(arg0 + 8 * 1, LinderaState.isLikeNone(ret) ? 0 : ret, true);
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 0, !LinderaState.isLikeNone(ret), true);
            };

            imports.wbg.__wbg_wbindgenstringget_0f16a6ddddef376f = function(arg0, arg1) {
                var obj = arg1;
                var ret = typeof(obj) === 'string' ? obj : undefined;
                var ptr1 = LinderaState.isLikeNone(ret) ? 0 : LinderaState.passStringToWasm(ret, LinderaState.wasm.__wbindgen_malloc, LinderaState.wasm.__wbindgen_realloc);
                var len1 = LinderaState.WASM_VECTOR_LEN;
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 1, len1, true);
                LinderaState.getDataViewMemory().setInt32(arg0 + 4 * 0, ptr1, true);
            };

            imports.wbg.__wbg_wbindgenthrow_451ec1a8469d7eb6 = function(arg0, arg1) {
                throw new Error(LinderaState.getStringFromWasm(arg0, arg1));
            };

            imports.wbg.__wbindgen_cast_2241b6af4c4b2941 = function(arg0, arg1) {
                return LinderaState.getStringFromWasm(arg0, arg1);
            };

            imports.wbg.__wbindgen_cast_4625c577ab2ec9ee = function(arg0) {
                return BigInt.asUintN(64, arg0);
            };

            imports.wbg.__wbindgen_cast_9ae0607507abb057 = function(arg0) {
                return arg0;
            };

            imports.wbg.__wbindgen_cast_d6cd19b81560fd6e = function(arg0) {
                return arg0;
            };

            imports.wbg.__wbindgen_init_externref_table = function() {
                var table = LinderaState.wasm.__wbindgen_export_4;
                LinderaState.externrefTable = table;
                var offset = table.grow(4);
                table.set(0, undefined);
                table.set(offset + 0, undefined);
                table.set(offset + 1, null);
                table.set(offset + 2, true);
                table.set(offset + 3, false);
            };

            return imports;
        }
    },

    LinderaWebGL_Initialize: function(onCompleteCallback) {
        if (LinderaState.initialized) {
            if (onCompleteCallback) {
                dynCall('vi', onCompleteCallback, [1]);
            }
            return;
        }

        if (LinderaState.initializing) {
            if (onCompleteCallback) {
                LinderaState.pendingCallbacks.push(onCompleteCallback);
            }
            return;
        }

        LinderaState.initializing = true;
        console.log('[LinderaWebGL] Starting initialization...');

        var wasmUrl = 'StreamingAssets/lindera_wasm_bg.wasm';

        fetch(wasmUrl)
            .then(function(response) {
                if (!response.ok) {
                    throw new Error('Failed to fetch WASM: ' + response.status);
                }
                return response.arrayBuffer();
            })
            .then(function(bytes) {
                console.log('[LinderaWebGL] WASM fetched, size:', bytes.byteLength);

                var imports = LinderaState.buildImports();
                return WebAssembly.instantiate(bytes, imports);
            })
            .then(function(result) {
                LinderaState.wasm = result.instance.exports;
                console.log('[LinderaWebGL] WASM instantiated');

                // Initialize externref table and start
                if (LinderaState.wasm.__wbindgen_start) {
                    LinderaState.wasm.__wbindgen_start();
                }

                // Reset memory caches after WASM init
                LinderaState.cachedDataViewMemory = null;
                LinderaState.cachedUint8ArrayMemory = null;

                LinderaState.initialized = true;
                LinderaState.initializing = false;

                console.log('[LinderaWebGL] Initialization complete');

                if (onCompleteCallback) {
                    dynCall('vi', onCompleteCallback, [1]);
                }

                for (var i = 0; i < LinderaState.pendingCallbacks.length; i++) {
                    dynCall('vi', LinderaState.pendingCallbacks[i], [1]);
                }
                LinderaState.pendingCallbacks = [];
            })
            .catch(function(error) {
                console.error('[LinderaWebGL] Initialization failed:', error);
                LinderaState.initializing = false;

                if (onCompleteCallback) {
                    dynCall('vi', onCompleteCallback, [0]);
                }

                for (var i = 0; i < LinderaState.pendingCallbacks.length; i++) {
                    dynCall('vi', LinderaState.pendingCallbacks[i], [0]);
                }
                LinderaState.pendingCallbacks = [];
            });
    },

    LinderaWebGL_IsInitialized: function() {
        return LinderaState.initialized ? 1 : 0;
    },

    LinderaWebGL_CreateTokenizer: function() {
        if (!LinderaState.initialized || !LinderaState.wasm) {
            console.error('[LinderaWebGL] Not initialized');
            return 0;
        }

        try {
            // Create TokenizerBuilder: returns [ptr, error_idx, has_error]
            var builderResult = LinderaState.wasm.tokenizerbuilder_new();
            if (builderResult[2]) {
                var error = LinderaState.takeFromExternrefTable(builderResult[1]);
                console.error('[LinderaWebGL] Failed to create TokenizerBuilder:', error);
                return 0;
            }
            var builderPtr = builderResult[0] >>> 0;

            // Build tokenizer: returns [tokenizer_ptr, error_idx, has_error]
            var tokenizerResult = LinderaState.wasm.tokenizerbuilder_build(builderPtr);
            if (tokenizerResult[2]) {
                var error = LinderaState.takeFromExternrefTable(tokenizerResult[1]);
                console.error('[LinderaWebGL] Failed to build Tokenizer:', error);
                return 0;
            }
            var tokenizerPtr = tokenizerResult[0] >>> 0;

            console.log('[LinderaWebGL] Tokenizer created:', tokenizerPtr);
            return tokenizerPtr;
        } catch (error) {
            console.error('[LinderaWebGL] CreateTokenizer error:', error);
            return 0;
        }
    },

    LinderaWebGL_DestroyTokenizer: function(tokenizerPtr) {
        if (LinderaState.wasm && tokenizerPtr) {
            try {
                LinderaState.wasm.__wbg_tokenizer_free(tokenizerPtr >>> 0, 0);
                console.log('[LinderaWebGL] Tokenizer destroyed');
            } catch (error) {
                console.error('[LinderaWebGL] DestroyTokenizer error:', error);
            }
        }
    },

    LinderaWebGL_Tokenize: function(tokenizerPtr, textPtr) {
        if (!LinderaState.initialized || !LinderaState.wasm || !tokenizerPtr) {
            console.error('[LinderaWebGL] Cannot tokenize: not ready');
            return 0;
        }

        try {
            var text = UTF8ToString(textPtr);
            console.log('[LinderaWebGL] Tokenizing:', text);

            // Pass string to WASM
            var strPtr = LinderaState.passStringToWasm(
                text,
                LinderaState.wasm.__wbindgen_malloc,
                LinderaState.wasm.__wbindgen_realloc
            );
            var strLen = LinderaState.WASM_VECTOR_LEN;

            // Call tokenize: returns [result_idx, error_idx, has_error]
            var result = LinderaState.wasm.tokenizer_tokenize(tokenizerPtr >>> 0, strPtr, strLen);

            if (result[2]) {
                var error = LinderaState.takeFromExternrefTable(result[1]);
                console.error('[LinderaWebGL] Tokenize failed:', error);
                return 0;
            }

            // Get the tokens array from externref table
            var tokens = LinderaState.takeFromExternrefTable(result[0]);
            console.log('[LinderaWebGL] Tokens:', tokens);

            // Convert tokens to JSON format for C#
            // lindera-wasm returns array of objects with: text, byte_start, byte_end, position, details
            var tokensArray = [];
            if (Array.isArray(tokens)) {
                for (var i = 0; i < tokens.length; i++) {
                    var t = tokens[i];
                    tokensArray.push({
                        text: t.text || '',
                        byte_start: t.byte_start || 0,
                        byte_end: t.byte_end || 0,
                        position: t.position || 0,
                        details: t.details || []
                    });
                }
            }

            var tokensJson = JSON.stringify({ tokens: tokensArray });
            var bufferSize = lengthBytesUTF8(tokensJson) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(tokensJson, buffer, bufferSize);
            return buffer;
        } catch (error) {
            console.error('[LinderaWebGL] Tokenize error:', error);
            return 0;
        }
    },

    LinderaWebGL_FreeString: function(ptr) {
        if (ptr) {
            _free(ptr);
        }
    },

    LinderaWebGL_GetVersion: function() {
        var version = "2.0.0-wasm";
        var bufferSize = lengthBytesUTF8(version) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(version, buffer, bufferSize);
        return buffer;
    }
};

autoAddDeps(LinderaWebGLPlugin, '$LinderaState');
mergeInto(LibraryManager.library, LinderaWebGLPlugin);
