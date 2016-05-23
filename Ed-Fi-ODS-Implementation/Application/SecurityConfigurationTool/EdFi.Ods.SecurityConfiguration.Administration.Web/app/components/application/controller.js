edfiApp.controller('ApplicationCtrl', function ($scope, $location, $filter, $routeParams, vendorService, sharedService) {

    var vendorId = $routeParams.vendorId;

    $scope.loading = true;
    vendorService.getVendorApplications(vendorId).then(function (result) { $scope.appList = result; $scope.loading = false; });
    vendorService.getById(vendorId).then(function (result) { $scope.vendor = result; });
    sharedService.getClaimSets().then(function (result) { $scope.claimSets = result; });
    sharedService.getProfiles().then(function (result) { $scope.allProfiles = result; });
    
    var appFactory = function() {
        return {
            educationOrganizations: [],
            associatedProfiles: [],
            keyStatus: 'No Key'
        };
    }
    var newApp = appFactory();

    $scope.error = function (name) {
        var s = $scope.editApplicationForm[name];
        var isValid = ($scope.submitted && s.$invalid) || (s.$invalid && s.$dirty);
        return isValid ? "has-error" : "";
    };

    $scope.add = function() {
        $scope.editMode = false;
        $scope.activeApp = newApp;
        $scope.availableProfiles = sharedService.clone($scope.allProfiles);
        $scope.editApplicationForm.$setPristine();
        $scope.submitted = false;
    };

    $scope.edit = function(app) {
        $scope.editMode = true;
        $scope.activeApp = sharedService.clone(app);

        $scope.availableProfiles = sharedService.clone($scope.allProfiles);
        angular.forEach($scope.activeApp.associatedProfiles, function(profile) {
            sharedService.removeFirst($scope.availableProfiles, function(item) {
                return item.profileId === profile.profileId;
            });
        });
    };

    $scope.delete = function(app) {
        $scope.activeApp = app;
    };

    $scope.genKey = function(app) {
        $scope.activeApp = app;
    };

    $scope.addEdOrg = function (edOrg) {

        var duplicateFound = ($.grep($scope.activeApp.educationOrganizations, function (e) { return e.educationOrganizationId == edOrg.educationOrganizationId; }).length > 0);

        if (duplicateFound) {
            $('#duplicateEdOrg').slideDown();
            setTimeout(function () { $('#duplicateEdOrg').slideUp(); }, 1500);
        }
        else {
            $scope.activeApp.educationOrganizations.push(edOrg);
            delete $scope.activeApp.selectedEdOrg;
            $scope.$apply();
        }
    };

    $scope.removeEdOrg = function (edOrg) {
        $scope.activeApp.educationOrganizations = jQuery.grep($scope.activeApp.educationOrganizations, function (i) {
            return i.educationOrganizationId != edOrg.educationOrganizationId;
        });
    };

    $scope.addProfile = function(profile) {
        sharedService.removeFirst($scope.availableProfiles, function(item) {
            return item.profileId === profile.profileId;
        });

        $scope.activeApp.associatedProfiles.push(profile);

    };

    $scope.removeProfile = function(profile) {
        sharedService.removeFirst($scope.activeApp.associatedProfiles, function (item) {
            return item.profileId === profile.profileId;
        });
        $scope.availableProfiles.push(profile);
    }

    $scope.updateProfiles = function () {
        var selection = $filter('filter')($scope.availableProfiles, { selected: true });

        $scope.activeApp.associatedProfiles = selection;
    };

    $scope.performAdding = function(app) {
        if ($scope.editMode)
            return;

        if ($scope.editApplicationForm.$invalid) {
            $scope.submitted = true;
            return;
        }

        $scope.disableActionButton = true;

        vendorService.addApplication(vendorId, app).then(
            function(response) {
                $('#edit-application-modal').modal('hide');
                app.applicationId = response.data.applicationId;
                $scope.appList.push(app);
                sharedService.alertMessage($scope, "Saved", "The application has been added successfully", "success");

                // reset the newApp object
                newApp = appFactory();
                $scope.submitted = false;

                $scope.editApplicationForm.$setPristine();
            },
            function(response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function() {
            $scope.disableActionButton = false;

        });
    };

    $scope.performUpdating = function(app) {

        if (!$scope.editMode)
            return;

        if ($scope.editApplicationForm.$invalid) {
            $scope.submitted = true;
            return;
        }

        $scope.submitted = false;
        $scope.disableActionButton = true;

        vendorService.updateApplication(vendorId, app).then(
            function() {
                $('#edit-application-modal').modal('hide');

                // refresh table row
                var found = sharedService.findFirst($scope.appList, function (i) { return i.applicationId === app.applicationId; });
                $scope.appList[found.index] = app;

                sharedService.alertMessage($scope, "Saved", "The application has been updated successfully", "success");
            },
            function(response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function () {
            $scope.disableActionButton = false;

        });
    };

    $scope.performDeleting = function(app) {
        $scope.disableActionButton = true;
        vendorService.deleteApplication(vendorId, app).then(
            function() {
                sharedService.alertMessage($scope, "Deleted", "The application has been deleted successfully", "success");
                $('#confirm-delete-modal').modal('hide');
                $scope.appList.splice($scope.appList.indexOf(app), 1);
            },
            function(response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function () {
            $scope.disableActionButton = false;

        });
    };

    $scope.performKeyGen = function(app) {
        $scope.disableActionButton = true;
        vendorService.regenApplicationKey(vendorId, app).then(
            function(response) {
                app.generated = true;
                sharedService.alertMessage($scope, "Done", "The key has been created successfully", "success");
                app.keyStatus = response.keyStatus;
                app.activationCode = response.activationCode;
            },
            function(response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function() {
            $scope.disableActionButton = false;
        });
    };
});
