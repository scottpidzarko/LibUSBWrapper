using Xunit;
using Xunit.Abstractions;

namespace UsbSession.Tests
{
    public class TestUsbSession
    {
        private readonly ITestOutputHelper output;

        public TestUsbSession(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestOpenInvokeClose()
        {
            output.WriteLine("Starting TestOpenInvokeClose");
            UsbSession s = new UsbSession();
            output.WriteLine("Opening Session");
            s.Open();
            Assert.True(s.TestSession());
            output.WriteLine("Session opened. Sending CRLF");
            output.WriteLine(s.Invoke(""));
            output.WriteLine("Clearing Buffer");
            s.ClearReadBuffer();
            output.WriteLine("Buffer cleared. Logging in, sending username");
            output.WriteLine(s.Invoke("crestron"));
            output.WriteLine("Sending p/w");
            output.WriteLine(s.Invoke("")); //Most crestron devices default to Blank p/w
            System.Threading.Thread.Sleep(1000);
            output.WriteLine("Sending ECHO OFF");
            output.WriteLine(s.Invoke("ECHO OFF"));
            output.WriteLine("Running two test commands");
            output.WriteLine(s.Invoke("VER -V"));
            output.WriteLine(s.Invoke("HOSTNAME"));
            s.Close();
            Assert.False(s.TestSession());
            output.WriteLine("TestOpenInvokeClose passed.");
        }

        [Fact]
        public void TestOpenInvokeWithPromptAndClose()
        {
            UsbSession s = new UsbSession();
            output.WriteLine("Opening Session");
            s.Open();
            Assert.True(s.TestSession());
            output.WriteLine("Session opened. Sending CRLF");
            output.WriteLine(s.Invoke(""));
            output.WriteLine("Clearing Buffer");
            s.ClearReadBuffer();
            output.WriteLine("Buffer cleared.");
            output.WriteLine("Running two test commands");
            output.WriteLine(s.Invoke("VER -V",">"));
            output.WriteLine(s.Invoke("HOSTNAME",">"));
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
            s.ClearReadBuffer();
            output.WriteLine(s.Invoke("VER -V"));
            output.WriteLine(s.Invoke("HOSTNAME"));
            s.Close();
            Assert.False(s.TestSession());

            UsbSession a = new UsbSession();
            a.Open();
            Assert.True(a.TestSession());
            a.Invoke("");
            a.Invoke("ECHO OFF");
            a.ClearReadBuffer();
            output.WriteLine(a.Invoke("VER -V"));
            output.WriteLine(a.Invoke("HOSTNAME"));
            a.Close();
            Assert.False(a.TestSession());
              
            UsbSession b = new UsbSession();
            b.Open();
            Assert.True(b.TestSession());
            b.Invoke("");
            b.Invoke("ECHO OFF");
            b.ClearReadBuffer();
            output.WriteLine(b.Invoke("VER -V"));
            output.WriteLine(b.Invoke("HOSTNAME"));
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
            s.ClearReadBuffer();
            output.WriteLine(s.Invoke("VER -V"));
            output.WriteLine(s.Invoke("HOSTNAME"));
            s.Close();
            Assert.False(s.TestSession());
            s.Exit();
        }
    }
}
