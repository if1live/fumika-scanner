using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

interface IGoogleDriveTokenStorage {
	/// <summary>
	/// Access token.
	/// </summary>
	string AccessToken { get; set; }

	/// <summary>
	/// Refresh token.
	/// </summary>
	string RefreshToken { get; set; }

	/// <summary>
	/// User's E-Mail address.
	/// </summary>
	string UserAccount { get; set; }
}

class GoogleDriveTokenStorage_PlayerPrefs : IGoogleDriveTokenStorage {
	public string clientId;

	public GoogleDriveTokenStorage_PlayerPrefs(string clientId) {
		this.clientId = clientId;
	}

	string accessToken = null;

	public string AccessToken
	{
		get
		{
			if (accessToken == null) {
				int key = clientId.GetHashCode();
				accessToken = PlayerPrefs.GetString("UnityGoogleDrive_Token_" + key, "");
			}

			return accessToken;
		}
		set
		{
			if (accessToken != value) {
				accessToken = value;

				int key = clientId.GetHashCode();

				if (accessToken != null)
					PlayerPrefs.SetString("UnityGoogleDrive_Token_" + key, accessToken);
				else
					PlayerPrefs.DeleteKey("UnityGoogleDrive_Token_" + key);
			}
		}
	}

	string refreshToken = null;

	public string RefreshToken
	{
		get
		{
			if (refreshToken == null) {
				int key = clientId.GetHashCode();
				refreshToken = PlayerPrefs.GetString("UnityGoogleDrive_RefreshToken_" + key, "");
			}

			return refreshToken;
		}
		set
		{
			if (refreshToken != value) {
				refreshToken = value;

				int key = clientId.GetHashCode();

				if (refreshToken != null)
					PlayerPrefs.SetString("UnityGoogleDrive_RefreshToken_" + key, refreshToken);
				else
					PlayerPrefs.DeleteKey("UnityGoogleDrive_RefreshToken_" + key);
			}
		}
	}

	string userAccount = null;

	public string UserAccount
	{
		get
		{
			if (userAccount == null) {
				int key = clientId.GetHashCode();
				userAccount = PlayerPrefs.GetString("UnityGoogleDrive_UserAccount_" + key, "");
			}

			return userAccount;
		}
		set
		{
			if (userAccount != value) {
				userAccount = value;

				int key = clientId.GetHashCode();

				if (userAccount != null)
					PlayerPrefs.SetString("UnityGoogleDrive_UserAccount_" + key, userAccount);
				else
					PlayerPrefs.DeleteKey("UnityGoogleDrive_UserAccount_" + key);
			}
		}
	}
}