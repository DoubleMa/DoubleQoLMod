using Steamworks;
using System.Threading.Tasks;

namespace DoubleQoL.Global.Utils {

    internal class SteamHelper {
        public static SteamId Id => SteamClient.SteamId;
        public static string StrId => SteamClient.SteamId.ToString();
        public static string Name => SteamClient.Name;
        public static bool IsOn => SteamClient.IsLoggedOn;

        public static Task<AuthTicket> GetAuthSessionTicketAsync() => SteamUser.GetAuthSessionTicketAsync();

        private string GenerateSessionToken() => System.Guid.NewGuid().ToString();
    }
}