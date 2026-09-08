using System;

namespace BloodDragon
{
    /// <summary>
    /// Focus-driven help for settings rows. The settings screen owns the footer
    /// and the optional detail card; rows only announce what is focused.
    /// </summary>
    public static class SettingsHint
    {
        public static event Action<string, string> Focused;
        public static event Action Cleared;
        public static event Action<string> DetailRequested;

        public static string FromLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return "";
            const string prefix = "set.";
            return label.StartsWith(prefix) ? label.Substring(prefix.Length) : label;
        }

        public static void Announce(string hintKey, string detailKey = "")
            => Focused?.Invoke(hintKey ?? "", detailKey ?? "");

        public static void Clear() => Cleared?.Invoke();

        public static void OpenDetail(string detailKey)
        {
            if (!string.IsNullOrEmpty(detailKey))
                DetailRequested?.Invoke(detailKey);
        }
    }
}
