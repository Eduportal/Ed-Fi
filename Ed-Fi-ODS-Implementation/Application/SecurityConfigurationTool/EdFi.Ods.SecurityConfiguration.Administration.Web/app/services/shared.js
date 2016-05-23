edfiApp.service('sharedService', function($http) {

    var serviceRoot = 'api/';

    return {
        getClaimSets: function() {
            var url = serviceRoot + 'list/claimsets';
            return $http({ cache: true, url: url, method: 'GET' })
                .then(function(response) { return response.data; });
        },

        getSystemDate: function () {
            var url = serviceRoot + 'server-date';
            return $http({ cache: true, url: url, method: 'GET' })
                .then(function (response) { return response.data; });
        },

        getUserInfo: function () {
            var url = serviceRoot + 'user-info';
            return $http({ cache: true, url: url, method: 'GET' })
                .then(function (response) { return response.data; });
        },

        getProfiles: function() {
            var url = serviceRoot + 'list/profiles';
            return $http({ cache: true, url: url, method: 'GET' })
                .then(function(response) { return response.data; });
        },

        alertMessage: function(scope, title, body, type) {
            if (!type) type = 'danger';
            scope.alertMessageTitle = title;
            scope.alertMessageBody = body;
            scope.alertMessageType = type;
            $('#alert-message').toggleClass('in');
            setTimeout(function() { $('#alert-message').toggleClass('in'); }, 3000);
        },

        clone: function(obj) {
            return angular.copy(obj);
        },

        removeFirst: function(array, comparer) {
            if (!comparer)
                return array.pop();

            for (var i = 0; i < array.length; ++i) {
                if (comparer(array[i])) {
                    var ret = array[i];
                    array.splice(i, 1);
                    return { index: i, data: ret }
                }
            }
            return null;
        },

        findFirst: function(array, comparer) {
            if (!comparer)
                return array[0];

            for (var i = 0; i < array.length; ++i) {
                if (comparer(array[i])) {
                    var ret = array[i];
                    return { index: i, data: ret }
                }
            }
            return null;
        },
    };
});
