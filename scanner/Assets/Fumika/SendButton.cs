using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Fumika {
    public class SendButton : MonoBehaviour {
        [SerializeField]
        string codeType = "";
        [SerializeField]
        string codeValue = "";

        Text buttonText = null;

        public void SetBarcode(string barCodeType, string barCodeValue) {
            this.codeType = barCodeType;
            this.codeValue = barCodeValue;

            buttonText.text = string.Format("Ready: {0}", codeValue);
        }

        void Awake() {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            buttonText = button.GetComponentInChildren<Text>();

            Debug.Assert(buttonText != null);
        }

        IEnumerator BeginSend() {
            buttonText.text = string.Format("Sending: {0}", codeValue);
            Debug.LogFormat("send type={0}, value={1}", codeType, codeValue);

            yield return null;


            // TODO http 통신 집어넣기
            // 중앙 서버 하나 있으면 될거같은데


            codeType = "";
            codeValue = "";
            buttonText.text = string.Format("Complete: {0}", codeValue);
        }

        bool IsSendableState() {
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

            if (IsSendableState()) {
                sendCoroutine = StartCoroutine(BeginSend());
            }
        }
    }
}
