using System.Collections.Generic;
using System.Text;

namespace Assets.Fumika {
    class QueryStringBuilder {
        struct KeyValue {
            public string key;
            public string value;

            public KeyValue(string key, string value) {
                this.key = key;
                this.value = value;
            }
        }

        readonly List<KeyValue> values = new List<KeyValue>();

        public void Add(string key, string value) {
            values.Add(new KeyValue(key, value));
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < values.Count; i++) {
                var kv = values[i];
                sb.Append(kv.key);
                sb.Append("=");
                sb.Append(kv.value);

                if (i != values.Count - 1) {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }
    }
}
