
$(function () {
    var svcUrl = appSettings.adminUrl;
    $("#token").click(function () {
        var auth = { Client_id: $("#appKey").val(), Response_type: "code" };
        $.support.cors = true;
        $.get(svcUrl + "authorize", auth, function (data, err) {
            var token = { Client_id: $("#appKey").val(), Client_secret: $("#appSecret").val(), Code: data.Code || data.code, Grant_type: "authorization_code" };
            $.support.cors = true;
            $.post(svcUrl + "token", token, function (d, e) {
                window.authorizations.add("key", new ApiKeyAuthorization("Authorization", "Bearer " + (d.Access_token || d.access_token), "header"));
                $("#input_apiKey").val(d.Access_token || d.access_token);
            }, "json")
                .error(function (s, e) {
                    alert("Unable to retrieve an access token. Please verify that your application secret is correct.");
                });
        }, "json")
            .error(function (s, e) {
                alert("Unable to retrieve an authorization code. Please verify that your application key is correct. Alternately, the service address may not be correct: " + svcUrl);
            });
    });
    $("#auth_token").click(function() {
    	window.authorizations.add("key", new ApiKeyAuthorization("Authorization", "Bearer " + $("#input_apiKey").val(), "header"));
	});
});