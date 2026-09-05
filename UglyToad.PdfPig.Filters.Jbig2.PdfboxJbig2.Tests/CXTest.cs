namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using Xunit;

    public class CXTest
    {
        [Fact]
        public void InitializationTest()
        {
            // Test that CX is initialized with correct size and index
            var cx = new CX(512, 5);

            Assert.Equal(5, cx.Index);
            // After initialization, cx value should be 0 (unset)
            Assert.Equal(0, cx.Cx);
            // After initialization, mps should be 0
            Assert.Equal(0, cx.Mps);
        }

        [Fact]
        public void SetAndGetIndexTest()
        {
            var cx = new CX(100, 0);

            cx.Index = 50;
            Assert.Equal(50, cx.Index);

            cx.Index = 99;
            Assert.Equal(99, cx.Index);

            cx.Index = 0;
            Assert.Equal(0, cx.Index);
        }

        [Fact]
        public void SetAndGetCxTest()
        {
            var cx = new CX(512, 0);

            cx.Cx = 42;
            Assert.Equal(42, cx.Cx);

            cx.Index = 1;
            cx.Cx = 127; // Maximum value (7 bits)
            Assert.Equal(127, cx.Cx);

            // Verify that different indices have independent values
            cx.Index = 0;
            Assert.Equal(42, cx.Cx);
        }

        [Fact]
        public void CxMaskingTest()
        {
            // CX should mask to 7 bits (0-127)
            var cx = new CX(512, 0);

            cx.Cx = 0xFF; // Set with all bits set
            Assert.Equal(0x7F, cx.Cx); // Should be masked to 7 bits

            cx.Cx = 0x80; // Bit 7 set
            Assert.Equal(0, cx.Cx); // Should be masked out
        }

        [Fact]
        public void ToggleMpsTest()
        {
            var cx = new CX(512, 0);

            // Initial state should be 0
            Assert.Equal(0, cx.Mps);

            // Toggle to 1
            cx.ToggleMps();
            Assert.Equal(1, cx.Mps);

            // Toggle back to 0
            cx.ToggleMps();
            Assert.Equal(0, cx.Mps);

            // Toggle again
            cx.ToggleMps();
            Assert.Equal(1, cx.Mps);
        }

        [Fact]
        public void MpsIndependentTest()
        {
            var cx = new CX(512, 0);

            cx.ToggleMps();
            Assert.Equal(1, cx.Mps);

            // Change index and verify MPS is independent
            cx.Index = 1;
            Assert.Equal(0, cx.Mps);

            cx.ToggleMps();
            Assert.Equal(1, cx.Mps);

            // Back to index 0, should still be 1
            cx.Index = 0;
            Assert.Equal(1, cx.Mps);
        }

        [Fact]
        public void CopyDeepCopyTest()
        {
            var original = new CX(512, 10);
            original.Cx = 50;
            original.ToggleMps();

            var copy = original.Copy();

            // Verify copy has same state
            Assert.Equal(10, copy.Index);
            Assert.Equal(50, copy.Cx);
            Assert.Equal(1, copy.Mps);
        }

        [Fact]
        public void CopyIndependenceTest()
        {
            var original = new CX(512, 0);
            original.Cx = 42;
            original.ToggleMps();

            var copy = original.Copy();

            // Modify original
            original.Cx = 100;
            original.ToggleMps();
            original.Index = 5;

            // Copy should be unaffected
            Assert.Equal(42, copy.Cx);
            Assert.Equal(1, copy.Mps);
            Assert.Equal(0, copy.Index);
        }

        [Fact]
        public void CopyMultipleIndicesTest()
        {
            var original = new CX(512, 0);

            // Set values at different indices
            original.Cx = 10;
            original.ToggleMps();

            original.Index = 1;
            original.Cx = 20;

            original.Index = 2;
            original.Cx = 30;
            original.ToggleMps();

            // Create copy
            var copy = original.Copy();

            // Verify all indices were copied
            copy.Index = 0;
            Assert.Equal(10, copy.Cx);
            Assert.Equal(1, copy.Mps);

            copy.Index = 1;
            Assert.Equal(20, copy.Cx);
            Assert.Equal(0, copy.Mps);

            copy.Index = 2;
            Assert.Equal(30, copy.Cx);
            Assert.Equal(1, copy.Mps);
        }

        [Fact]
        public void LargeSizeContextTest()
        {
            // Test with large context size (typical for bitmap decoding)
            var cx = new CX(65536, 0);

            cx.Index = 65535;
            cx.Cx = 127;
            Assert.Equal(127, cx.Cx);

            cx.ToggleMps();
            Assert.Equal(1, cx.Mps);
        }
    }
}
