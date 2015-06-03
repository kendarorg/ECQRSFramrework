var maintenance = angular.module('maintenanceModule', ['commonModule', 'sgDialogService']);


maintenance.controller('MaintenanceController', ['$scope', '$http', 'globalMessagesService', function ($scope, $http, globalMessagesService) {
    $scope.loadUsers = function () {
        $http.get("/api/Maintenance/InitializeDb/LoadUsers").
                success(function (data, status, headers, config) {
                    globalMessagesService.showMessage("Users Initialized!", status,"info");
                }).
                error(function (data, status, headers, config) {
                    globalMessagesService.showMessage(data.message, status);
                });
    }

    $scope.loadApplications = function () {
        $http.get("/api/Maintenance/InitializeDb/LoadApplications").
                success(function (data, status, headers, config) {
                    globalMessagesService.showMessage("Applications Initialized!", status, "info");
                }).
                error(function (data, status, headers, config) {
                    globalMessagesService.showMessage(data.message, status);
                });
    }

    $scope.loadRolesAndPermissions = function () {
        $http.get("/api/Maintenance/InitializeDb/LoadRolesAndPermissions").
                success(function (data, status, headers, config) {
                    globalMessagesService.showMessage("Roles and permissions Initialized!", status, "info");
                }).
                error(function (data, status, headers, config) {
                    globalMessagesService.showMessage(data.message, status);
                });
    }

    $scope.loadOrganizations = function () {
        $http.get("/api/Maintenance/InitializeDb/LoadOrganizations").
                success(function (data, status, headers, config) {
                    globalMessagesService.showMessage("Organizations Initialized!", status, "info");
                }).
                error(function (data, status, headers, config) {
                    globalMessagesService.showMessage(data.message, status);
                });
    }

    $scope.loadOrganizationGroups = function () {
        $http.get("/api/Maintenance/InitializeDb/LoadOrganizationGroups").
                success(function (data, status, headers, config) {
                    globalMessagesService.showMessage("Groups Initialized!", status, "info");
                }).
                error(function (data, status, headers, config) {
                    globalMessagesService.showMessage(data.message, status);
                });
    }
}]);