applications.service('applicationRolesDataService', [
    function () {
        this.newTemplate = 'app/application/role/_new.html';
        this.editTemplate = 'app/application/role/_edit.html';

        this.deleteConfirm = function (item) { return "You want to delete role '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "role '" + item.Code + "'" }
        this.editMessage = function (item) { return "Editing role '" + item.Code + "'"; }
        this.createMessage = function () { return "Create role"; }

        this.list = function (currentPage, pageSize, count, filter, scope) {
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
            
            var result = "/api/ApplicationRoles/list/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.get = function (item, scope) { return '/api/ApplicationRoles/' + item.Id + ''; }
        this.put = function (item, scope) { return '/api/ApplicationRoles/' + item.Id + ''; }
        this.add = function (item, scope) { return '/api/ApplicationRoles'; }
        this.delete = function (item,scope) { return '/api/ApplicationRoles/' + item.Id ; }

    }])

applications.controller('rolesListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'applicationRolesDataService',
	function ($scope, $http, globalMessagesService, $controller, rolesDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: rolesDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;
	}]);

applications.controller('roleDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'applicationRolesDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, rolesDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: rolesDataService,
	        modalInstance: modalInstance
	    });
	}]);


applications.controller('roleNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'applicationRolesDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, rolesDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: rolesDataService,
	        modalInstance: modalInstance,
	        commonControllersCallbacks: {
	            preSave: function (data) {
	                data.ApplicationId = $scope.parentScope.item.Id;
	                return data;
	            }
	        }
	    });
	}]);

applications.controller('roleEditController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'applicationRolesDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, rolesDataService) {
	    $controller('EditController', {
	        $scope: $scope,
	        dataService: rolesDataService,
	        modalInstance: modalInstance
	    });

	    $controller('PanesController', {
	        $scope: $scope,
	        panes: [
			    { selected: true, title: "Data", address: "app/application/role/edit/_data.html" },
			    { selected: false, title: "Permissions", address: "app/application/role/permission/_list.html" }
	        ]
	    });
	}]);
