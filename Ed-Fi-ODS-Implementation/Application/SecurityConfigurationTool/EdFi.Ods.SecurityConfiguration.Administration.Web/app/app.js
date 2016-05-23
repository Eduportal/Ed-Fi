var edfiApp = angular.module('edfiApp', ['ngRoute']);

edfiApp.config([
    '$routeProvider', '$locationProvider', function($routeProvider, $locationProvider) {

        $locationProvider.html5Mode(false);

        $routeProvider.
            when('/vendors', {
                templateUrl: 'app/components/vendor/index.html',
                controller: 'VendorCtrl'
            }).
            when('/vendors/:vendorId/applications', {
                templateUrl: 'app/components/application/index.html',
                controller: 'ApplicationCtrl'
            }).
            otherwise({
                redirectTo: '/vendors'
            });
    }
]);

edfiApp.run(function ($rootScope) {
    $rootScope.fadeClass = 'fade'; // changes the behaviour of the modals
});
