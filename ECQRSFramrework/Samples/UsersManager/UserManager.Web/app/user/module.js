
var users = angular.module('usersModule', ['commonModule', 'sgDialogService']);

users.service('usersDataService', [function () {

    var getName = function (item) {
        if (item.UserName) return item.UserName;
        if (item.EMail) return item.EMail;
        return item.Id;
    }
    this.newTemplate = 'app/user/new.html';
    this.editTemplate = 'app/user/edit.html';

    

    this.deleteConfirm = function (item) { return "You want to delete user '" + getName(item) + "'?"; }

    this.editMessage = function (item) { return "Editing user '" + getName(item) + "'"; }
    this.createMessage = function () { return "Create user"; }

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
        var result = "/api/users?range=[" + start + "," + end + "]";
        if (didSomething) {
            result += "&filter=" + encodeURI(JSON.stringify(realFilter));
        } 
        return result;
    };
    this.get = function (item, scope) { return '/api/users/' + item.Id + ''; }
    this.put = function (item, scope) { return '/api/users/' + item.Id + ''; }
    this.add = function (item, scope) { return '/api/users'; }
    this.delete = function (item, scope) { return '/api/users/' + item.Id + ''; }
    this.availableGroups = function () { return "/api/UserOrganizations/list/" + encodeURI('00000000-0000-0000-0000-000000000000') + ''; }

    /*this.getListCount = function (data, headers) {
        var contentRange = headers()['content-range'];
        var length = contentRange.split('/');
        return parseInt(length[1]);
    }*/
}])

users.controller('usersListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'usersDataService',
	function ($scope, $http, globalMessagesService, $controller, usersDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: usersDataService
	    });
	    $scope.pageSize = 10;
	    $scope.maxPages = 10;
	}]);

users.controller('userDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'usersDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, usersDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: usersDataService,
	        modalInstance: modalInstance
	    });
	}]);


users.controller('userNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'usersDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, usersDataService) {


	    $http.get(usersDataService.availableGroups())
            .success(function (data, status, headers, config) {

                $scope.availableOrganizations = [];
                for (var i = 0; i < data.length; i++) {
                    $scope.availableOrganizations.push({
                        key: data[i].OrganizationId,
                        value: data[i].Name
                    })
                }
                $controller('NewController', {
                    $scope: $scope,
                    dataService: usersDataService,
                    modalInstance: modalInstance
                });
            })
            .error(function (data, status, headers, config) {
                globalMessagesService.showMessage(data.message, status);
            });

	    
	}]);

users.controller('userEditController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'usersDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, usersDataService) {
	    $controller('EditController', {
	        $scope: $scope,
	        dataService: usersDataService,
	        modalInstance: modalInstance
	    });



	    $controller('PanesController', {
	        $scope: $scope,
	        panes: [
			    { selected: true, title: "Data", address: "app/user/edit/_data.html" },
                { selected: false, title: "Organizations", address: "app/user/organization/_list.html" }
	        ]
	    });
	}]);
