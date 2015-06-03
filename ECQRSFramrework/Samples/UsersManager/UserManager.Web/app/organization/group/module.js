organizations.service('organizationGroupsDataService', [
    function () {
        this.newTemplate = 'app/organization/group/_new.html';
        this.editTemplate = 'app/organization/group/_edit.html';

        this.deleteConfirm = function (item) { return "You want to delete group '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "group '" + item.Code + "'" }
        this.editMessage = function (item) { return "Editing group '" + item.Code + "'"; }
        this.createMessage = function () { return "Create group"; }

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
            
            var result = "/api/OrganizationGroups/list/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.get = function (item, scope) { return '/api/OrganizationGroups/' + item.Id + ''; }
        this.put = function (item, scope) { return '/api/OrganizationGroups/' + item.Id + ''; }
        this.add = function (item, scope) { return '/api/OrganizationGroups'; }
        this.delete = function (item,scope) { return '/api/OrganizationGroups/' + item.Id ; }

    }])

organizations.controller('groupsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'organizationGroupsDataService',
	function ($scope, $http, globalMessagesService, $controller, groupsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: groupsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;
	}]);

organizations.controller('groupDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'organizationGroupsDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, groupsDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: groupsDataService,
	        modalInstance: modalInstance
	    });
	}]);


organizations.controller('groupNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'organizationGroupsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, groupsDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: groupsDataService,
	        modalInstance: modalInstance,
	        commonControllersCallbacks: {
	            preSave: function (data) {
	                data.OrganizationId = $scope.parentScope.item.Id;
	                return data;
	            }
	        }
	    });
	}]);

organizations.controller('groupEditController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'organizationGroupsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, groupsDataService) {
	    $controller('EditController', {
	        $scope: $scope,
	        dataService: groupsDataService,
	        modalInstance: modalInstance
	    });

	    $controller('PanesController', {
	        $scope: $scope,
	        panes: [
			    { selected: true, title: "Data", address: "app/organization/group/edit/_data.html" },
			    { selected: false, title: "Roles", address: "app/organization/group/role/_list.html" }
	        ]
	    });
	}]);
