edfiApp.service('vendorService', function($http) {

    var serviceRoot = 'api/vendors/';

    return {
        getAll: function() {
            var url = serviceRoot;
            return $http.get(url)
                .then(function(response) { return response.data; });
        },

        getById: function(vendorId) {
            var url = serviceRoot + vendorId;
            return $http.get(url)
                .then(function (response) { return response.data; });
        },

        getVendorApplications: function(vendorId) {
            var url = serviceRoot + vendorId + '/applications';
            return $http.get(url)
                .then(function (response) { return response.data; });
        },

        getVendorApplication: function(vendorId, applicationId) {
            var url = serviceRoot + vendorId + '/applications/' + applicationId;
            return $http.get(url)
                .then(function(response) { return response.data; });
        },

        addApplication: function(vendorId, app) {
            var url = serviceRoot + vendorId + '/applications';
            return $http.post(url, app)
                .then(function(response) { return response; });
        },

        deleteApplication: function(vendorId, app) {
            var url = serviceRoot + vendorId + '/applications/' + app.applicationId;
            return $http.delete(url)
                .then(function(response) { return response; });
        },

        regenApplicationKey: function(vendorId, app) {
            var url = serviceRoot + vendorId + '/applications/' + app.applicationId + '/regen';
            return $http.put(url)
                .then(function(response) { return response.data; });
        },

        updateApplication: function(vendorId, app) {
            var url = serviceRoot + vendorId + '/applications';
            return $http.put(url, app)
                .then(function(response) { return response; });
        },

        addVendor: function(vendor) {
            var url = serviceRoot;
            return $http.post(url, vendor)
                .then(function(response) { return response; });
        },

        updateVendor: function(vendor) {
            var url = serviceRoot;
            return $http.put(url, vendor)
                .then(function(response) { return response; });
        },

        deleteVendor: function(vendorId) {
            var url = serviceRoot + vendorId;
            return $http.delete(url)
                .then(function(response) { return response; });
        }
    }
});
