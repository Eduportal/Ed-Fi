edfiApp.controller('VendorCtrl', function ($scope, vendorService, sharedService) {

    $scope.loading = true;
    vendorService.getAll().then(function (result) { $scope.vendorList = result; $scope.loading = false; });

    var vendorFactory = function () {
        return {};
    }
    var newVendor = vendorFactory();

    $scope.error = function (name) {
        var s = $scope.editVendorForm[name];
        var isValid = ($scope.submitted && s.$invalid) || (s.$invalid && s.$dirty);
        return isValid ? "has-error" : "";
    };

    $scope.add = function () {
        $scope.editMode = false;
        $scope.activeVendor = newVendor;
        $scope.editVendorForm.$setPristine();
        $scope.submitted = false;
    }

    $scope.edit = function (vendor) {
        $scope.editMode = true;
        $scope.activeVendor = sharedService.clone(vendor);
    }

    $scope.delete = function (vendor) {
        $scope.activeVendor = vendor;
    }

    $scope.performAdding = function (vendor) {

        if ($scope.editMode)
            return;

        if ($scope.editVendorForm.$invalid) {
            $scope.submitted = true;
            return;
        }

        $scope.submitted = false;
        $scope.disableActionButton = true;

        vendorService.addVendor(vendor).then(
            function (response) {
                $('#edit-vendor-modal').modal('hide');
                vendor.vendorId = response.data.vendorId;
                $scope.vendorList.push(vendor);
                sharedService.alertMessage($scope, "Saved", "The vendor has been added successfully", "success");

                // reset the newVendor object
                newVendor = vendorFactory();

                $scope.editVendorForm.$setPristine();
            },
            function (response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function () {
            $scope.disableActionButton = false;
        });
    };

    $scope.performDeleting = function (vendor) {
        $scope.disableActionButton = true;
        vendorService.deleteVendor(vendor.vendorId).then(
            function() {
                sharedService.alertMessage($scope, "Deleted", "The vendor has been deleted successfully", "success");
                $('#confirm-delete-modal').modal('hide');
                $scope.vendorList.splice($scope.vendorList.indexOf(vendor), 1);
            },
            function(response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
                $('#confirm-delete-modal').modal('hide');
            }
        ).finally(function () {
            $scope.disableActionButton = false;
        });
    };

    $scope.performUpdating = function (vendor) {

        if (!$scope.editMode)
            return;

        if ($scope.editVendorForm.$invalid) {
            $scope.submitted = true;
            return;
        }

        $scope.submitted = false;
        $scope.disableActionButton = true;

        vendorService.updateVendor(vendor).then(
            function () {
                $('#edit-vendor-modal').modal('hide');

                // refresh table row
                var found = sharedService.findFirst($scope.vendorList, function (i) { return i.vendorId === vendor.vendorId; });
                $scope.vendorList[found.index] = vendor;

                sharedService.alertMessage($scope, "Saved", "The vendor has been updated successfully", "success");
            },
            function (response) {
                console.debug(response);
                sharedService.alertMessage($scope, "Error!", response.data.message, "danger");
            }
        ).finally(function () {
            $scope.disableActionButton = false;

        });
    };
});
