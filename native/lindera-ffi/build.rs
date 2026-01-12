fn main() {
    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_dll_name("lindera_ffi")
        .csharp_namespace("Lindera")
        .csharp_class_name("NativeMethodsGenerated")
        .csharp_class_accessibility("internal")
        .csharp_use_function_pointer(false) // Unity compatibility
        .csharp_dll_name_if("UNITY_IOS && !UNITY_EDITOR", "__Internal")
        .generate_csharp_file("../../Assets/Scripts/Lindera/NativeMethodsGenerated.cs")
        .unwrap();
}
