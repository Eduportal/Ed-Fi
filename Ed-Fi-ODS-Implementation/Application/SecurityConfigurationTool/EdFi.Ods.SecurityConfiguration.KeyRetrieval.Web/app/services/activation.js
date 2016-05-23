edfiApp.service('activationService', function ($http) {

    return {
        activate: function(challengeId, activationCode) {
            var url = 'activate';
            var data = { challengeId: challengeId, activationCode: activationCode };

            return $http.post(url, data)
                .then(function(response) { return response.data; });
        },

        validate: function(challengeId) {
            var url = 'validate/' + challengeId;
            
            return $http.get(url)
                .then(function (response) { return response.data; });
        }
    }
});
