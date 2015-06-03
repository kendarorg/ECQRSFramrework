//USERS
users.service('userOrganizationDataService', [
    function () {

        this.deleteConfirm = function (item) { return "You want to delete association '" + item.Id + "'?"; }

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

            if (scope.isAssociated != null && scope.isAssociated!=undefined) {
                realFilter.isAssociated = scope.isAssociated;
                didSomething = true;
            }

            var start = currentPage * pageSize;
            var end = start + count;
            var result = "/api/UserOrganizations/list/" + encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };
        this.add = function (item, scope) { return '/api/UserOrganizations/' + scope.item.Id; }
        this.delete = function (item, scope) { return '/api/UserOrganizations/' + scope.item.Id+'/' +item.OrganizationId+'/'+ item.Id + ''; }
    }
]);

users.controller('userOrganizationsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'userOrganizationDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;

	    $scope.associate = function (applicationAssociation) {
	        $http.post(applicationsDataService.add(applicationAssociation,$scope), applicationAssociation).
				success(function (result, status, headers, config) {
	                $scope.loadData(0);
	            }).
				error(function (data, status, headers, config) {
				    globalMessagesService.showMessage(data.message, status);
				});
	    }

	    $scope.dissociate = function (applicationAssociation) {
	        $scope.delete(applicationAssociation, $scope);
	    }
	}]);