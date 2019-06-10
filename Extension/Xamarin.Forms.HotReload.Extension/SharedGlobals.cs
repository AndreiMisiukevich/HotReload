namespace Xamarin.Forms.HotReload.Extension
{
    public static class SharedGlobals
    {
        //Package defining constants.
        public const string PackageCollectionPath = "HotReloadExtension";
        public const string AppVersion = "1.3.0";

        //Settings store keys.
        public const string SavePreferencesPath = "SavePreferences";
        public const string SerializedConnectionItemsPath = "SerializedConnectionItems";
        public const string ShowEnableHotReloadTooltipPath = "ShowEnableHotReloadTooltip";

        //ConnectionsDialog constants.
        public static string DefaultSerializedConnectionItemsValue;
        public static string[] DefaultAvailableProtocolsValue = {"http", "https"};
        public const string DefaultProtocolValue = "http";
        public const string DefaultIpAddressValue = "127.0.0.1";
        public const string DefaultPortValue = "8000";
        public const int MaxConnectionsCount = 20;
        public const int DefaultUdpAutoDiscoveryPort = 15000;

        //UI globals.
        public const string ToolBarName = "Xamarin.Forms Hot Reload";
    }
}