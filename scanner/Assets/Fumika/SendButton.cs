using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
            if(host != null && host != "" && port > 0) {
                return true;
            }
            return false;
        }
    }

    public class SendButton : MonoBehaviour {
        [SerializeField]
        string codeType = "";
        [SerializeField]
        string codeValue = "";

        [SerializeField]
        ServerInfo server = new ServerInfo();

        [SerializeField]
        InputField serverAddrInput = null;

        Text buttonText = null;

        public void SetBarcode(string barCodeType, string barCodeValue) {
            this.codeType = barCodeType;
            this.codeValue = barCodeValue;

            buttonText.text = string.Format("Ready: {0}", codeValue);
        }

        void Awake() {
            Debug.Assert(serverAddrInput != null);

            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            buttonText = button.GetComponentInChildren<Text>();
            Debug.Assert(buttonText != null);

            serverAddrInput.onValueChanged.AddListener(delegate (string text)
            {
                server.TryFill(text);
            });
        }

        IEnumerator BeginSend() {
            if (IsSendable() == false) {
                buttonText.text = string.Format("cannot send: {0}", codeValue);
                yield break;
            }

            buttonText.text = string.Format("Sending: {0}", codeValue);
            Debug.LogFormat("send type={0}, value={1}, server={2}", codeType, codeValue, server.GetName());

            yield return null;


            // TODO http 통신 집어넣기
            // 중앙 서버 하나 있으면 될거같은데


            buttonText.text = string.Format("Complete: {0}", codeValue);
            codeType = "";
            codeValue = "";
        }

        bool IsSendable() {
            if (!server.IsValid()) {
                return false;
            }

            if (codeType == null || codeType == "") {
                return false;
            }
            if (codeValue == null || codeValue == "") {
                return false;
            }

            return true;
        }

        Coroutine sendCoroutine;

        void OnClick() {
            if(sendCoroutine != null) {
                StopCoroutine(sendCoroutine);
                sendCoroutine = null;
            }

            sendCoroutine = StartCoroutine(BeginSend());
        }
    }
}
