applications.service('applicationPermissionsDataService', [
    function () {
        this.newTemplate = 'app/application/permission/_new.html';
        this.detailTemplate = 'app/application/permission/_detail.html';

        this.deleteConfirm = function (item) { return "You want to delete permission '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "permission '" + item.Code + "'" }
        this.createMessage = function () { return "Create permission"; }

        this.list = function (currentPage, pageSize, count, filter,scope) {
            var realFilter = {};
            var didSomething = false;
            for (var prop in filter) {
                var value = filter[prop];
                if (value != undefined && value !== null && value.length > 0) {
                    realFilter[prop] = value;
                    didSomething = true;
                }
            }

            var start = currentPage * pageSize;
            var end = start + count;
            
            var result = "/api/ApplicationPermissions/list/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.get = function (item, scope) { return '/api/ApplicationPermissions/' + item.Id; }
        this.add = function (item, scope) { return '/api/ApplicationPermissions'; }
        this.delete = function (item, scope) { return '/api/ApplicationPermissions/' + item.Id; }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

applications.controller('applicationPermissionsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'applicationPermissionsDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;
	}]);

applications.controller('applicationPermissionDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'applicationPermissionsDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, applicationsDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: applicationsDataService,
	        modalInstance: modalInstance
	    });

	}]);

applications.controller('applicationPermissionNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'applicationPermissionsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, applicationsDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: applicationsDataService,
	        modalInstance: modalInstance,
	        commonControllersCallbacks: {
	            preSave:function(data) {
	                data.ApplicationId = $scope.parentScope.item.Id;
	                return data;
	            }
	        }
	    });
	}]);