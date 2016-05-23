var EdFiAdmin = EdFiAdmin || {};

function setUrls() {
    var urls = EdFiAdmin.Urls = EdFiAdmin.Urls || {};

    urls.apiBase = urls.home + '/api';
    urls.login = urls.home + '/account/login';
    urls.logout = urls.home + '/account/logout';
    urls.client = urls.apiBase + '/client';
    urls.sandbox = urls.apiBase + '/sandbox';
}

function setWindowOrigin() {
    if (!window.location.origin) {
        window.location.origin = window.location.protocol + "//" + window.location.host;
    }
}

$(function () {
    setWindowOrigin();
    setUrls();
});