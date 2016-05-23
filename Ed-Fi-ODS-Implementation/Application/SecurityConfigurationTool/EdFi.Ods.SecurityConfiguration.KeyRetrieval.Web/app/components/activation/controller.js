edfiApp.controller('ActivationCtrl', function ($scope, $location, $routeParams, activationService) {

    var challengeId = $routeParams.challengeId;

    $scope.validChallenge = false;

    activationService.validate(challengeId)
        .then(
            function (data) {
                $scope.validChallenge = data;
                if (!$scope.validChallenge) {
                    $location.path('/');
                }
            },
            function() {
                $location.path('/');
            });

    $scope.activate = function () {

        if ($scope.form.$invalid) {
            $scope.submitted = true;
            return;
        }

        activationService.activate(challengeId, $scope.activationCode)
            .then(
                function(data) {
                    $scope.activated = data.activated;
                    $scope.isValid = data.isValid;
                    $scope.leftChances = data.leftChances;
                    $scope.apiKey = data.apiKey;
                    $scope.apiSecret = data.apiSecret;
                },
                function() {
                    $scope.disableForm = true;
                    $scope.leftChances = 'no';
                });

        $scope.submitted = false;
        $scope.activationCode = '';
    };

    $scope.error = function (name) {
        var s = $scope.form[name];
        var isValid = ($scope.submitted && s.$invalid) || (s.$invalid && s.$dirty);
        return isValid ? "has-error" : "";
    };
});
