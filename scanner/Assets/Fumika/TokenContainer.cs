using UnityEngine;

namespace Assets.Fumika {
    class TokenContainer : ScriptableObject, IGoogleDriveTokenStorage {
        [SerializeField]
        string _accessToken;

        [SerializeField]
        string _refreshToken;

        [SerializeField]
        string _userAccount;

        public string AccessToken
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        public string RefreshToken
        {
            get { return _refreshToken; }
            set { _refreshToken = value; }
        }
        public string UserAccount
        {
            get { return _userAccount; }
            set { _userAccount = value; }
        }
    }
}
