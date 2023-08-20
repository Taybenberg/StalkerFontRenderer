using System.Text;

namespace StalkerFontProcessing.Helpers
{
    internal static class TextHelper
    {
        static TextHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static byte[] EncodeText(string text)
        {
            var txt = text.Replace(Environment.NewLine, "\n");
            var encoding = Encoding.GetEncoding(1251);
            var srcBytes = Encoding.Default.GetBytes(txt);
            return Encoding.Convert(Encoding.Default, encoding, srcBytes);
        }
    }
}
