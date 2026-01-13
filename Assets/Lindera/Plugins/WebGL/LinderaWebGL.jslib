var LinderaWebGLPlugin = {
    $LinderaState: {
        initialized: false,
        initializing: false,
        initPromise: null,
        tokenizer: null,
        wasmModule: null,
        TokenizerBuilder: null,
        pendingCallbacks: []
    },

    LinderaWebGL_Initialize: function(onCompleteCallback) {
        if (LinderaState.initialized) {
            // Already initialized
            if (onCompleteCallback) {
                dynCall('vi', onCompleteCallback, [1]);
            }
            return;
        }

        if (LinderaState.initializing) {
            // Already initializing, queue the callback
            if (onCompleteCallback) {
                LinderaState.pendingCallbacks.push(onCompleteCallback);
            }
            return;
        }

        LinderaState.initializing = true;

        // Load the WASM module
        var wasmUrl = 'StreamingAssets/lindera_wasm_bg.wasm';

        // Dynamic import of the JS module
        var scriptUrl = 'StreamingAssets/lindera_wasm.js';

        // For Unity WebGL, we need to use a different approach
        // We'll fetch and initialize the WASM directly
        fetch(wasmUrl)
            .then(function(response) {
                return response.arrayBuffer();
            })
            .then(function(bytes) {
                return WebAssembly.compile(bytes);
            })
            .then(function(module) {
                LinderaState.wasmModule = module;
                LinderaState.initialized = true;
                LinderaState.initializing = false;

                console.log('[LinderaWebGL] WASM module loaded successfully');

                // Call the completion callback
                if (onCompleteCallback) {
                    dynCall('vi', onCompleteCallback, [1]);
                }

                // Call any pending callbacks
                for (var i = 0; i < LinderaState.pendingCallbacks.length; i++) {
                    dynCall('vi', LinderaState.pendingCallbacks[i], [1]);
                }
                LinderaState.pendingCallbacks = [];
            })
            .catch(function(error) {
                console.error('[LinderaWebGL] Failed to load WASM:', error);
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
        if (!LinderaState.initialized) {
            console.error('[LinderaWebGL] Not initialized');
            return 0;
        }

        try {
            // Create tokenizer using the loaded module
            // This is a simplified version - actual implementation depends on lindera-wasm API
            var tokenizerId = Date.now();
            LinderaState.tokenizer = {
                id: tokenizerId,
                // Tokenizer instance will be created here
            };
            return tokenizerId;
        } catch (error) {
            console.error('[LinderaWebGL] Failed to create tokenizer:', error);
            return 0;
        }
    },

    LinderaWebGL_DestroyTokenizer: function(tokenizerId) {
        if (LinderaState.tokenizer && LinderaState.tokenizer.id === tokenizerId) {
            LinderaState.tokenizer = null;
        }
    },

    LinderaWebGL_Tokenize: function(tokenizerId, textPtr) {
        if (!LinderaState.initialized || !LinderaState.tokenizer) {
            return null;
        }

        try {
            var text = UTF8ToString(textPtr);

            // TODO: Implement actual tokenization using lindera-wasm
            // For now, return a placeholder JSON
            var result = JSON.stringify({
                tokens: [
                    { surface: text, reading: '', pos: 'UNK' }
                ]
            });

            var bufferSize = lengthBytesUTF8(result) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(result, buffer, bufferSize);
            return buffer;
        } catch (error) {
            console.error('[LinderaWebGL] Tokenize error:', error);
            return null;
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
