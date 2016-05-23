edfiApp.service('sharedService', function($http) {

    var serviceRoot = 'api/';

    return {
        getSystemDate: function () {
            var url = serviceRoot + 'server-date';
            return $http({ cache: true, url: url, method: 'GET' })
                .then(function (response) { return response.data; });
        },
    };
});
