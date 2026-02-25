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

/// N-Best result containing multiple segmentation paths (FFI)
pub struct LinderaNBestResultHandle {
    paths: Vec<NBestPath>,
}

/// A single N-Best path with tokens and cost
struct NBestPath {
    tokens: Vec<TokenData>,
    cost: i64,
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

/// Tokenize text with N-Best algorithm returning multiple segmentation paths
///
/// # Arguments
/// - `handle`: Tokenizer handle
/// - `text`: UTF-8 encoded text
/// - `text_len`: Length of text in bytes
/// - `n`: Number of best paths to return
/// - `deduplicate`: 1 to deduplicate paths with same word boundaries, 0 otherwise
/// - `cost_threshold_valid`: 1 if cost_threshold should be used, 0 for no threshold
/// - `cost_threshold`: Maximum cost delta from best result (only used if cost_threshold_valid is 1)
///
/// # Returns
/// - Pointer to N-Best result on success
/// - NULL on failure
///
/// # Safety
/// - `handle` must be a valid tokenizer handle
/// - `text` must be a valid pointer to `text_len` bytes
#[no_mangle]
pub unsafe extern "C" fn lindera_tokenize_nbest(
    handle: *mut LinderaTokenizerHandle,
    text: *const u8,
    text_len: i32,
    n: i32,
    deduplicate: u8,
    cost_threshold_valid: u8,
    cost_threshold: i64,
) -> *mut LinderaNBestResultHandle {
    if handle.is_null() || text.is_null() || text_len < 0 || n <= 0 {
        return ptr::null_mut();
    }

    let tokenizer = &(*handle).tokenizer;
    let text_slice = slice::from_raw_parts(text, text_len as usize);

    let text_str = match std::str::from_utf8(text_slice) {
        Ok(s) => s,
        Err(_) => return ptr::null_mut(),
    };

    let unique = deduplicate != 0;
    let threshold = if cost_threshold_valid != 0 {
        Some(cost_threshold)
    } else {
        None
    };

    match tokenizer.tokenize_nbest(text_str, n as usize, unique, threshold) {
        Ok(results) => {
            let paths: Vec<NBestPath> = results
                .into_iter()
                .map(|(mut tokens, cost)| {
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

                    NBestPath {
                        tokens: token_data,
                        cost,
                    }
                })
                .collect();

            let result = Box::new(LinderaNBestResultHandle { paths });
            Box::into_raw(result)
        }
        Err(_) => ptr::null_mut(),
    }
}

/// Get the number of paths in an N-Best result
///
/// # Returns
/// - Number of paths, or -1 on error
#[no_mangle]
pub unsafe extern "C" fn lindera_nbest_path_count(
    handle: *mut LinderaNBestResultHandle,
) -> i32 {
    if handle.is_null() {
        return -1;
    }
    (*handle).paths.len() as i32
}

/// Get the number of tokens in a specific N-Best path
///
/// # Returns
/// - Number of tokens, or -1 on error
#[no_mangle]
pub unsafe extern "C" fn lindera_nbest_get_path_token_count(
    handle: *mut LinderaNBestResultHandle,
    path_index: i32,
) -> i32 {
    if handle.is_null() || path_index < 0 {
        return -1;
    }
    let result = &(*handle);
    let idx = path_index as usize;
    if idx >= result.paths.len() {
        return -1;
    }
    result.paths[idx].tokens.len() as i32
}

/// Get a token from a specific N-Best path
///
/// # Returns
/// - Pointer to token data, or NULL if out of bounds
#[no_mangle]
pub unsafe extern "C" fn lindera_nbest_get_path_token(
    handle: *mut LinderaNBestResultHandle,
    path_index: i32,
    token_index: i32,
) -> *const TokenData {
    if handle.is_null() || path_index < 0 || token_index < 0 {
        return ptr::null();
    }
    let result = &(*handle);
    let pidx = path_index as usize;
    let tidx = token_index as usize;
    if pidx >= result.paths.len() {
        return ptr::null();
    }
    if tidx >= result.paths[pidx].tokens.len() {
        return ptr::null();
    }
    &result.paths[pidx].tokens[tidx] as *const TokenData
}

/// Get the cost of a specific N-Best path
///
/// # Returns
/// - Path cost, or 0 on error
#[no_mangle]
pub unsafe extern "C" fn lindera_nbest_get_path_cost(
    handle: *mut LinderaNBestResultHandle,
    path_index: i32,
) -> i64 {
    if handle.is_null() || path_index < 0 {
        return 0;
    }
    let result = &(*handle);
    let idx = path_index as usize;
    if idx >= result.paths.len() {
        return 0;
    }
    result.paths[idx].cost
}

/// Destroy N-Best result and free all associated memory
///
/// # Safety
/// - `handle` must be a valid pointer returned by `lindera_tokenize_nbest`
/// - Must not be called twice on the same handle
#[no_mangle]
pub unsafe extern "C" fn lindera_nbest_destroy(handle: *mut LinderaNBestResultHandle) {
    if !handle.is_null() {
        let mut result = Box::from_raw(handle);
        for path in &mut result.paths {
            for token in &mut path.tokens {
                token.free();
            }
        }
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

    #[test]
    fn test_tokenize_nbest_basic() {
        unsafe {
            let handle = lindera_tokenizer_create();
            assert!(!handle.is_null());

            let text = "すもももももももものうち";
            let text_bytes = text.as_bytes();

            let result = lindera_tokenize_nbest(
                handle,
                text_bytes.as_ptr(),
                text_bytes.len() as i32,
                3,
                0, // no dedup
                0, // no threshold
                0,
            );
            assert!(!result.is_null());

            let count = lindera_nbest_path_count(result);
            assert!(count >= 1, "Expected at least 1 path, got {}", count);
            assert!(count <= 3, "Expected at most 3 paths, got {}", count);

            // Check first path has tokens
            let token_count = lindera_nbest_get_path_token_count(result, 0);
            assert!(token_count > 0);

            // Check cost is returned
            let cost = lindera_nbest_get_path_cost(result, 0);
            println!("Best path cost: {}", cost);

            lindera_nbest_destroy(result);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_nbest_single_matches_normal() {
        unsafe {
            let handle = lindera_tokenizer_create();
            assert!(!handle.is_null());

            let text = "東京都に住んでいます";
            let text_bytes = text.as_bytes();

            let normal = lindera_tokenize(handle, text_bytes.as_ptr(), text_bytes.len() as i32);
            let nbest = lindera_tokenize_nbest(
                handle,
                text_bytes.as_ptr(),
                text_bytes.len() as i32,
                1,
                0,
                0,
                0,
            );

            let normal_count = lindera_tokens_count(normal);
            let nbest_path_count = lindera_nbest_path_count(nbest);
            assert_eq!(nbest_path_count, 1);

            let nbest_token_count = lindera_nbest_get_path_token_count(nbest, 0);
            assert_eq!(normal_count, nbest_token_count);

            // Compare first token surfaces
            let normal_token = lindera_tokens_get(normal, 0);
            let nbest_token = lindera_nbest_get_path_token(nbest, 0, 0);
            assert!(!normal_token.is_null());
            assert!(!nbest_token.is_null());

            let normal_surface = std::ffi::CStr::from_ptr((*normal_token).text);
            let nbest_surface = std::ffi::CStr::from_ptr((*nbest_token).text);
            assert_eq!(normal_surface, nbest_surface);

            lindera_tokens_destroy(normal);
            lindera_nbest_destroy(nbest);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_nbest_with_threshold() {
        unsafe {
            let handle = lindera_tokenizer_create();
            let text = "すもももももももものうち";
            let text_bytes = text.as_bytes();

            let result = lindera_tokenize_nbest(
                handle,
                text_bytes.as_ptr(),
                text_bytes.len() as i32,
                10,
                0,
                1, // has threshold
                1000,
            );
            assert!(!result.is_null());

            let count = lindera_nbest_path_count(result);
            assert!(count >= 1);

            lindera_nbest_destroy(result);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_nbest_deduplicate() {
        unsafe {
            let handle = lindera_tokenizer_create();
            let text = "すもももももももものうち";
            let text_bytes = text.as_bytes();

            let with_dedup = lindera_tokenize_nbest(
                handle,
                text_bytes.as_ptr(),
                text_bytes.len() as i32,
                5,
                1, // deduplicate
                0,
                0,
            );
            let without_dedup = lindera_tokenize_nbest(
                handle,
                text_bytes.as_ptr(),
                text_bytes.len() as i32,
                5,
                0, // no deduplicate
                0,
                0,
            );

            let count_with = lindera_nbest_path_count(with_dedup);
            let count_without = lindera_nbest_path_count(without_dedup);
            assert!(count_with <= count_without);

            lindera_nbest_destroy(with_dedup);
            lindera_nbest_destroy(without_dedup);
            lindera_tokenizer_destroy(handle);
        }
    }

    #[test]
    fn test_tokenize_nbest_null_handle() {
        unsafe {
            let text = "test";
            let result = lindera_tokenize_nbest(
                ptr::null_mut(),
                text.as_ptr(),
                text.len() as i32,
                3,
                0,
                0,
                0,
            );
            assert!(result.is_null());
        }
    }

    #[test]
    fn test_nbest_destroy_null() {
        unsafe {
            lindera_nbest_destroy(ptr::null_mut());
            // Should not crash
        }
    }
}
