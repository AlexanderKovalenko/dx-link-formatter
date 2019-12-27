using System.Windows.Forms;

namespace DXLinkFormatter {
    public static class UIHelper {
        public static void ShowInfo(string message, bool error = true) {
            MessageBox.Show(SplitToLines(message), "LinkFormatter", MessageBoxButtons.OK, (error ? MessageBoxIcon.Error : MessageBoxIcon.Information));
        }

        private static string SplitToLines(string message) {
            return message.Replace(". ", ".\r\n").Trim();
        }
    }
}