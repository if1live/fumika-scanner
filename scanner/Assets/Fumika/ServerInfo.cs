using System;

namespace Assets.Fumika {
    [Serializable]
    public class ServerInfo {
        public string host;
        public int port;

        public string GetName() {
            return host + ":" + port;
        }

        public bool TryFill(string text) {
            host = "";
            port = 0;

            if (text == null || text == "") {
                return false;
            }

            var tokens = text.Split(':');
            if (tokens.Length != 2) {
                return false;
            }

            if (!int.TryParse(tokens[1], out port)) {
                return false;
            }

            host = tokens[0];
            return true;
        }

        public bool IsValid() {
            if (host != null && host != "" && port > 0) {
                return true;
            }
            return false;
        }
    }
}
