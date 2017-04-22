using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Fumika {
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
        Button button = null;

        public void SetBarcode(string barCodeType, string barCodeValue) {
            this.codeType = barCodeType;
            this.codeValue = barCodeValue;

            buttonText.text = string.Format("Ready: {0}", codeValue);
        }

        void Awake() {
            Debug.Assert(serverAddrInput != null);

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            buttonText = button.GetComponentInChildren<Text>();
            Debug.Assert(buttonText != null);

            serverAddrInput.onValueChanged.AddListener(delegate (string text)
            {
                server.TryFill(text);
            });
        }

        void Start() {
            StartCoroutine(BeginWaitInitialize());
        }

        IEnumerator BeginWaitInitialize() {
            button.interactable = false;

            var api = SheetsAPI.Instance;
            yield return api.BeginWaitInitialize();

            button.interactable = true;
        }

        IEnumerator BeginSend() {
            if (IsSendable() == false) {
                buttonText.text = string.Format("cannot send: {0}", codeValue);
                yield break;
            }

            buttonText.text = string.Format("Sending: {0}", codeValue);
            Debug.LogFormat("send type={0}, value={1}, server={2}", codeType, codeValue, server.GetName());

            var api = SheetsAPI.Instance;
            yield return api.BeginAppendValue(codeValue);
            var resp = api.LastResponse;

            if (resp.IsError) {
                buttonText.text = string.Format("API Error {0}", resp.Error);
            } else if (resp.ResponseCode != 200) {
                buttonText.text = string.Format("Fail {0}", resp.ResponseCode);
            } else {
                buttonText.text = string.Format("Complete: {0}", codeValue);
            }

            codeType = "";
            codeValue = "";
        }

        bool IsSendable() {
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
