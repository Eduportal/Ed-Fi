var edfiApp = angular.module('edfiApp', ['ngRoute']);

edfiApp.config([
    '$routeProvider', '$locationProvider', function($routeProvider, $locationProvider) {

        $locationProvider.html5Mode(false);

        $routeProvider.
            when('/', {
                templateUrl: 'app/components/shared/startup.html',
            }).
            when('/activate/:challengeId', {
                templateUrl: 'app/components/activation/activation.html',
                controller: 'ActivationCtrl'
            }).
            otherwise({
                templateUrl: 'app/components/activation/invalidLink.html',
            });
    }
]);
