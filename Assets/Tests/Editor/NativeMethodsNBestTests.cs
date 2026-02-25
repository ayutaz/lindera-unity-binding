using System;
using NUnit.Framework;
using LinderaUnityBinding;

namespace LinderaUnityBinding.Tests
{
    /// <summary>
    /// N-Best NativeMethods のユニットテスト
    /// </summary>
    public class NativeMethodsNBestTests
    {
        [Test]
        [Category("RequiresNativeLibrary")]
        public unsafe void NBestTokenize_NullHandle_ReturnsZero()
        {
            var result = NativeMethods.lindera_tokenize_nbest(IntPtr.Zero, null, 0, 3, true, null);
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [Test]
        [Category("RequiresNativeLibrary")]
        public void NBestPathCount_NullHandle_ReturnsNegative()
        {
            var count = NativeMethods.lindera_nbest_path_count(IntPtr.Zero);
            Assert.AreEqual(-1, count);
        }

        [Test]
        [Category("RequiresNativeLibrary")]
        public void NBestGetPathTokenCount_NullHandle_ReturnsNegative()
        {
            var count = NativeMethods.lindera_nbest_get_path_token_count(IntPtr.Zero, 0);
            Assert.AreEqual(-1, count);
        }

        [Test]
        [Category("RequiresNativeLibrary")]
        public void NBestGetPathCost_NullHandle_ReturnsZero()
        {
            var cost = NativeMethods.lindera_nbest_get_path_cost(IntPtr.Zero, 0);
            Assert.AreEqual(0, cost);
        }

        [Test]
        [Category("RequiresNativeLibrary")]
        public void NBestDestroy_NullHandle_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NativeMethods.lindera_nbest_destroy(IntPtr.Zero));
        }
    }
}
