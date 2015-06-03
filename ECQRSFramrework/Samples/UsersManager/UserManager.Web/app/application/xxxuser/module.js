//APPLICATIONS
applications.service('applicationUsersDataService', [
    function() {

        this.deleteConfirm = function(item) { return "You want to delete association '" + item.Id + "'?"; }

        this.list = function(currentPage, pageSize, count, filter, scope) {
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
            var result = "/api/applicationusers/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.delete = function (item, scope) { return '/api/ApplicationUsers/' + encodeURI(scope.item.Id)+"/" + item.Id + ''; }
    }
]);

applications.controller('applicationUsersListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'applicationUsersDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;

	    $scope.dissociate = function (user) {
	        $scope.delete(user);
	    }
	}]);
