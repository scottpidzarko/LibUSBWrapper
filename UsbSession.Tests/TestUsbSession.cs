using Xunit;

namespace UsbSession.Tests
{
    public class TestUsbSession
    {
        [Fact]
        public void TestTestSession()
        {
            UsbSession s = new UsbSession();
            Assert.False(s.TestSession());
        }
        [Fact]
        public void TestOpenInvokeClose()
        {
            UsbSession s = new UsbSession();
            //Check that it's not yet open, then try opening at check
            Assert.False(s.TestSession());
            s.Open();
            Assert.True(s.TestSession());
            s.Invoke("");
            s.Invoke("ECHO OFF");
            s.Invoke("VER -V");
            s.Close();
            Assert.False(s.TestSession());
        }
        [Fact]
        public void TestRepeatedOpenInvokeClose()
        {
            UsbSession s = new UsbSession();
            //Check that it's not yet open, then try opening at check
            Assert.False(s.TestSession());
            s.Open();
            Assert.True(s.TestSession());
            s.Invoke("");
            s.Invoke("ECHO OFF");
            s.Invoke("VER -V");
            s.Close();
            Assert.False(s.TestSession());

            UsbSession a = new UsbSession();
            //Check that it's not yet open, then try opening at check
            Assert.False(s.TestSession());
            a.Open();
            Assert.True(a.TestSession());
            a.Invoke("");
            a.Invoke("ECHO OFF");
            a.Invoke("VER -V");
            a.Close();
            Assert.False(a.TestSession());
              
            UsbSession b = new UsbSession();
            //Check that it's not yet open, then try opening at check
            Assert.False(b.TestSession());
            b.Open();
            Assert.True(b.TestSession());
            b.Invoke("");
            b.Invoke("ECHO OFF");
            b.Invoke("VER -V");
            b.Close();
            Assert.False(s.TestSession());
        }
    }
}
