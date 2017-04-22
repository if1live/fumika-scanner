using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Fumika {
    public class APIResponse {
        readonly public long ResponseCode;

        readonly public string Error;
        readonly public bool IsError;
        readonly JSONObject ResponseJSON;

        public APIResponse(UnityWebRequest www) {
            IsError = www.isError;

            if (IsError == false) {
                ResponseCode = www.responseCode;
                Error = www.error;

                ResponseJSON = new JSONObject(www.downloadHandler.text, int.MaxValue);
            }
        }
    }

    public class SheetsAPI : MonoBehaviour {
        public static SheetsAPI Instance { get; private set; }

        public string sheetName = "ISBN List";

        public string SheetID { get; private set; }
        string clientId = "488206440345-ncua72rcgirjkubrn0ubru20r6vn0f7k.apps.googleusercontent.com";
        string clientSecret = "XyV8fpK4i9VqIXdlLWguUhtM";

        IGoogleDriveTokenStorage storage;
        GoogleDrive drive;

        public APIResponse LastResponse { get; private set; }

        public IEnumerator BeginAppendValue(string val) {
            var range = "A1";
            var uri = string.Format("https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}:append", SheetID, range);

            // Query parameters
            var qs = new QueryStringBuilder();
            qs.Add("valueInputOption", "USER_ENTERED");
            qs.Add("insertDataOption", "INSERT_ROWS");
            qs.Add("includeValuesInResponse", "true");
            qs.Add("responseValueRenderOption", "FORMULA");
            qs.Add("responseDateTimeRenderOption", "FORMATTED_STRING");
            var querystring = qs.ToString();

            JSONObject row = new JSONObject(JSONObject.Type.ARRAY);
            row.Add(val);

            JSONObject values = new JSONObject(JSONObject.Type.ARRAY);
            values.Add(row);

            JSONObject requestJSON = new JSONObject();
            requestJSON.AddField("values", values);

            var url = uri + "?" + querystring;
            UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

            www.SetRequestHeader("Authorization", "Bearer " + storage.AccessToken);

            byte[] bytes = Encoding.UTF8.GetBytes(requestJSON.ToString());
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            uH.contentType = "application/json";
            www.uploadHandler = uH;

            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.Send();

            while(!www.isDone) {
                yield return null;
            }

            var resp = new APIResponse(www);
            LastResponse = resp;
        }

        bool initInProgress = false;

        IEnumerator BeginInit() {
            initInProgress = true;

            var scopes = new string[] {
                "https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/drive.appdata",

                "https://www.googleapis.com/auth/drive",
                "https://www.googleapis.com/auth/spreadsheets",
            };
            storage = new GoogleDriveTokenStorage_PlayerPrefs(clientId);
            drive = new GoogleDrive(storage);
            drive.ClientID = clientId;
            drive.ClientSecret = clientSecret;
            drive.Scopes = scopes;

            var authorization = drive.Authorize();
            yield return StartCoroutine(authorization);

            if (authorization.Current is GoogleDrive.Exception) {
                Debug.LogWarning(authorization.Current as GoogleDrive.Exception);
                initInProgress = false;
                yield break;

            } else {
                Debug.Log("User Account: " + drive.UserAccount);
            }

            var finder = new SheetsAPI_FindSheet(drive, this);
            yield return finder.BeginFindSheet(sheetName);
            if (!finder.Found) {
                Debug.LogWarningFormat("Cannot find Sheet, {0}", sheetName);
            }
            SheetID = finder.SheetID;

            initInProgress = false;
        }

        public IEnumerator BeginWaitInitialize() {
            while(drive == null) {
                yield return null;
            }
            while (initInProgress == true) {
                yield return null;
            }
        }

        void Awake() {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        void Start() {
            StartCoroutine(BeginInit());
        }

        void OnDestroy() {
            Debug.Assert(Instance == this);
            Instance = null;
        }

        bool revokeInProgress = false;
        IEnumerator BeginRevoke() {
            revokeInProgress = true;
            yield return StartCoroutine(drive.Unauthorize());
            revokeInProgress = false;
        }



    }
}