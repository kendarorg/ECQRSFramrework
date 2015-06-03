var organizations = angular.module('organizationsModule', ['commonModule', 'sgDialogService']);

organizations.service('organizationsDataService', [
    function() {

        this.newTemplate = 'app/organization/new.html';
        this.editTemplate = 'app/organization/edit.html';

        this.deleteConfirm = function (item) { return "You want to delete organization '" + item.Name + "'?"; }

        this.editMessage = function(item) { return "Editing organization '" + item.Name + "'"; }
        this.createMessage = function() { return "Create organization"; }

        this.list = function(currentPage, pageSize, count, filter) {
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
            var result = "/api/organizations?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };
        this.get = function (item, scope) { return '/api/organizations/' + item.Id + ''; }
        this.put = function (item, scope) { return '/api/organizations/' + item.Id + ''; }
        this.add = function (item, scope) { return '/api/organizations'; }
        this.delete = function (item, scope) { return '/api/organizations/' + item.Id + ''; }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

organizations.controller('organizationsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'organizationsDataService',
	function ($scope, $http, globalMessagesService, $controller, organizationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: organizationsDataService
	    });
	    $scope.pageSize = 10;
	    $scope.maxPages = 10;
	}]);

organizations.controller('organizationDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'organizationsDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, organizationsDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: organizationsDataService,
	        modalInstance: modalInstance
	    });

	}]);


organizations.controller('organizationNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'organizationsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, organizationsDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: organizationsDataService,
	        modalInstance: modalInstance
	    });
	}]);

organizations.controller('organizationEditController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'organizationsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, organizationsDataService) {
	    $controller('EditController', {
	        $scope: $scope,
	        dataService: organizationsDataService,
	        modalInstance: modalInstance
	    });

	    $controller('PanesController', {
	        $scope: $scope,
	        panes: [
			    { selected: true, title: "Data", address: "app/organization/edit/_data.html" },
			    { selected: false, title: "Users", address: "app/organization/user/_list.html" },
			    { selected: false, title: "Available Roles", address: "app/organization/role/_list.html" },
			    { selected: false, title: "Groups", address: "app/organization/group/_list.html" }
	        ]
	    });
	}]);

