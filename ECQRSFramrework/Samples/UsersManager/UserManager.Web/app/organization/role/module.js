organizations.service('organizationRolesDataService', [
    function () {
        this.newTemplate = 'app/organization/role/_new.html';
        this.detailTemplate = 'app/organization/role/_detail.html';

        this.deleteConfirm = function (item) { return "You want to delete role '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "role '" + item.Code + "'" }
        this.createMessage = function () { return "Create role"; }

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
            console.log(start + "/" + end);
            var result = "/api/OrganizationRoles/list/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.get = function (item, scope) {
            
            return '/api/OrganizationRoles/' + item.Id;
        }
        /*this.add = function (item, scope) { return '/api/OrganizationRoles'; }
        this.delete = function (item, scope) { return '/api/OrganizationRoles/' + item.Id; }*/


        this.add = function (item, scope) {
            return '/api/OrganizationRoles/' +
            encodeURI(scope.item.Id)
        }
        this.delete = function (item, scope) {
            console.log(item);
            return '/api/OrganizationRoles/' +
            encodeURI(scope.item.Id) + '/' + item.Id;
        }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

organizations.controller('organizationRolesListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'organizationRolesDataService',
	function ($scope, $http, globalMessagesService, $controller, organizationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: organizationsDataService
	    });
	    $scope.pageSize = 6;
	    $scope.maxPages = 6;

	    $scope.associate = function (rolePermission) {
	        $http.post(organizationsDataService.add(rolePermission, $scope), rolePermission).
				success(function (result, status, headers, config) {
				    $scope.loadData(0);
				}).
				error(function (data, status, headers, config) {
				    globalMessagesService.showMessage(data.message, status);
				});
	    }

	    $scope.dissociate = function (rolePermission) {
	        $scope.delete(rolePermission, $scope);
	    }
	}]);

organizations.controller('organizationRoleDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'organizationRolesDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, organizationsDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: organizationsDataService,
	        modalInstance: modalInstance
	    });

	}]);

organizations.controller('organizationRoleNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'organizationRolesDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, organizationsDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: organizationsDataService,
	        modalInstance: modalInstance,
	        commonControllersCallbacks: {
	            preSave:function(data) {
	                data.OrganizationId = $scope.parentScope.item.Id;
	                return data;
	            }
	        }
	    });
	}]);