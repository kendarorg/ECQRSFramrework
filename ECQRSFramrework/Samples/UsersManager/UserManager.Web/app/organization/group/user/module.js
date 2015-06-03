//APPLICATIONS
organizations.service('organizationGroupUsersDataService', [
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
            var result = "/api/OrganizationUsers/list/" + encodeURI(scope.item.OrganizationId)+"/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };

        this.add = function (item, scope) {
            console.log(item);
            return '/api/OrganizationUsers/' +
            encodeURI(scope.item.OrganizationId) + "/" +
            encodeURI(scope.item.Id) + '/' + item.UserId;
        }
        this.delete = function (item, scope) {
            console.log(item);
            return '/api/OrganizationUsers/' +
            encodeURI(scope.item.OrganizationId) + "/" +
            encodeURI(scope.item.Id) + '/' + item.UserId;
        }
    }
]);

organizations.controller('organizationGroupUsersListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'organizationGroupUsersDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;

	    $scope.associate = function (groupRole) {
	        $http.post(applicationsDataService.add(groupRole, $scope), groupRole).
				success(function (result, status, headers, config) {
				    $scope.loadData(0);
				}).
				error(function (data, status, headers, config) {
				    globalMessagesService.showMessage(data.message, status);
				});
	    }

	    $scope.dissociate = function (groupRole) {
	        $scope.delete(groupRole, $scope);
	    }
	}]);
