using Xunit;

namespace UsbSession.Tests
{
    public class TestUsbSession
    {
        [Fact]
        public void TestOpenInvokeClose()
        {
            UsbSession s = new UsbSession();
            s.Open();
            Assert.True(s.TestSession());
            s.Invoke("");
            s.Invoke("ECHO OFF");
            s.ClearBuffer();
            s.Invoke("VER -V");
            s.Close();
            Assert.False(s.TestSession());
        }
        [Fact]
        public void TestRepeatedOpenInvokeClose()
        {
            UsbSession s = new UsbSession();
            s.Open();
            Assert.True(s.TestSession());
            s.Invoke("");
            s.Invoke("ECHO OFF");
            s.ClearBuffer();
            s.Invoke("VER -V");
            s.Close();
            Assert.False(s.TestSession());

            UsbSession a = new UsbSession();
            a.Open();
            Assert.True(a.TestSession());
            a.Invoke("");
            a.Invoke("ECHO OFF");
            a.ClearBuffer();
            a.Invoke("VER -V");
            a.Close();
            Assert.False(a.TestSession());
              
            UsbSession b = new UsbSession();
            b.Open();
            Assert.True(b.TestSession());
            b.Invoke("");
            b.Invoke("ECHO OFF");
            b.ClearBuffer();
            b.Invoke("VER -V");
            b.Close();
            Assert.False(s.TestSession());
        }

        [Fact]
        public void TestOpenInvokeCloseExit()
        {
            UsbSession s = new UsbSession();
            s.Open();
            Assert.True(s.TestSession());
            s.Invoke("");
            s.Invoke("ECHO OFF");
            s.ClearBuffer();
            s.Invoke("VER -V");
            s.Close();
            Assert.False(s.TestSession());
            s.Exit();
        }
    }
}
