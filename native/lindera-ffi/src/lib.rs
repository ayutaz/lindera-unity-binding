//! Lindera FFI bindings for Unity
//!
//! This crate provides C-compatible FFI bindings for the Lindera
//! Japanese morphological analyzer, designed for use with Unity via csbindgen.

use std::ffi::CString;
use std::os::raw::c_char;
use std::ptr;
use std::slice;

use lindera::dictionary::load_dictionary;
use lindera::mode::Mode;
use lindera::segmenter::Segmenter;
use lindera::tokenizer::Tokenizer;

/// Opaque handle to the Lindera tokenizer (FFI)
pub struct LinderaTokenizerHandle {
    tokenizer: Tokenizer,
}

/// Result of tokenization containing tokens (FFI)
pub struct LinderaTokenResultHandle {
    tokens: Vec<TokenData>,
}

/// FFI-safe token data structure
#[repr(C)]
pub struct TokenData {
    /// Token surface text (null-terminated UTF-8)
    pub text: *mut c_char,
    /// Byte start position
    pub byte_start: u32,
    /// Byte end position
    pub byte_end: u32,
    /// Token position index
    pub position: u32,
    /// Token details (null-terminated UTF-8, comma-separated)
    pub details: *mut c_char,
}

impl TokenData {
    unsafe fn free(&mut self) {
        if !self.text.is_null() {
            let _ = CString::from_raw(self.text);
            self.text = ptr::null_mut();
        }
        if !self.details.is_null() {
            let _ = CString::from_raw(self.details);
            self.details = ptr::null_mut();
        }
    }
}

/// Create a new Lindera tokenizer with embedded IPADIC dictionary
///
/// # Returns
/// - Pointer to tokenizer handle on success
/// - NULL on failure
#[no_mangle]
pub extern "C" fn lindera_tokenizer_create() -> *mut LinderaTokenizerHandle {
    // Load embedded IPADIC dictionary
    let dictionary = match load_dictionary("embedded://ipadic") {
        Ok(dict) => dict,
        Err(_) => return ptr::null_mut(),
    };

    // Create segmenter with Normal mode
    let segmenter = Segmenter::new(Mode::Normal, dictionary, None);

    // Create tokenizer
    let tokenizer = Tokenizer::new(segmenter);

    let handle = Box::new(LinderaTokenizerHandle { tokenizer });
    Box::into_raw(handle)
}

/// Destroy a tokenizer and free its resources
///
/// # Safety
/// - `handle` must be a valid pointer returned by `lindera_tokenizer_create`
/// - Must not be called twice on the same handle
#[no_mangle]
pub unsafe extern "C" fn lindera_tokenizer_destroy(handle: *mut LinderaTokenizerHandle) {
    if !handle.is_null() {
        let _ = Box::from_raw(handle);
    }
}

/// Tokenize UTF-8 text
///
/// # Arguments
/// - `handle`: Tokenizer handle
/// - `text`: UTF-8 encoded text (not null-terminated)
/// - `text_len`: Length of text in bytes
///
/// # Returns
/// - Pointer to token result on success
/// - NULL on failure
///
/// # Safety
/// - `handle` must be a valid tokenizer handle
/// - `text` must be a valid pointer to `text_len` bytes
#[no_mangle]
pub unsafe extern "C" fn lindera_tokenize(
    handle: *mut LinderaTokenizerHandle,
    text: *const u8,
    text_len: i32,
) -> *mut LinderaTokenResultHandle {
    if handle.is_null() || text.is_null() || text_len < 0 {
        return ptr::null_mut();
    }

    let tokenizer = &(*handle).tokenizer;
    let text_slice = slice::from_raw_parts(text, text_len as usize);

    let text_str = match std::str::from_utf8(text_slice) {
        Ok(s) => s,
        Err(_) => return ptr::null_mut(),
    };

    match tokenizer.tokenize(text_str) {
        Ok(mut tokens) => {
            let token_data: Vec<TokenData> = tokens
                .iter_mut()
                .enumerate()
                .map(|(i, t)| {
                    let surface = t.surface.to_string();
                    let text_ptr = CString::new(surface)
                        .unwrap_or_default()
                        .into_raw();

                    let details_str = t.details().join(",");
                    let details_ptr = CString::new(details_str)
                        .unwrap_or_default()
                        .into_raw();

                    TokenData {
                        text: text_ptr,
                        byte_start: t.byte_start as u32,
                        byte_end: t.byte_end as u32,
                        position: i as u32,
                        details: details_ptr,
                    }
                })
                .collect();

            let result = Box::new(LinderaTokenResultHandle { tokens: token_data });
            Box::into_raw(result)
        }
        Err(_) => ptr::null_mut(),
    }
}

/// Get the number of tokens in a result
///
/// # Arguments
/// - `handle`: Token result handle
///
/// # Returns
/// - Number of tokens, or -1 on error
#[no_mangle]
pub unsafe extern "C" fn lindera_tokens_count(handle: *mut LinderaTokenResultHandle) -> i32 {
    if handle.is_null() {
        return -1;
    }

    (*handle).tokens.len() as i32
}

/// Get a token at the specified index
///
/// # Arguments
/// - `handle`: Token result handle
/// - `index`: Token index
///
/// # Returns
/// - Pointer to token data, or NULL if out of bounds
///
/// # Note
/// The returned pointer is valid until `lindera_tokens_destroy` is called
#[no_mangle]
pub unsafe extern "C" fn lindera_tokens_get(
    handle: *mut LinderaTokenResultHandle,
    index: i32,
) -> *const TokenData {
    if handle.is_null() || index < 0 {
        return ptr::null();
    }

    let result = &(*handle);
    let idx = index as usize;

    if idx >= result.tokens.len() {
        return ptr::null();
    }

    &result.tokens[idx] as *const TokenData
}

/// Destroy token result and free all associated memory
///
/// # Safety
/// - `handle` must be a valid pointer returned by `lindera_tokenize`
/// - Must not be called twice on the same handle
#[no_mangle]
pub unsafe extern "C" fn lindera_tokens_destroy(handle: *mut LinderaTokenResultHandle) {
    if !handle.is_null() {
        let mut result = Box::from_raw(handle);
        for token in &mut result.tokens {
            token.free();
        }
    }
}

/// Free a string allocated by this library
///
/// # Safety
/// - `str` must be a valid pointer returned by this library
#[no_mangle]
pub unsafe extern "C" fn lindera_string_free(str: *mut c_char) {
    if !str.is_null() {
        let _ = CString::from_raw(str);
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_tokenizer_create_destroy() {
        unsafe {
            let handle = lindera_tokenizer_create();
            assert!(!handle.is_null());
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_japanese() {
        unsafe {
            let handle = lindera_tokenizer_create();
            assert!(!handle.is_null());

            let text = "東京都に住んでいます";
            let text_bytes = text.as_bytes();

            let result = lindera_tokenize(handle, text_bytes.as_ptr(), text_bytes.len() as i32);
            assert!(!result.is_null());

            let count = lindera_tokens_count(result);
            assert!(count > 0, "Expected tokens, got {}", count);

            // Check first token
            let token = lindera_tokens_get(result, 0);
            assert!(!token.is_null());

            let surface = std::ffi::CStr::from_ptr((*token).text);
            println!("First token: {}", surface.to_str().unwrap());

            lindera_tokens_destroy(result);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_empty() {
        unsafe {
            let handle = lindera_tokenizer_create();
            let result = lindera_tokenize(handle, "".as_ptr(), 0);
            assert!(!result.is_null());

            let count = lindera_tokens_count(result);
            assert_eq!(count, 0);

            lindera_tokens_destroy(result);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_reading_extraction() {
        unsafe {
            let handle = lindera_tokenizer_create();
            assert!(!handle.is_null());

            let text = "東京";
            let text_bytes = text.as_bytes();

            let result = lindera_tokenize(handle, text_bytes.as_ptr(), text_bytes.len() as i32);
            assert!(!result.is_null());

            let token = lindera_tokens_get(result, 0);
            assert!(!token.is_null());

            let details = std::ffi::CStr::from_ptr((*token).details);
            let details_str = details.to_str().unwrap();
            println!("Details: {}", details_str);

            // IPADIC format: 品詞,品詞細分類1,品詞細分類2,品詞細分類3,活用型,活用形,原形,読み,発音
            let parts: Vec<&str> = details_str.split(',').collect();
            if parts.len() > 7 {
                println!("Reading: {}", parts[7]);
                assert_eq!(parts[7], "トウキョウ");
            }

            lindera_tokens_destroy(result);
            lindera_tokenizer_destroy(handle);
        }
    }
}
