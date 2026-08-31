// Google Auth JS Interop
// This uses Google Identity Services (GIS) for OAuth2

window.googleAuth = {
    // Configuration - these should be set via Google Cloud Console
    clientId: '', // Set your Google OAuth Client ID here
    scopes: [
        'https://www.googleapis.com/auth/spreadsheets',
        'https://www.googleapis.com/auth/calendar',
        'https://www.googleapis.com/auth/gmail.send',
        'https://www.googleapis.com/auth/userinfo.email',
        'https://www.googleapis.com/auth/userinfo.profile'
    ].join(' '),

    tokenClient: null,

    init: function (clientId) {
        this.clientId = clientId;
    },

    signIn: function () {
        return new Promise(function (resolve, reject) {
            if (!window.googleAuth.clientId) {
                // Prompt for client ID if not configured
                var clientId = prompt('Enter your Google OAuth Client ID:\n(Create one at https://console.cloud.google.com/apis/credentials)');
                if (!clientId) {
                    reject('No client ID provided');
                    return;
                }
                window.googleAuth.clientId = clientId;
                localStorage.setItem('google_client_id', clientId);
            }

            // Use Google Identity Services token model
            window.googleAuth.tokenClient = google.accounts.oauth2.initTokenClient({
                client_id: window.googleAuth.clientId,
                scope: window.googleAuth.scopes,
                callback: function (tokenResponse) {
                    if (tokenResponse.error) {
                        reject(tokenResponse.error);
                        return;
                    }

                    // Get user info
                    fetch('https://www.googleapis.com/oauth2/v2/userinfo', {
                        headers: { 'Authorization': 'Bearer ' + tokenResponse.access_token }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (userInfo) {
                        var tokenInfo = {
                            AccessToken: tokenResponse.access_token,
                            RefreshToken: '',
                            Email: userInfo.email || '',
                            Name: userInfo.name || '',
                            Picture: userInfo.picture || '',
                            ExpiresAt: new Date(Date.now() + tokenResponse.expires_in * 1000).toISOString()
                        };
                        localStorage.setItem('google_token_info', JSON.stringify(tokenInfo));

                        // Notify Blazor
                        DotNet.invokeMethodAsync('MoviePlanner', 'OnGoogleSignInCallback', JSON.stringify(tokenInfo));
                        resolve(tokenInfo);
                    })
                    .catch(reject);
                }
            });

            window.googleAuth.tokenClient.requestAccessToken();
        });
    }
};

// Restore client ID from storage
(function () {
    var savedClientId = localStorage.getItem('google_client_id');
    if (savedClientId) {
        window.googleAuth.clientId = savedClientId;
    }
})();
