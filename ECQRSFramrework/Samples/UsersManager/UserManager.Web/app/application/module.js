var applications = angular.module('applicationsModule', ['commonModule', 'sgDialogService']);

applications.service('applicationsDataService', [
    function() {

        this.newTemplate = 'app/application/new.html';
        this.editTemplate = 'app/application/edit.html';

        this.deleteConfirm = function (item) { return "You want to delete application '" + item.Name + "'?"; }

        this.editMessage = function(item) { return "Editing application '" + item.Name + "'"; }
        this.createMessage = function() { return "Create application"; }

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
            var result = "/api/applications?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };
        this.get = function (item, scope) { return '/api/applications/' + item.Id + ''; }
        this.put = function (item, scope) { return '/api/applications/' + item.Id + ''; }
        this.add = function (item, scope) { return '/api/applications'; }
        this.delete = function (item, scope) { return '/api/applications/' + item.Id + ''; }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

applications.controller('applicationsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'applicationsDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 10;
	    $scope.maxPages = 10;
	}]);

applications.controller('applicationDetailController', ['$scope', '$http', '$routeParams', '$location', 'globalMessagesService',
		'modalInstance', '$controller', 'applicationsDataService',
	function ($scope, $http, $routeParams, $location, globalMessagesService, modalInstance, $controller, applicationsDataService) {
	    $controller('DetailController', {
	        $scope: $scope,
	        dataService: applicationsDataService,
	        modalInstance: modalInstance
	    });

	}]);


applications.controller('applicationNewController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'applicationsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, applicationsDataService) {
	    $controller('NewController', {
	        $scope: $scope,
	        dataService: applicationsDataService,
	        modalInstance: modalInstance
	    });
	}]);

applications.controller('applicationEditController', ['$scope', '$http', '$location', 'globalMessagesService',
		'$controller', 'modalInstance', 'applicationsDataService',
	function ($scope, $http, $location, globalMessagesService, $controller, modalInstance, applicationsDataService) {
	    $controller('EditController', {
	        $scope: $scope,
	        dataService: applicationsDataService,
	        modalInstance: modalInstance
	    });

	    $controller('PanesController', {
	        $scope: $scope,
	        panes: [
			    { selected: true, title: "Data", address: "app/application/edit/_data.html" },
			    { selected: false, title: "Permissions", address: "app/application/permission/_list.html" },
			    { selected: false, title: "Roles", address: "app/application/role/_list.html" }
	        ]
	    });
	}]);

